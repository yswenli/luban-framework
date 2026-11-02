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
*Author：yswenli
*命名空间：LuBan.AIAgent.Hosting
*文件名： DefaultSeams
*版本号： V1.0.0.0
*唯一标识：d24a2754-a1ed-4c6a-bdd5-3f761c1bb4c3
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：默认接缝实现：单工作区 / Profile / 用户 / 模型选择
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Hosting;

/// <summary>单工作区解析：取 <c>LuBanAgent:WorkspaceRoot</c>，缺省为进程当前目录。</summary>
public sealed class SingleWorkspaceResolver(IOptions<LuBanAgentOptions> options) : IAgentWorkspaceResolver
{
    /// <inheritdoc />
    public Task<WorkspaceDescriptor> ResolveAsync(string sessionId, CancellationToken ct = default)
    {
        var root = string.IsNullOrWhiteSpace(options.Value.WorkspaceRoot)
            ? Directory.GetCurrentDirectory()
            : options.Value.WorkspaceRoot!;

        return Task.FromResult(new WorkspaceDescriptor("default", "Default", root, "normal"));
    }
}

/// <summary>恒返回固定 Profile。</summary>
public sealed class DefaultAgentProfileResolver(IAgentProfile profile) : IAgentProfileResolver
{
    /// <inheritdoc />
    public IAgentProfile Resolve(WorkspaceDescriptor workspace) => profile;
}

/// <summary>默认用户解析：读 <see cref="IUserContext"/>，缺失回落 <c>local</c>。</summary>
public sealed class DefaultAgentUserResolver : IAgentUserResolver
{
    /// <inheritdoc />
    public string ResolveUserId(DiContainer services)
        => services.GetService<IUserContext>()?.UserId is { Length: > 0 } id ? id : "local";
}

/// <summary>
/// 默认模型选择：读 <c>LuBanAgent:DefaultModel</c>（格式 <c>provider:model</c>）。
/// </summary>
/// <remarks>
/// 未配置时 <see cref="QualifiedModelName"/> 为空字符串，由工厂按自身缺省模型处理；
/// 配置值无法按 <c>:</c> 拆成 provider/model 时，两个分量均为 null，但限定名仍原样返回。
/// </remarks>
public sealed class DefaultAgentModelSelection : IAgentModelSelection
{
    private readonly string _qualified;
    private readonly string? _provider;
    private readonly string? _model;

    /// <summary>创建默认模型选择。</summary>
    /// <param name="options">Agent 配置选项。</param>
    public DefaultAgentModelSelection(IOptions<LuBanAgentOptions> options)
    {
        _qualified = options.Value.DefaultModel?.Trim() ?? string.Empty;

        var idx = _qualified.IndexOf(':');
        if (idx > 0 && idx < _qualified.Length - 1)
        {
            _provider = _qualified[..idx];
            _model = _qualified[(idx + 1)..];
        }
    }

    /// <inheritdoc />
    public string? ProviderId => _provider;

    /// <inheritdoc />
    public string? ModelId => _model;

    /// <inheritdoc />
    public string QualifiedModelName => _qualified;
}