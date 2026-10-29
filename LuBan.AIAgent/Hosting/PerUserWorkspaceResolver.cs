/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Hosting
*文件名： PerUserWorkspaceResolver
*版本号： V1.0.0.0
*唯一标识：c4e9b6a2-7d31-4f85-9b02-6e8a3c5f1d97
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：按登录用户隔离工作区
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：按登录用户隔离工作区
*
*****************************************************************************/

using LuBan.AIAgent.Configuration;

using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Hosting;

/// <summary>
/// 按登录用户隔离工作区：<c>&lt;LuBanAgent:WorkspaceRoot&gt;/users/&lt;safe(userId)&gt;</c>。
/// </summary>
/// <remarks>
/// 必须 Scoped 并注入当前 scope 的 <see cref="IServiceProvider"/>：
/// <see cref="IAgentUserResolver.ResolveUserId(IServiceProvider)"/> 需要一个 provider，
/// 注册成 Singleton 会永久捕获首个请求 scope（已释放）。
/// </remarks>
public sealed class PerUserWorkspaceResolver(
    IOptions<LuBanAgentOptions> options,
    IAgentUserResolver users,
    IServiceProvider services) : IAgentWorkspaceResolver
{
    /// <inheritdoc />
    public Task<WorkspaceDescriptor> ResolveAsync(string sessionId, CancellationToken ct = default)
    {
        var userId = users.ResolveUserId(services);
        if (string.IsNullOrWhiteSpace(userId)) userId = "local";

        var baseRoot = string.IsNullOrWhiteSpace(options.Value.WorkspaceRoot)
            ? Directory.GetCurrentDirectory()
            : options.Value.WorkspaceRoot!;

        var root = Path.GetFullPath(Path.Combine(baseRoot, "users", Safe(userId)));
        Directory.CreateDirectory(root);

        return Task.FromResult(new WorkspaceDescriptor(
            $"user:{userId}", $"User {userId}", root, "normal"));
    }

    /// <summary>只保留 <c>[A-Za-z0-9_-]</c>，其余（含 <c>.</c> 与分隔符）替换为 <c>_</c>；防止 <c>..</c> 穿越。</summary>
    /// <param name="userId">原始用户标识。</param>
    /// <returns>可用于目录名的安全片段。</returns>
    internal static string Safe(string userId)
    {
        var chars = userId.Select(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' ? c : '_').ToArray();
        return new string(chars);
    }
}