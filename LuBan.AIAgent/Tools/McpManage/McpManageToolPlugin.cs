/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.McpManage
*文件名： McpManageToolPlugin
*版本号： V1.0.0.0
*唯一标识：820612b0-fe81-4048-bae0-f6abe4a0d9c4
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/14
*描述：MCP 管理工具组插件（GroupName="mcpmanage"）
*
*****************************************************************************/

namespace LuBan.AIAgent.Tools.McpManage;

/// <summary>
/// MCP 管理工具组插件。
/// </summary>
public class McpManageToolPlugin : ILuBanToolPlugin
{
    private readonly McpManageToolOptions _options;
    private readonly McpOptions _mcpOptions;
    private readonly IWorkspaceContext? _workspaceContext;
    private readonly string? _workspaceRootFallback;
    private readonly bool _isolateWorkspaceByUser;

    /// <summary>
    /// 从全局选项读取 MCP 管理配置，并保留可选的工作区上下文。
    /// </summary>
    /// <param name="options">宿主全局选项，取其中的 MCP 相关配置节。</param>
    /// <param name="workspaceContext">工作区上下文，用于提供默认根路径。</param>
    public McpManageToolPlugin(IOptions<LuBanAgentOptions> options, IWorkspaceContext? workspaceContext = null)
    {
        _options = options.Value.Tools.McpManage;
        _mcpOptions = options.Value.Mcp;
        _workspaceContext = workspaceContext;
        // 工作区上下文缺失时回退全局配置（与 FileSystemToolPlugin 同口径）
        _workspaceRootFallback = options.Value.WorkspaceRoot;
        // 按用户隔离时，绝对路径同样不得越出用户工作区
        _isolateWorkspaceByUser = options.Value.IsolateWorkspaceByUser;
    }

    /// <summary>
    /// 工具组名称，固定为 <c>mcpmanage</c>。
    /// </summary>
    public string GroupName => "mcpmanage";
    /// <summary>
    /// 工具组描述，说明用于创建、删除、测试工作区级 MCP 服务器。
    /// </summary>
    public string? Description => "MCP 管理工具，创建/删除/测试工作区级 MCP 服务器";

    /// <summary>
    /// 构建写入、删除、测试三个 MCP 管理工具函数。
    /// </summary>
    /// <param name="sp">用于解析注册表与重载触发器的服务提供者。</param>
    /// <param name="toolsOptions">工具组级选项，当前未使用。</param>
    /// <returns>该工具组暴露的 AIFunction 列表。</returns>
    public IReadOnlyList<AIFunction> GetTools(DiContainer sp, ToolGroupOptions? toolsOptions = null)
    {
        var registry = sp.GetRequiredService<MCPRegistry>();
        var reloadTrigger = sp.GetService<IAgentReloadTrigger>();
        var resolver = sp.GetService<IMcpConfigValueResolver>();
        // 写/删必须经确认服务（Default/AcceptEdits 弹确认、Plan 记计划、Bypass 放行）；测试连接不确认
        var confirmationService = sp.GetRequiredService<IToolConfirmationService>();

        // 工作区根：会话上下文优先，缺失回退全局配置；作为工具省略 workspaceRoot 时的兜底
        Func<string?> workspaceRootProvider = () => _workspaceContext?.RootPath ?? _workspaceRootFallback;

        var toolGroup = new McpManageToolGroup(
            registry, _options, _mcpOptions, reloadTrigger, resolver, confirmationService, workspaceRootProvider,
            _isolateWorkspaceByUser);

        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(toolGroup, nameof(McpManageToolGroup.WriteMcpServerAsync), workspaceRootProvider, fallbackToWorkspaceRootWhenMissing: false),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(McpManageToolGroup.DeleteMcpServerAsync), workspaceRootProvider, fallbackToWorkspaceRootWhenMissing: false),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(McpManageToolGroup.TestMcpServerAsync), workspaceRootProvider, fallbackToWorkspaceRootWhenMissing: false),
        };
    }

    /// <summary>
    /// 按配置判断 MCP 管理工具组是否启用。
    /// </summary>
    /// <param name="options">宿主全局选项。</param>
    /// <returns>启用返回 true。</returns>
    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.McpManage.Enabled;
}