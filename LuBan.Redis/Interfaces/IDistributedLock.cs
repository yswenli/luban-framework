/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Redis.Interfaces
*文件名： IDistributedLock
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：分布式锁接口
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V1.0.0.0
*描述：分布式锁接口
*
*****************************************************************************/
using LuBan.Redis.Core;

namespace LuBan.Redis.Interfaces;

/// <summary>
/// 分布式锁接口
/// </summary>
public interface IDistributedLock
{
    /// <summary>
    /// 获取锁，返回可释放的锁令牌；获取失败返回 null
    /// </summary>
    /// <param name="waitTimeout">等待获取锁的最大时长，null 表示仅尝试一次</param>
    /// <param name="retryDelay">重试间隔，默认 50ms</param>
    /// <returns>锁令牌，可用于 using 自动释放；获取失败返回 null</returns>
    Task<DistributedLockToken?> AcquireAsync(TimeSpan? waitTimeout = null, TimeSpan? retryDelay = null);

    /// <summary>
    /// 续期锁（仅当持有者匹配时）
    /// </summary>
    /// <param name="newExpiry">新的过期时长</param>
    /// <returns>是否续期成功</returns>
    Task<bool> RenewAsync(TimeSpan newExpiry);

    /// <summary>
    /// 锁键名
    /// </summary>
    string Key { get; }
}
