/***************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Defaults
*文件名： DefaultContextState
*创建时间：2026/10/14
*描述：默认上下文的可写状态（Scoped），供 AddLuBanAgentDefaults 的最小宿主使用
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions.Models;

namespace LuBan.AIAgent.Defaults;

/// <summary>
/// 默认上下文的可写状态（Scoped）。
/// <para>
/// 宿主在会话作用域内设置工作区根 / 会话标识等，再由
/// <see cref="DefaultSessionContext"/>、<see cref="DefaultWorkspaceContext"/>、
/// <see cref="DefaultUserContext"/> 以只读视图形式暴露给框架。
/// 多用户 / 多工作区宿主应替换为自定义上下文实现（<c>TryAdd</c> 语义允许覆盖）。
/// </para>
/// </summary>
public sealed class DefaultContextState
{
    /// <summary>会话标识（null 表示未绑定会话）。</summary>
    public string? SessionId { get; set; }

    /// <summary>工作区标识（null 表示未绑定工作区）。</summary>
    public string? WorkspaceId { get; set; }

    /// <summary>
    /// 工作区根路径（null 时回退 <see cref="Configuration.LuBanAgentOptions.WorkspaceRoot"/>，
    /// 再回退进程当前目录）。
    /// </summary>
    public string? WorkspaceRoot { get; set; }

    /// <summary>当前用户标识（null 表示未归属用户）。</summary>
    public string? UserId { get; set; }

    /// <summary>活动 worktree（无则 null）。</summary>
    public WorktreeSession? ActiveWorktree { get; set; }

    /// <summary>允许访问的根路径集合（在有效根之外额外放行的目录）。</summary>
    public List<string> AllowedRoots { get; } = new();

    /// <summary>会话临时目录（可覆盖）。</summary>
    public string? TempDirectory { get; set; }
}