/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common
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

using System.Threading.Channels;

namespace LuBan.Threading;

/// <summary>
/// 简单的Task池，适用于大部分耗时小的任务
/// </summary>
public class SimpleTaskPool : ISimplePool, IDisposable
{
    private readonly Channel<PoolTaskInfo2> _channel;
    private readonly ConcurrentDictionary<Guid, PoolTaskInfo2> _taskStatusDict = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly CancellationTokenSource _cts = new();
    private readonly int _maxDegreeOfParallelism;
    private volatile bool _isRunning;
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
        _maxDegreeOfParallelism = maxDegreeOfParallelism;
        _semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);
        _channel = Channel.CreateUnbounded<PoolTaskInfo2>();
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
        _channel.Writer.TryWrite(poolTask);
        _ = ProcessTaskAsync(poolTask);
        return poolTask.Id;
    }

    private async Task ProcessTaskAsync(PoolTaskInfo2 poolTask)
    {
        await _semaphore.WaitAsync(_cts.Token).ConfigureAwait(false);
        try
        {
            poolTask.Status = PoolTaskStatus.Running;
            poolTask.StartTime = DateTime.Now;
            await poolTask.Func().ConfigureAwait(false);
            poolTask.Status = PoolTaskStatus.Success;
        }
        catch (Exception ex)
        {
            poolTask.Status = PoolTaskStatus.Failed;
            poolTask.Exception = ex;
        }
        finally
        {
            poolTask.EndTime = DateTime.Now;
            _semaphore.Release();
        }
    }

    private void MonitorStatus()
    {
        while (_isRunning && !_cts.IsCancellationRequested)
        {
            try
            {
                Thread.Sleep(5000);
                CleanupCompletedTasks();

                int pending = _taskStatusDict.Values.Count(t => t.Status == PoolTaskStatus.Pending);
                int running = _taskStatusDict.Values.Count(t => t.Status == PoolTaskStatus.Running);
                int success = _taskStatusDict.Values.Count(t => t.Status == PoolTaskStatus.Success);
                int failed = _taskStatusDict.Values.Count(t => t.Status == PoolTaskStatus.Failed);
                int queueCount = _channel.Reader.Count;

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
            catch { }
        }
    }

    private void CleanupCompletedTasks()
    {
        var completedIds = _taskStatusDict
            .Where(kvp => kvp.Value.Status == PoolTaskStatus.Success || kvp.Value.Status == PoolTaskStatus.Failed)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var id in completedIds)
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
        _isRunning = false;
        _cts.Cancel();
        _channel.Writer.TryComplete();
        _monitorThread?.Join();
        _semaphore.Dispose();
        _cts.Dispose();
    }
}
