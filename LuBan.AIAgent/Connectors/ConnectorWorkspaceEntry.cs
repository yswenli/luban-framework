/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Connectors
*文件名： ConnectorWorkspaceEntry
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/20
*描述：工作区连接器条目 —— .luban-agent/connectors/<name>.json 的对象模型
*
*****************************************************************************/
using System.Text.Json.Serialization;
using LuBan.AIAgent.Configuration;

namespace LuBan.AIAgent.Connectors;

/// <summary>
/// 工作区连接器条目：<c>.luban-agent/connectors/&lt;name&gt;.json</c> 的对象模型。
/// </summary>
/// <remarks>
/// <para>
/// 该文件是「连接器是否在当前工作区启用」的落点，同时可承载由对话生成的<b>完整声明</b>
/// （提供方 + 非敏感参数）；框架把它并入宿主 <c>config.json:connectors</c> 声明之上
/// （合并语义：工作区覆盖 config.json，<c>settings</c> 逐键合并）。
/// </para>
/// <para>
/// <b>兼容</b>：旧格式只用 <c>{ "enabled": true }</c>（不写 name/provider/settings），
/// 仍按「只覆盖启用状态」处理。
/// </para>
/// <para>
/// <b>严禁</b>承载 PAT 等凭据——凭据一律走 <c>ICredentialStore</c>；非敏感参数如需引用凭据，
/// 请写 <c>$credential:&lt;键&gt;</c>。
/// </para>
/// </remarks>
public sealed class ConnectorWorkspaceEntry
{
    /// <summary>
    /// 连接器实例名（缺省时取文件名）。
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 提供方键（如 <c>myVendor</c>）；缺省表示本文件只覆盖启用状态。
    /// </summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    /// <summary>
    /// 非敏感参数（<c>endpoint</c> / <c>organizationId</c> 等）。
    /// </summary>
    [JsonPropertyName("settings")]
    public Dictionary<string, string?>? Settings { get; set; }

    /// <summary>
    /// 是否启用（默认 <c>true</c>）。
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 是否只声明了「启用状态」（未承载提供方 / 参数）。
    /// </summary>
    [JsonIgnore]
    public bool IsEnableOnly => string.IsNullOrWhiteSpace(Provider);

    /// <summary>
    /// 取有效实例名（优先 <see cref="Name"/>，缺省时由宿主用文件名回填）。
    /// </summary>
    /// <param name="fallbackName">文件名回填的实例名。</param>
    /// <returns>有效实例名。</returns>
    public string ResolveName(string fallbackName)
        => string.IsNullOrWhiteSpace(Name) ? fallbackName : Name!.Trim();

    /// <summary>
    /// 转为连接器声明（<c>config.json:connectors</c> 同构）。
    /// </summary>
    /// <param name="fallbackName">文件名回填的实例名。</param>
    /// <returns>连接器声明。</returns>
    public ConnectorConfig ToConnectorConfig(string fallbackName) => new()
    {
        Name = ResolveName(fallbackName),
        Provider = Provider?.Trim() ?? string.Empty,
        Enabled = Enabled,
        Settings = Settings is null
            ? new Dictionary<string, string?>()
            : new Dictionary<string, string?>(Settings, StringComparer.OrdinalIgnoreCase)
    };
}