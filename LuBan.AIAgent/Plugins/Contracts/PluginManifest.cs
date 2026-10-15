/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins
*文件名： PluginManifest
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：插件清单 plugin.json 的对象模型（PRD §3.2）
*
*****************************************************************************/
using System.Text.Json.Serialization;

namespace LuBan.AIAgent.Plugins;

/// <summary>
/// 插件清单 <c>plugin.json</c> 的对象模型（PRD §3.2，v0.18 含 <c>mcpServers</c>）。
/// </summary>
public sealed class PluginManifest
{
    /// <summary>
    /// 全局唯一标识（建议反向域名，如 <c>com.tjc.newproject</c>）。
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 展示名（如「新建项目」）。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 语义化版本。
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 作者 / 团队。
    /// </summary>
    [JsonPropertyName("author")]
    public string? Author { get; set; }

    /// <summary>
    /// 简介。
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 入口程序集文件名（含 <see cref="IPlugin"/> 实现）。
    /// </summary>
    [JsonPropertyName("entryAssembly")]
    public string? EntryAssembly { get; set; }

    /// <summary>
    /// 宿主标识（如 <c>LubanAgentCodex</c>）。
    /// </summary>
    [JsonPropertyName("targetApp")]
    public string? TargetApp { get; set; }

    /// <summary>
    /// 最低宿主版本。
    /// </summary>
    [JsonPropertyName("minHostVersion")]
    public string? MinHostVersion { get; set; }

    /// <summary>
    /// 侧栏条目（<c>{ id, label, icon, panelType }</c>）。
    /// </summary>
    [JsonPropertyName("uiContributions")]
    public List<PluginSidebarEntry> UiContributions { get; set; } = new();

    /// <summary>
    /// 插件贡献的专属 Skill 定义（PRD §4.10）。
    /// </summary>
    [JsonPropertyName("skills")]
    public List<SkillDefinition> Skills { get; set; } = new();

    /// <summary>
    /// 插件启用的内置 MCP 服务器声明（PRD §4.11）。
    /// </summary>
    [JsonPropertyName("mcpServers")]
    public List<PluginMcpServerDeclaration> McpServers { get; set; } = new();

    /// <summary>
    /// 权限声明。
    /// </summary>
    [JsonPropertyName("permissions")]
    public PluginPermissions Permissions { get; set; } = new();

    /// <summary>
    /// 依赖插件 id 列表（仅校验存在性，PRD §3.2）。
    /// </summary>
    [JsonPropertyName("dependencies")]
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// 取启用状态的 MCP 服务器声明。
    /// </summary>
    /// <returns>已启用的声明集合。</returns>
    public IEnumerable<PluginMcpServerDeclaration> EnabledMcpServers =>
        McpServers.Where(server => server.Enabled && !string.IsNullOrWhiteSpace(server.Name));
}

/// <summary>
/// 插件启用的内置 MCP 服务器声明（PRD §3.2 <c>mcpServers</c> 元素、§4.11）。
/// </summary>
/// <remarks>
/// 连接器本身属框架共享集，<b>不在本字段声明</b>；此处仅声明「随插件启停的内置 MCP 服务」，
/// 如 <c>{ "name": "yunxiao", "transport": "inproc", "enabled": true }</c>。
/// </remarks>
public sealed class PluginMcpServerDeclaration
{
    /// <summary>
    /// 服务器名（如 <c>yunxiao</c>，宿主据此映射到内置客户端）。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 传输方式（进程内固定 <c>inproc</c>）。
    /// </summary>
    [JsonPropertyName("transport")]
    public string? Transport { get; set; } = "inproc";

    /// <summary>
    /// 是否启用。
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
}