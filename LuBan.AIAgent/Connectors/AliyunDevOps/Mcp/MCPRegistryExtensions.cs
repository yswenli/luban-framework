/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Connectors.AliyunDevOps.Mcp
*文件名： MCPRegistryExtensions
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：MCPRegistry 扩展方法（进程内插件客户端注册/注销）
*
*****************************************************************************/

namespace LuBan.AIAgent.Connectors.AliyunDevOps.Mcp;

/// <summary>
/// <see cref="MCPRegistry"/> 的插件客户端注册/注销扩展方法。
/// </summary>
/// <remarks>
/// 实际逻辑位于 <see cref="MCPRegistry.RegisterPluginClient"/> /
/// <see cref="MCPRegistry.UnregisterPluginClient"/>，本类仅提供语义更明确的调用别名，
/// 便于插件与连接器代码表达「这是进程内插件客户端」的意图。
/// </remarks>
public static class MCPRegistryExtensions
{
    /// <summary>
    /// 注册插件贡献的进程内 MCP 客户端（语义别名）。
    /// </summary>
    /// <param name="registry">MCP 注册表</param>
    /// <param name="client">进程内 MCP 客户端实例</param>
    /// <returns>是否注册成功（同名已存在时覆盖并返回 false）</returns>
    public static bool RegisterPluginClient(this MCPRegistry registry, IMCPClient client)
    {
        ArgumentNullException.ThrowIfNull(registry);
        return registry.RegisterPluginClient(client);
    }

    /// <summary>
    /// 注销插件贡献的进程内 MCP 客户端（语义别名）。
    /// </summary>
    /// <param name="registry">MCP 注册表</param>
    /// <param name="name">客户端名称</param>
    /// <returns>是否成功注销</returns>
    public static bool UnregisterPluginClient(this MCPRegistry registry, string name)
    {
        ArgumentNullException.ThrowIfNull(registry);
        return registry.UnregisterPluginClient(name);
    }
}