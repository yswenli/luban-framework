namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 结果映射，用于将业务返回值映射到流程变量。
/// </summary>
public class ResultMapping
{
    /// <summary>
    /// 源字段名称。
    /// </summary>
    public string Source { get; set; } = string.Empty;
    /// <summary>
    /// 目标字段名称。
    /// </summary>
    public string Target { get; set; } = string.Empty;
}