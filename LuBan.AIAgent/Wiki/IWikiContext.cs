/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki
*文件名： IWikiContext
*版本号： V1.0.0.0
*唯一标识：b4d81e72-3c56-4a9f-91d0-2e7f6a8c5b34
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：wiki 工作区上下文接口
*
*=================================================
*修改标记
*修改时间：2026/9/20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：wiki 工作区上下文接口
*
*****************************************************************************/
namespace LuBan.AIAgent.Wiki;

/// <summary>
/// 宿主提供的工作区上下文，供 wiki 定位工作区根。
/// 由 agent 侧实现（读取 WorkspaceManager.Current）。
/// </summary>
public interface IWikiContext
{
    /// <summary>当前工作区根目录绝对路径；无工作区时为 null。</summary>
    string? WorkspaceRoot { get; }

    /// <summary>
    /// 指定工作区的根目录绝对路径；<paramref name="workspaceId"/> 为 null 时等价于 <see cref="WorkspaceRoot"/>。
    /// 用于后台任务显式指定目标工作区，避免受"当前工作区"切换影响。
    /// </summary>
    string? WorkspaceRootFor(string? workspaceId) => WorkspaceRoot;
}