/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Threading
*文件名： SimpleTaskPool
*版本号： V1.0.0.0
*唯一标识：112050a9-3609-4e2a-9b07-97c10c8f7b96
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/5/21 10:22:56
*描述：简单的Task池，适用于大部分耗时小的任务
*
*=================================================
*修改标记
*修改时间：2025/5/21 10:22:56
*修改人： yswenli
*版本号： V1.0.0.0
*描述：简单的Task池，适用于大部分耗时小的任务
*
*****************************************************************************/

namespace LuBan.Threading;

/// <summary>
/// 简单的Task池，适用于大部分耗时小的任务
/// </summary>
public class SimpleTaskPool : ISimplePool
{
    private readonly ConcurrentDictionary<Guid, PoolTaskInfo2> _taskStatusDict = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly CancellationTokenSource _cts = new();
    private volatile int _pendingCount = 0;
    private volatile bool _isRunning = true;
    private volatile bool _isDisposed = false;
    private Thread? _monitorThread;

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 运行时事件
    /// </summary>
    public event EventHandler<TaskInfoArgs>? OnRunning;

    /// <summary>
    /// 简单的Task池，适用于大部分耗时小的任务
    /// </summary>
    /// <param name="name"></param>
    /// <param name="maxDegreeOfParallelism"></param>
    public SimpleTaskPool(string name, int maxDegreeOfParallelism)
    {
        _semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);
        Name = name;

        _isRunning = true;
        _monitorThread = new Thread(MonitorStatus) { IsBackground = true };
        _monitorThread.Start();
    }

    /// <summary>
    /// 入队同步任务
    /// </summary>
    public Guid Enqueue(Action task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        return Enqueue(() => { task(); return Task.CompletedTask; });
    }

    /// <summary>
    /// 入队异步任务
    /// </summary>
    public Guid Enqueue(Func<Task> task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        if (!_isRunning) return Guid.Empty;
        var poolTask = new PoolTaskInfo2(task);
        _taskStatusDict[poolTask.Id] = poolTask;
        Interlocked.Increment(ref _pendingCount);
        _ = ProcessTaskAsync(poolTask);
        return poolTask.Id;
    }

    private async Task ProcessTaskAsync(PoolTaskInfo2 poolTask)
    {
        bool acquired = false;
        try
        {
            await _semaphore.WaitAsync(_cts.Token).ConfigureAwait(false);
            acquired = true;
            poolTask.Status = PoolTaskStatus.Running;
            poolTask.StartTime = DateTime.UtcNow;
            await poolTask.Func().ConfigureAwait(false);
            poolTask.Status = PoolTaskStatus.Success;
        }
        catch (OperationCanceledException) when (_cts.IsCancellationRequested)
        {
            poolTask.Status = PoolTaskStatus.Failed;
            poolTask.Exception = new TaskCanceledException("任务池已关闭");
        }
        catch (Exception ex)
        {
            poolTask.Status = PoolTaskStatus.Failed;
            poolTask.Exception = ex;
        }
        finally
        {
            poolTask.EndTime = DateTime.UtcNow;
            Interlocked.Decrement(ref _pendingCount);
            if (acquired)
            {
                try { _semaphore.Release(); }
                catch (SemaphoreFullException) { }
            }
        }
    }

    private void MonitorStatus()
    {
        while (_isRunning && !_cts.IsCancellationRequested)
        {
            try
            {
                Thread.Sleep(5000);

                int pending = _taskStatusDict.Values.Count(t => t.Status == PoolTaskStatus.Pending);
                int running = _taskStatusDict.Values.Count(t => t.Status == PoolTaskStatus.Running);
                int success = _taskStatusDict.Values.Count(t => t.Status == PoolTaskStatus.Success);
                int failed = _taskStatusDict.Values.Count(t => t.Status == PoolTaskStatus.Failed);
                int queueCount = _pendingCount;

                CleanupCompletedTasks();

                OnRunning?.Invoke(this, new TaskInfoArgs
                {
                    Title = Name,
                    QueueCount = queueCount,
                    PendingCount = pending,
                    RunningCount = running,
                    SuccessCount = success,
                    FailCount = failed
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MonitorStatus异常: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 已完成任务在状态字典中的保留时长，超过后清理，避免调用方尚未查询结果就被移除。
    /// </summary>
    private static readonly TimeSpan CompletedRetention = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 状态字典的容量上限，超出后强制清理最旧的已完成任务，防止高吞吐下内存持续增长。
    /// </summary>
    private const int MaxTrackedTasks = 10000;

    private void CleanupCompletedTasks()
    {
        var cutoff = DateTime.UtcNow - CompletedRetention;

        // 仅清理已超过保留期的终态任务，避免刚完成就被移除导致调用方查不到状态
        var completedIds = _taskStatusDict
            .Where(kvp => (kvp.Value.Status == PoolTaskStatus.Success || kvp.Value.Status == PoolTaskStatus.Failed)
                          && kvp.Value.EndTime.HasValue
                          && kvp.Value.EndTime.Value < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var id in completedIds)
        {
            _taskStatusDict.TryRemove(id, out _);
        }

        // 容量兜底：仍超上限时，按 EndTime 从旧到新强制清理终态任务
        if (_taskStatusDict.Count <= MaxTrackedTasks) return;

        var overflow = _taskStatusDict.Count - MaxTrackedTasks;
        var oldestIds = _taskStatusDict
            .Where(kvp => kvp.Value.Status == PoolTaskStatus.Success || kvp.Value.Status == PoolTaskStatus.Failed)
            .OrderBy(kvp => kvp.Value.EndTime ?? DateTime.MaxValue)
            .Take(overflow)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var id in oldestIds)
        {
            _taskStatusDict.TryRemove(id, out _);
        }
    }

    /// <summary>
    /// 查询任务状态
    /// </summary>
    public PoolTaskStatus? GetTaskStatus(Guid taskId)
    {
        if (_taskStatusDict.TryGetValue(taskId, out var task))
            return task.Status;
        return null;
    }

    /// <summary>
    /// 查询任务详细信息
    /// </summary>
    public PoolTaskInfo2? GetTaskInfo(Guid taskId)
    {
        _taskStatusDict.TryGetValue(taskId, out var task);
        return task;
    }

    /// <summary>
    /// 释放简单的Task池
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _isRunning = false;
        _cts.Cancel();
        _monitorThread?.Join();
        _semaphore.Dispose();
        _cts.Dispose();
    }
}
