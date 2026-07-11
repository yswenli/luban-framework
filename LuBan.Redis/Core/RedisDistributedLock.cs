/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Redis.Core
*文件名： RedisDistributedLock
*版本号： V2.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：基于 StackExchange.Redis 的分布式锁实现，支持可重入锁机制
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V2.0.0.0
*描述：重构为接口驱动设计，支持可重入锁
*
*****************************************************************************/
using LuBan.Redis.Interfaces;

namespace LuBan.Redis.Core;

/// <summary>
/// 基于 StackExchange.Redis 的分布式锁实现
/// 采用 Redis Hash 存储锁状态，支持可重入锁机制
/// 使用 Lua 脚本保证原子性操作
/// </summary>
public sealed class RedisDistributedLock : IDistributedLock
{
    private readonly IDatabase _database;
    private readonly string _lockKey;
    private readonly string _lockValue;
    private readonly TimeSpan _expiry;

    /// <summary>
    /// 获取锁的 Lua 脚本（支持可重入）
    /// 如果锁不存在，创建锁并设置重入计数为 1
    /// 如果锁已存在且是同一持有者，重入计数加 1
    /// 如果锁被其他持有者持有，返回失败
    /// </summary>
    private static readonly LuaScript _acquireScript = LuaScript.Prepare(@"
        local key = KEYS[1]
        local token = ARGV[1]
        local expiry = tonumber(ARGV[2])
        
        -- 检查是否是同一持有者
        local current = redis.call('hget', key, token)
        if current then
            -- 可重入：重入计数加 1
            redis.call('hincrby', key, token, 1)
            redis.call('pexpire', key, expiry)
            return 1
        end
        
        -- 检查锁是否被其他人持有
        if redis.call('exists', key) == 1 then
            return 0
        end
        
        -- 首次获取：设置重入计数为 1
        redis.call('hset', key, token, 1)
        redis.call('pexpire', key, expiry)
        return 1
    ");

    /// <summary>
    /// 释放锁的 Lua 脚本（支持可重入）
    /// 如果是持有者且重入计数大于 1，计数减 1
    /// 如果是持有者且重入计数等于 1，删除锁
    /// 如果不是持有者，返回失败
    /// </summary>
    private static readonly LuaScript _releaseScript = LuaScript.Prepare(@"
        local key = KEYS[1]
        local token = ARGV[1]
        
        -- 检查是否是锁的持有者
        local current = redis.call('hget', key, token)
        if not current then
            return 0  -- 不是持有者，释放失败
        end
        
        local count = tonumber(current)
        if count > 1 then
            -- 可重入：重入计数减 1
            redis.call('hincrby', key, token, -1)
            return 1
        else
            -- 最后一次释放：删除锁
            redis.call('del', key)
            return 1
        end
    ");

    /// <summary>
    /// 续期锁的 Lua 脚本
    /// 只有持有者才能续期
    /// </summary>
    private static readonly LuaScript _renewScript = LuaScript.Prepare(@"
        local key = KEYS[1]
        local token = ARGV[1]
        
        -- 检查是否是锁的持有者
        if redis.call('hget', key, token) then
            -- 是持有者，续期成功
            redis.call('pexpire', key, ARGV[2])
            return 1
        end
        -- 不是持有者，续期失败
        return 0
    ");

    /// <summary>
    /// 构造函数（内部使用，通过工厂方法创建）
    /// </summary>
    /// <param name="database">Redis 数据库实例</param>
    /// <param name="lockKey">锁键名</param>
    /// <param name="lockValue">持有者标识（token）</param>
    /// <param name="expiry">锁过期时长</param>
    internal RedisDistributedLock(IDatabase database, string lockKey, string lockValue, TimeSpan expiry)
    {
        _database = database;
        _lockKey = lockKey;
        _lockValue = lockValue;
        _expiry = expiry;
    }

    /// <summary>
    /// 锁键名
    /// </summary>
    public string Key => _lockKey;

    /// <summary>
    /// 获取锁，返回可释放的锁令牌；获取失败返回 null
    /// </summary>
    /// <param name="waitTimeout">等待获取锁的最大时长，null 表示仅尝试一次</param>
    /// <param name="retryDelay">重试间隔，默认 50ms</param>
    /// <returns>锁令牌，可用于 using 自动释放；获取失败返回 null</returns>
    public async Task<DistributedLockToken?> AcquireAsync(TimeSpan? waitTimeout = null, TimeSpan? retryDelay = null)
    {
        var deadline = waitTimeout.HasValue ? DateTime.UtcNow + waitTimeout.Value : (DateTime?)null;
        var delay = retryDelay ?? TimeSpan.FromMilliseconds(50);

        do
        {
            var fullKey = $"{CacheConst.KeyDistributeLock}{_lockKey}";
            var result = (int)(await _database.ScriptEvaluateAsync(
                _acquireScript.ExecutableScript,
                [fullKey],
                [_lockValue, (long)_expiry.TotalMilliseconds]
            ));

            if (result == 1)
            {
                return new DistributedLockToken(_lockKey, _lockValue, ReleaseInternalAsync);
            }

            if (deadline == null || DateTime.UtcNow >= deadline.Value)
            {
                return null;
            }

            await Task.Delay(delay);
        } while (true);
    }

    /// <summary>
    /// 续期锁（仅当持有者匹配时）
    /// </summary>
    /// <param name="newExpiry">新的过期时长</param>
    /// <returns>是否续期成功</returns>
    public async Task<bool> RenewAsync(TimeSpan newExpiry)
    {
        var fullKey = $"{CacheConst.KeyDistributeLock}{_lockKey}";
        var result = (int)(await _database.ScriptEvaluateAsync(
            _renewScript.ExecutableScript,
            [fullKey],
            [_lockValue, (long)newExpiry.TotalMilliseconds]
        ));
        return result == 1;
    }

    /// <summary>
    /// 释放锁（内部方法，由 DistributedLockToken 调用）
    /// </summary>
    /// <param name="token">持有者标识</param>
    /// <returns>是否释放成功</returns>
    internal async Task<bool> ReleaseInternalAsync(string token)
    {
        var fullKey = $"{CacheConst.KeyDistributeLock}{_lockKey}";
        var result = (int)(await _database.ScriptEvaluateAsync(
            _releaseScript.ExecutableScript,
            [fullKey],
            [token]
        ));
        return result > 0;
    }
}