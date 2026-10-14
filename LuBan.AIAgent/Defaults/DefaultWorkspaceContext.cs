/***************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Defaults
*文件名： DefaultWorkspaceContext
*创建时间：2026/10/14
*描述：默认工作区上下文（Scoped 只读视图），状态来自 DefaultContextState
*
*****************************************************************************/
namespace LuBan.AIAgent.Defaults;

/// <summary>
/// 默认工作区上下文（Scoped 只读视图）。
/// <para>
/// 单工作区默认实现：<see cref="RootPathFor"/> 恒返回当前工作区根（不区分 workspaceId）。
/// 多工作区宿主请替换为自定义实现。
/// </para>
/// </summary>
public sealed class DefaultWorkspaceContext(DefaultContextState state, IOptions<LuBanAgentOptions> options)
    : IWorkspaceContext
{
    /// <inheritdoc />
    public string? WorkspaceId => state.WorkspaceId;

    /// <inheritdoc />
    public string? RootPath
        => string.IsNullOrWhiteSpace(state.WorkspaceRoot) ? options.Value.WorkspaceRoot : state.WorkspaceRoot;

    /// <inheritdoc />
    public string? RootPathFor(string? workspaceId) => RootPath;
}