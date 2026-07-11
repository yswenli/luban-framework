/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common
*文件名： SimpleThreadPool
*版本号： V1.0.0.0
*唯一标识：112050a9-3609-4e2a-9b07-97c10c8f7b96
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/5/21 10:22:56
*描述：简单的Task池，实用于大部分耗时小的任务
*
*=================================================
*修改标记
*修改时间：2025/5/21 10:22:56
*修改人： yswenli
*版本号： V1.0.0.0
*描述：简单的Task池，实用于大部分耗时小的任务
*
*****************************************************************************/

namespace LuBan.Threading;

/// <summary>
/// 简单的Task池，实用于大部分耗时小的任务
/// </summary>
public class SimpleTaskPool : ISimplePool, IDisposable
{
    private readonly ConcurrentQueue<PoolTaskInfo2> _taskQueue = new();
    private readonly ConcurrentDictionary<Guid, PoolTaskInfo2> _taskStatusDict = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly CancellationTokenSource _cts = new();
    private readonly int _maxDegreeOfParallelism;
    private bool _isRunning;
    private Thread? _monitorThread;
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; }


    /// <summary>
    /// 运行时事件
    /// </summary>
    public event EventHandler<TaskInfoArgs> OnRunning;

    /// <summary>
    /// 简单的Task池，实用于大部分耗时小的任务
    /// </summary>
    /// <param name="name"></param>
    /// <param name="maxDegreeOfParallelism"></param>
    public SimpleTaskPool(string name, int maxDegreeOfParallelism)
    {
        _maxDegreeOfParallelism = maxDegreeOfParallelism;
        _semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);
        Name = name;

        _isRunning = true;
        for (int i = 0; i < _maxDegreeOfParallelism; i++)
        {
            _ = ProcessQueueAsync();
        }
        _monitorThread = new Thread(MonitorStatus) { IsBackground = true };
        _monitorThread.Start();
    }

    /// <summary>
    /// 入队
    /// </summary>
    public Guid Enqueue(Func<Task> task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        if (_isRunning == false) return Guid.Empty;
        var poolTask = new PoolTaskInfo2(task);
        _taskQueue.Enqueue(poolTask);
        _taskStatusDict[poolTask.Id] = poolTask;
        if (_isRunning)
        {
            _ = ProcessQueueAsync();
        }
        return poolTask.Id;
    }

    private async Task ProcessQueueAsync()
    {
        while (_isRunning && !_cts.IsCancellationRequested)
        {
            if (_taskQueue.TryDequeue(out var poolTask))
            {
                await _semaphore.WaitAsync(_cts.Token);
                poolTask.Status = PoolTaskStatus.Running;
                poolTask.StartTime = DateTime.Now;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await poolTask.Func();
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
                }, _cts.Token);
            }
            else
            {
                await TaskUtil.Delay(50, _cts.Token);
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
                int queueCount = _taskQueue.Count;

                OnRunning?.Invoke(this, new TaskInfoArgs
                {
                    Title = Name,
                    QueeueCount = queueCount,
                    PendingCount = pending,
                    RunningCount = running,
                    SuccessCount = success,
                    FailCount = failed
                });
            }
            catch { }
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
        _monitorThread?.Join();
        _semaphore.Dispose();
        _cts.Dispose();
    }

    /// <summary>
    /// NotImplementedException
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Guid Enqueue(Action task)
    {
        throw new NotImplementedException();
    }
}
