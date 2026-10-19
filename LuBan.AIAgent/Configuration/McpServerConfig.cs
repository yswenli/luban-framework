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
    /// 传输方式：<c>stdio</c>（默认）/ <c>sse</c>（旧 HTTP+SSE）/ <c>http</c>（Streamable HTTP）。
    /// </summary>
    public string Transport { get; set; } = "stdio";

    /// <summary>
    /// 启动命令（<c>stdio</c>）。
    /// </summary>
    public string Command { get; set; } = "";

    /// <summary>
    /// 命令参数列表（<c>stdio</c>）。
    /// </summary>
    public List<string> Args { get; set; } = new();

    /// <summary>
    /// HTTP/SSE 端点（<c>http</c> / <c>sse</c>）。
    /// </summary>
    /// <remarks>
    /// 优先于历史写法「把 URL 塞进 <c>Args[0]</c> 或 <c>Command</c>」；两者都为空时视为未配置。
    /// </remarks>
    public string Url { get; set; } = "";

    /// <summary>
    /// HTTP 请求头（<c>http</c> / <c>sse</c>）。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 值支持<b>凭据引用</b>语法 <c>$credential:&lt;凭据键&gt;</c>，连接时经
    /// <c>IMcpConfigValueResolver</c> 从 <c>ICredentialStore</c> 解析。
    /// </para>
    /// <para>
    /// <b>严禁</b>在此写明文令牌（如 <c>Authorization: Bearer xxx</c>）——<c>McpManage</c> 写入工具会直接拒绝，
    /// 请一律使用凭据引用。
    /// </para>
    /// </remarks>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// stdio 子进程环境变量。
    /// </summary>
    /// <remarks>
    /// 值同样支持 <c>$credential:&lt;凭据键&gt;</c> 引用（如 <c>MY_ACCESS_TOKEN</c> 指向
    /// <c>connector:myconnector:token</c>），<b>严禁</b>写明文令牌。
    /// </remarks>
    public Dictionary<string, string> Environment { get; set; } = new();

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;
}
