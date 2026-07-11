using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 内存工具注册表，用于在内存中注册和管理工具
/// </summary>
public class InMemoryToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, ITool> _tools = new();

    /// <summary>
    /// 注册单个工具
    /// </summary>
    /// <param name="tool">工具</param>
    public void Register(ITool tool)
    {
        _tools[tool.Name] = tool;
    }

    /// <summary>
    /// 注册多个工具
    /// </summary>
    /// <param name="tools">工具集合</param>
    public void Register(IEnumerable<ITool> tools)
    {
        foreach (var tool in tools)
        {
            Register(tool);
        }
    }

    /// <summary>
    /// 尝试获取工具
    /// </summary>
    /// <param name="name">工具名称</param>
    /// <param name="tool">输出参数，获取到的工具</param>
    /// <returns>是否成功获取工具</returns>
    public bool TryGetTool(string name, out ITool? tool)
    {
        return _tools.TryGetValue(name, out tool);
    }

    /// <summary>
    /// 获取所有工具
    /// </summary>
    /// <returns>所有工具的只读列表</returns>
    public IReadOnlyList<ITool> GetAllTools()
    {
        return _tools.Values.ToList();
    }

    /// <summary>
    /// 获取工具定义列表
    /// </summary>
    /// <returns>工具定义的只读列表</returns>
    public IReadOnlyList<ToolDefinition> GetToolDefinitions()
    {
        return _tools.Values.Select(t => new ToolDefinition
        {
            Name = t.Name,
            Description = t.Description,
            ParametersSchema = t.ParametersSchema,
            ApprovalMode = ToolApprovalMetadataResolver.ResolveApprovalMode(t)
        }).ToList();
    }
}
