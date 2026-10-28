/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.ConnectorManage
*文件名： ConnectorManageToolPlugin
*版本号： V1.0.0.0
*唯一标识：8df83af0-853c-42a8-9ec6-91a26979878d
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/20
*描述：连接器管理工具组插件（GroupName="connectormanage"）
*
*****************************************************************************/

namespace LuBan.AIAgent.Tools.ConnectorManage;

/// <summary>
/// 连接器管理工具组插件。
/// </summary>
public class ConnectorManageToolPlugin : ILuBanToolPlugin
{
    private readonly ConnectorManageToolOptions _options;
    private readonly IWorkspaceContext? _workspaceContext;

    /// <summary>
    /// 创建连接器管理工具组插件。
    /// </summary>
    /// <param name="options">Agent 配置。</param>
    /// <param name="workspaceContext">工作区上下文（可选）。</param>
    public ConnectorManageToolPlugin(IOptions<LuBanAgentOptions> options, IWorkspaceContext? workspaceContext = null)
    {
        _options = options.Value.Tools.ConnectorManage;
        _workspaceContext = workspaceContext;
    }

    /// <inheritdoc />
    public string GroupName => "connectormanage";

    /// <inheritdoc />
    public string? Description => "连接器管理工具，列出/创建/删除工作区级连接器（提供方须已注册，凭据一律走引用）";

    /// <inheritdoc />
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp, ToolGroupOptions? toolsOptions = null)
    {
        var registry = sp.GetRequiredService<ConnectorRegistry>();
        var factories = sp.GetRequiredService<ConnectorFactoryRegistry>();
        var writer = sp.GetRequiredService<IConnectorConfigWriter>();
        var reloadTrigger = sp.GetService<IAgentReloadTrigger>();

        var toolGroup = new ConnectorManageToolGroup(registry, factories, writer, reloadTrigger);

        Func<string?> workspaceRootProvider = () => _workspaceContext?.RootPath;

        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(toolGroup, nameof(ConnectorManageToolGroup.ListConnectors), workspaceRootProvider, fallbackToWorkspaceRootWhenMissing: false),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(ConnectorManageToolGroup.WriteConnectorAsync), workspaceRootProvider, fallbackToWorkspaceRootWhenMissing: false),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(ConnectorManageToolGroup.DeleteConnectorAsync), workspaceRootProvider, fallbackToWorkspaceRootWhenMissing: false),
        };
    }

    /// <inheritdoc />
    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.ConnectorManage.Enabled;
}