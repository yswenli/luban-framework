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
/// 语义为<b>目录白名单 + 默认拒绝</b>：插件的权限声明必须<b>落在目录内</b>，否则加载前静态校验即判不合规
/// （直接 <c>Disabled</c>，PRD §3.7）；运行期出口再按目录拦截越界访问。
/// </para>
/// <para>
/// 本类型是纯数据 + 规范化，不含匹配逻辑；匹配语义见 <see cref="PluginPermissionEvaluator"/>。
/// 宿主可从配置节点 <c>LuBanAgent:Plugins:Security</c> 覆盖各项。
/// </para>
/// </remarks>
public sealed class PluginSecurityOptions
{
    /// <summary>
    /// 网络域名白名单（PRD §3.7：<c>codeup.aliyun.com</c>、<c>flow.aliyun.com</c>、<c>*.aliyuncs.com</c>）。
    /// </summary>
    /// <remarks>
    /// 支持通配前缀 <c>*.</c>（如 <c>*.aliyuncs.com</c> 覆盖 <c>openapi-rdc.aliyuncs.com</c>）。
    /// 插件<b>不得</b>声明此目录之外的主机，也不得声明裸 <c>*</c>（即「放行全网」）。
    /// </remarks>
    public List<string> NetworkWhitelist { get; set; } = new()
    {
        "codeup.aliyun.com",
        "flow.aliyun.com",
        "*.aliyuncs.com"
    };

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
    /// 已知凭据键目录（PRD §3.7：如 <c>yunxiao_pat</c>）。
    /// </summary>
    /// <remarks>
    /// 插件 <c>credential</c> 声明必须落在目录内；未声明即无法读取（运行期默认拒绝）。
    /// 键按 <see cref="CredentialKeys.Normalize"/> 归一化后比较。
    /// </remarks>
    public List<string> CredentialCatalog { get; set; } = new()
    {
        CredentialKeys.YunxiaoPat
    };

    /// <summary>
    /// Git 能力白名单（PRD §3.7：<c>clone_push</c>）。
    /// </summary>
    /// <remarks>
    /// 插件声明 <c>git: ["clone_push"]</c> 即同时获得克隆与推送；宿主出口按操作名
    /// （<c>clone</c> / <c>push</c>）判定，二者均视为 <c>clone_push</c> 的子操作。
    /// </remarks>
    public List<string> GitWhitelist { get; set; } = new()
    {
        "clone_push"
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
    public void Normalize()
    {
        NetworkWhitelist = NormalizeList(NetworkWhitelist);
        ProcessWhitelist = NormalizeList(ProcessWhitelist);
        CredentialCatalog = NormalizeList(CredentialCatalog.Select(CredentialKeys.Normalize));
        GitWhitelist = NormalizeList(GitWhitelist);
        FilesystemWhitelist = NormalizeList(FilesystemWhitelist);
    }

    private static List<string> NormalizeList(IEnumerable<string> source) => source
        .Where(item => !string.IsNullOrWhiteSpace(item))
        .Select(item => item.Trim())
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();
}