/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins
*文件名： IPluginPanel
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：插件面板契约 - 插件提供的界面
*
*****************************************************************************/
namespace LuBan.AIAgent.Plugins;

/// <summary>
/// 插件面板契约：插件提供的界面（PRD §3.4「面板约定」）。
/// </summary>
/// <remarks>
/// 本契约刻意<b>不引用任何 UI 框架类型</b>（框架 <c>LuBan.AIAgent</c> 不依赖 Avalonia），
/// 具体实现通常是宿主 UI 的 <c>UserControl</c>（同时实现本接口），由宿主内容区挂载时强转。
/// 卸载前宿主必须调用 <see cref="OnUnloading"/>，由面板释放界面订阅与定时器。
/// </remarks>
public interface IPluginPanel
{
    /// <summary>
    /// 面板标题（宿主可显示在标题栏 / 标签页）。
    /// </summary>
    string Title { get; }

    /// <summary>
    /// 卸载前由宿主调用：释放界面订阅、停止定时器、解绑事件。
    /// </summary>
    void OnUnloading();
}