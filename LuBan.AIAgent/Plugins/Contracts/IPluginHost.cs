/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins
*文件名： IPluginHost
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：宿主能力接口 - 宿主提供给插件的「手」
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins;

/// <summary>
/// 宿主能力接口：宿主提供给插件的「手」（PRD §3.4）。
/// </summary>
/// <remarks>
/// 能力范围：写日志、访问宿主核心服务、读取配置、注册面板、注册侧栏条目、注册命令、注册技能、
/// 注册插件启用的内置 MCP 服务器，以及在 UI 线程投递回调。
/// </remarks>
public interface IPluginHost
{
    /// <summary>
    /// 宿主服务容器。插件据此解析所需核心服务（工作区 / 配置等）。
    /// </summary>
    /// <remarks>
    /// 注意 DI 硬约束（PRD §3.9）：<c>ValidateScopes=true</c> 下不得把 Scoped 服务提升到单例；
    /// 插件应每次按需解析，不要长期持有。
    /// </remarks>
    IServiceProvider Services { get; }

    /// <summary>
    /// 当前插件标识（宿主在分发 host 时回填）。
    /// </summary>
    string PluginId { get; }

    /// <summary>
    /// 当前插件在清单中声明的权限（宿主在分发 host 时回填）。
    /// </summary>
    /// <remarks>
    /// 供插件在<b>宿主未包裹权限环境</b>的场景（如 UI 面板事件回调、向导按钮）自行进入权限环境：
    /// 见 <c>PluginBase.EnterPermissionScope</c>。框架自身进入插件代码时会自动包裹，插件无需重复。
    /// </remarks>
    PluginPermissions Permissions { get; }

    /// <summary>
    /// 写一条插件日志（宿主负责落地到统一日志通道）。
    /// </summary>
    /// <param name="message">日志内容。</param>
    void Log(string message);

    /// <summary>
    /// 写一条插件错误日志。
    /// </summary>
    /// <param name="message">错误描述。</param>
    /// <param name="exception">异常对象，可为空。</param>
    void LogError(string message, Exception? exception = null);

    /// <summary>
    /// 读取宿主配置项。
    /// </summary>
    /// <param name="key">配置键。</param>
    /// <returns>配置值；不存在时返回 <c>null</c>。</returns>
    string? GetConfig(string key);

    /// <summary>
    /// 注册面板工厂：<paramref name="entryId"/> 通常等于侧栏条目 id。
    /// </summary>
    /// <param name="entryId">条目 / 面板标识。</param>
    /// <param name="factory">面板工厂（延迟创建，宿主挂载时才实例化）。</param>
    void RegisterPanel(string entryId, Func<IPluginPanel> factory);

    /// <summary>
    /// 注册一个侧栏条目（宿主在侧栏渲染入口）。
    /// </summary>
    /// <param name="entry">侧栏条目。</param>
    void RegisterSidebarEntry(PluginSidebarEntry entry);

    /// <summary>
    /// 注销一个侧栏条目（卸载流程第 2 步「注销 UI」，须早于 ALC 卸载）。
    /// </summary>
    /// <param name="entryId">侧栏条目 id。</param>
    void UnregisterSidebarEntry(string entryId);

    /// <summary>
    /// 注销一个面板（卸载流程第 2 步，宿主须先解绑内容区再调用本方法）。
    /// </summary>
    /// <param name="entryId">侧栏条目 id（与面板注册键一致）。</param>
    void UnregisterPanel(string entryId);

    /// <summary>
    /// 注册一个命令（PRD §3.8 v0.15：斜杠命令本期不做，此处为声明性契约）。
    /// </summary>
    /// <param name="command">命令描述。</param>
    void RegisterCommand(PluginCommandDescriptor command);

    /// <summary>
    /// 贡献一项技能到宿主技能表（PRD §4.10.1）。
    /// </summary>
    /// <remarks>
    /// 宿主实现应复用§3.11 三件套：写文件到 <c>.luban-agent/skills</c>（工作区作用域）→
    /// <c>SkillRegistry.LoadFromWorkspace()</c> → 即时重载；<b>不</b>新增注册 API。
    /// </remarks>
    /// <param name="definition">技能定义。</param>
    void RegisterSkill(SkillDefinition definition);

    /// <summary>
    /// 注销一项由插件贡献的技能（卸载流程第 3 步，须早于 ALC 卸载）。
    /// </summary>
    /// <param name="skillId">技能标识。</param>
    void UnregisterSkill(string skillId);

    /// <summary>
    /// 启用一项内置 MCP 服务器（对应 <c>plugin.json</c> 的 <c>mcpServers</c> 声明）。
    /// </summary>
    /// <remarks>
    /// 宿主实现应把内置的进程内客户端（如 <c>YunxiaoMCPClient</c>）注册进 <see cref="MCP.MCPRegistry"/>；
    /// MCP 注册表持有该实例是阻止 ALC 卸载的唯一强引用，故注销须排在卸载流程第 3 步。
    /// </remarks>
    /// <param name="name">MCP 服务器名（如 <c>yunxiao</c>）。</param>
    void RegisterMcpServer(string name);

    /// <summary>
    /// 注销一项内置 MCP 服务器。
    /// </summary>
    /// <param name="name">MCP 服务器名。</param>
    void UnregisterMcpServer(string name);

    /// <summary>
    /// 把回调原样投递到 UI 线程（宿主实现，通常是 <c>Dispatcher.UIThread.Post</c>）。
    /// </summary>
    /// <remarks>
    /// <b>插件不得直接调用</b> <c>Dispatcher.UIThread.Post</c>（PRD §6 #22），应改用
    /// <c>PluginBase.PostToUi</c>——后者内置「卸载即失效」令牌。
    /// </remarks>
    /// <param name="action">回调。</param>
    void PostToUi(Action action);

    /// <summary>
    /// 读取已注册的侧栏条目（宿主 UI 渲染用）。
    /// </summary>
    /// <returns>当前全部侧栏条目。</returns>
    IReadOnlyList<PluginSidebarEntry> GetSidebarEntries();

    /// <summary>
    /// 取面板实例供内容区挂载（宿主 UI 用）。
    /// </summary>
    /// <param name="entryId">侧栏条目 id。</param>
    /// <returns>面板实例；未注册返回 <c>null</c>。</returns>
    IPluginPanel? GetPanel(string entryId);
}