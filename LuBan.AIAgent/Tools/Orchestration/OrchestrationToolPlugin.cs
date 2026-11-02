/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Tools.Orchestration
*文件名： OrchestrationToolPlugin
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：编排工具插件，将 orchestrate 能力暴露给主 Agent 自动调用
*
*****************************************************************************/

namespace LuBan.AIAgent.Tools.Orchestration;

/// <summary>
/// 编排工具插件，将 orchestrate 能力暴露给主 Agent 自动调用。
/// </summary>
public class OrchestrationToolPlugin : ILuBanToolPlugin
{
    /// <inheritdoc/>
    public string GroupName => "orchestration";

    /// <inheritdoc/>
    public string? Description => "复合任务编排：拆解 DAG 并调度 SubAgent 执行";

    /// <inheritdoc/>
    public bool IsEnabled(LuBanAgentOptions options)
        => options.Orchestration?.Enabled ?? false;

    /// <inheritdoc/>
    public IReadOnlyList<AIFunction> GetTools(DiContainer sp, ToolGroupOptions? toolsOptions = null)
    {
        var toolGroup = new OrchestrationToolGroup(sp);
        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(toolGroup, nameof(OrchestrationToolGroup.PlanTaskAsync), name: OrchestrationToolGroup.PlanTaskToolName),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(OrchestrationToolGroup.RunOrchestrationAsync), name: OrchestrationToolGroup.RunOrchestrationToolName)
        };
    }
}
