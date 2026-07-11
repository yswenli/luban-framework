[English](README.en.md) | 中文

# LuBan.Threading

> **作者**: yswenli | **联系邮箱**: yswenli@outlook.com | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **"多线程不再翻车：锁、池、队列，一行代码搞定并发。"**

你是否还在为这些并发问题头疼？

- 命名锁写起来又臭又长，一不小心就死锁？
- 线程池要么手写 `Thread[]`，要么 `Task.Run` 满天飞，资源失控？
- 生产者-消费者队列还要自己搞 `ManualResetEvent`？
- Task 超时、取消、异常捕获，每次都要写一堆样板代码？

如果是，那 **LuBan.Threading** 就是为你准备的。

它是 **.NET 8** 下的轻量级多线程工具库，零外部依赖，提供命名锁、线程池、任务池、阻塞队列、Task 扩展等一套完整的并发解决方案。不造框架，只做你手里最顺手的那把扳手。

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md)

---

## 为什么选择 LuBan.Threading？

| 痛点 | LuBan.Threading 的解法 |
|---|---|
| 命名锁实现复杂，容易泄漏 | `LockerBuilder` 命名锁 + `using` 自动释放，告别死锁 |
| 线程池手写难维护 | `SimpleThreadPool` 固定线程池，自带监控事件 |
| 异步任务池不好控并发 | `SimpleTaskPool` 信号量限流，异步任务有序执行 |
| Task 超时/取消写法繁琐 | `TimeoutAfterAsync`、`WithCancellationAsync` 一行搞定 |
| 线程间通信队列难写 | `BlockingQueue<T>` 线程安全阻塞队列，开箱即用 |
| 循环任务/后台轮询写法乱 | `ThreadUtil.ThreadWhile`、`TaskUtil.WhileAsync` 清晰表达意图 |

---

## 一分钟预览

```csharp
using System;
using LuBan.Threading;
using LuBan.Threading.Core;

// 命名锁：按用户ID加锁，using 自动释放
using (await LockerBuilder.Default.GetLockerAsync("user:1001"))
{
    // 同一用户同一时刻只有一个线程能进来
    Console.WriteLine("处理用户 1001 的业务逻辑");
}

// 线程池：固定 4 线程，提交任务并监控
using var pool = new SimpleThreadPool("OrderPool", threadCount: 4);
pool.OnRunning += (s, e) => Console.WriteLine($"[队列:{e.QueeueCount} 运行:{e.RunningCount} 成功:{e.SuccessCount}]");
var taskId = pool.Enqueue(() => Console.WriteLine("处理订单中..."));

// Task 超时控制
var result = await TaskUtil.TimeoutAfterAsync(
    async ct => await FetchDataAsync(ct),
    timeout: TimeSpan.FromSeconds(5),
    cancellationToken: default);

// 阻塞队列：生产者-消费者
var queue = new BlockingQueue<string>();
queue.Enqueue("msg-001");
var msg = queue.Dequeue(maxTimeout: 3000);
```

> 没有废话，只有生产力。

---

## 技术栈

- **目标框架**：.NET 8.0
- **项目类型**：类库（Class Library）
- **NuGet 包**：`LuBan.Threading`
- **外部依赖**：无（零第三方 NuGet 依赖）

---

## 安装

```bash
dotnet add package LuBan.Threading
```

---

## 工具箱全览

### 线程工具 — `ThreadUtil`

> 命名空间：`System`

| 方法 | 说明 |
|---|---|
| `ThreadRun(Action)` | 线程池执行同步委托 |
| `ThreadRun(Func<Task?>)` | Task.Run 执行异步委托 |
| `ThreadWhile(Action, int)` | 后台线程无限循环执行，可设间隔 |
| `ThreadWhile(Func<bool>, int)` | 后台循环直到返回 true 时退出 |
| `Sleep(int, CancellationToken)` | 支持取消的线程睡眠，长睡眠自动分段唤醒 |

### Task 扩展 — `TaskUtil`

> 命名空间：`System`

| 方法 | 说明 |
|---|---|
| `RunAsync(Action)` | Action 快速转 Task |
| `Catch(Action<Exception>)` | Task 异常捕获，不中断管道 |
| `Catch<T>(Func<Exception, T>)` | 带返回值的异常降级 |
| `LongRunning(Action)` | 独立长时间运行线程 |
| `WhileAsync(Action, int)` | 后台独立线程循环执行，可设间隔 |
| `WhileAsync(Func<bool>, int)` | 循环直到条件不满足 |
| `Until(Action, int)` | 持续执行直到条件满足 |
| `Until(Func<bool>, int)` | 轮询直到返回 true |
| `WithCancellationAsync<T>(Task<T>, CancellationToken)` | 为 Task 附加取消能力 |
| `WithCancellationTimeout<T>(Task<T>, TimeSpan, CancellationToken)` | Task 超时 + 取消 |
| `TimeoutAfterAsync(Func<CancellationToken, Task>, TimeSpan, CancellationToken)` | 带超时的异步操作，超时抛 `TimeoutException` |
| `WaitForTasks(int, params Task<object>[])` | 等待多个 Task，支持超时 |
| `Delay(int, CancellationToken)` | 非阻塞延迟，最小 50ms |

### 命名锁 — `LockerBuilder`

> 命名空间：`LuBan.Threading`

| 方法 | 说明 |
|---|---|
| `GetLockerAsync(string, TimeSpan?, CancellationToken)` | 异步获取命名锁 |
| `GetLocker(string, TimeSpan?, CancellationToken)` | 同步获取命名锁 |
| `Create(string)` / `Create()` | 快捷获取锁（同步） |
| `CreateAsync(string)` / `CreateAsync()` | 快捷获取锁（异步） |
| `Default` | 静态全局单例，直接用 |

> `LockerReleaser` 实现 `IDisposable`，配合 `using` 自动释放锁，引用计数归零时自动清理锁池。

### 线程池 — `SimpleThreadPool`

> 命名空间：`LuBan.Threading`

固定大小的后台线程池，基于 `BlockingCollection` + `Thread[]`，适用于对资源上限敏感的场景。

| 方法 | 说明 |
|---|---|
| `Enqueue(Action)` | 提交同步任务，返回 `Guid` 任务ID |
| `GetTaskStatus(Guid)` | 查询任务状态（Pending / Running / Success / Failed） |
| `GetTaskInfo(Guid)` | 查询任务详细信息（含开始/结束时间、异常） |
| `OnRunning` | 监控事件，每 5 秒推送池状态（队列数、运行数、成功数、失败数） |

### 任务池 — `SimpleTaskPool`

> 命名空间：`LuBan.Threading`

基于 `ConcurrentQueue` + `SemaphoreSlim` 的异步任务池，适用于大部分耗时小的异步任务。

| 方法 | 说明 |
|---|---|
| `Enqueue(Func<Task>)` | 提交异步任务，返回 `Guid` 任务ID |
| `GetTaskStatus(Guid)` | 查询任务状态 |
| `GetTaskInfo(Guid)` | 查询任务详细信息 |
| `OnRunning` | 监控事件，每 5 秒推送池状态 |

### 阻塞队列 — `BlockingQueue<T>`

> 命名空间：`LuBan.Threading`

基于 `LinkedList<T>` + `ManualResetEvent` 的线程安全阻塞队列。

| 方法 | 说明 |
|---|---|
| `Enqueue(T)` | 入队 |
| `Dequeue(int)` / `Dequeue(TimeSpan)` | 阻塞出队，支持超时 |
| `PeekAndWait(int)` | 查看队首元素但不移除 |
| `RemoveFirst(Predicate<T>)` | 按条件移除队首元素 |
| `FirstOrDefault(Predicate<T>)` | 按条件查找元素 |
| `Clear()` | 清空队列 |
| `Count` / `IsEmpty` | 队列长度 / 是否为空 |

---

## 实战示例

### 命名锁：按业务维度加锁

```csharp
using LuBan.Threading;
using LuBan.Threading.Core;

// 使用全局默认实例
await using var locker = await LockerBuilder.Default.GetLockerAsync("order:10086");
try
{
    // 同一订单号同一时刻只有一个线程能执行
    Console.WriteLine("处理订单 10086...");
}
// locker 离开 using 自动释放

// 同步场景
using (var syncLocker = LockerBuilder.Default.Create("stock:SKU001"))
{
    // 扣减库存
}

// 自定义超时
await using var timedLocker = await LockerBuilder.Default.GetLockerAsync(
    "payment:pay-001",
    timeout: TimeSpan.FromSeconds(5));
```

### 线程池：固定线程处理后台任务

```csharp
using LuBan.Threading;

using var pool = new SimpleThreadPool("NotifyPool", threadCount: 2);

pool.OnRunning += (sender, args) =>
{
    Console.WriteLine($"[{args.Title}] 队列:{args.QueeueCount} 运行:{args.RunningCount} 成功:{args.SuccessCount} 失败:{args.FailCount}");
};

var id1 = pool.Enqueue(() => SendNotification("用户A"));
var id2 = pool.Enqueue(() => SendNotification("用户B"));

// 查询任务状态
var status = pool.GetTaskStatus(id1);  // Pending / Running / Success / Failed
```

### 任务池：异步并发限流

```csharp
using LuBan.Threading;

using var taskPool = new SimpleTaskPool("HttpPool", maxDegreeOfParallelism: 5);

taskPool.OnRunning += (s, e) => Console.WriteLine($"运行中: {e.RunningCount}");

foreach (var url in urls)
{
    taskPool.Enqueue(async () =>
    {
        var html = await httpClient.GetStringAsync(url);
        Console.WriteLine($"已获取: {url}");
    });
}
```

### Task 超时与取消

```csharp
using System;

// 超时控制：5秒内未完成则抛 TimeoutException
try
{
    var data = await TaskUtil.TimeoutAfterAsync(
        async ct => await FetchRemoteDataAsync(ct),
        timeout: TimeSpan.FromSeconds(5),
        cancellationToken: default);
}
catch (TimeoutException)
{
    Console.WriteLine("请求超时");
}

// 为已有 Task 附加取消
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
var result = await someTask.WithCancellationAsync(cts.Token);

// 异常降级
var value = await SomeAsyncMethod().Catch(ex =>
{
    Console.WriteLine($"出错了: {ex.Message}");
    return defaultValue;
});
```

### 后台轮询

```csharp
using System;

// 每隔 1 秒轮询一次
await TaskUtil.WhileAsync(() =>
{
    CheckNewMessages();
}, priod: 1000);

// 轮询直到条件满足
await ((Func<bool>)(() =>
{
    return IsServiceReady();
})).Until(milliseconds: 500);
```

### 阻塞队列：生产者-消费者

```csharp
using LuBan.Threading;

var queue = new BlockingQueue<WorkItem>();

// 生产者
queue.Enqueue(new WorkItem { Id = 1, Data = "hello" });

// 消费者（阻塞等待，最多等 10 秒）
var item = queue.Dequeue(maxTimeout: 10_000);
if (item != null)
{
    Process(item);
}
```

---

## 使用小贴士

1. **命名空间别搞错**：`ThreadUtil` 和 `TaskUtil` 在 `System` 命名空间；`LockerBuilder`、`SimpleThreadPool`、`SimpleTaskPool`、`BlockingQueue<T>` 在 `LuBan.Threading`；`LockerReleaser` 在 `LuBan.Threading.Core`。
2. **优先用 `LockerBuilder.Default`**：全局单例，避免重复创建锁池。
3. **`using` 释放锁**：`LockerReleaser` 实现了 `IDisposable`，务必用 `using` 包裹，否则锁不会释放。
4. **线程池选型**：同步耗时任务用 `SimpleThreadPool`，异步轻量任务用 `SimpleTaskPool`。
5. **阻塞队列有超时**：`Dequeue` 默认等 10 秒，按需调整避免线程长时间阻塞。
6. **池记得 Dispose**：`SimpleThreadPool`、`SimpleTaskPool`、`BlockingQueue<T>` 都实现了 `IDisposable`，用 `using` 包裹确保资源释放。

---

## 许可证

MIT License

---

**LuBan.Threading** —— 让 .NET 并发编程更简单、更安全、更可控。

> 如果你也受够了手写线程同步的痛苦，那就把它加进你的工具箱吧。
