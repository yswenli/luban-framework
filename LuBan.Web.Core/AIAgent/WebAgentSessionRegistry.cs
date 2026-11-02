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
*命名空间：LuBan.Web.Core.AIAgent
*文件名： WebAgentSessionRegistry
*版本号： V1.0.0.0
*唯一标识：f5c1a8d3-42e7-4b90-9c56-8d2b0f7e3a41
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：按登录用户维护 Agent 会话并校验会话归属
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：按登录用户维护 Agent 会话并校验会话归属
*
*****************************************************************************/

using System.Collections.Concurrent;

using LuBan.AIAgent.Sessions;
using LuBan.DI;

namespace LuBan.Web.Core.AIAgent;

/// <summary>会话解析状态。</summary>
public enum WebSessionStatus
{
    /// <summary>可用。</summary>
    Ok,
    /// <summary>指定的会话不存在（重启 / 伪造 / 已清理）。</summary>
    NotFound,
    /// <summary>会话不属于当前用户。</summary>
    Forbidden
}

/// <summary>会话解析结果。</summary>
/// <param name="SessionId">最终使用的会话标识。</param>
/// <param name="Created">本次是否新建。</param>
/// <param name="Status">状态；非 <see cref="WebSessionStatus.Ok"/> 时控制器直接返回对应状态码。</param>
public sealed record WebSessionResolution(string SessionId, bool Created, WebSessionStatus Status);

/// <summary>
/// 按登录用户维护 Agent 会话（进程内，接口重启即失效）。
/// </summary>
/// <remarks>
/// <para>
/// 默认 <see cref="ISessionManager"/> 是单例且不区分用户，<c>CurrentSession</c> 为进程级指针，
/// 多用户并发会互相覆盖，因此会话归属由本注册表独立维护：每用户一条「最后一次」记录。
/// </para>
/// <para>
/// 注册为单例（<see cref="ISingleton"/>），由框架反射扫描注册。
/// </para>
/// </remarks>
public sealed class WebAgentSessionRegistry : ISingleton
{
    private readonly ConcurrentDictionary<long, string> _sessionIds = new();
    private readonly ConcurrentDictionary<long, SemaphoreSlim> _gates = new();

    /// <summary>该会话是否是此用户当前的会话。</summary>
    /// <param name="sessionId">会话标识。</param>
    /// <param name="userId">登录用户标识。</param>
    /// <returns>是否属于该用户。</returns>
    public bool IsOwnedBy(string sessionId, long userId)
        => !string.IsNullOrWhiteSpace(sessionId)
            && _sessionIds.TryGetValue(userId, out var current)
            && string.Equals(current, sessionId, StringComparison.Ordinal);

    /// <summary>
    /// 解析本轮使用的会话。
    /// </summary>
    /// <param name="userId">登录用户标识。</param>
    /// <param name="requestedSessionId">前端指定的会话；null/空白表示沿用该用户最后一次。</param>
    /// <param name="sessions">会话管理器。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>解析结果；非 <see cref="WebSessionStatus.Ok"/> 时调用方应直接返回对应状态码。</returns>
    public async Task<WebSessionResolution> ResolveAsync(
        long userId,
        string? requestedSessionId,
        ISessionManager sessions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sessions);
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId), "用户标识必须为正数");

        if (!string.IsNullOrWhiteSpace(requestedSessionId))
        {
            var requested = requestedSessionId.Trim();

            // 归属口径统一走 CheckOwnershipAsync，避免与确认接口分叉
            var status = await CheckOwnershipAsync(userId, requested, sessions)
                .ConfigureAwait(false);
            if (status != WebSessionStatus.Ok)
                return new WebSessionResolution(requested, false, status);

            // 属于本人 → 切换「最后一次」指向
            _sessionIds[userId] = requested;
            return new WebSessionResolution(requested, false, WebSessionStatus.Ok);
        }

        if (await TryGetCurrentAsync(userId, sessions).ConfigureAwait(false) is { } current)
            return new WebSessionResolution(current, false, WebSessionStatus.Ok);

        // 同一用户的并发首轮请求只允许创建一个会话
        var gate = _gates.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (await TryGetCurrentAsync(userId, sessions).ConfigureAwait(false) is { } created)
                return new WebSessionResolution(created, false, WebSessionStatus.Ok);

            var session = await sessions.CreateSessionAsync("web", workspaceId: null).ConfigureAwait(false);

            // 尽力回填归属：内存实现保存的是同一实例，便于排查；权威来源仍是本注册表
            session.UserId = userId.ToString();
            _sessionIds[userId] = session.SessionId;
            return new WebSessionResolution(session.SessionId, true, WebSessionStatus.Ok);
        }
        finally
        {
            gate.Release();

            // 建会话只是首轮动作，门用完即回收，避免按用户无限增长。
            // 不 Dispose：可能仍有等待者持有引用，交给 GC 即可。
            _gates.TryRemove(new KeyValuePair<long, SemaphoreSlim>(userId, gate));
        }
    }

    /// <summary>
    /// 校验会话归属（不改变「最后一次」指向），供对话与确认接口共用。
    /// </summary>
    /// <param name="userId">登录用户标识。</param>
    /// <param name="sessionId">会话标识。</param>
    /// <param name="sessions">会话管理器。</param>
    /// <returns>归属状态：<see cref="WebSessionStatus.Ok"/> 表示属于该用户。</returns>
    public async Task<WebSessionStatus> CheckOwnershipAsync(
        long userId,
        string? sessionId,
        ISessionManager sessions)
    {
        ArgumentNullException.ThrowIfNull(sessions);
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId), "用户标识必须为正数");

        if (string.IsNullOrWhiteSpace(sessionId))
            return WebSessionStatus.NotFound;

        var id = sessionId.Trim();
        if (IsOwnedBy(id, userId))
            return WebSessionStatus.Ok;

        var info = await sessions.GetSessionAsync(id).ConfigureAwait(false);
        if (info is null)
            return WebSessionStatus.NotFound;

        // 权威来源是本注册表；SessionInfo.UserId 只在内存实现下被回填，仅作辅助校验（缺失按拒绝处理）
        return string.Equals(info.UserId, userId.ToString(), StringComparison.Ordinal)
            ? WebSessionStatus.Ok
            : WebSessionStatus.Forbidden;
    }

    /// <summary>取该用户「最后一次」会话；会话已被删除/清理时丢弃记录并返回 null。</summary>
    private async Task<string?> TryGetCurrentAsync(long userId, ISessionManager sessions)
    {
        if (!_sessionIds.TryGetValue(userId, out var existing))
            return null;

        // 会话可能已被删除或清理：失效即丢弃，让下一轮重新创建，避免把幽灵会话发给宿主
        if (await sessions.GetSessionAsync(existing).ConfigureAwait(false) is not null)
            return existing;

        _sessionIds.TryRemove(new KeyValuePair<long, string>(userId, existing));
        return null;
    }
}