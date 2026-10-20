/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Tools.ConnectorManage
*文件名： ConnectorManageToolGroup
*描述：连接器管理工具实现（WriteConnectorAsync / DeleteConnectorAsync / ListConnectorsAsync）
*
*****************************************************************************/
using System.Text.Json;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Connectors;

namespace LuBan.AIAgent.Tools.ConnectorManage;

/// <summary>
/// 连接器管理工具实现（厂商无关）。
/// </summary>
/// <remarks>
/// <para>
/// 「对话生成连接器」的框架侧入口（PRD §4.12 C 路线）：模型只能选择<b>已注册</b>的提供方
/// （<see cref="ConnectorFactoryRegistry"/>），并写入<b>非敏感</b>参数；疑似承载令牌的参数必须是
/// 凭据引用（<c>$credential:&lt;凭据键&gt;</c>），否则被拒绝（<see cref="ConnectorSettingsSecurityValidator"/>）。
/// </para>
/// <para>
/// 落盘由 <see cref="IConnectorConfigWriter"/> 负责（默认写工作区
/// <c>.luban-agent/connectors/&lt;name&gt;.json</c>），本类只做校验 + 触发重载。
/// </para>
/// </remarks>
public class ConnectorManageToolGroup
{
    private readonly ConnectorRegistry _registry;
    private readonly ConnectorFactoryRegistry _factories;
    private readonly IConnectorConfigWriter _writer;
    private readonly IAgentReloadTrigger? _reloadTrigger;

    /// <summary>
    /// 创建连接器管理工具实现。
    /// </summary>
    /// <param name="registry">会话连接器注册表。</param>
    /// <param name="factories">连接器工厂表（提供方可用性来源）。</param>
    /// <param name="writer">连接器配置写入器。</param>
    /// <param name="reloadTrigger">重载触发器（可选）。</param>
    public ConnectorManageToolGroup(
        ConnectorRegistry registry,
        ConnectorFactoryRegistry factories,
        IConnectorConfigWriter writer,
        IAgentReloadTrigger? reloadTrigger = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _factories = factories ?? throw new ArgumentNullException(nameof(factories));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _reloadTrigger = reloadTrigger;
    }

    /// <summary>
    /// 列出全部连接器状态（已声明 / 已启用 / 提供方可用性）。
    /// </summary>
    /// <param name="workspaceRoot">工作区根目录。</param>
    /// <returns>状态列表的 JSON。</returns>
    public ToolResult<string> ListConnectors(string workspaceRoot)
    {
        var states = _registry.GetStates(workspaceRoot);
        return ToolResult.Ok(JsonSerializer.Serialize(states, new JsonSerializerOptions { WriteIndented = true }));
    }

    /// <summary>
    /// 创建或覆盖一个连接器声明（对话生成连接器的落点）。
    /// </summary>
    /// <remarks>
    /// <paramref name="provider"/> 必须是<b>已注册</b>的提供方键（宿主 / 产品 / 插件登记）；
    /// <paramref name="settings"/> 只承载非敏感参数，疑似令牌必须写 <c>$credential:&lt;凭据键&gt;</c>。
    /// </remarks>
    /// <param name="name">连接器实例名。</param>
    /// <param name="provider">提供方键。</param>
    /// <param name="settings">非敏感参数。</param>
    /// <param name="enabled">是否启用。</param>
    /// <param name="overwrite">是否允许覆盖同名声明。</param>
    /// <param name="workspaceRoot">工作区根目录。</param>
    /// <returns>写入结果（配置文件路径）。</returns>
    public async Task<ToolResult<string>> WriteConnectorAsync(
        string name,
        string provider,
        Dictionary<string, string>? settings,
        bool enabled,
        bool overwrite,
        string workspaceRoot)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ToolResult.Fail<string>("连接器名称不能为空。");

        if (string.IsNullOrWhiteSpace(provider))
            return ToolResult.Fail<string>("提供方键不能为空。");

        var providerKey = provider.Trim();
        if (!_factories.Has(providerKey))
        {
            var available = string.Join(", ", _factories.ProviderKeys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase));
            return ToolResult.Fail<string>(
                $"提供方 '{providerKey}' 未注册，连接器不可用。已注册提供方：{(available.Length == 0 ? "（无）" : available)}");
        }

        var entry = new ConnectorWorkspaceEntry
        {
            Name = name.Trim(),
            Provider = providerKey,
            Settings = settings is null
                ? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                : settings.ToDictionary(kvp => kvp.Key, kvp => (string?)kvp.Value, StringComparer.OrdinalIgnoreCase),
            Enabled = enabled
        };

        var securityError = ConnectorSettingsSecurityValidator.Validate(entry.Settings);
        if (securityError is not null)
            return ToolResult.Fail<string>(securityError);

        var result = await _writer.WriteAsync(entry, overwrite, workspaceRoot).ConfigureAwait(false);
        if (!result.IsSuccess) return result;

        _registry.LoadFromWorkspace(workspaceRoot);
        _reloadTrigger?.RequestReload();

        return result;
    }

    /// <summary>
    /// 删除一个工作区连接器声明。
    /// </summary>
    /// <param name="name">连接器实例名。</param>
    /// <param name="workspaceRoot">工作区根目录。</param>
    /// <returns>删除结果。</returns>
    public async Task<ToolResult<string>> DeleteConnectorAsync(string name, string workspaceRoot)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ToolResult.Fail<string>("连接器名称不能为空。");

        var result = await _writer.DeleteAsync(name.Trim(), workspaceRoot).ConfigureAwait(false);
        if (!result.IsSuccess) return result;

        _registry.LoadFromWorkspace(workspaceRoot);
        _reloadTrigger?.RequestReload();

        return result;
    }
}