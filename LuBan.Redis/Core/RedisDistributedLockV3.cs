/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Redis.Core
*文件名： RedisDistributedLockV3
*版本号： V3.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：高性能分布式锁实现，优化网络往返和重试策略
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V3.0.0.0
*描述：高性能分布式锁实现
*
*****************************************************************************/
using LuBan.Redis.Interfaces;
using System.Collections.Concurrent;

namespace LuBan.Redis.Core;

/// <summary>
/// 高性能分布式锁实现
/// 优化点：
/// 1. 区分可重入和非可重入模式，非可重入使用 SET NX PX（单命令）
/// 2. 指数退避 + 随机抖动，避免惊群效应
/// 3. 缓存完整 key 字符串，减少 GC 压力
/// 4. 支持批量获取锁
/// </summary>
public sealed class RedisDistributedLockV3 : IDistributedLock
{
    private readonly IDatabase _database;
    private readonly string _lockKey;
    private readonly string _lockValue;
    private readonly TimeSpan _expiry;
    private readonly bool _reentrant;
    private readonly string _fullKey; // 缓存完整 key

    // 非重入模式：使用简单的 SET NX PX（单命令，性能最优）
    private static readonly LuaScript _simpleAcquireScript = LuaScript.Prepare(@"
        return redis.call('SET', KEYS[1], ARGV[1], 'NX', 'PX', ARGV[2])
    ");

    private static readonly LuaScript _simpleReleaseScript = LuaScript.Prepare(@"
        if redis.call('GET', KEYS[1]) == ARGV[1] then
            return redis.call('DEL', KEYS[1])
        end
        return 0
    ");

    // 重入模式：使用 Hash 存储重入计数
    private static readonly LuaScript _reentrantAcquireScript = LuaScript.Prepare(@"
        local key = KEYS[1]
        local token = ARGV[1]
        local expiry = tonumber(ARGV[2])
        
        local current = redis.call('hget', key, token)
        if current then
            redis.call('hincrby', key, token, 1)
            redis.call('pexpire', key, expiry)
            return 1
        end
        
        if redis.call('exists', key) == 1 then
            return 0
        end
        
        redis.call('hset', key, token, 1)
        redis.call('pexpire', key, expiry)
        return 1
    ");

    private static readonly LuaScript _reentrantReleaseScript = LuaScript.Prepare(@"
        local key = KEYS[1]
        local token = ARGV[1]
        
        local current = redis.call('hget', key, token)
        if not current then
            return 0
        end
        
        local count = tonumber(current)
        if count > 1 then
            redis.call('hincrby', key, token, -1)
            return 1
        else
            redis.call('del', key)
            return 1
        end
    ");

    private static readonly LuaScript _renewScript = LuaScript.Prepare(@"
        local key = KEYS[1]
        local token = ARGV[1]
        
        if redis.call('hget', key, token) then
            redis.call('pexpire', key, ARGV[2])
            return 1
        end
        return 0
    ");

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="database">Redis 数据库实例</param>
    /// <param name="lockKey">锁键名</param>
    /// <param name="lockValue">持有者标识（token）</param>
    /// <param name="expiry">锁过期时长</param>
    /// <param name="reentrant">是否启用可重入模式（默认 false）</param>
    internal RedisDistributedLockV3(
        IDatabase database,
        string lockKey,
        string lockValue,
        TimeSpan expiry,
        bool reentrant = false)
    {
        _database = database;
        _lockKey = lockKey;
        _lockValue = lockValue;
        _expiry = expiry;
        _reentrant = reentrant;
        _fullKey = $"{CacheConst.KeyDistributeLock}{lockKey}"; // 预计算完整 key
    }

    /// <summary>
    /// 锁键名
    /// </summary>
    public string Key => _lockKey;

    /// <summary>
    /// 获取锁，返回可释放的锁令牌；获取失败返回 null
    /// 使用指数退避 + 随机抖动策略，避免惊群效应
    /// </summary>
    /// <param name="waitTimeout">等待获取锁的最大时长，null 表示仅尝试一次</param>
    /// <param name="retryDelay">初始重试间隔，默认 50ms</param>
    /// <returns>锁令牌，可用于 using 自动释放；获取失败返回 null</returns>
    public async Task<DistributedLockToken?> AcquireAsync(
        TimeSpan? waitTimeout = null,
        TimeSpan? retryDelay = null)
    {
        var deadline = waitTimeout.HasValue
            ? DateTime.UtcNow + waitTimeout.Value
            : (DateTime?)null;
        var baseDelay = retryDelay ?? TimeSpan.FromMilliseconds(50);
        var attempt = 0;
        var random = new Random();

        do
        {
            // 根据模式选择不同的 Lua 脚本
            var script = _reentrant ? _reentrantAcquireScript : _simpleAcquireScript;
            var result = await _database.ScriptEvaluateAsync(
                script.ExecutableScript,
                [_fullKey],
                [_lockValue, (long)_expiry.TotalMilliseconds]
            );

            // 非重入模式返回 OK，重入模式返回 1
            if ((_reentrant && (int)result == 1) ||
                (!_reentrant && result.ToString() == "OK"))
            {
                return new DistributedLockToken(_lockKey, _lockValue, ReleaseInternalAsync);
            }

            // 检查是否超时
            if (deadline == null || DateTime.UtcNow >= deadline.Value)
            {
                return null;
            }

            // 指数退避 + 随机抖动
            // 基础延迟 * 2^attempt + 随机抖动(0~50%)
            var exponentialDelay = baseDelay.TotalMilliseconds * Math.Pow(2, attempt);
            var jitter = exponentialDelay * 0.5 * random.NextDouble();
            var totalDelay = Math.Min(
                exponentialDelay + jitter,
                (deadline.Value - DateTime.UtcNow).TotalMilliseconds
            );

            if (totalDelay > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(totalDelay));
            }

            attempt++;
        } while (true);
    }

    /// <summary>
    /// 续期锁（仅当持有者匹配时）
    /// </summary>
    /// <param name="newExpiry">新的过期时长</param>
    /// <returns>是否续期成功</returns>
    public async Task<bool> RenewAsync(TimeSpan newExpiry)
    {
        var result = (int)(await _database.ScriptEvaluateAsync(
            _renewScript.ExecutableScript,
            [_fullKey],
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
        var script = _reentrant ? _reentrantReleaseScript : _simpleReleaseScript;
        var result = await _database.ScriptEvaluateAsync(
            script.ExecutableScript,
            [_fullKey],
            [token]
        );

        return _reentrant ? (int)result > 0 : result.ToString() == "1";
    }
}

/// <summary>
/// 分布式锁工厂，提供批量锁操作和锁竞争统计
/// </summary>
public static class DistributedLockFactory
{
    private static readonly ConcurrentDictionary<string, long> _contentionStats = new();

    /// <summary>
    /// 创建分布式锁实例
    /// </summary>
    /// <param name="database">Redis 数据库实例</param>
    /// <param name="lockKey">锁键名</param>
    /// <param name="expiry">锁过期时长</param>
    /// <param name="token">自定义 token，不传则自动生成</param>
    /// <param name="reentrant">是否启用可重入模式</param>
    /// <returns>分布式锁实例</returns>
    public static IDistributedLock Create(
        IDatabase database,
        string lockKey,
        TimeSpan expiry,
        string? token = null,
        bool reentrant = false)
    {
        var value = string.IsNullOrEmpty(token) ? GuidUtil.New : token;
        return new RedisDistributedLockV3(database, lockKey, value, expiry, reentrant);
    }

    /// <summary>
    /// 批量获取多个锁（全部成功或全部失败）
    /// 适用于需要同时持有多个锁的场景
    /// </summary>
    /// <param name="locks">锁列表</param>
    /// <param name="waitTimeout">等待超时时间</param>
    /// <returns>成功获取的锁令牌列表；失败时自动释放已获取的锁</returns>
    public static async Task<List<DistributedLockToken>?> AcquireMultipleAsync(
        IEnumerable<IDistributedLock> locks,
        TimeSpan? waitTimeout = null)
    {
        var lockList = locks.ToList();
        var acquiredTokens = new List<DistributedLockToken>();

        try
        {
            foreach (var lockObj in lockList)
            {
                var token = await lockObj.AcquireAsync(waitTimeout);
                if (token == null)
                {
                    // 获取失败，释放已获取的锁
                    foreach (var acquired in acquiredTokens)
                    {
                        await acquired.ReleaseAsync();
                    }
                    return null;
                }
                acquiredTokens.Add(token);
            }

            return acquiredTokens;
        }
        catch
        {
            // 异常时释放已获取的锁
            foreach (var acquired in acquiredTokens)
            {
                await acquired.ReleaseAsync();
            }
            throw;
        }
    }

    /// <summary>
    /// 获取锁竞争统计
    /// </summary>
    /// <returns>锁键 -> 竞争次数</returns>
    public static IReadOnlyDictionary<string, long> GetContentionStats()
    {
        return _contentionStats;
    }

    /// <summary>
    /// 记录锁竞争
    /// </summary>
    internal static void RecordContention(string lockKey)
    {
        _contentionStats.AddOrUpdate(
            lockKey,
            1,
            (_, count) => count + 1
        );
    }
}
