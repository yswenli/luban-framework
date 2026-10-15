/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins
*文件名： PluginState
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：插件生命周期状态枚举（4 态，PRD §3.5）
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins;

/// <summary>
/// 插件生命周期状态（PRD §3.5 定案 4 态；已移除 §2.5 多余的 <c>Running</c>）。
/// </summary>
/// <remarks>
/// 状态机：<c>Installed → Enabled → Disabled → (Uninstalled，即从注册表移除)</c>，异常时进入 <c>Error</c>。
/// 状态需可持久化，重启后恢复。
/// </remarks>
public enum PluginState
{
    /// <summary>
    /// 已安装：清单校验通过、文件就位，但尚未初始化。
    /// </summary>
    Installed = 0,

    /// <summary>
    /// 已启用：ALC 已创建、<c>InitializeAsync</c> 完成、UI 贡献与 Skill / MCP 已注册。
    /// </summary>
    Enabled = 1,

    /// <summary>
    /// 已禁用：已走完卸载流程（含降级情况），文件保留。
    /// </summary>
    Disabled = 2,

    /// <summary>
    /// 异常：初始化 / 运行期捕获到未处理异常，宿主已移除其 UI 贡献。
    /// </summary>
    Error = 3
}