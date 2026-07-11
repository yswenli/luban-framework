namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 工具注册表接口，用于注册和管理工具
/// </summary>
public interface IToolRegistry
{
    /// <summary>
    /// 注册工具
    /// </summary>
    /// <param name="tool">工具实例</param>
    void Register(ITool tool);
    
    /// <summary>
    /// 注册多个工具
    /// </summary>
    /// <param name="tools">工具实例集合</param>
    void Register(IEnumerable<ITool> tools);
    
    /// <summary>
    /// 尝试根据名称获取工具
    /// </summary>
    /// <param name="name">工具名称</param>
    /// <param name="tool">工具实例，如果不存在则返回null</param>
    /// <returns>是否找到工具</returns>
    bool TryGetTool(string name, out ITool? tool);
    
    /// <summary>
    /// 获取所有工具
    /// </summary>
    /// <returns>工具列表</returns>
    IReadOnlyList<ITool> GetAllTools();
    
    /// <summary>
    /// 获取所有工具定义
    /// </summary>
    /// <returns>工具定义列表</returns>
    IReadOnlyList<ToolDefinition> GetToolDefinitions();
}
