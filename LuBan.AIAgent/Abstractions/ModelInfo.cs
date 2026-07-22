namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 模型信息，包含模型的基本元数据
/// </summary>
public record ModelInfo
{
    /// <summary>
    /// 模型 ID
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// 模型名称
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// 模型所有者
    /// </summary>
    public string? OwnedBy { get; init; }
}