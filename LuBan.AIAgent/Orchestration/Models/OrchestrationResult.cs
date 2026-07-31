/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Models
*文件名： OrchestrationResult
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：编排总结果，返回给主 Agent 或 CLI
*
*****************************************************************************/
namespace LuBan.AIAgent.Orchestration.Models;

/// <summary>
/// 编排总结果，返回给主 Agent 或 CLI。
/// </summary>
public class OrchestrationResult
{
    /// <summary>
    /// 获取或设置图谱标识。
    /// </summary>
    public string GraphId { get; set; } = "";

    /// <summary>
    /// 获取或设置原始任务描述。
    /// </summary>
    public string OriginalTask { get; set; } = "";

    /// <summary>
    /// 获取或设置总体状态（completed / partial / failed）。
    /// </summary>
    public string OverallStatus { get; set; } = "";

    /// <summary>
    /// 获取或设置所有节点结果。
    /// </summary>
    public List<NodeResult> Nodes { get; set; } = new();

    /// <summary>
    /// 获取或设置最终综合输出（由终点节点输出聚合而成）。
    /// </summary>
    public string? FinalOutput { get; set; }

    /// <summary>
    /// 获取或设置编排总耗时。
    /// </summary>
    public TimeSpan TotalElapsed { get; set; }

    /// <summary>
    /// 获取或设置总 Token 消耗。当前未实现，预留字段。
    /// </summary>
    public long TotalTokens { get; set; }

    /// <summary>
    /// 获取是否执行成功。
    /// </summary>
    public bool IsSuccess => OverallStatus == "completed";
}
