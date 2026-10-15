/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins
*文件名： IPlugin
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：插件主程序契约 - 插件入口，提供标识、初始化、关闭与技能贡献声明
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins;

/// <summary>
/// 插件主程序契约。宿主只依赖本契约程序集，绝不直接依赖插件内部类型（PRD §2.3）。
/// </summary>
/// <remarks>
/// 具体插件在独立 ALC 中加载（PRD §3.4），实例由 <c>PluginLoader</c> 创建，生命周期由
/// <c>PluginManager</c> 驱动：<c>InitializeAsync</c> → 运行期 → <c>ShutdownAsync</c>。
/// </remarks>
public interface IPlugin
{
    /// <summary>
    /// 插件唯一标识（建议反向域名，如 <c>com.tjc.newproject</c>）。
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 展示名（如「新建项目」）。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 插件版本（语义化版本）。
    /// </summary>
    string Version { get; }

    /// <summary>
    /// 插件简介。
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 初始化插件：注册侧栏条目与面板工厂（PRD §3.4 加载流程第 5 步）。
    /// </summary>
    /// <param name="host">宿主能力接口。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task InitializeAsync(IPluginHost host, CancellationToken cancellationToken = default);

    /// <summary>
    /// 关闭插件：退订全部事件、停止定时器、释放文件句柄、取消后台任务（卸载流程第 1 步）。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    Task ShutdownAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 声明本插件要贡献的技能（PRD §4.10.1）。由 <c>PluginManager</c> 加载后逐个登记到宿主技能表。
    /// </summary>
    /// <returns>技能定义集合。</returns>
    IEnumerable<SkillDefinition> ContributeSkills();
}