/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Plugins.Security
*文件名： PluginSecurityOptions
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/16
*描述：插件权限白名单策略（PRD §3.7 / M3）
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins.Security;

/// <summary>
/// 插件权限白名单策略：宿主允许插件声明与访问的「授权目录」（PRD §3.7）。
/// </summary>
/// <remarks>
/// <para>
/// 语义为<b>目录白名单 + 默认拒绝</b>：插件声明的<b>具体范围</b>必须<b>落在目录内</b>，否则加载前静态校验即判不合规
/// （直接 <c>Disabled</c>，PRD §3.7）；声明通配 <c>*</c> 表示范围由本白名单决定（声明期放行并记警告）；
/// 运行期出口再按目录拦截越界访问。
/// </para>
/// <para>
/// 本类型是纯数据 + 规范化，不含匹配逻辑；匹配语义见 <see cref="PluginPermissionEvaluator"/>。
/// 宿主可从配置节点 <c>LuBanAgent:Plugins:Security</c> 覆盖各项。
/// </para>
/// </remarks>
public sealed class PluginSecurityOptions
{
    /// <summary>
    /// 网络域名白名单。
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>默认空</b>（默认拒绝）：具体域名属厂商知识，由宿主 / 产品按需在
    /// <c>LuBanAgent:Plugins:Security:NetworkWhitelist</c> 配置。
    /// </para>
    /// <para>
    /// 支持通配前缀 <c>*.</c>（如 <c>*.example.com</c>）。插件<b>不得</b>声明此目录之外的具体主机；
    /// 声明通配 <c>*</c>（含数组简写的裸维度 <c>"network"</c>）时，实际可访问范围以本白名单为上限
    /// （声明期放行并记警告，运行期按白名单判定）。
    /// </para>
    /// </remarks>
    public List<string> NetworkWhitelist { get; set; } = new();

    /// <summary>
    /// 进程命令白名单（PRD §3.7：<c>dotnet</c>、<c>git</c>）。
    /// </summary>
    /// <remarks>取命令名（不含路径与扩展名，如 <c>git</c>），忽略大小写。</remarks>
    public List<string> ProcessWhitelist { get; set; } = new()
    {
        "dotnet",
        "git"
    };

    /// <summary>
    /// 已知凭据键目录。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 插件 <c>credential</c> 声明必须落在目录内；未声明即无法读取（运行期默认拒绝）。
    /// </para>
    /// <para>
    /// <b>默认空</b>（默认拒绝）：具体凭据键属产品或厂商知识，由宿主按需在
    /// <c>LuBanAgent:Plugins:Security:CredentialCatalog</c> 配置（如 <c>connector:myconnector:token</c>）。
    /// 键按 <see cref="CredentialKeys.Normalize"/> 归一化（可经 <see cref="ICredentialKeyAliases"/> 解析历史拼写）后比较。
    /// </para>
    /// </remarks>
    public List<string> CredentialCatalog { get; set; } = new();

    /// <summary>
    /// 连接器提供方白名单（PRD §3.7：插件可注册 / 使用的 <c>connectors</c> 提供方键）。
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>默认空</b>（默认拒绝）：具体提供方键属产品 / 插件知识，由宿主按需在
    /// <c>LuBanAgent:Plugins:Security:Connectors</c> 配置（如 <c>aliyunDevOps</c>）。
    /// </para>
    /// <para>
    /// 插件 <c>permissions.connectors</c> 声明必须落在目录内；未声明即无法注册 / 使用（运行期默认拒绝）。
    /// </para>
    /// </remarks>
    public List<string> ConnectorWhitelist { get; set; } = new();

    /// <summary>
    /// Git 能力白名单（PRD §3.7：<c>clone_push</c>、<c>commit</c>、<c>inspect</c>）。
    /// </summary>
    /// <remarks>
    /// 能力组覆盖其子操作：<c>clone_push</c> → <c>clone</c> / <c>push</c>（远端读写）；
    /// <c>commit</c> → <c>add</c> / <c>commit</c>（本地暂存与提交）；
    /// <c>inspect</c> → <c>rev-parse</c> / <c>status</c> / <c>diff</c> / <c>branch</c> / <c>config</c> / <c>checkout</c>
    /// （本地只读检查与切分支）。插件必须声明到「组」或「具体操作名」，未声明即拒绝。
    /// </remarks>
    public List<string> GitWhitelist { get; set; } = new()
    {
        "clone_push",
        "commit",
        "inspect"
    };

    /// <summary>
    /// 文件系统范围白名单（PRD §3.7：<c>workspace</c>）。
    /// </summary>
    /// <remarks>当前仅作声明期校验，无运行期出口拦截（框架未提供统一文件出口）。</remarks>
    public List<string> FilesystemWhitelist { get; set; } = new()
    {
        "workspace"
    };

    /// <summary>
    /// 是否在运行期强制拦截越界访问（默认 <c>true</c>）。
    /// </summary>
    /// <remarks>
    /// 关闭后仅做声明期静态校验与拒绝审计，不做出口拦截；供宿主排查兼容性问题时使用，
    /// 生产环境应保持开启。
    /// </remarks>
    public bool EnforceAtRuntime { get; set; } = true;

    /// <summary>
    /// 是否记录越界拒绝审计（默认 <c>true</c>）。
    /// </summary>
    public bool AuditDenials { get; set; } = true;

    /// <summary>
    /// 内存中保留的最近拒绝记录条数上限（默认 64）。
    /// </summary>
    public int MaxAuditRecords { get; set; } = 64;

    /// <summary>
    /// 把各白名单项去重、去空白（忽略大小写）。
    /// </summary>
    /// <param name="aliases">
    /// 凭据键别名来源（可选）：用于把目录里的历史拼写解析为规范键。
    /// 不传则仅做通用归一化。
    /// </param>
    public void Normalize(ICredentialKeyAliases? aliases = null)
    {
        NetworkWhitelist = NormalizeList(NetworkWhitelist);
        ProcessWhitelist = NormalizeList(ProcessWhitelist);
        CredentialCatalog = NormalizeList(CredentialCatalog.Select(ResolveCredentialKey));

        GitWhitelist = NormalizeList(GitWhitelist);
        FilesystemWhitelist = NormalizeList(FilesystemWhitelist);
        ConnectorWhitelist = NormalizeList(ConnectorWhitelist);

        string ResolveCredentialKey(string key)
            => aliases is null ? CredentialKeys.Normalize(key) : aliases.Resolve(key);
    }

    private static List<string> NormalizeList(IEnumerable<string> source) => source
        .Where(item => !string.IsNullOrWhiteSpace(item))
        .Select(item => item.Trim())
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();
}