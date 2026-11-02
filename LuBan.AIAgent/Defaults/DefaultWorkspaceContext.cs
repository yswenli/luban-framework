/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Defaults
*文件名： DefaultWorkspaceContext
*版本号： V1.0.0.0
*唯一标识：fcece278-5a2a-4660-8f27-dd20084722a4
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
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