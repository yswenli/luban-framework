/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Defaults
*文件名： DefaultUserContext
*版本号： V1.0.0.0
*唯一标识：648536ef-a607-4908-9930-e2a6309d11dc
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
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