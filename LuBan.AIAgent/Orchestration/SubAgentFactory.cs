/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration
*文件名： SubAgentFactory
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：SubAgent 工厂，封装 LuBanAgentFactory 的子 Agent 创建逻辑
*
*=================================================
*修改标记
*修改时间：2026/8/7
*修改人： yswenli
*版本号： V1.0.0.0
*描述：支持 Role 映射、toolGroups 过滤、null 校验
*
*****************************************************************************/
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Orchestration.Models;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Orchestration;

/// <summary>
/// SubAgent 工厂，封装 <see cref="LuBanAgentFactory"/> 的子 Agent 创建逻辑。
/// 不依赖 ISessionManager：LuBanAgent 内部已有 AgentSession 管理，
/// ISessionManager 仅用于 SessionChatHistoryProvider 的持久化，
/// SubAgent 不启用 SessionHistory，因此无需 ISessionManager。
/// </summary>
public class SubAgentFactory : ISubAgentExecutor
{
    private readonly LuBanAgentFactory _innerFactory;
    private readonly SubAgentRoleRegistry _roleRegistry;
    private readonly IOptions<LuBanAgentOptions> _options;

    /// <summary>
    /// 创建 SubAgentFactory 实例。
    /// </summary>
    /// <param name="innerFactory">内部 LuBanAgent 工厂。</param>
    /// <param name="roleRegistry">角色注册表。</param>
    /// <param name="options">配置选项。</param>
    public SubAgentFactory(
        LuBanAgentFactory innerFactory,
        SubAgentRoleRegistry roleRegistry,
        IOptions<LuBanAgentOptions> options)
    {
        _innerFactory = innerFactory;
        _roleRegistry = roleRegistry;
        _options = options;
    }

    /// <summary>
    /// 根据 SubAgent 规格创建子 Agent。
    /// </summary>
    /// <param name="spec">SubAgent 创建规格。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>LuBanAgent 实例。</returns>
    public async Task<LuBanAgent> CreateAsync(SubAgentSpec spec, CancellationToken ct = default)
    {
        // Resolve tool groups: explicit > role default > configured fallback
        List<string>? resolvedToolGroups = spec.ToolGroups;
        string? systemPrompt = null;

        if (!string.IsNullOrEmpty(spec.Role))
        {
            var role = _roleRegistry.GetRole(spec.Role);
            if (role != null)
            {
                resolvedToolGroups = spec.ToolGroups ?? role.DefaultToolGroups;
                // 角色模板只描述角色职责，工作区上下文必须由 BuildSubAgentSystemPrompt 统一追加
                systemPrompt = $"你是任务图谱中的子执行单元，角色为「{role.Name}」，负责完成「{spec.NodeId}」节点的任务。\n{role.SystemPromptTemplate.Replace("{prompt}", spec.Prompt)}"
                    + BuildWorkspaceContext(spec)
                    + BuildSharedContext(spec);
            }
            else
            {
                Logger.Warn($"Role '{spec.Role}' not found, falling back to generic SubAgent");
            }
        }

        // 兜底：既无显式 ToolGroups、又无有效 Role 默认值时，使用配置的默认工具组，避免整图因单节点失败
        if (resolvedToolGroups == null)
        {
            var fallback = _options.Value.Orchestration?.DefaultToolGroups;
            resolvedToolGroups = fallback is { Count: > 0 } ? new List<string>(fallback) : new List<string>();
            Logger.Warn($"节点 '{spec.NodeId}' 未指定 ToolGroups 且 Role='{spec.Role ?? "null"}' 未解析到默认工具组，" +
                $"已回退到配置默认值 [{(resolvedToolGroups.Count == 0 ? "无工具" : string.Join(",", resolvedToolGroups))}]");
        }

        // Filter out orchestration to prevent recursion
        if (resolvedToolGroups != null)
        {
            resolvedToolGroups = resolvedToolGroups
                .Where(g => !string.Equals(g, "orchestration", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var agent = await _innerFactory.CreateSubAgentAsync(
            modelName: spec.ModelName,
            toolGroups: resolvedToolGroups,
            systemPrompt: systemPrompt ?? BuildSubAgentSystemPrompt(spec),
            cancellationToken: ct,
            timingTag: spec.NodeId);

        spec.SessionId = agent.Id;
        return agent;
    }

    /// <summary>
    /// 构建 SubAgent 系统提示词。
    /// </summary>
    /// <param name="spec">SubAgent 规格。</param>
    /// <returns>系统提示词字符串。</returns>
    private static string BuildSubAgentSystemPrompt(SubAgentSpec spec)
        => $"你是任务图谱中的子执行单元，负责完成「{spec.NodeId}」节点的任务。" +
           "请专注于当前任务，使用可用工具完成任务后给出简洁结果。"
           + BuildWorkspaceContext(spec)
           + BuildSharedContext(spec);

    /// <summary>
    /// 构建工作区路径上下文。子代理不会继承父 Agent 的系统提示词，
    /// 缺少该上下文时 LLM 会漏传 <c>rootPath</c>/<c>path</c> 等必填参数，
    /// 导致 AIFunctionFactory 参数绑定抛 ArgumentException（表现为工具调用失败）。
    /// </summary>
    /// <param name="spec">SubAgent 规格。</param>
    /// <returns>工作区路径上下文片段。</returns>
    private static string BuildWorkspaceContext(SubAgentSpec spec)
    {
        var workspaceRoot = string.IsNullOrWhiteSpace(spec.WorkspaceRoot)
            ? Environment.CurrentDirectory
            : spec.WorkspaceRoot;

        return $"\n\n当前工作区根目录: {workspaceRoot}" +
               "\n路径使用说明：" +
               $"\n- 调用文件系统工具时，rootPath 参数请使用工作区根目录的绝对路径 \"{workspaceRoot}\"，或使用 \".\"（已指向工作区根目录）" +
               "\n- 任何情况下都不要省略 rootPath/path 参数；不确定时传 \".\"" +
               $"\n- 示例: Grep(rootPath=\"{workspaceRoot}\", pattern=\"关键字\")" +
               "\n- 示例: ListDirectory(path=\".\")";
    }

    /// <summary>
    /// 构建跨节点共享上下文片段（长期记忆召回、规则注入等）。
    /// 子代理不继承父 Agent 的会话历史与规则注入，需由编排入口显式传入，
    /// 否则子代理会丢失工作区长期记忆，表现为"有记忆却用不上"。
    /// </summary>
    /// <param name="spec">SubAgent 规格。</param>
    /// <returns>共享上下文片段；无内容时返回空串。</returns>
    private static string BuildSharedContext(SubAgentSpec spec)
        => string.IsNullOrWhiteSpace(spec.SharedContext)
            ? string.Empty
            : "\n\n以下是当前工作区的长期记忆与规则上下文，回答时请优先参考：\n" + spec.SharedContext;

    /// <summary>
    /// 默认执行实现：直接创建子代理并在子代理确认作用域内流式执行，产出框架原生事件流。
    /// 确认作用域沿用默认语义（按本轮已允许集合代确认，未启用"全放行"），
    /// 未启用 A2A 时行为与改造前严格一致。
    /// </summary>
    /// <param name="spec">子代理规格。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>执行事件流。</returns>
    public async IAsyncEnumerable<SubAgentExecutionEvent> ExecuteAsync(
        SubAgentSpec spec, [EnumeratorCancellation] CancellationToken ct = default)
    {
        LuBanAgent? agent = null;
        Exception? createError = null;
        try
        {
            agent = await CreateAsync(spec, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            createError = ex;
        }

        if (createError is not null)
        {
            yield return new SubAgentExecutionEvent(SubAgentEventKind.Failed, spec.SessionId, createError.Message, null);
            yield break;
        }

        yield return new SubAgentExecutionEvent(SubAgentEventKind.Started, spec.SessionId, null, null);

        var output = new StringBuilder();
        // 子代理执行期进入确认作用域：工具确认不弹窗，由框架按本轮已允许集合代确认。
        using (ToolConfirmationContext.EnterSubAgentScope(allowAll: false))
        {
            await using var enumerator = agent!.RunStreamingAsync(spec.Prompt, ct).GetAsyncEnumerator(ct);
            while (true)
            {
                AgentResponseUpdate? update = null;
                Exception? streamError = null;
                try
                {
                    if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                    {
                        break;
                    }
                    update = enumerator.Current;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    streamError = ex;
                }

                if (streamError is not null)
                {
                    yield return new SubAgentExecutionEvent(SubAgentEventKind.Failed, spec.SessionId, streamError.Message, null);
                    yield break;
                }

                if (update!.Contents is null) continue;

                foreach (var content in update.Contents)
                {
                    switch (content)
                    {
                        case TextReasoningContent reasoning when !string.IsNullOrEmpty(reasoning.Text):
                            yield return new SubAgentExecutionEvent(SubAgentEventKind.Thinking, spec.SessionId, reasoning.Text, null);
                            break;

                        case FunctionCallContent functionCall:
                            yield return new SubAgentExecutionEvent(SubAgentEventKind.ToolCall, spec.SessionId, functionCall.Name, SummarizeArguments(functionCall.Arguments));
                            break;

                        case FunctionResultContent functionResult:
                            var resultText = functionResult.Exception is not null
                                ? $"❌ {functionResult.Exception.Message}"
                                : SummarizeResult(functionResult.Result);
                            yield return new SubAgentExecutionEvent(SubAgentEventKind.ToolResult, spec.SessionId, resultText, null);
                            break;

                        case TextContent text when !string.IsNullOrEmpty(text.Text):
                            output.Append(text.Text);
                            yield return new SubAgentExecutionEvent(SubAgentEventKind.Text, spec.SessionId, text.Text, null);
                            break;
                    }
                }
            }
        }

        yield return new SubAgentExecutionEvent(SubAgentEventKind.Completed, spec.SessionId, output.ToString(), null);
    }

    /// <summary>
    /// 把工具调用参数压成单行摘要（过长截断），避免参数 JSON 撑爆 UI 行宽。
    /// </summary>
    /// <param name="arguments">工具参数集合。</param>
    /// <returns>参数摘要文本；无参数时返回 null。</returns>
    private static string? SummarizeArguments(IDictionary<string, object?>? arguments)
    {
        if (arguments is null || arguments.Count == 0) return null;

        var sb = new StringBuilder();
        foreach (var kv in arguments)
        {
            if (sb.Length > 0) sb.Append(", ");
            sb.Append(kv.Key).Append('=').Append(FormatArgumentValue(kv.Value));
            if (sb.Length > 200)
            {
                sb.Length = 200;
                sb.Append('…');
                break;
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// 格式化单个参数值：字符串去换行，复杂对象转字符串。
    /// </summary>
    private static string FormatArgumentValue(object? value) => value switch
    {
        null => "null",
        string s => s.Replace("\r", " ").Replace("\n", " "),
        _ => value.ToString() ?? "null"
    };

    /// <summary>
    /// 把工具返回内容压成单行摘要（过长截断），避免大段文件内容灌进 UI。
    /// </summary>
    /// <param name="result">工具返回内容。</param>
    /// <returns>结果摘要文本；无内容时返回 null。</returns>
    private static string? SummarizeResult(object? result)
    {
        if (result is null) return null;

        var text = result switch
        {
            string s => s,
            _ => result.ToString()
        };
        if (string.IsNullOrWhiteSpace(text)) return null;

        text = text.Replace("\r", " ").Replace("\n", " ").Trim();
        return text.Length > 200 ? text[..200] + "…" : text;
    }
}
