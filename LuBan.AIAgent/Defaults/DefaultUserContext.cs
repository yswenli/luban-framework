/***************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Defaults
*文件名： DefaultUserContext
*创建时间：2026/10/14
*描述：默认用户上下文（Scoped 只读视图），状态来自 DefaultContextState
*
*****************************************************************************/
namespace LuBan.AIAgent.Defaults;

/// <summary>
/// 默认用户上下文（Scoped 只读视图）。
/// <para>
/// 单用户默认实现：<see cref="UserId"/> 默认 null（等价未归属）。
/// 多用户宿主请替换为自定义实现（<c>TryAdd</c> 语义允许覆盖）。
/// </para>
/// </summary>
public sealed class DefaultUserContext(DefaultContextState state) : IUserContext
{
    /// <inheritdoc />
    public string? UserId => state.UserId;
}