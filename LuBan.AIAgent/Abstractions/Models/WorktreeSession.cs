/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Abstractions.Models
*文件名： WorktreeSession
*版本号： V1.0.0.0
*唯一标识：a5fe0fdf-f996-4803-ac7e-a91ccaadb7c8
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/10
*描述：一次 worktree 会话的登记记录
*
*****************************************************************************/

namespace LuBan.AIAgent.Abstractions.Models;

/// <summary>一次 worktree 会话的登记记录。</summary>
public sealed record WorktreeSession
{
    /// <summary>所属会话标识。</summary>
    public required string SessionId { get; init; }

    /// <summary>所属工作区标识。</summary>
    public required string WorkspaceId { get; init; }

    /// <summary>worktree 目录绝对路径。</summary>
    public required string WorktreePath { get; init; }

    /// <summary>临时分支名。</summary>
    public required string Branch { get; init; }

    /// <summary>基线分支名。</summary>
    public required string BaseBranch { get; init; }

    /// <summary>基线提交。</summary>
    public required string BaseCommit { get; init; }

    /// <summary>创建时间。</summary>
    public DateTimeOffset CreatedAt { get; init; }
}