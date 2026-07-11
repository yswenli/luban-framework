[中文](README.md) | English
# LuBan.Redis

> **Author**: yswenli | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> A one-stop Redis solution — caching, queues, pub/sub, distributed locks, all in a single SDK.

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## Why LuBan.Redis?

In everyday .NET development, Redis is everywhere: caching, message queues, distributed locks, real-time notifications... But the low-level API is too cumbersome — you end up rewriting connection management, serialization, and retry logic every time.

**LuBan.Redis** provides ready-to-use high-level abstractions:
- Singleton facade `LuBanRedis` — initialize in one line of code
- Built-in distributed locks (reentrant / non-reentrant dual modes, Lua script atomic operations, exponential backoff to prevent thundering herd)
- List queues + Stream messaging + Pub/Sub — three messaging models to choose from
- `RedisCache` implements the `IServiceCache` interface, seamlessly integrating with the framework caching system
- Key expiry listener for easy delayed task implementation

## Quick Preview

```csharp
// Initialize (automatically reads RedisOptions from Nacos config center)
var redis = LuBanRedis.Instance;

// Cache: get or auto-populate
var user = await redis.GetFromCacheAsync<User>("user:1001", async () =>
{
    return await _userService.GetByIdAsync(1001);
}, timeout: 60000);

// Distributed lock (automatically released with using)
var lockObj = redis.GetDistributedLock("order:pay:12345", timeout: 30000);
await using var token = await lockObj.AcquireAsync(waitTimeout: TimeSpan.FromSeconds(5));
if (token != null)
{
    // Execute business logic
}

// Pub/Sub
var publisher = redis.GetPublisher("notification");
await publisher.PublishAsync("Hello World");

var subscriber = redis.GetSubscriber("notification");
subscriber.OnMessageReceived += (sub, msg) => Console.WriteLine(msg);
await subscriber.StartAsync();

// Stream message queue
var producer = redis.GetRedisProducer("task-stream");
await producer.PublishAsync(new { TaskId = 1, Name = "Data Sync" });

var consumer = redis.GetRedisConsumer("task-stream", groupName: "worker-group");
var messages = await consumer.GetMessagesAsync<MyTask>(count: 10);
```

## Installation

```bash
dotnet add package LuBan.Redis
```

## Feature Overview

| Module | Core Class | Description |
|--------|------------|-------------|
| Quick Init | `LuBanRedis` | Singleton facade, auto-reads configuration |
| Connection Builder | `RedisClientBuilder` | Supports custom multi-instance building (with caching) |
| Cache | `RedisCache` | Implements `IServiceCache`, supports expiry and prefix deletion |
| Distributed Lock | `RedisDistributedLockV3` | Lua script atomic operations, supports reentrant/non-reentrant |
| Lock Token | `DistributedLockToken` | Supports `using` / `await using` for auto-release |
| List Queue | `RedisQueue<T>` | Enqueue/dequeue/blocking dequeue/queue state management |
| Stream Producer | `RedisProducer` | Message publishing based on Redis Stream |
| Stream Consumer | `RedisConsumer` | Consumer group support, ACK mechanism |
| Pub/Sub Publisher | `RedisPublisher` | Channel message publishing (string/object) |
| Pub/Sub Subscriber | `RedisSubscriber` / `RedisSubscriber<T>` | Event-driven message subscription |
| Key Expiry Listener | `RedisKeyExpireListener` | Auto-configures keyspace notifications |

## Detailed Usage

### Configuration

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

### Distributed Lock

```csharp
// Non-reentrant lock (best performance, single SET NX PX command)
var lockObj = redis.GetDistributedLock("resource-key", timeout: 60000, reentrant: false);
await using var token = await lockObj.AcquireAsync(
    waitTimeout: TimeSpan.FromSeconds(10),
    retryDelay: TimeSpan.FromMilliseconds(50));

if (token != null)
{
    // Renew lock
    await lockObj.RenewAsync(TimeSpan.FromSeconds(30));
}

// Reentrant lock (Hash structure, supports multiple acquisitions by the same thread)
var reentrantLock = redis.GetDistributedLock("reentrant-key", reentrant: true, token: "my-thread-id");
```

### Stream Message Queue

```csharp
// Producer
var producer = redis.GetRedisProducer("order-events", maxLength: 50000);
var msgId = await producer.PublishAsync(new { OrderId = 1001, Amount = 99.9m });

// Consumer (consumer group mode)
var consumer = redis.GetRedisConsumer("order-events", groupName: "order-workers");
var messages = await consumer.GetMessagesAsync<OrderEvent>(count: 20);

if (messages != null)
{
    foreach (var msg in messages)
    {
        // Process message
        Console.WriteLine($"MessageId: {msg.Id}, Data: {msg.Data.OrderId}");
    }
    // ACK confirmation
    await consumer.AcknowledgeAsync(messages.Select(m => m.Id).ToList());
}
```

### Key Expiry Listener

```csharp
var listener = redis.GetKeyExpireListener();
listener.OnKeyExpired += (sender, key) =>
{
    Console.WriteLine($"Key expired: {key}");
};
await listener.StartAsync();
```

### RedisCache

```csharp
var cache = RedisCache.Instance;

// Set cache (with expiry)
cache.Set("config:app", appConfig, TimeSpan.FromHours(1));

// Get or auto-populate
var config = cache.GetOrSet("config:app", key => LoadFromDb(), TimeSpan.FromHours(1));

// Batch delete by prefix
var removed = cache.RemoveByPrefix("config:");
```

## Tips

- `LuBanRedis` is a singleton — use `LuBanRedis.Instance` globally, no need to create multiple instances
- Use `await using` syntax for distributed locks to ensure proper release even on exceptions
- Non-reentrant locks outperform reentrant locks — prefer non-reentrant mode when there's no nested locking requirement
- Stream consumer group `groupName` cannot be empty, otherwise it degrades to normal read mode
- Key expiry listener requires Redis server support for `notify-keyspace-events` configuration (the component sets this automatically)
- `RedisClientBuilder.Build()` uses `ConcurrentDictionary` internally to cache connections — identical configurations won't create duplicate connections

## License

Copyright (c) yswenli. All rights reserved.
