/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Abstractions
*文件名： IWorkspaceContext
*创建时间：2026/10/10
*描述：工作区级上下文契约（workspace-scoped；P0 由会话作用域承载）
*
*****************************************************************************/
namespace LuBan.AIAgent.Abstractions;

/// <summary>工作区级上下文（workspace-scoped；P0 由会话作用域承载）。</summary>
public interface IWorkspaceContext
{
    /// <summary>当前工作区标识。</summary>
    string? WorkspaceId { get; }

    /// <summary>当前工作区根路径。</summary>
    string? RootPath { get; }

    /// <summary>获取指定工作区的根路径。</summary>
    string? RootPathFor(string? workspaceId);
}