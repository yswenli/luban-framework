/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.AspNetCore
*文件名： DistributedLock
*版本号： V1.0.0.0
*唯一标识：4cc99177-6684-4834-8e7d-c50b55015486
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/9/15 18:14:01
*描述：分布式锁
*
*=================================================
*修改标记
*修改时间：2026/9/2
*修改人： AI 辅助
*版本号： V1.0.0.1
*描述：改为异步优先。新增 IAsyncDisposable 与 CreateAsync 工厂，
*      请求路径请改用 CreateAsync 以避免阻塞请求线程导致线程池饥饿。
*
*****************************************************************************/
using System.Threading.Tasks;

using LuBan.Redis.Interfaces;

namespace LuBan.Web.Core.AspNetCore;

/// <summary>
/// 分布式锁
/// </summary>
public class DistributedLock : IDisposable, IAsyncDisposable
{
    /// <summary>默认锁超时时间（毫秒）：10 秒</summary>
    public const int DefaultTimeoutMs = 10000;

    /// <summary>默认最大重试次数：5 次</summary>
    public const int DefaultMaxRetries = 5;

    private readonly IDistributedLock _distributedLock;
    private DistributedLockToken? _token;

    /// <summary>
    /// 锁名称
    /// </summary>
    public string LockName { get; set; }

    /// <summary>
    /// 获取锁的总超时时间（毫秒）。
    /// 实际每次重试的等待间隔 = Timeout / MaxRetries，默认 10000/5 = 2000 毫秒。
    /// </summary>
    public int Timeout { get; set; } = DefaultTimeoutMs;

    /// <summary>
    /// 最大重试次数。与 <see cref="Timeout"/> 共同决定重试间隔（Timeout / MaxRetries）。
    /// </summary>
    public int MaxRetries { get; set; } = DefaultMaxRetries;

    /// <summary>
    /// 私有构造：仅完成对象与底层锁的创建，不获取锁。锁的实际获取由 <see cref="CreateAsync"/> 或公开构造完成。
    /// </summary>
    private DistributedLock(IDistributedLock distributedLock, string lockName, int timeout, int maxRetries)
    {
        _distributedLock = distributedLock;
        LockName = lockName;
        Timeout = timeout;
        MaxRetries = maxRetries;
    }

    /// <summary>
    /// 异步获取分布式锁（推荐）。在异步上下文中调用，不会阻塞请求线程，可避免线程池饥饿。
    /// 配合 <c>await using</c> 使用，释放时走 <see cref="DisposeAsync"/>。
    /// </summary>
    /// <param name="lockName">锁名称</param>
    /// <param name="timeout">获取锁的总超时时间（毫秒），重试间隔 = timeout / maxRetries</param>
    /// <param name="maxRetries">最大重试次数</param>
    /// <param name="dbIndex">Redis 库索引</param>
    /// <returns>已获取锁的 <see cref="DistributedLock"/> 实例</returns>
    /// <exception cref="Exception">在超时范围内仍未获取成功时抛出</exception>
    public static async Task<DistributedLock> CreateAsync(string lockName, int timeout = DefaultTimeoutMs, int maxRetries = DefaultMaxRetries, int dbIndex = 0)
    {
        var dl = new DistributedLock(LuBanRedis.Instance.GetDistributedLock(lockName, timeout, dbIndex), lockName, timeout, maxRetries);
        dl._token = await dl.AcquireCoreAsync().ConfigureAwait(false);
        if (dl._token == null) throw new Exception("获取分布式锁失败");
        return dl;
    }

    /// <summary>
    /// 同步获取分布式锁（兼容旧代码）。会阻塞当前调用线程，
    /// <b>请勿在 ASP.NET 请求路径中使用</b>，否则在高并发下会占用请求线程、导致线程池饥饿。
    /// 请求路径请改用 <see cref="CreateAsync"/>。
    /// </summary>
    /// <param name="lockName">锁名称</param>
    /// <param name="timeout">获取锁的总超时时间（毫秒），重试间隔 = timeout / maxRetries</param>
    /// <param name="maxRetries">最大重试次数</param>
    /// <param name="dbIndex">Redis 库索引</param>
    public DistributedLock(string lockName, int timeout = DefaultTimeoutMs, int maxRetries = DefaultMaxRetries, int dbIndex = 0)
        : this(LuBanRedis.Instance.GetDistributedLock(lockName, timeout, dbIndex), lockName, timeout, maxRetries)
    {
        _token = AcquireCoreAsync().GetAwaiter().GetResult();
        if (_token == null) throw new Exception("获取分布式锁失败");
    }

    /// <summary>
    /// 获取锁的核心逻辑（异步）。
    /// </summary>
    private async Task<DistributedLockToken?> AcquireCoreAsync()
    {
        if (Timeout > 0 && MaxRetries > 0)
        {
            var retryTime = Timeout / MaxRetries;
            if (retryTime < 50) retryTime = 50;
            return await _distributedLock.AcquireAsync(TimeSpan.FromMilliseconds(Timeout), TimeSpan.FromMilliseconds(Timeout / MaxRetries)).ConfigureAwait(false);
        }

        return await _distributedLock.AcquireAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// 同步释放锁
    /// </summary>
    public void Dispose() => _token?.Dispose();

    /// <summary>
    /// 异步释放锁（配合 <c>await using</c>）
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_token != null) await _token.DisposeAsync().ConfigureAwait(false);
    }
}
