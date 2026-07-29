namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 外部 MCP 服务器注册项
/// </summary>
public class McpServerConfig
{
    /// <summary>
    /// 服务器名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 服务器描述
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 传输方式（如 stdio）
    /// </summary>
    public string Transport { get; set; } = "stdio";

    /// <summary>
    /// 启动命令
    /// </summary>
    public string Command { get; set; } = "";

    /// <summary>
    /// 命令参数列表
    /// </summary>
    public List<string> Args { get; set; } = new();

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;
}
