[中文](README.md) | English

# LuBan.Threading

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **"No more concurrency headaches: locks, pools, queues — one line of code handles it all."**

Are you still struggling with these concurrency issues?

- Named locks are verbose and error-prone, leading to deadlocks?
- Thread pools either hand-crafted `Thread[]` or `Task.Run` everywhere, with uncontrolled resources?
- Producer-consumer queues still require manual `ManualResetEvent` setup?
- Task timeout, cancellation, and exception handling require boilerplate code every time?

If so, **LuBan.Threading** is built for you.

It's a lightweight multi-threading utility library for **.NET 8** with zero external dependencies, providing named locks, thread pools, task pools, blocking queues, Task extensions, and a complete concurrency solution. It's not a framework — it's just the handiest wrench in your toolbox.

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md)

---

## Why Choose LuBan.Threading?

| Pain Point | LuBan.Threading's Solution |
|---|---|
| Named lock implementation is complex and leak-prone | `LockerBuilder` named locks + `using` auto-release, goodbye deadlocks |
| Hand-written thread pools are hard to maintain | `SimpleThreadPool` fixed thread pool with built-in monitoring events |
| Async task pool can't control concurrency | `SimpleTaskPool` semaphore-based throttling, ordered async execution |
| Task timeout/cancellation patterns are tedious | `TimeoutAfterAsync`, `WithCancellationAsync` — one line does it all |
| Inter-thread communication queues are hard to write | `BlockingQueue<T>` thread-safe blocking queue, ready to use |
| Loop tasks / background polling patterns are messy | `ThreadUtil.ThreadWhile`, `TaskUtil.WhileAsync` clearly express intent |

---

## One-Minute Preview

```csharp
using System;
using LuBan.Threading;
using LuBan.Threading.Core;

// Named lock: lock by user ID, auto-release with using
using (await LockerBuilder.Default.GetLockerAsync("user:1001"))
{
    // Only one thread can enter at a time for the same user
    Console.WriteLine("Processing business logic for user 1001");
}

// Thread pool: fixed 4 threads, submit tasks and monitor
using var pool = new SimpleThreadPool("OrderPool", threadCount: 4);
pool.OnRunning += (s, e) => Console.WriteLine($"[Queue:{e.QueueCount} Running:{e.RunningCount} Success:{e.SuccessCount}]");
var taskId = pool.Enqueue(() => Console.WriteLine("Processing order..."));

// Task timeout control
var result = await TaskUtil.TimeoutAfterAsync(
    async ct => await FetchDataAsync(ct),
    timeout: TimeSpan.FromSeconds(5),
    cancellationToken: default);

// Blocking queue: producer-consumer
var queue = new BlockingQueue<string>();
queue.Enqueue("msg-001");
var msg = queue.Dequeue(maxTimeout: 3000);
```

> No fluff, just productivity.

---

## Tech Stack

- **Target Framework**: .NET 8.0
- **Project Type**: Class Library
- **NuGet Package**: `LuBan.Threading`
- **External Dependencies**: None (zero third-party NuGet dependencies)

---

## Installation

```bash
dotnet add package LuBan.Threading
```

---

## Toolbox Overview

### Thread Utilities — `ThreadUtil`

> Namespace: `System`

| Method | Description |
|---|---|
| `ThreadRun(Action)` | Execute synchronous delegate via thread pool |
| `ThreadRun(Func<Task?>)` | Execute async delegate via Task.Run |
| `ThreadWhile(Action, int)` | Background thread infinite loop with configurable interval |
| `ThreadWhile(Func<bool>, int)` | Background loop until returning true |
| `Sleep(int, CancellationToken)` | Cancellable thread sleep, auto-segmented for long sleeps |

### Task Extensions — `TaskUtil`

> Namespace: `System`

| Method | Description |
|---|---|
| `RunAsync(Action)` | Quick Action to Task conversion |
| `Catch(Action<Exception>)` | Task exception capture without breaking the pipeline |
| `Catch<T>(Func<Exception, T>)` | Exception fallback with return value |
| `LongRunning(Action)` | Independent long-running thread |
| `WhileAsync(Action, int)` | Background independent thread loop with configurable interval |
| `WhileAsync(Func<bool>, int)` | Loop until condition is no longer met |
| `Until(Action, int)` | Execute until condition is met |
| `Until(Func<bool>, int)` | Poll until returning true |
| `WithCancellationAsync<T>(Task<T>, CancellationToken)` | Attach cancellation capability to a Task |
| `WithCancellationTimeout<T>(Task<T>, TimeSpan, CancellationToken)` | Task timeout + cancellation |
| `TimeoutAfterAsync(Func<CancellationToken, Task>, TimeSpan, CancellationToken)` | Async operation with timeout, throws `TimeoutException` on expiry |
| `WaitForTasks(int, params Task<object>[])` | Wait for multiple Tasks with timeout support |
| `Delay(int, CancellationToken)` | Non-blocking delay, minimum 50ms |

### Named Locks — `LockerBuilder`

> Namespace: `LuBan.Threading`

| Method | Description |
|---|---|
| `GetLockerAsync(string, TimeSpan?, CancellationToken)` | Async acquire named lock |
| `GetLocker(string, TimeSpan?, CancellationToken)` | Sync acquire named lock |
| `Create(string)` / `Create()` | Quick lock acquisition (sync) |
| `CreateAsync(string)` / `CreateAsync()` | Quick lock acquisition (async) |
| `Default` | Static global singleton, use directly |

> `LockerReleaser` implements `IDisposable`, auto-releases locks with `using`, and cleans up the lock pool when reference count reaches zero.

### Thread Pool — `SimpleThreadPool`

> Namespace: `LuBan.Threading`

Fixed-size background thread pool based on `BlockingCollection` + `Thread[]`, suitable for scenarios with sensitive resource limits.

| Method | Description |
|---|---|
| `Enqueue(Action)` | Submit sync task, returns `Guid` task ID |
| `GetTaskStatus(Guid)` | Query task status (Pending / Running / Success / Failed) |
| `GetTaskInfo(Guid)` | Query detailed task info (including start/end time, exceptions) |
| `OnRunning` | Monitoring event, pushes pool status every 5 seconds (queue count, running count, success count, failure count) |

### Task Pool — `SimpleTaskPool`

> Namespace: `LuBan.Threading`

Async task pool based on `ConcurrentQueue` + `SemaphoreSlim`, suitable for most short-duration async tasks.

| Method | Description |
|---|---|
| `Enqueue(Func<Task>)` | Submit async task, returns `Guid` task ID |
| `GetTaskStatus(Guid)` | Query task status |
| `GetTaskInfo(Guid)` | Query detailed task info |
| `OnRunning` | Monitoring event, pushes pool status every 5 seconds |

### Blocking Queue — `BlockingQueue<T>`

> Namespace: `LuBan.Threading`

Thread-safe blocking queue based on `LinkedList<T>` + `ManualResetEvent`.

| Method | Description |
|---|---|
| `Enqueue(T)` | Enqueue |
| `Dequeue(int)` / `Dequeue(TimeSpan)` | Blocking dequeue with timeout support |
| `PeekAndWait(int)` | Peek at front element without removing |
| `RemoveFirst(Predicate<T>)` | Remove front element by condition |
| `FirstOrDefault(Predicate<T>)` | Find element by condition |
| `Clear()` | Clear the queue |
| `Count` / `IsEmpty` | Queue length / whether empty |

---

## Practical Examples

### Named Lock: Lock by Business Dimension

```csharp
using LuBan.Threading;
using LuBan.Threading.Core;

// Use global default instance
await using var locker = await LockerBuilder.Default.GetLockerAsync("order:10086");
try
{
    // Only one thread can execute at a time for the same order number
    Console.WriteLine("Processing order 10086...");
}
// locker auto-released when leaving using

// Synchronous scenario
using (var syncLocker = LockerBuilder.Default.Create("stock:SKU001"))
{
    // Deduct inventory
}

// Custom timeout
await using var timedLocker = await LockerBuilder.Default.GetLockerAsync(
    "payment:pay-001",
    timeout: TimeSpan.FromSeconds(5));
```

### Thread Pool: Fixed Threads for Background Tasks

```csharp
using LuBan.Threading;

using var pool = new SimpleThreadPool("NotifyPool", threadCount: 2);

pool.OnRunning += (sender, args) =>
{
    Console.WriteLine($"[{args.Title}] Queue:{args.QueueCount} Running:{args.RunningCount} Success:{args.SuccessCount} Failed:{args.FailCount}");
};

var id1 = pool.Enqueue(() => SendNotification("UserA"));
var id2 = pool.Enqueue(() => SendNotification("UserB"));

// Query task status
var status = pool.GetTaskStatus(id1);  // Pending / Running / Success / Failed
```

### Task Pool: Async Concurrency Throttling

```csharp
using LuBan.Threading;

using var taskPool = new SimpleTaskPool("HttpPool", maxDegreeOfParallelism: 5);

taskPool.OnRunning += (s, e) => Console.WriteLine($"Running: {e.RunningCount}");

foreach (var url in urls)
{
    taskPool.Enqueue(async () =>
    {
        var html = await httpClient.GetStringAsync(url);
        Console.WriteLine($"Fetched: {url}");
    });
}
```

### Task Timeout and Cancellation

```csharp
using System;

// Timeout control: throws TimeoutException if not completed within 5 seconds
try
{
    var data = await TaskUtil.TimeoutAfterAsync(
        async ct => await FetchRemoteDataAsync(ct),
        timeout: TimeSpan.FromSeconds(5),
        cancellationToken: default);
}
catch (TimeoutException)
{
    Console.WriteLine("Request timed out");
}

// Attach cancellation to existing Task
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
var result = await someTask.WithCancellationAsync(cts.Token);

// Exception fallback
var value = await SomeAsyncMethod().Catch(ex =>
{
    Console.WriteLine($"Error: {ex.Message}");
    return defaultValue;
});
```

### Background Polling

```csharp
using System;

// Poll every 1 second
await TaskUtil.WhileAsync(() =>
{
    CheckNewMessages();
}, priod: 1000);

// Poll until condition is met
await ((Func<bool>)(() =>
{
    return IsServiceReady();
})).Until(milliseconds: 500);
```

### Blocking Queue: Producer-Consumer

```csharp
using LuBan.Threading;

var queue = new BlockingQueue<WorkItem>();

// Producer
queue.Enqueue(new WorkItem { Id = 1, Data = "hello" });

// Consumer (blocking wait, max 10 seconds)
var item = queue.Dequeue(maxTimeout: 10_000);
if (item != null)
{
    Process(item);
}
```

---

## Usage Tips

1. **Don't mix up namespaces**: `ThreadUtil` and `TaskUtil` are in the `System` namespace; `LockerBuilder`, `SimpleThreadPool`, `SimpleTaskPool`, `BlockingQueue<T>` are in `LuBan.Threading`; `LockerReleaser` is in `LuBan.Threading.Core`.
2. **Prefer `LockerBuilder.Default`**: Global singleton, avoid creating duplicate lock pools.
3. **Release locks with `using`**: `LockerReleaser` implements `IDisposable`, always wrap with `using`, otherwise locks won't be released.
4. **Thread pool selection**: Use `SimpleThreadPool` for sync time-consuming tasks, `SimpleTaskPool` for async lightweight tasks.
5. **Blocking queue has timeout**: `Dequeue` defaults to 10 seconds wait, adjust as needed to avoid long thread blocking.
6. **Remember to Dispose pools**: `SimpleThreadPool`, `SimpleTaskPool`, `BlockingQueue<T>` all implement `IDisposable`, wrap with `using` to ensure resource cleanup.

---

## License

MIT License

---

**LuBan.Threading** — Making .NET concurrency programming simpler, safer, and more controllable.

> If you're tired of hand-writing thread synchronization, add it to your toolbox.
