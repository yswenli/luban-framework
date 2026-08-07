/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.LocalMemory
*文件名： IWorkspaceContextProvider
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：当前工作区上下文提供者接口
*
*****************************************************************************/
namespace LuBan.AIAgent.LocalMemory;

/// <summary>
/// 当前工作区上下文提供者（供 LocalMemory 按工作区隔离记忆）
/// </summary>
public interface IWorkspaceContextProvider
{
    /// <summary>
    /// 当前工作区 ID，无工作区时为 null
    /// </summary>
    string? CurrentWorkspaceId { get; }
}
