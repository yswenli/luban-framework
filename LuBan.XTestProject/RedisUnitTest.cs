/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.XTestProject
*文件名： RedisUnitTest
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：LuBan.Redis 单元测试示例
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBan.Redis 单元测试示例
*
*****************************************************************************/
using System.Diagnostics;
using LuBan.Redis;
using LuBan.Redis.Core;
using LuBan.Redis.Exceptions;
using LuBan.Redis.Interfaces;
using LuBan.Redis.Models;
using LuBan.XTestProject.Models;

namespace LuBan.XTestProject;

/// <summary>
/// Redis 单元测试
/// 测试 LuBan.Redis 的各种功能：缓存、队列、发布订阅、分布式锁等
/// 注意：需要 Redis 服务器运行才能执行这些测试
/// </summary>
[TestClass]
public class RedisUnitTest
{
    private LuBanRedis _redis = null!;

    [TestInitialize]
    public void Initialize()
    {
        // 初始化 Redis 客户端（单例模式）
        _redis = LuBanRedis.Instance;
    }

    #region 基本缓存操作测试

    /// <summary>
    /// 测试 Redis 基本的 Set/Get 操作
    /// </summary>
    [TestMethod]
    public void TestBasicStringOperations()
    {
        // 准备
        var key = "test:basic:string";
        var value = "Hello Redis";

        // 执行
        var db = _redis.GetDatabase();
        db.StringSet(key, value, TimeSpan.FromSeconds(10));
        var result = db.StringGet(key);

        // 验证
        Assert.IsTrue(result.HasValue);
        Assert.AreEqual(value, result.ToString());

        // 清理
        db.KeyDelete(key);
    }

    /// <summary>
    /// 测试 Redis 对象序列化/反序列化
    /// </summary>
    [TestMethod]
    public void TestObjectSerialization()
    {
        // 准备
        var key = "test:basic:object";
        var testInfo = new TestInfo { Name = "测试对象", Description = "描述", StartTime = DateTime.Now };

        // 执行
        var db = _redis.GetDatabase();
        db.StringSet(key, testInfo.ToJson(), TimeSpan.FromSeconds(10));
        var result = db.StringGet(key);

        // 验证
        Assert.IsTrue(result.IsNotNullOrEmpty());
        var deserialized = result.GetT<TestInfo>();
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(testInfo.Name, deserialized.Name);
        Assert.AreEqual(testInfo.Description, deserialized.Description);

        // 清理
        db.KeyDelete(key);
    }

    #endregion

    #region RedisCache 测试

    /// <summary>
    /// 测试 RedisCache 的 Set/Get 操作
    /// </summary>
    [TestMethod]
    public void TestRedisCache_SetAndGet()
    {
        // 准备
        var cache = new RedisCache();
        var key = "test:cache:setget";
        var value = "缓存测试值";

        // 执行
        cache.Set(key, value);
        var result = cache.Get<string>(key);

        // 验证
        Assert.IsNotNull(result);
        Assert.AreEqual(value, result);

        // 清理
        cache.Delete(key);
    }

    /// <summary>
    /// 测试 RedisCache 的 GetOrSet 操作（缓存穿透保护）
    /// </summary>
    [TestMethod]
    public void TestRedisCache_GetOrSet()
    {
        // 准备
        var cache = new RedisCache();
        var key = "test:cache:getorset";
        var callCount = 0;

        // 执行：第一次调用会执行工厂方法，第二次应该从缓存获取
        var result1 = cache.GetOrSet<string>(key, (k) =>
        {
            callCount++;
            return "工厂方法返回值";
        });

        var result2 = cache.GetOrSet<string>(key, (k) =>
        {
            callCount++;
            return "工厂方法返回值";
        });

        // 验证
        Assert.AreEqual("工厂方法返回值", result1);
        Assert.AreEqual("工厂方法返回值", result2);
        Assert.AreEqual(1, callCount); // 工厂方法只应被调用一次

        // 清理
        cache.Delete(key);
    }

    /// <summary>
    /// 测试 RedisCache 的 Delete 操作
    /// </summary>
    [TestMethod]
    public void TestRedisCache_Delete()
    {
        // 准备
        var cache = new RedisCache();
        var key = "test:cache:delete";
        cache.Set(key, "待删除的值");

        // 执行
        cache.Delete(key);
        var result = cache.Get<string>(key);

        // 验证
        Assert.IsNull(result);
    }

    /// <summary>
    /// 测试 RedisCache 的 ContainsKey 操作
    /// </summary>
    [TestMethod]
    public void TestRedisCache_ContainsKey()
    {
        // 准备
        var cache = new RedisCache();
        var key = "test:cache:contains";

        // 执行
        cache.Set(key, "存在性测试");
        var exists = cache.ContainsKey(key);

        // 验证
        Assert.IsTrue(exists);

        // 清理
        cache.Delete(key);
    }

    #endregion

    #region RedisQueue 测试

    /// <summary>
    /// 测试 RedisQueue 的入队和出队操作
    /// </summary>
    [TestMethod]
    public async Task TestRedisQueue_EnqueueAndDequeue()
    {
        // 准备
        var topic = "test:queue:basic";
        var queue = _redis.GetRedisQueue<TestInfo>(topic);
        var testInfo = new TestInfo { Name = "队列测试", Description = "测试描述", StartTime = DateTime.Now };

        // 执行入队
        var queueId = await queue.EnqueueAsync(testInfo);
        Assert.IsNotNull(queueId);

        // 执行出队
        var refKey = new RefValue<string>(string.Empty);
        var dequeued = await queue.DequeueAsync(refKey);

        // 验证
        Assert.IsNotNull(dequeued);
        Assert.AreEqual(testInfo.Name, dequeued.Name);
        Assert.AreEqual(testInfo.Description, dequeued.Description);

        // 清理
        await queue.ClearAsync();
    }

    /// <summary>
    /// 测试 RedisQueue 的队列长度
    /// </summary>
    [TestMethod]
    public async Task TestRedisQueue_Length()
    {
        // 准备
        var topic = "test:queue:length";
        var queue = _redis.GetRedisQueue<TestInfo>(topic);

        // 清空队列
        await queue.ClearAsync();

        // 执行：入队 3 个元素
        for (int i = 0; i < 3; i++)
        {
            await queue.EnqueueAsync(new TestInfo { Name = $"测试{i}", Description = $"描述{i}" });
        }

        // 验证
        Assert.AreEqual(3, queue.Length);

        // 清理
        await queue.ClearAsync();
    }

    #endregion

    #region 发布订阅测试

    /// <summary>
    /// 测试 Redis 发布订阅功能
    /// </summary>
    [TestMethod]
    public async Task TestRedisPubSub()
    {
        // 准备
        var channel = "test:pubsub:channel";
        var receivedMessage = string.Empty;
        var tcs = new TaskCompletionSource<bool>();

        // 创建订阅者
        var subscriber = _redis.GetSubscriber(channel);
        subscriber.OnMessageReceived += (sender, message) =>
        {
            receivedMessage = message;
            tcs.TrySetResult(true);
        };

        // 启动订阅
        await subscriber.StartAsync();

        // 等待订阅建立
        await Task.Delay(100);

        // 创建发布者并发布消息
        var publisher = _redis.GetPublisher(channel);
        await publisher.PublishAsync("测试消息");

        // 等待接收消息（最多 5 秒）
        await Task.WhenAny(tcs.Task, Task.Delay(5000));

        // 验证
        Assert.AreEqual("测试消息", receivedMessage);

        // 清理
        await subscriber.StopAsync();
        subscriber.Dispose();
    }

    /// <summary>
    /// 测试 Redis 发布订阅传递对象
    /// </summary>
    [TestMethod]
    public async Task TestRedisPubSub_WithObject()
    {
        // 准备
        var channel = "test:pubsub:object";
        TestInfo? receivedObject = null;
        var tcs = new TaskCompletionSource<bool>();

        // 创建订阅者
        var subscriber = _redis.GetSubscriber<TestInfo>(channel);
        subscriber.OnMessageReceived += (sender, message) =>
        {
            receivedObject = message;
            tcs.TrySetResult(true);
        };

        // 启动订阅
        await subscriber.StartAsync();
        await Task.Delay(100);

        // 创建发布者并发布对象
        var publisher = _redis.GetPublisher(channel);
        var testInfo = new TestInfo { Name = "对象订阅测试", Description = "测试描述", StartTime = DateTime.Now };
        await publisher.PublishAsync(testInfo);

        // 等待接收消息
        await Task.WhenAny(tcs.Task, Task.Delay(5000));

        // 验证
        Assert.IsNotNull(receivedObject);
        Assert.AreEqual(testInfo.Name, receivedObject.Name);
        Assert.AreEqual(testInfo.Description, receivedObject.Description);

        // 清理
        await subscriber.StopAsync();
        subscriber.Dispose();
    }

    #endregion

    #region 分布式锁测试

    /// <summary>
    /// 测试分布式锁的基本获取和释放
    /// </summary>
    [TestMethod]
    public async Task TestDistributedLock_BasicAcquireAndRelease()
    {
        // 准备
        var lockKey = "test:lock:basic";

        // 执行：获取锁
        var distributedLock = _redis.GetDistributedLock(lockKey, 10000);
        var token = await distributedLock.AcquireAsync(TimeSpan.FromSeconds(5));

        // 验证：成功获取锁
        Assert.IsNotNull(token);
        Assert.AreEqual(lockKey, token.Key);

        // 执行：释放锁
        var released = await token.ReleaseAsync();

        // 验证：成功释放锁
        Assert.IsTrue(released);
    }

    /// <summary>
    /// 测试分布式锁的 using 模式（自动释放）
    /// </summary>
    [TestMethod]
    public async Task TestDistributedLock_UsingPattern()
    {
        // 准备
        var lockKey = "test:lock:using";

        // 执行：使用 using 模式获取锁
        var distributedLock = _redis.GetDistributedLock(lockKey, 10000);
        await using var token = await distributedLock.AcquireAsync(TimeSpan.FromSeconds(5));

        // 验证：成功获取锁
        Assert.IsNotNull(token);

        // 执行：在 using 块内执行业务逻辑
        await Task.Delay(100);

        // 离开 using 块时自动释放锁
    }

    /// <summary>
    /// 测试分布式锁的互斥性（同一锁不能被重复获取）
    /// </summary>
    [TestMethod]
    public async Task TestDistributedLock_MutualExclusion()
    {
        // 准备
        var lockKey = "test:lock:mutex";
        var distributedLock = _redis.GetDistributedLock(lockKey, 10000);

        // 执行：第一次获取锁
        var token1 = await distributedLock.AcquireAsync(TimeSpan.FromSeconds(1));
        Assert.IsNotNull(token1);

        // 执行：第二次尝试获取同一锁（应失败）
        var token2 = await distributedLock.AcquireAsync(TimeSpan.FromMilliseconds(500));

        // 验证：第二次获取应失败
        Assert.IsNull(token2);

        // 清理：释放第一次获取的锁
        await token1.ReleaseAsync();

        // 执行：释放后再次获取应成功
        var token3 = await distributedLock.AcquireAsync(TimeSpan.FromSeconds(1));
        Assert.IsNotNull(token3);
        await token3.ReleaseAsync();
    }

    /// <summary>
    /// 测试分布式锁的可重入性
    /// </summary>
    [TestMethod]
    public async Task TestDistributedLock_Reentrant()
    {
        // 准备
        var lockKey = "test:lock:reentrant";
        var distributedLock = _redis.GetDistributedLock(lockKey, 10000);

        // 执行：第一次获取锁
        var token1 = await distributedLock.AcquireAsync(TimeSpan.FromSeconds(1));
        Assert.IsNotNull(token1);

        // 执行：同一持有者再次获取锁（可重入）
        var token2 = await distributedLock.AcquireAsync(TimeSpan.FromSeconds(1));
        Assert.IsNotNull(token2);

        // 执行：释放第二次获取的锁（计数减 1）
        await token2.ReleaseAsync();

        // 执行：尝试获取锁（应失败，因为第一次锁还在）
        var token3 = await distributedLock.AcquireAsync(TimeSpan.FromMilliseconds(500));
        Assert.IsNull(token3);

        // 清理：释放第一次获取的锁
        await token1.ReleaseAsync();
    }

    /// <summary>
    /// 测试分布式锁的续期功能
    /// </summary>
    [TestMethod]
    public async Task TestDistributedLock_Renew()
    {
        // 准备
        var lockKey = "test:lock:renew";
        var distributedLock = _redis.GetDistributedLock(lockKey, 5000); // 5 秒过期

        // 执行：获取锁
        var token = await distributedLock.AcquireAsync(TimeSpan.FromSeconds(1));
        Assert.IsNotNull(token);

        // 执行：续期锁
        var renewed = await distributedLock.RenewAsync(TimeSpan.FromSeconds(10));

        // 验证：续期成功
        Assert.IsTrue(renewed);

        // 清理
        await token.ReleaseAsync();
    }

    /// <summary>
    /// 测试分布式锁获取失败时抛出专用异常
    /// </summary>
    [TestMethod]
    public async Task TestDistributedLock_AcquireException()
    {
        // 准备
        var lockKey = "test:lock:exception";
        var distributedLock = _redis.GetDistributedLock(lockKey, 10000);

        // 执行：第一次获取锁
        var token = await distributedLock.AcquireAsync(TimeSpan.FromSeconds(1));
        Assert.IsNotNull(token);

        // 执行：使用 DistributedLock 包装类尝试获取同一锁（应抛出异常）
        try
        {
            await Task.Run(() =>
            {
                // DistributedLock 在构造函数中获取锁，失败时抛出异常
                using var lock2 = new LuBan.Web.Core.AspNetCore.DistributedLock(lockKey, 1000, 3);
            });
            Assert.Fail("应该抛出 DistributedLockAcquireException 异常");
        }
        catch (DistributedLockAcquireException)
        {
            // 预期的异常
        }

        // 清理
        await token.ReleaseAsync();
    }

    /// <summary>
    /// 测试分布式锁的超时等待功能
    /// </summary>
    [TestMethod]
    public async Task TestDistributedLock_WaitTimeout()
    {
        // 准备
        var lockKey = "test:lock:wait";
        var distributedLock = _redis.GetDistributedLock(lockKey, 10000);

        // 执行：第一次获取锁
        var token1 = await distributedLock.AcquireAsync(TimeSpan.FromSeconds(1));
        Assert.IsNotNull(token1);

        // 执行：在后台线程中延迟释放锁
        _ = Task.Delay(2000).ContinueWith(async _ =>
        {
            await token1.ReleaseAsync();
        });

        // 执行：等待获取锁（最多 5 秒）
        var startTime = DateTime.UtcNow;
        var token2 = await distributedLock.AcquireAsync(TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(100));
        var elapsed = DateTime.UtcNow - startTime;

        // 验证：成功获取锁（在等待期间锁被释放）
        Assert.IsNotNull(token2);
        Assert.IsTrue(elapsed.TotalSeconds >= 1.5); // 应该等待了约 2 秒

        // 清理
        await token2.ReleaseAsync();
    }

    #endregion

    #region 综合测试

    /// <summary>
    /// 测试 Redis 多个功能的综合使用场景
    /// </summary>
    [TestMethod]
    public async Task TestRedis_IntegratedScenario()
    {
        // 场景：模拟订单处理流程
        // 1. 将订单加入队列
        // 2. 获取分布式锁处理订单
        // 3. 处理完成后发布消息通知

        var queueTopic = "test:integrated:order_queue";
        var pubsubChannel = "test:integrated:order_notifications";
        var lockKey = "test:integrated:order_lock";

        // 步骤 1：将订单加入队列
        var queue = _redis.GetRedisQueue<TestInfo>(queueTopic);
        var order = new TestInfo { Name = "集成测试订单", Description = "订单描述", StartTime = DateTime.Now };
        await queue.EnqueueAsync(order);

        // 步骤 2：从队列取出订单
        var refKey = new RefValue<string>(string.Empty);
        var dequeuedOrder = await queue.DequeueAsync(refKey);
        Assert.IsNotNull(dequeuedOrder);

        // 步骤 3：获取分布式锁处理订单
        var distributedLock = _redis.GetDistributedLock(lockKey, 10000);
        await using var token = await distributedLock.AcquireAsync(TimeSpan.FromSeconds(5));
        Assert.IsNotNull(token);

        // 步骤 4：模拟订单处理
        await Task.Delay(100);

        // 步骤 5：发布处理完成通知
        var publisher = _redis.GetPublisher(pubsubChannel);
        await publisher.PublishAsync($"订单 {dequeuedOrder.Name} 处理完成");

        // 清理
        await queue.ClearAsync();
    }

    #endregion

    #region 性能优化版本测试

    /// <summary>
    /// 测试优化版本分布式锁（非重入模式）
    /// 使用 SET NX PX 单命令，性能更优
    /// </summary>
    [TestMethod]
    public async Task TestDistributedLockV3_NonReentrant()
    {
        // 准备
        var lockKey = "test:lock:v3:basic";

        // 执行：获取锁（默认使用优化版本）
        var distributedLock = _redis.GetDistributedLock(lockKey, 10000);
        var token = await distributedLock.AcquireAsync(TimeSpan.FromSeconds(5));

        // 验证：成功获取锁
        Assert.IsNotNull(token);
        Assert.AreEqual(lockKey, token.Key);

        // 执行：释放锁
        var released = await token.ReleaseAsync();
        Assert.IsTrue(released);
    }

    /// <summary>
    /// 测试优化版本分布式锁（可重入模式）
    /// 显式启用可重入支持
    /// </summary>
    [TestMethod]
    public async Task TestDistributedLockV3_Reentrant()
    {
        // 准备
        var lockKey = "test:lock:v3:reentrant";

        // 执行：获取锁（显式启用可重入）
        var distributedLock = _redis.GetDistributedLock(lockKey, 10000, reentrant: true);

        // 第一次获取
        var token1 = await distributedLock.AcquireAsync(TimeSpan.FromSeconds(1));
        Assert.IsNotNull(token1);

        // 可重入获取
        var token2 = await distributedLock.AcquireAsync(TimeSpan.FromSeconds(1));
        Assert.IsNotNull(token2);

        // 释放第二次
        await token2.ReleaseAsync();

        // 尝试获取（应失败，因为第一次锁还在）
        var token3 = await distributedLock.AcquireAsync(TimeSpan.FromMilliseconds(500));
        Assert.IsNull(token3);

        // 清理
        await token1.ReleaseAsync();
    }

    /// <summary>
    /// 测试批量获取多个锁
    /// </summary>
    [TestMethod]
    public async Task TestDistributedLock_MultipleAcquire()
    {
        // 准备
        var locks = new List<IDistributedLock>();
        for (int i = 0; i < 3; i++)
        {
            locks.Add(_redis.GetDistributedLock($"test:lock:multi:{i}", 10000));
        }

        // 执行：批量获取锁
        var tokens = await DistributedLockFactory.AcquireMultipleAsync(
            locks,
            TimeSpan.FromSeconds(5));

        // 验证：成功获取所有锁
        Assert.IsNotNull(tokens);
        Assert.AreEqual(3, tokens.Count);

        // 清理
        foreach (var token in tokens)
        {
            await token.ReleaseAsync();
        }
    }

    /// <summary>
    /// 性能基准测试：对比 V2（可重入）和 V3（非重入）的性能差异
    /// </summary>
    [TestMethod]
    public async Task Benchmark_LockPerformance()
    {
        var iterations = 100;
        var results = new Dictionary<string, long>();

        // 测试 V3 非重入模式（优化版本）
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var lockObj = _redis.GetDistributedLock($"bench:v3:{i}", 5000);
            await using var token = await lockObj.AcquireAsync();
            Assert.IsNotNull(token);
        }
        sw.Stop();
        results["V3-NonReentrant"] = sw.ElapsedMilliseconds;

        // 测试 V2 可重入模式
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var lockObj = _redis.GetDistributedLock($"bench:v2:{i}", 5000, reentrant: true);
            await using var token = await lockObj.AcquireAsync();
            Assert.IsNotNull(token);
        }
        sw.Stop();
        results["V2-Reentrant"] = sw.ElapsedMilliseconds;

        // 输出结果
        Console.WriteLine($"=== 分布式锁性能测试 ({iterations} 次迭代) ===");
        foreach (var kvp in results)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}ms (平均 {kvp.Value / iterations}ms/次)");
        }

        // 验证：V3 应该比 V2 快
        Assert.IsTrue(results["V3-NonReentrant"] <= results["V2-Reentrant"],
            "V3 非重入模式应该比 V2 可重入模式更快");
    }

    /// <summary>
    /// 高并发锁竞争测试
    /// </summary>
    [TestMethod]
    public async Task TestDistributedLock_HighConcurrency()
    {
        // 准备
        var lockKey = "test:lock:concurrency";
        var counter = 0;
        var tasks = new List<Task>();

        // 执行：100 个并发任务竞争同一把锁
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var lockObj = _redis.GetDistributedLock(lockKey, 100000);
                await using var token = await lockObj.AcquireAsync(TimeSpan.FromSeconds(100));
                Assert.IsNotNull(token);

                // 临界区：模拟业务操作
                var current = counter;
                await Task.Delay(10);
                counter = current + 1;
            }));
        }

        await Task.WhenAll(tasks);

        // 验证：计数器应该正确递增（无竞态条件）
        Assert.AreEqual(100, counter, "高并发下计数器应该正确递增");
    }

    #endregion
}
