/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Controllers.Default
*文件名： AgentController
*版本号： V1.0.0.0
*唯一标识：Agent 示例接口
*当前的用户域：WALLE
*创建人： yswenli
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
*****************************************************************************/

using LuBan.AIAgent.Hosting;
using LuBan.AIAgent.Sessions;

using System.Text;

using WebApplication1.Models.Entities;
using WebApplication1.Services.Agent;

namespace WebApplication1.Controllers.Default;

/// <summary>
/// Agent 对话示例接口（需登录）。
/// </summary>
/// <remarks>
/// 会话按登录用户隔离：同一用户复用同一会话（多轮对话保留历史），
/// 不同用户的会话互不可见。本接口不暴露会话标识，归属由服务端维护。
/// </remarks>
[Route("api/agent")]
public class AgentController : BaseApiController
{
    /// <summary>
    /// 一轮对话，返回模型输出的纯文本。
    /// </summary>
    [DisplayName("对话"), HttpPost("chat")]
    public async Task<IActionResult> ChatAsync([FromBody] AgentChatInput? input, CancellationToken cancellationToken)
    {
        // 全局模型校验过滤器被关闭，必须自己判空
        if (string.IsNullOrWhiteSpace(input?.Prompt))
            return StatusCode(StatusCodes.Status400BadRequest, ErrorResult("prompt 不能为空", 400));

        // [Authorize] 已拦截匿名请求，这里只做防御性校验
        var userId = SessionUser.UserId;
        if (userId <= 0)
            return StatusCode(StatusCodes.Status401Unauthorized, ErrorResult("未登录", 401));

        // 用 GetService 而不是构造注入：未启用时控制器仍能构造，返回 503 而非 500
        var host = HttpContext.RequestServices.GetService<IAgentHost>();
        if (host is null)
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                ErrorResult("LuBan Agent 未启用：请配置 db_config 的 sys-luban-framework:aiagent 或 appsettings 的 LuBanAgent 节", 503));

        var sessions = HttpContext.RequestServices.GetRequiredService<ISessionManager>();
        var registry = HttpContext.RequestServices.GetRequiredService<WebAgentSessionRegistry>();

        // 每用户固定一个会话；不调用 SetCurrentSessionAsync（单例实现里它是进程级指针，多用户会互相覆盖）
        var sessionId = await registry.GetOrCreateSessionIdAsync(userId, sessions, "web", cancellationToken);
        await host.InitializeAsync(sessionId, cancellationToken);

        var sb = new StringBuilder();
        await foreach (var evt in host.RunStreamingAsync(
            new AgentRunRequest { Input = input.Prompt, SessionId = sessionId }, cancellationToken))
        {
            if (evt is TextDeltaEvent delta) sb.Append(delta.Delta);
        }

        return Ok(SuccessResult(sb.ToString()));
    }
}