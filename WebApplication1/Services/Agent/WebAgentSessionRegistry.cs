/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Services.Agent
*文件名： WebAgentSessionRegistry
*版本号： V1.0.0.0
*唯一标识：c4e8a1b6-5d2f-4a73-8b19-6f3c2d94a0e5
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/28
*描述：按登录用户维护 Agent 会话标识，使 Web 接口的会话按用户隔离
*
*****************************************************************************/

using LuBan.AIAgent.Sessions;

using System.Collections.Concurrent;

namespace WebApplication1.Services.Agent;

/// <summary>
/// 按登录用户维护 Agent 会话标识（进程内，接口重启即失效）。
/// <para>
/// 默认的 <see cref="ISessionManager"/> 是单例且不区分用户，其
/// <c>CurrentSession</c> 为进程级指针，多用户并发下会互相覆盖。
/// 因此本接口的会话归属由本注册表独立维护：同一个用户复用同一个会话（多轮有历史），
/// 不同用户互不可见；调用方始终显式传入 <c>SessionId</c>，不依赖「当前会话」。
/// </para>
/// <para>
/// 注册为单例（<see cref="ISingleton"/>），由框架自动注入扫描注册。
/// </para>
/// </summary>
public sealed class WebAgentSessionRegistry : ISingleton
{
    private readonly ConcurrentDictionary<long, string> _sessionIds = new();
    private readonly ConcurrentDictionary<long, SemaphoreSlim> _gates = new();

    /// <summary>
    /// 取当前用户已有的会话标识；没有则为该用户创建一个。
    /// </summary>
    /// <param name="userId">登录用户标识（JWT 的 UserId claim）。</param>
    /// <param name="sessions">会话管理器（默认单例实现）。</param>
    /// <param name="title">首次创建时的会话标题。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>该用户独占的会话标识。</returns>
    public async Task<string> GetOrCreateSessionIdAsync(
        long userId,
        ISessionManager sessions,
        string? title = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sessions);
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId), "用户标识必须为正数");

        if (_sessionIds.TryGetValue(userId, out var existing))
            return existing;

        // 同一用户的并发首轮请求只允许创建一个会话
        var gate = _gates.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_sessionIds.TryGetValue(userId, out existing))
                return existing;

            var session = await sessions.CreateSessionAsync(title, workspaceId: null).ConfigureAwait(false);

            // 尽力回填归属：内存实现保存的是同一实例，便于后续按用户排查；权威来源仍是本注册表
            session.UserId = userId.ToString();
            _sessionIds[userId] = session.SessionId;
            return session.SessionId;
        }
        finally
        {
            gate.Release();
        }
    }
}