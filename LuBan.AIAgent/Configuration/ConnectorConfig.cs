/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Configuration
*文件名： ConnectorConfig
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/19
*描述：连接器声明项（config.json 的 connectors 节点）
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 连接器声明项：宿主在 <c>config.json</c> 的 <c>connectors</c> 节点声明「可用连接器」。
/// </summary>
/// <remarks>
/// <para>
/// 该层只负责<b>可用性 + 非敏感参数</b>；是否在当前工作区启用另由
/// <c>.luban-agent/connectors/&lt;name&gt;.json</c> 决定（工作区优先）。
/// </para>
/// <para>
/// <b>严禁</b>在此承载 PAT 等凭据——凭据一律走 <c>ICredentialStore</c>。
/// </para>
/// </remarks>
public class ConnectorConfig
{
    /// <summary>
    /// 连接器实例名（如 <c>yunxiao</c>）。凭据按此名取用。
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 提供方键（如 <c>aliyunDevOps</c>），决定用哪个连接器工厂实例化。
    /// </summary>
    public string Provider { get; set; } = "";

    /// <summary>
    /// 是否启用（工作区未显式覆盖时以此为准）。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 非敏感参数（<c>endpoint</c> / <c>organizationId</c> 等）。
    /// </summary>
    public Dictionary<string, string?> Settings { get; set; } = new();
}