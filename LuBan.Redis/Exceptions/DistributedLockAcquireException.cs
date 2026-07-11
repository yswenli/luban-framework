/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Redis.Exceptions
*文件名： DistributedLockAcquireException
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：分布式锁获取失败异常
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V1.0.0.0
*描述：分布式锁获取失败异常
*
*****************************************************************************/
namespace LuBan.Redis.Exceptions;

/// <summary>
/// 分布式锁获取失败异常
/// 当在指定超时时间内无法获取分布式锁时抛出此异常
/// </summary>
public class DistributedLockAcquireException : Exception
{
    /// <summary>
    /// 锁键名
    /// </summary>
    public string LockKey { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="lockKey">锁键名</param>
    /// <param name="timeout">超时时长</param>
    public DistributedLockAcquireException(string lockKey, TimeSpan timeout)
        : base($"获取分布式锁失败，锁键: {lockKey}，超时: {timeout.TotalMilliseconds}ms")
    {
        LockKey = lockKey;
    }

    /// <summary>
    /// 构造函数（包含内部异常）
    /// </summary>
    /// <param name="lockKey">锁键名</param>
    /// <param name="timeout">超时时长</param>
    /// <param name="innerException">内部异常</param>
    public DistributedLockAcquireException(string lockKey, TimeSpan timeout, Exception innerException)
        : base($"获取分布式锁失败，锁键: {lockKey}，超时: {timeout.TotalMilliseconds}ms", innerException)
    {
        LockKey = lockKey;
    }
}