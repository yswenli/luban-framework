/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：WebApplication1.Controllers.Default
*文件名： AgentController
*版本号： V1.0.0.0
*唯一标识：Agent 示例接口
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/28
*描述：Agent 对话示例接口（需登录，会话按用户隔离），未启用时返回 503
*
*=================================================
*修改标记
*修改时间：2026/10/28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：取消匿名访问；会话按登录用户隔离并复用
*
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.1
*描述：新增 chat / chatstream（SSE）/ confirm 三动作，支持人设与附件
*
*****************************************************************************/

using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Hosting;
using LuBan.AIAgent.Protocol;
using LuBan.AIAgent.Sessions;
using LuBan.Web.Core.AIAgent;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

using WebApplication1.Models.Entities;

namespace WebApplication1.Controllers.Default;

/// <summary>
/// Agent 对话示例接口（需登录）。
/// </summary>
/// <remarks>
/// <para>会话按登录用户隔离；<c>sessionId</c> 可显式指定，未指定时复用该用户最后一次会话。</para>
/// <para>
/// 三个动作都标 <see cref="NoApiLogAttribute"/>：全局 API 日志会缓冲并记录请求/响应正文
/// （人设提示词、带签名的附件 URL），且 SSE 不能被缓冲。
/// </para>
/// </remarks>
[Route("api/agent")]
public class AgentController : BaseApiController
{
    private const string SessionNotFound = "会话不存在或已过期";
    private const string SessionForbidden = "无权访问该会话";
    private const string AgentDisabled =
        "LuBan Agent 未启用：请配置 db_config 的 sys-luban-framework:aiagent 或 appsettings 的 LuBanAgent 节";

    /// <summary>
    /// 一轮对话（<b>不装配任何工具</b>），返回正文 + 思考 + 附件摘要。
    /// </summary>
    [DisplayName("对话"), HttpPost("chat"), NoApiLog]
    public async Task<IActionResult> ChatAsync(
        [FromBody] AgentChatInput? input, CancellationToken cancellationToken)
    {
        var prepared = await PrepareAsync(input, cancellationToken).ConfigureAwait(false);
        if (prepared.Error is { } error)
            return error;

        var content = new StringBuilder();
        var thinking = new StringBuilder();
        var added = new List<AgentAttachmentAdded>();
        var skipped = new List<AgentAttachmentSkipped>();
        string? failure = null;

        var request = BuildRequest(input!, prepared.SessionId!, enableTools: false);
        await foreach (var evt in prepared.Host!.RunStreamingAsync(request, cancellationToken)
                           .ConfigureAwait(false))
        {
            switch (evt)
            {
                case TextDeltaEvent delta:
                    content.Append(delta.Delta);
                    break;
                case ThinkingDeltaEvent think:
                    thinking.Append(think.Delta);
                    break;
                case AttachmentAddedEvent attachment:
                    // Web 侧来源一定来自请求的 attachments；Source 为空只可能是本地选择器路径，用文件名兜底
                    added.Add(new AgentAttachmentAdded(
                        attachment.Source ?? attachment.FileName,
                        attachment.FileName, attachment.MediaType, attachment.FileSize));
                    break;
                case AttachmentSkippedEvent attachmentSkip:
                    skipped.Add(new AgentAttachmentSkipped(attachmentSkip.Source, attachmentSkip.Reason));
                    break;
                case ErrorEvent modelError:
                    failure = modelError.Message;
                    break;
            }
        }

        // 无正文的模型异常按 502 返回；有正文则 200 + error 字段（已产出部分内容，不该丢）
        if (failure is not null && content.Length == 0)
            return StatusCode(StatusCodes.Status502BadGateway, ErrorResult(failure, 502));

        var output = new AgentChatOutput(
            prepared.SessionId!, content.ToString(), thinking.ToString(),
            new AgentAttachmentOverview(added, skipped), failure);

        return Ok(SuccessResult(output));
    }

    /// <summary>
    /// 一轮对话（SSE 事件流，装配工具）。
    /// </summary>
    /// <remarks>
    /// 所有前置校验（401/403/404/400/503）都在切换 <c>Content-Type</c> 之前完成，
    /// 否则响应头已经是 <c>text/event-stream</c>，无法再返回 JSON 错误。
    /// </remarks>
    [DisplayName("对话流"), HttpPost("chatstream"), NoApiLog]
    public async Task<IActionResult> ChatStreamAsync(
        [FromBody] AgentChatInput? input, CancellationToken cancellationToken)
    {
        var prepared = await PrepareAsync(input, cancellationToken).ConfigureAwait(false);
        if (prepared.Error is { } error)
            return error;

        var heartbeat = ResolveHeartbeat();
        var json = HttpContext.RequestServices
            .GetRequiredService<IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>>()
            .Value.JsonSerializerOptions;

        var writer = new SseEventWriter(Response, json);
        await writer.StartAsync(cancellationToken).ConfigureAwait(false);

        var request = BuildRequest(input!, prepared.SessionId!, enableTools: true);
        var frames = StreamFramesAsync(
            prepared.Host!, request, prepared.Created, cancellationToken);

        await SseEventPump.PumpAsync(frames, writer, heartbeat, cancellationToken)
            .ConfigureAwait(false);

        return new EmptyResult();
    }

    /// <summary>
    /// 回传 <c>tool.confirm</c> 的确认结果。
    /// </summary>
    [DisplayName("工具确认"), HttpPost("confirm"), NoApiLog]
    public async Task<IActionResult> ConfirmAsync(
        [FromBody] AgentConfirmInput? input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input?.SessionId) || string.IsNullOrWhiteSpace(input.ConfirmationId))
            return StatusCode(StatusCodes.Status400BadRequest,
                ErrorResult("sessionId 与 confirmationId 不能为空", 400));

        var userId = SessionUser.UserId;
        if (userId <= 0)
            return StatusCode(StatusCodes.Status401Unauthorized, ErrorResult("未登录", 401));

        var broker = HttpContext.RequestServices.GetService<IToolConfirmationBroker>();
        if (broker is null)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ErrorResult(AgentDisabled, 503));

        if (!TryParseConfirmResult(input.Result, out var result))
            return StatusCode(StatusCodes.Status400BadRequest,
                ErrorResult("result 只能是 allow / deny / allowAll / allowSession", 400));

        // 归属校验：拿别人的 confirmationId 不能放行别人的工具调用
        var ownership = await CheckSessionOwnershipAsync(userId, input.SessionId, cancellationToken)
            .ConfigureAwait(false);
        if (ownership is { } ownershipError)
            return ownershipError;

        if (!broker.TryResolve(input.SessionId, input.ConfirmationId, result))
            return StatusCode(StatusCodes.Status404NotFound,
                ErrorResult("确认请求不存在或已过期", 404));

        return Ok(SuccessResult(new { resolved = true }));
    }

    /// <summary>前置校验 + 会话解析；任一项失败直接给出响应。</summary>
    private async Task<PreparedRequest> PrepareAsync(
        AgentChatInput? input, CancellationToken cancellationToken)
    {
        // 全局模型校验过滤器被关闭，必须自己判空
        if (string.IsNullOrWhiteSpace(input?.Prompt))
            return PreparedRequest.Fail(StatusCode(StatusCodes.Status400BadRequest,
                ErrorResult("prompt 不能为空", 400)));

        // [Authorize] 已拦截匿名请求，这里只做防御性校验
        var userId = SessionUser.UserId;
        if (userId <= 0)
            return PreparedRequest.Fail(StatusCode(StatusCodes.Status401Unauthorized,
                ErrorResult("未登录", 401)));

        // 用 GetService 而不是构造注入：未启用时控制器仍能构造，返回 503 而非 500
        var host = HttpContext.RequestServices.GetService<IAgentHost>();
        if (host is null)
            return PreparedRequest.Fail(StatusCode(StatusCodes.Status503ServiceUnavailable,
                ErrorResult(AgentDisabled, 503)));

        if (ValidateAttachments(input!) is { } attachmentError)
            return PreparedRequest.Fail(attachmentError);

        var sessions = HttpContext.RequestServices.GetRequiredService<ISessionManager>();
        var registry = HttpContext.RequestServices.GetRequiredService<WebAgentSessionRegistry>();
        var resolution = await registry
            .ResolveAsync(userId, input!.SessionId, sessions, cancellationToken)
            .ConfigureAwait(false);

        if (resolution.Status == WebSessionStatus.NotFound)
            return PreparedRequest.Fail(StatusCode(StatusCodes.Status404NotFound,
                ErrorResult(SessionNotFound, 404)));

        if (resolution.Status == WebSessionStatus.Forbidden)
            return PreparedRequest.Fail(StatusCode(StatusCodes.Status403Forbidden,
                ErrorResult(SessionForbidden, 403)));

        return new PreparedRequest(host, resolution.SessionId, resolution.Created, null);
    }

    /// <summary>附件数量/开关校验（体积与类型交给框架逐项判定）。</summary>
    private IActionResult? ValidateAttachments(AgentChatInput input)
    {
        if (input.Attachments.Count == 0)
            return null;

        var options = HttpContext.RequestServices
            .GetRequiredService<IOptions<LuBanAgentOptions>>().Value.Attachments;

        if (!options.Enabled)
            return StatusCode(StatusCodes.Status400BadRequest, ErrorResult("附件功能未启用", 400));

        if (options.MaxCount > 0 && input.Attachments.Count > options.MaxCount)
            return StatusCode(StatusCodes.Status400BadRequest,
                ErrorResult($"附件数量超过上限（{options.MaxCount}）", 400));

        return null;
    }

    /// <summary>确认接口的会话归属校验；通过返回 null。</summary>
    private async Task<IActionResult?> CheckSessionOwnershipAsync(
        long userId, string sessionId, CancellationToken cancellationToken)
    {
        var registry = HttpContext.RequestServices.GetRequiredService<WebAgentSessionRegistry>();
        if (registry.IsOwnedBy(sessionId, userId))
            return null;

        var sessions = HttpContext.RequestServices.GetRequiredService<ISessionManager>();
        var info = await sessions.GetSessionAsync(sessionId).ConfigureAwait(false);

        if (info is null)
            return StatusCode(StatusCodes.Status404NotFound, ErrorResult(SessionNotFound, 404));

        // 权威来源是注册表；SessionInfo.UserId 只在内存实现下被回填，仅作辅助校验
        if (!string.Equals(info.UserId, userId.ToString(), StringComparison.Ordinal))
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResult(SessionForbidden, 403));

        return null;
    }

    /// <summary>构造运行请求；<paramref name="enableTools"/> 为 false 时本轮没有任何工具。</summary>
    private static AgentRunRequest BuildRequest(AgentChatInput input, string sessionId, bool enableTools) => new()
    {
        Input = input.Prompt.Trim(),
        SessionId = sessionId,
        SystemPrompt = string.IsNullOrWhiteSpace(input.System) ? null : input.System,
        Attachments = input.Attachments
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .ToList(),
        EnableTools = enableTools,
        PermissionMode = ToolPermissionMode.Default
    };

    /// <summary>把宿主事件映射为帧，并保证「error 之后仍有 done」。</summary>
    private async IAsyncEnumerable<AgentEventFrame> StreamFramesAsync(
        IAgentHost host,
        AgentRunRequest request,
        bool created,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var sessionId = request.SessionId!;
        yield return new AgentEventFrame(AgentEventProtocol.Session, new { sessionId, created });

        var stopwatch = Stopwatch.StartNew();
        await using var enumerator = AgentFramesAsync(host, request, ct).GetAsyncEnumerator(ct);

        while (true)
        {
            AgentEventFrame? frame = null;
            string? failure = null;
            bool hasMore;

            try
            {
                hasMore = await enumerator.MoveNextAsync().ConfigureAwait(false);
                if (hasMore)
                    frame = enumerator.Current;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // 客户端断开：不写 done（连接已断）
                throw;
            }
            catch (Exception ex)
            {
                failure = ex.Message;
                hasMore = false;
            }

            if (failure is not null)
            {
                yield return new AgentEventFrame("error", new { message = failure });
                break;
            }

            if (!hasMore)
                break;

            yield return frame!;
        }

        yield return new AgentEventFrame(
            AgentEventProtocol.Done, new { sessionId, elapsedMs = stopwatch.ElapsedMilliseconds });
    }

    /// <summary>宿主事件 → 帧；忽略未映射的类型。</summary>
    private static async IAsyncEnumerable<AgentEventFrame> AgentFramesAsync(
        IAgentHost host, AgentRunRequest request, [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var evt in host.RunStreamingAsync(request, ct)
                           .WithCancellation(ct)
                           .ConfigureAwait(false))
        {
            var frame = AgentEventProtocol.ToFrame(evt);
            if (frame is not null)
                yield return frame;
        }
    }

    /// <summary>心跳间隔：站点配置 <c>Api:SseHeartbeatSeconds</c>，默认 15 秒，≤0 关闭。</summary>
    private TimeSpan ResolveHeartbeat()
    {
        var seconds = HttpContext.RequestServices.GetService<IConfiguration>()
            ?.GetValue<int?>("Api:SseHeartbeatSeconds") ?? 15;
        return seconds <= 0 ? TimeSpan.Zero : TimeSpan.FromSeconds(seconds);
    }

    /// <summary>确认结果字符串 → <see cref="ConfirmResult"/>。</summary>
    private static bool TryParseConfirmResult(string? value, out ConfirmResult result)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "allow":
                result = ConfirmResult.Allow;
                return true;
            case "deny":
                result = ConfirmResult.Deny;
                return true;
            case "allowall":
                result = ConfirmResult.AllowAll;
                return true;
            case "allowsession":
                result = ConfirmResult.AllowSession;
                return true;
            default:
                result = ConfirmResult.Deny;
                return false;
        }
    }

    /// <summary>前置校验结果。</summary>
    private sealed record PreparedRequest(
        IAgentHost? Host, string? SessionId, bool Created, IActionResult? Error)
    {
        public static PreparedRequest Fail(IActionResult error) => new(null, null, false, error);
    }
}