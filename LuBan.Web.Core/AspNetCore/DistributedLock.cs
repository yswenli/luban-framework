/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
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
*修改时间：2025/9/15 18:14:01
*修改人： yswenli
*版本号： V1.0.0.0
*描述：分布式锁
*
*****************************************************************************/
using LuBan.Redis.Interfaces;

namespace LuBan.Web.Core.AspNetCore;

/// <summary>
/// 分布式锁
/// </summary>
public class DistributedLock : IDisposable
{
    private readonly IDistributedLock _distributedLock;
    private readonly DistributedLockToken? _token;

    /// <summary>
    /// 锁名称
    /// </summary>
    public string LockName { get; set; }
    /// <summary>
    /// 超时时间
    /// </summary>
    public int Timeout { get; set; } = 10000;

    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetries { get; set; } = 5;

    /// <summary>
    /// 分布式锁
    /// </summary>
    /// <param name="lockName"></param>
    /// <param name="timeout"></param>
    /// <param name="maxRetries"></param>
    /// <param name="dbIndex"></param>
    public DistributedLock(string lockName, int timeout = 10000, int maxRetries = 5, int dbIndex = 0)
    {
        LockName = lockName;
        Timeout = timeout;
        MaxRetries = maxRetries;

        _distributedLock = LuBanRedis.Instance.GetDistributedLock(lockName, timeout, dbIndex);
        if (Timeout > 0 && MaxRetries > 0)
        {
            var retryTime = Timeout / MaxRetries;
            if (retryTime < 50) retryTime = 50;
            _token = _distributedLock.AcquireAsync(TimeSpan.FromMilliseconds(Timeout), TimeSpan.FromMilliseconds(Timeout / MaxRetries)).GetAwaiter().GetResult();
            if (_token == null) throw new Exception("获取分布式锁失败");
        }
        else
        {
            _token = _distributedLock.AcquireAsync().GetAwaiter().GetResult();
            if (_token == null) throw new Exception("获取分布式锁失败");
        }
    }


    /// <summary>
    /// 释放锁
    /// </summary>
    public void Dispose()
    {
        _token?.Dispose();
    }
}
