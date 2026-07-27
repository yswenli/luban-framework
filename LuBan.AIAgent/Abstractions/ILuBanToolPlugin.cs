namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// LuBan 工具插件接口，用于定义工具分组和提供工具函数
/// </summary>
public interface ILuBanToolPlugin
{
    /// <summary>
    /// 工具分组名称，如 "browser"、"filesystem"、"script" 等
    /// </summary>
    string GroupName { get; }

    /// <summary>
    /// 工具分组描述，供 Agent 理解用途
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// 获取该分组下的所有工具函数
    /// </summary>
    /// <param name="sp">服务提供者，用于解析工具依赖</param>
    /// <returns>AIFunction 工具函数列表</returns>
    IReadOnlyList<AIFunction> GetTools(IServiceProvider sp);

    /// <summary>
    /// 根据配置判断该插件是否启用
    /// </summary>
    /// <param name="options">LuBan Agent 配置选项</param>
    /// <returns>是否启用</returns>
    bool IsEnabled(LuBanAgentOptions options);
}
