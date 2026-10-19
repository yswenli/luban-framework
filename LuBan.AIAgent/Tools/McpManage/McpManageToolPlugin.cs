/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Tools.McpManage
*文件名： McpManageToolPlugin
*描述：MCP 管理工具组插件（GroupName="mcpmanage"）
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.MCP;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tools.McpManage;

/// <summary>
/// MCP 管理工具组插件。
/// </summary>
public class McpManageToolPlugin : ILuBanToolPlugin
{
    private readonly McpManageToolOptions _options;
    private readonly McpOptions _mcpOptions;
    private readonly IWorkspaceContext? _workspaceContext;

    public McpManageToolPlugin(IOptions<LuBanAgentOptions> options, IWorkspaceContext? workspaceContext = null)
    {
        _options = options.Value.Tools.McpManage;
        _mcpOptions = options.Value.Mcp;
        _workspaceContext = workspaceContext;
    }

    public string GroupName => "mcpmanage";
    public string? Description => "MCP 管理工具，创建/删除/测试工作区级 MCP 服务器";

    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp, ToolGroupOptions? toolsOptions = null)
    {
        var registry = sp.GetRequiredService<MCPRegistry>();
        var reloadTrigger = sp.GetService<IAgentReloadTrigger>();
        var resolver = sp.GetService<IMcpConfigValueResolver>();

        var toolGroup = new McpManageToolGroup(registry, _options, _mcpOptions, reloadTrigger, resolver);

        Func<string?> workspaceRootProvider = () => _workspaceContext?.RootPath;

        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(toolGroup, nameof(McpManageToolGroup.WriteMcpServerAsync), workspaceRootProvider, fallbackToWorkspaceRootWhenMissing: false),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(McpManageToolGroup.DeleteMcpServerAsync), workspaceRootProvider, fallbackToWorkspaceRootWhenMissing: false),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(McpManageToolGroup.TestMcpServerAsync), workspaceRootProvider, fallbackToWorkspaceRootWhenMissing: false),
        };
    }

    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.McpManage.Enabled;
}