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
        var sessionManager = sp.GetRequiredService<ISessionManager>();
        var group = new CompactContextToolGroup(_scopeFactory, sessionManager, _options.Value);
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

    /// <summary>
    /// 创建工具组实例
    /// </summary>
    public CompactContextToolGroup(
        IServiceScopeFactory scopeFactory,
        ISessionManager sessionManager,
        LuBanAgentOptions options)
    {
        _scopeFactory = scopeFactory;
        _sessionManager = sessionManager;
        _options = options;
    }

    /// <summary>
    /// 压缩当前会话的对话上下文，将较旧的消息摘要化以释放 token 预算
    /// </summary>
    /// <param name="targetCount">压缩后保留的消息数，默认使用配置值</param>
    [Description("压缩当前会话的对话上下文，将较旧的消息摘要化以释放 token 预算")]
    public async Task<ToolResult<string>> CompactContextAsync(
        [Description("压缩后保留的消息数，默认使用配置值")] int? targetCount = null)
    {
        var sessionId = _sessionManager.CurrentSession?.SessionId;
        if (string.IsNullOrEmpty(sessionId))
            return ToolResult.Fail<string>("无当前会话，无需压缩");

        var target = targetCount ?? _options.Session.CompactTargetMessages;
        var threshold = _options.Session.CompactThreshold;

        var active = (await _sessionManager.GetActiveMessagesAsync(sessionId)).ToList();

        var summaries = active.Where(m => m.Role == "summary").OrderByDescending(m => m.Id).ToList();
        var messages = active.Where(m => m.Role != "summary").ToList();
        var oldSummary = summaries.FirstOrDefault();

        if (messages.Count <= target + threshold)
            return ToolResult.Ok($"低于压缩阈值，无需压缩。当前{target}+{threshold}={target + threshold}，实际 {messages.Count} 条",
                $"压缩检查: 活跃 {messages.Count} 条消息，未达阈值");

        using var scope = _scopeFactory.CreateScope();
        var chatClient = scope.ServiceProvider.GetRequiredService<IChatClient>();

        var history = messages
            .Select(m => new ChatMessage(m.Role == "user" ? ChatRole.User : ChatRole.Assistant, m.Content))
            .ToList();

#pragma warning disable MEAI001
        var reducer = new SummarizingChatReducer(chatClient, target, threshold);
#pragma warning restore MEAI001
        var reduced = (await reducer.ReduceAsync(history, CancellationToken.None)).ToList();

        if (reduced.Count == 0 || reduced.Count >= history.Count)
            return ToolResult.Fail<string>("压缩后消息数未减少，摘要可能失败");

        var keptCount = Math.Min(reduced.Count - 1, history.Count);
        var keptTail = messages.Skip(messages.Count - keptCount).ToList();
        var compactedIds = messages.Take(messages.Count - keptCount).Select(m => m.Id)
            .Concat(summaries.Select(s => s.Id))
            .ToList();
        await _sessionManager.MarkMessagesCompactedAsync(sessionId, compactedIds);

        var summaryText = reduced[0].Text ?? "";
        var summaryTokens = Math.Max(1, summaryText.Length / 4);
        await _sessionManager.AddMessageAsync(sessionId, "summary", summaryText, summaryTokens);

        var result = new
        {
            oldSummary = oldSummary?.Content ?? "(无旧摘要)",
            newSummary = summaryText,
            compactedCount = compactedIds.Count,
            remainingCount = keptTail.Count
        };
        var json = JsonSerializer.Serialize(result);
        return ToolResult.Ok(json, $"压缩完成: 归档 {compactedIds.Count} 条消息, 保留 {keptTail.Count} 条活跃");
    }
}