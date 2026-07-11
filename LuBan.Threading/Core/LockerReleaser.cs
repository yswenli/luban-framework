/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Threading.Core
*文件名： AsyncLockerReleaser
*版本号： V1.0.0.0
*唯一标识：5acf518f-16e2-4f45-baba-1769e73733d9
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/11 13:21:25
*描述：
*
*=================================================
*修改标记
*修改时间：2025/10/11 13:21:25
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Threading.Core;

/// <summary>
/// 异步锁释放器：实现 IDisposable，配合 using 自动释放锁
/// </summary>
public sealed class LockerReleaser : IDisposable
{
    private readonly SemaphoreSlim _semaphore; // 对应的 SemaphoreSlim 锁实例
    private readonly string _lockName; // 锁名称
    private readonly LockerBuilder _builder; // 关联的锁构建器（用于释放后清理锁池）
    private bool _isDisposed = false; // 是否已释放

    /// <summary>
    /// 构造释放器
    /// </summary>
    public LockerReleaser(SemaphoreSlim semaphore, string lockName, LockerBuilder builder)
    {
        _semaphore = semaphore ?? throw new ArgumentNullException(nameof(semaphore));
        _lockName = lockName ?? throw new ArgumentNullException(nameof(lockName));
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>
    /// 释放锁（using 块结束时自动调用）
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;
        _semaphore.Release();
        _builder.ReleaseLockReference(_lockName);
        _isDisposed = true;
    }
}