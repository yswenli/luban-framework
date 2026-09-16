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
public class SubAgentFactory
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
                    + BuildWorkspaceContext(spec);
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
           + BuildWorkspaceContext(spec);

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
}
