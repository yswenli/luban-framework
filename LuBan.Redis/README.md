[English](README.en.md) | 中文
# LuBan.Redis

> **作者**: yswenli | **联系邮箱**: yswenli@outlook.com | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> 一站式 Redis 解决方案 —— 缓存、队列、发布订阅、分布式锁，一个 SDK 全搞定。

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## 为什么选择 LuBan.Redis？

在日常 .NET 开发中，Redis 的使用场景无处不在：缓存加速、消息队列、分布式锁、实时通知…… 但底层 API 过于繁琐，每次都要重复编写连接管理、序列化、重试逻辑。

**LuBan.Redis** 提供了开箱即用的高层抽象：
- 单例门面 `LuBanRedis`，一行代码完成初始化
- 内置分布式锁（可重入 / 非重入双模式，Lua 脚本原子操作，指数退避防惊群）
- List 队列 + Stream 消息流 + Pub/Sub 发布订阅，三种消息模型按需选用
- `RedisCache` 实现 `IServiceCache` 接口，无缝接入框架缓存体系
- 键过期监听器，轻松实现延迟任务

## 快速预览

```csharp
// 初始化（自动从 配置中心读取 RedisOptions）
var redis = LuBanRedis.Instance;

// 缓存：获取或自动填充
var user = await redis.GetFromCacheAsync<User>("user:1001", async () =>
{
    return await _userService.GetByIdAsync(1001);
}, timeout: 60000);

// 分布式锁（using 自动释放）
var lockObj = redis.GetDistributedLock("order:pay:12345", timeout: 30000);
await using var token = await lockObj.AcquireAsync(waitTimeout: TimeSpan.FromSeconds(5));
if (token != null)
{
    // 执行业务逻辑
}

// 发布订阅
var publisher = redis.GetPublisher("notification");
await publisher.PublishAsync("Hello World");

var subscriber = redis.GetSubscriber("notification");
subscriber.OnMessageReceived += (sub, msg) => Console.WriteLine(msg);
await subscriber.StartAsync();

// Stream 消息队列
var producer = redis.GetRedisProducer("task-stream");
await producer.PublishAsync(new { TaskId = 1, Name = "数据同步" });

var consumer = redis.GetRedisConsumer("task-stream", groupName: "worker-group");
var messages = await consumer.GetMessagesAsync<MyTask>(count: 10);
```

## 安装

```bash
dotnet add package LuBan.Redis
```

## 功能概览

| 功能模块 | 核心类 | 说明 |
|---------|--------|------|
| 快速初始化 | `LuBanRedis` | 单例门面，自动读取配置 |
| 连接构建 | `RedisClientBuilder` | 支持自定义多实例构建（带缓存） |
| 缓存 | `RedisCache` | 实现 `IServiceCache`，支持过期、前缀删除 |
| 分布式锁 | `RedisDistributedLockV3` | Lua 脚本原子操作，支持可重入/非重入 |
| 锁令牌 | `DistributedLockToken` | 支持 `using` / `await using` 自动释放 |
| List 队列 | `RedisQueue<T>` | 入队/出队/阻塞出队/队列状态管理 |
| Stream 生产者 | `RedisProducer` | 基于 Redis Stream 的消息发布 |
| Stream 消费者 | `RedisConsumer` | 消费组支持，ACK 确认机制 |
| Pub/Sub 发布 | `RedisPublisher` | 频道消息发布（字符串/对象） |
| Pub/Sub 订阅 | `RedisSubscriber` / `RedisSubscriber<T>` | 事件驱动消息订阅 |
| 键过期监听 | `RedisKeyExpireListener` | 自动配置 keyspace notifications |

## 详细用法

### 配置

```json
{
  "RedisOptions": {
    "Type": 0,
    "Masters": "127.0.0.1:6379",
    "Password": "your_password",
    "Slaves": "",
    "ServiceName": "mymaster",
    "DefaultDatabase": 0,
    "AllowAdmin": true,
    "KeepAlive": 180,
    "ConnectTimeout": 10000,
    "ConnectRetry": 1,
    "CommandTimeout": 60000
  }
}
```

### 分布式锁

```csharp
// 非重入锁（性能最优，SET NX PX 单命令）
var lockObj = redis.GetDistributedLock("resource-key", timeout: 60000, reentrant: false);
await using var token = await lockObj.AcquireAsync(
    waitTimeout: TimeSpan.FromSeconds(10),
    retryDelay: TimeSpan.FromMilliseconds(50));

if (token != null)
{
    // 续期
    await lockObj.RenewAsync(TimeSpan.FromSeconds(30));
}

// 可重入锁（Hash 结构，支持同一线程多次获取）
var reentrantLock = redis.GetDistributedLock("reentrant-key", reentrant: true, token: "my-thread-id");
```

### Stream 消息队列

```csharp
// 生产者
var producer = redis.GetRedisProducer("order-events", maxLength: 50000);
var msgId = await producer.PublishAsync(new { OrderId = 1001, Amount = 99.9m });

// 消费者（消费组模式）
var consumer = redis.GetRedisConsumer("order-events", groupName: "order-workers");
var messages = await consumer.GetMessagesAsync<OrderEvent>(count: 20);

if (messages != null)
{
    foreach (var msg in messages)
    {
        // 处理消息
        Console.WriteLine($"MessageId: {msg.Id}, Data: {msg.Data.OrderId}");
    }
    // ACK 确认
    await consumer.AcknowledgeAsync(messages.Select(m => m.Id).ToList());
}
```

### 键过期监听

```csharp
var listener = redis.GetKeyExpireListener();
listener.OnKeyExpired += (sender, key) =>
{
    Console.WriteLine($"Key expired: {key}");
};
await listener.StartAsync();
```

### RedisCache 缓存

```csharp
var cache = RedisCache.Instance;

// 设置缓存（带过期时间）
cache.Set("config:app", appConfig, TimeSpan.FromHours(1));

// 获取或自动填充
var config = cache.GetOrSet("config:app", key => LoadFromDb(), TimeSpan.FromHours(1));

// 前缀批量删除
var removed = cache.RemoveByPrefix("config:");
```

## 使用提示

- `LuBanRedis` 为单例模式，全局使用 `LuBanRedis.Instance` 即可，无需重复创建
- 分布式锁推荐使用 `await using` 语法，确保异常时也能正确释放
- 非重入锁性能优于可重入锁，无嵌套加锁需求时优先选择非重入模式
- Stream 消费组的 `groupName` 不能为空，否则退化为普通读取模式
- 键过期监听需要 Redis 服务端支持 `notify-keyspace-events` 配置（组件会自动设置）
- `RedisClientBuilder.Build()` 内部使用 `ConcurrentDictionary` 缓存连接，相同配置不会重复创建

## 许可证

Copyright (c) yswenli. All rights reserved.
