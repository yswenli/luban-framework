namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 可标识组件接口
/// </summary>
public interface IIdentifiable
{
    /// <summary>
    /// 组件唯一标识
    /// </summary>
    string Id { get; }
}
