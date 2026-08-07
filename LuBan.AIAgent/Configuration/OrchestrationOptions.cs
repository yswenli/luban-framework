/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Configuration
*文件名： OrchestrationOptions
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：编排子系统配置
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 编排子系统配置。
/// </summary>
public class OrchestrationOptions
{
    /// <summary>
    /// 获取或设置是否启用编排功能。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 获取或设置规划器类型（llm / template / composite），默认为 composite。
    /// </summary>
    public string PlannerType { get; set; } = "composite";

    /// <summary>
    /// 获取或设置规划器使用的模型。null 表示继承主模型。
    /// </summary>
    public string? PlannerModel { get; set; }

    /// <summary>
    /// 获取或设置 SubAgent 默认超时时间（秒）。
    /// </summary>
    public int DefaultNodeTimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// 获取或设置同层最大并行度。0 表示不限制。
    /// </summary>
    public int MaxParallelism { get; set; } = 4;

    /// <summary>
    /// 获取或设置单图谱最大节点数，防止 LLM 拆出过多节点。
    /// </summary>
    public int MaxNodes { get; set; } = 10;

    /// <summary>
    /// 获取或设置模板目录路径（相对工作目录）。
    /// </summary>
    public string TemplatesDirectory { get; set; } = "Templates";

    /// <summary>
    /// 获取或设置是否自动暴露为工具供主 Agent 自动调用。
    /// </summary>
    public bool ExposeAsTool { get; set; } = true;

    /// <summary>
    /// 获取或设置是否启用自动判定（每轮输入由 planner 判定是否为复合任务）。
    /// </summary>
    public bool AutoDetect { get; set; } = true;

    /// <summary>
    /// 获取或设置关键节点失败后的最大重规划尝试次数。0 表示禁用重规划。
    /// </summary>
    public int MaxReplanAttempts { get; set; } = 3;

    /// <summary>
    /// 获取或设置反思阶段 LLM 调用的超时时间（秒）。
    /// </summary>
    public int ReflectionTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// 获取或设置启发式预过滤配置。
    /// </summary>
    public HeuristicFilterOptions HeuristicFilter { get; set; } = new();
}
