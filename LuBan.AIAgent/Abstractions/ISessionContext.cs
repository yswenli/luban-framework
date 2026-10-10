/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Abstractions
*文件名： ISessionContext
*创建时间：2026/10/10
*描述：会话级上下文契约（session-scoped），所有路径按有效根解析
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions.Models;

namespace LuBan.AIAgent.Abstractions;

/// <summary>会话级上下文（session-scoped）。所有路径按 <see cref="EffectiveRoot"/> 解析。</summary>
public interface ISessionContext
{
    /// <summary>当前会话标识。</summary>
    string? SessionId { get; }

    /// <summary>有活动 worktree 即 worktree 路径，否则工作区根。</summary>
    string EffectiveRoot { get; }

    /// <summary>当前活动 worktree（无则 null）。</summary>
    WorktreeSession? ActiveWorktree { get; }

    /// <summary>本会话允许访问的根路径集合。</summary>
    IReadOnlyList<string> AllowedRoots { get; }

    /// <summary>解析本会话的临时目录。</summary>
    string ResolveTempDirectory();

    /// <summary>判断路径是否位于有效根或允许根内。</summary>
    bool IsWithinEffectiveRoot(string path);
}