/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Tools.Orchestration
*文件名： OrchestrationToolGroup
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：编排工具组，将 orchestrate 能力作为工具方法暴露
*
*****************************************************************************/
using LuBan.AIAgent.Orchestration;
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.AIAgent.Tools.Orchestration;

/// <summary>
/// 编排工具组，将 orchestrate 能力作为工具方法暴露给主 Agent。
/// </summary>
public class OrchestrationToolGroup
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 创建 OrchestrationToolGroup 实例。
    /// </summary>
    /// <param name="serviceProvider">服务提供者。</param>
    public OrchestrationToolGroup(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 将复合任务拆解为 DAG 任务图谱，并调度多个子 Agent 串行/并行执行。
    /// 适用于：多步骤任务、需要不同工具组合的任务、可并行的独立子任务。
    /// </summary>
    /// <param name="task">复合任务描述。</param>
    /// <returns>编排结果字符串。</returns>
    [Description("将复合任务拆解为 DAG 任务图谱，并调度多个子 Agent 串行/并行执行。适用于：多步骤任务、需要不同工具组合的任务、可并行的独立子任务。")]
    public async Task<string> OrchestrateAsync(string task)
    {
        try
        {
            var orchestrator = _serviceProvider.GetRequiredService<IOrchestrator>();
            var result = await orchestrator.RunAsync(task);
            return FormatResult(result);
        }
        catch (Exception ex)
        {
            Logger.Error("编排执行失败", ex);
            return $"编排失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 格式化编排结果为字符串，供主 Agent 读取。
    /// </summary>
    /// <param name="result">编排结果。</param>
    /// <returns>格式化后的字符串。</returns>
    private static string FormatResult(OrchestrationResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"编排状态: {result.OverallStatus}");
        sb.AppendLine($"节点执行: {result.Nodes.Count(n => n.Status == TaskNodeStatus.Succeeded)}/{result.Nodes.Count} 成功");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(result.FinalOutput))
        {
            sb.AppendLine("最终结果:");
            sb.AppendLine(result.FinalOutput);
        }

        var failed = result.Nodes.Where(n => n.Status == TaskNodeStatus.Failed).ToList();
        if (failed.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("失败节点:");
            foreach (var f in failed)
                sb.AppendLine($"  - {f.NodeId}: {f.Error}");
        }

        return sb.ToString();
    }
}
