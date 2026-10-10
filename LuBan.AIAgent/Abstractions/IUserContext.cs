/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Abstractions
*文件名： IUserContext
*创建时间：2026/10/10
*描述：用户级上下文契约（root/keyed），承载 userId 以隔离用户级缓存
*
*****************************************************************************/
namespace LuBan.AIAgent.Abstractions;

/// <summary>用户级上下文（root/keyed）。</summary>
public interface IUserContext
{
    /// <summary>当前用户标识。</summary>
    string? UserId { get; }
}