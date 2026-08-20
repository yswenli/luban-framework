/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： McpServerConfig
*版本号： V1.0.0.0
*唯一标识：978eac7f-0d34-492d-a728-98bdec2767a1
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：MCP 服务器配置
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：MCP 服务器配置
*
*****************************************************************************/
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
