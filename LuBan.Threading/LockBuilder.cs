/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Threading
*文件名： LockBuilder
*版本号： V1.0.0.0
*唯一标识：e38dc02f-3f62-49c1-9cc0-5c46fbb6e546
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/11 13:19:36
*描述：锁构建器
*
*=================================================
*修改标记
*修改时间：2025/10/11 13:19:36
*修改人： yswenli
*版本号： V1.0.0.0
*描述：锁构建器
*
*****************************************************************************/

namespace LuBan.Threading;

/// <summary>
/// 锁构建器：管理命名锁池，提供异步获取锁的入口
/// </summary>
public class LockerBuilder : IDisposable
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _lockPool = new();
    private volatile bool _isDisposed = false;
    const string DefaultLockName = "default";

    /// <summary>
    /// 异步获取命名锁
    /// </summary>
    /// <param name="lockName">锁名称（如"张三"、"李四"）</param>
    /// <param name="timeout">等待超时时间（默认 10 秒）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步锁释放器（用于 using 自动释放）</returns>
    /// <exception cref="ArgumentNullException">锁名称为空</exception>
    /// <exception cref="TimeoutException">等待超时</exception>
    /// <exception cref="TaskCanceledException">等待被取消</exception>
    public async Task<LockerReleaser> GetLockerAsync(
        string lockName,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(lockName))
            throw new ArgumentNullException(nameof(lockName), "锁名称不能为空或空白");
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(LockerBuilder), "锁构建器已释放");

        timeout ??= TimeSpan.FromSeconds(10);
        var targetSemaphore = _lockPool.GetOrAdd(lockName, name => new SemaphoreSlim(initialCount: 1, maxCount: 1));

        bool isAcquired = await targetSemaphore.WaitAsync(timeout.Value, cancellationToken);
        if (!isAcquired)
        {
            throw new TimeoutException($"获取锁「{lockName}」超时（超时时间：{timeout.Value.TotalSeconds} 秒）");
        }

        return new LockerReleaser(targetSemaphore, lockName, this);
    }

    /// <summary>
    /// 异步获取命名锁
    /// </summary>
    /// <param name="lockName"></param>
    /// <returns></returns>
    public async Task<LockerReleaser> GetLockerAsync(
        string lockName) => await GetLockerAsync(lockName, TimeSpan.FromMilliseconds(-1), default);

    /// <summary>
    /// 异步获取命名锁
    /// </summary>
    /// <param name="lockName"></param>
    /// <returns></returns>
    public async Task<LockerReleaser> CreateAsync(
        string lockName) => await GetLockerAsync(lockName);

    /// <summary>
    /// 异步获取命名锁
    /// </summary>
    /// <returns></returns>
    public async Task<LockerReleaser> CreateAsync(
       ) => await CreateAsync(DefaultLockName);

    /// <summary>
    /// 同步获取命名锁
    /// </summary>
    /// <param name="lockName">锁名称（如"张三"、"李四"）</param>
    /// <param name="timeout">等待超时时间（默认 10 秒）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步锁释放器（用于 using 自动释放）</returns>
    /// <exception cref="ArgumentNullException">锁名称为空</exception>
    /// <exception cref="TimeoutException">等待超时</exception>
    /// <exception cref="TaskCanceledException">等待被取消</exception>
    public LockerReleaser GetLocker(
        string lockName,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(lockName))
            throw new ArgumentNullException(nameof(lockName), "锁名称不能为空或空白");
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(LockerBuilder), "锁构建器已释放");

        timeout ??= TimeSpan.FromSeconds(10);
        var targetSemaphore = _lockPool.GetOrAdd(lockName, name => new SemaphoreSlim(initialCount: 1, maxCount: 1));

        bool isAcquired = targetSemaphore.Wait(timeout.Value, cancellationToken);
        if (!isAcquired)
        {
            throw new TimeoutException($"获取锁「{lockName}」超时（超时时间：{timeout.Value.TotalSeconds} 秒）");
        }

        return new LockerReleaser(targetSemaphore, lockName, this);
    }

    /// <summary>
    /// 同步获取命名锁
    /// </summary>
    /// <param name="lockName"></param>
    /// <returns></returns>
    public LockerReleaser GetLocker(
        string lockName) => GetLocker(lockName, TimeSpan.FromMilliseconds(-1), default);

    /// <summary>
    /// 同步获取命名锁
    /// </summary>
    /// <param name="lockName"></param>
    /// <returns></returns>
    public LockerReleaser Create(string lockName) => GetLocker(lockName);
    /// <summary>
    /// 同步获取命名锁
    /// </summary>
    /// <returns></returns>
    public LockerReleaser Create() => Create(DefaultLockName);

    /// <summary>
    /// 释放锁构建器资源（如程序退出时）
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        foreach (var (_, semaphore) in _lockPool)
        {
            semaphore.Dispose();
        }
        _lockPool.Clear();
    }

    /// <summary>
    /// 默认锁构建器
    /// </summary>
    public static LockerBuilder Default { get; } = new LockerBuilder();
}
