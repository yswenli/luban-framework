/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins
*文件名： IPluginStateRepository
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：插件状态持久化契约与状态记录
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins;

/// <summary>
/// 插件状态持久化契约（PRD §3.5）。
/// </summary>
/// <remarks>
/// PRD 原计划在宿主 <c>LubanAgentCore</c> 侧复用 SQLite 仓储模式；但框架 <c>LuBan.AIAgent</c>
/// 不依赖 SQLite / Orm，故此处只定义契约，并随框架提供一个 JSON 文件式默认实现
/// （<see cref="Lifecycle.PluginStateRepository"/>）。宿主若需 SQLite 语义，可另行实现本接口并在 DI 中替换。
/// </remarks>
public interface IPluginStateRepository
{
    /// <summary>
    /// 读取全部插件状态记录。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>状态记录列表。</returns>
    Task<IReadOnlyList<PluginStateRecord>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取单个插件状态记录。
    /// </summary>
    /// <param name="pluginId">插件标识。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>状态记录；不存在返回 <c>null</c>。</returns>
    Task<PluginStateRecord?> GetAsync(string pluginId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入（新增或覆盖）插件状态记录。
    /// </summary>
    /// <param name="record">状态记录。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task SaveAsync(PluginStateRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除插件状态记录。
    /// </summary>
    /// <param name="pluginId">插件标识。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task DeleteAsync(string pluginId, CancellationToken cancellationToken = default);
}

/// <summary>
/// 插件状态记录（PRD §3.5）：状态需可持久化，重启后恢复。
/// </summary>
public sealed class PluginStateRecord
{
    /// <summary>
    /// 插件标识。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 记录时插件版本（便于识别「升级后需重启」）。
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 生命周期状态。
    /// </summary>
    public PluginState State { get; set; } = PluginState.Installed;

    /// <summary>
    /// 安装时间（本地时间）。
    /// </summary>
    public DateTimeOffset InstalledAt { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// 最近一次状态变更时间（本地时间）。
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

    /// <summary>
    /// 最近一次错误信息（状态为 <see cref="PluginState.Error"/> 时有值）。
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// 是否因「卸载验证未通过」而降级为禁用（PRD §3.12 第 7 步）。
    /// </summary>
    /// <remarks>
    /// 为 <c>true</c> 时宿主应提示用户「重启后彻底生效」，并已记录诊断日志。
    /// </remarks>
    public bool RequiresRestart { get; set; }
}