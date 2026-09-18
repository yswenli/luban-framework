/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.Context
*文件名： CompactContextToolPlugin
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/14
*描述：上下文压缩工具插件，允许 Agent 主动调用压缩对话上下文释放 token 预算
*
*=================================================
*修改标记
*修改时间：2026/9/14
*修改人： yswenli
*版本号： V1.0.0.0
*描述：上下文压缩工具插件
*修改时间：2026/9/14
*描述：修复无会话宿主下建 Agent 抛异常（GetService 回落）；接入权限门控（Plan 模式只记录不执行）；
*      targetCount 入参钳制；摘要先写后归档；工具结果改为只回传统计信息；支持取消令牌。
*
*****************************************************************************/
using System.Text.Json;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Sessions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tools.Context;

/// <summary>
/// 上下文压缩工具插件，允许 Agent 主动调用压缩对话上下文
/// </summary>
public class CompactContextToolPlugin : ILuBanToolPlugin
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<LuBanAgentOptions> _options;

    /// <summary>
    /// 创建 CompactContextToolPlugin 实例
    /// </summary>
    public CompactContextToolPlugin(IServiceScopeFactory scopeFactory, IOptions<LuBanAgentOptions> options)
    {
        _scopeFactory = scopeFactory;
        _options = options;
    }

    /// <inheritdoc/>
    public string GroupName => "context";

    /// <inheritdoc/>
    public string? Description => "上下文压缩工具：压缩当前会话对话历史的 token 占用";

    /// <inheritdoc/>
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp, ToolGroupOptions? toolsOptions = null)
    {
        // 会话能力由宿主提供：只注册 AddLuBanAgent 而未注册 ISessionManager 的容器
        // （如 CLI 的 CommandBase.BuildServiceProvider）不应因本工具而建 Agent 失败，
        // 故用 GetService 探测，缺失时不出该工具（与 RetrievalToolPlugin 同策略）。
        var sessionManager = sp.GetService<ISessionManager>();
        if (sessionManager is null) return Array.Empty<AIFunction>();

        var confirmationService = sp.GetRequiredService<IToolConfirmationService>();
        var group = new CompactContextToolGroup(_scopeFactory, sessionManager, _options.Value, confirmationService);
        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(group, nameof(CompactContextToolGroup.CompactContextAsync))
        };
    }

    /// <inheritdoc/>
    public bool IsEnabled(LuBanAgentOptions options) => true;
}

/// <summary>
/// 上下文压缩工具组，封装单方法供 LLM 调用
/// </summary>
public class CompactContextToolGroup
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISessionManager _sessionManager;
    private readonly LuBanAgentOptions _options;
    private readonly IToolConfirmationService _confirmationService;

    /// <summary>
    /// 创建工具组实例
    /// </summary>
    public CompactContextToolGroup(
        IServiceScopeFactory scopeFactory,
        ISessionManager sessionManager,
        LuBanAgentOptions options,
        IToolConfirmationService confirmationService)
    {
        _scopeFactory = scopeFactory;
        _sessionManager = sessionManager;
        _options = options;
        _confirmationService = confirmationService;
    }

    /// <summary>
    /// 压缩当前会话的对话上下文，将较旧的消息摘要化以释放 token 预算
    /// </summary>
    /// <param name="targetCount">压缩后保留的消息数，默认使用配置值</param>
    /// <param name="cancellationToken">取消令牌（由框架注入，支持 ESC 中断摘要调用）</param>
    [Description("压缩当前会话的对话上下文，将较旧的消息摘要化以释放 token 预算")]
    public async Task<ToolResult<string>> CompactContextAsync(
        [Description("压缩后保留的消息数，默认使用配置值")] int? targetCount = null,
        CancellationToken cancellationToken = default)
    {
        // 权限模式门控：本工具会写库（归档消息 + 写入摘要），不属于只读操作。
        // Plan 模式在此记录为计划项而不执行；被用户拒绝时返回取消。
        var outcome = await _confirmationService.EvaluateAsync(
            nameof(CompactContextAsync), null,
            new Dictionary<string, object?> { ["targetCount"] = targetCount }).ConfigureAwait(false);
        if (outcome == EnumConfirmationOutcome.Planned)
            return ToolResult.Plan<string>();
        if (outcome != EnumConfirmationOutcome.Allowed)
            return ToolResult.Denied<string>();

        var sessionId = _sessionManager.CurrentSession?.SessionId;
        if (string.IsNullOrEmpty(sessionId))
            return ToolResult.Fail<string>("无当前会话，无需压缩");

        var active = (await _sessionManager.GetActiveMessagesAsync(sessionId)).ToList();

        var summaries = active.Where(m => m.Role == "summary").OrderByDescending(m => m.Id).ToList();
        var messages = active.Where(m => m.Role != "summary").ToList();
        var oldSummary = summaries.FirstOrDefault();

        // 入参防护：targetCount 由 LLM 提供，可能为 0 / 负数 / 极大值，
        // 一律钳制到 [1, 现有消息数]，避免一次调用把全部历史归档掉。
        var target = Math.Clamp(targetCount ?? _options.Session.CompactTargetMessages, 1, Math.Max(1, messages.Count));
        var threshold = Math.Max(0, _options.Session.CompactThreshold);

        if (messages.Count <= target + threshold)
            return ToolResult.Ok(
                $"低于压缩阈值，无需压缩。当前 target+threshold={target}+{threshold}={target + threshold}，实际 {messages.Count} 条",
                $"压缩检查: 活跃 {messages.Count} 条消息，未达阈值");

        using var scope = _scopeFactory.CreateScope();
        var chatClient = scope.ServiceProvider.GetRequiredService<IChatClient>();

        var history = messages
            .Select(m => new ChatMessage(MapRole(m.Role), m.Content))
            .ToList();

#pragma warning disable MEAI001
        var reducer = new SummarizingChatReducer(chatClient, target, threshold);
#pragma warning restore MEAI001
        var reduced = (await reducer.ReduceAsync(history, cancellationToken).ConfigureAwait(false)).ToList();

        if (reduced.Count == 0 || reduced.Count >= history.Count)
            return ToolResult.Fail<string>("压缩后消息数未减少，摘要可能失败");

        var keptCount = Math.Min(reduced.Count - 1, history.Count);
        var keptTail = messages.Skip(messages.Count - keptCount).ToList();
        var archivedMessages = messages.Take(messages.Count - keptCount).ToList();
        var compactedIds = archivedMessages.Select(m => m.Id)
            .Concat(summaries.Select(s => s.Id))
            .ToList();

        // 先写摘要、后归档：反序执行时若归档成功而写摘要失败，历史会被永久裁掉且无摘要兜底。
        var summaryText = reduced[0].Text ?? "";
        await _sessionManager.AddMessageAsync(sessionId, "summary", summaryText, Math.Max(1, summaryText.Length / 4)).ConfigureAwait(false);
        await _sessionManager.MarkMessagesCompactedAsync(sessionId, compactedIds).ConfigureAwait(false);

        // 只回传统计信息：把新旧摘要全文塞进工具结果会让上下文不减反增
        // （新摘要下一轮会由 SessionChatHistoryProvider 自动注入，重复注入无意义）。
        var result = new
        {
            oldSummaryChars = oldSummary?.Content.Length ?? 0,
            newSummaryChars = summaryText.Length,
            archivedMessages = archivedMessages.Count,
            archivedSummaries = summaries.Count,
            remainingMessages = keptTail.Count
        };
        var json = SerializeUtil.Serialize(result);
        return ToolResult.Ok(json, $"压缩完成: 归档 {archivedMessages.Count} 条消息 + {summaries.Count} 条旧摘要, 保留 {keptTail.Count} 条活跃");
    }

    private static ChatRole MapRole(string role) => role switch
    {
        "user" => ChatRole.User,
        "assistant" => ChatRole.Assistant,
        "system" => ChatRole.System,
        _ => ChatRole.Assistant
    };
}
