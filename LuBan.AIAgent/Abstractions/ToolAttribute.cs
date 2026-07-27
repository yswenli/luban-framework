namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 标记方法为 AI 工具
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class ToolAttribute : Attribute
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 创建 ToolAttribute 实例
    /// </summary>
    /// <param name="name">工具名称</param>
    public ToolAttribute(string name)
    {
        Name = name;
    }
}
