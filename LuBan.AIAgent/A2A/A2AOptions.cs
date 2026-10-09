/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.A2A
*文件名： A2AOptions
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/8
*描述：A2A 子系统配置
*
*****************************************************************************/
namespace LuBan.AIAgent.A2A;

/// <summary>
/// A2A 服务端配置。
/// </summary>
public class A2AServerOptions
{
    /// <summary>监听地址。约束：仅允许回环地址（127.0.0.1 / ::1 / localhost）。</summary>
    public string Host { get; set; } = "127.0.0.1";

    /// <summary>监听端口。0 = 随机可用端口。</summary>
    public int Port { get; set; }

    /// <summary>端点路径前缀（如 "/a2a"）。</summary>
    public string Path { get; set; } = "/a2a";

    /// <summary>是否要求 API Key 鉴权。</summary>
    public bool RequireApiKey { get; set; } = true;

    /// <summary>允许的 API Key 列表（入站校验）。</summary>
    public List<string> ApiKeys { get; set; } = new();

    /// <summary>最大并发任务数。</summary>
    public int MaxConcurrentTasks { get; set; } = 8;

    /// <summary>任务保留时长（分钟）。</summary>
    public int TaskRetentionMinutes { get; set; } = 60;

    /// <summary>允许的 webhook 推送域名白名单（出站推送防护）。</summary>
    public List<string> AllowedWebhookDomains { get; set; } = new();

    /// <summary>对外暴露的 Agent 名称。</summary>
    public string AgentName { get; set; } = "luban-agent";

    /// <summary>对外暴露的 Agent 描述。</summary>
    public string? AgentDescription { get; set; }

    /// <summary>对外暴露的 Agent 版本。</summary>
    public string AgentVersion { get; set; } = "1.0.0";
}

/// <summary>
/// A2A 客户端（远端连接）配置。
/// </summary>
public class A2AClientOptions
{
    /// <summary>客户端逻辑名称（唯一）。</summary>
    public string Name { get; set; } = "";

    /// <summary>远端端点基址。</summary>
    public string Endpoint { get; set; } = "";

    /// <summary>
    /// 出站 API Key（明文）。
    /// <b>安全提示</b>：工作区 <c>.luban-agent/a2a/*.json</c> 中的 apiKey 为明文，
    /// 建议将 <c>.luban-agent/a2a/</c> 加入 <c>.gitignore</c>，避免密钥入库。
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// 是否注册为本地 SubAgent 角色（角色 Id = <see cref="Name"/>）。
    /// true 时节点可通过 <c>Role</c> 路由到远端执行器。
    /// </summary>
    public bool RegisterAsSubAgent { get; set; }

    /// <summary>远端 Agent 名称（用于相合性校验；null 表示不校验）。</summary>
    public string? RemoteAgentName { get; set; }

    /// <summary>
    /// 该远端客户端是否全放行（A3 语义）。远端执行器在<b>远端</b>执行、<b>不设本地作用域</b>，
    /// 本开关仅作为对外身份声明透传，不作为本地确认绕过。
    /// </summary>
    public bool AllowAll { get; set; }
}

/// <summary>
/// A2A 子系统配置。
/// </summary>
public class A2AOptions
{
    /// <summary>配置节名称（appsettings.json 中 PascalCase 子节点）。</summary>
    public const string SectionName = "LuBanAgent:Agent2Agent";

    /// <summary>
    /// 总开关。false 时整个 A2A 子系统不启用（已注册的默认实现保持 NotSupported 语义）。
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 默认角色模式：off | server | client。
    /// 仅在<b>工作区未提供 <c>.luban-agent/a2a/*.json</c> 配置</b>时作为回退；
    /// 工作区文件存在时以其并集为准（server.json + client*.json 可共存 ⇒ 双角色）。
    /// </summary>
    public string Mode { get; set; } = "off";

    /// <summary>服务端配置。</summary>
    public A2AServerOptions? Server { get; set; }

    /// <summary>客户端（远端连接）列表。</summary>
    public List<A2AClientOptions> Clients { get; set; } = new();

    /// <summary>
    /// 是否允许内部子代理全放行（A3 语义）。
    /// 仅 <c>InProcessSubAgentExecutor</c> 会读取本开关以 <c>allowAll:true</c> 进入子代理作用域；
    /// 远端执行不设本地作用域。默认 false。
    /// </summary>
    public bool SubAgentAllowAll { get; set; }
}