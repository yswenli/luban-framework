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
*描述：复用后台线程的线程池，适用于对资源使用上限敏感的任务
*
*=================================================
*修改标记
*修改时间：2025/5/21 10:22:56
*修改人： yswenli
*版本号： V1.0.0.0
*描述：复用后台线程的线程池，适用于对资源使用上限敏感的任务
*
*****************************************************************************/
namespace LuBan.Threading;


/// <summary>
/// 复用后台线程的线程池，适用于对资源使用上限敏感的任务
/// </summary>
public class SimpleThreadPool : ISimplePool, IDisposable
{
    private readonly BlockingCollection<PoolTaskInfo> _taskQueue;
    private readonly Thread[] _workers;
    private volatile bool _isDisposed = false;
    private readonly ConcurrentDictionary<Guid, PoolTaskInfo> _taskStatusDict = [];
    private readonly object _disposeLocker = new();
    private readonly string _name;
    private readonly Thread _monitorThread;

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get { return _name; } }

    /// <summary>
    /// 运行时事件
    /// </summary>
    public event EventHandler<TaskInfoArgs> OnRunning;

    /// <summary>
    /// 复用后台线程的线程池，适用于对资源使用上限敏感的任务
    /// </summary>
    /// <param name="name"></param>
    /// <param name="threadCount"></param>
    public SimpleThreadPool(string name, int threadCount)
    {
        _name = name ?? "ThreadPool";
        if (threadCount < 1) threadCount = 1;
        _taskQueue = new BlockingCollection<PoolTaskInfo>();
        _workers = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            _workers[i] = new Thread(Work)
            {
                IsBackground = true,
                Name = $"{_name}_Worker_{i}"
            };
            _workers[i].Start();
        }

        // 启动监控线程
        _monitorThread = new Thread(MonitorStatus)
        {
            IsBackground = true,
            Name = $"{_name}_Monitor"
        };
        _monitorThread.Start();
    }

    /// <summary>
    /// 提交任务，返回任务ID
    /// </summary>
    public Guid Enqueue(Action action)
    {
        if (_isDisposed) return Guid.Empty;
        if (action == null) throw new ArgumentNullException(nameof(action));
        lock (_disposeLocker)
        {
            var task = new PoolTaskInfo(action);
            _taskStatusDict[task.Id] = task;
            _taskQueue.Add(task);
            return task.Id;
        }
    }

    private void Work()
    {
        foreach (var task in _taskQueue.GetConsumingEnumerable())
        {
            task.Status = PoolTaskStatus.Running;
            task.StartTime = DateTime.Now;
            try
            {
                task.Action?.Invoke();
                task.Status = PoolTaskStatus.Success;
            }
            catch (Exception ex)
            {
                task.Status = PoolTaskStatus.Failed;
                task.Exception = ex;
                Console.WriteLine($"[ThreadPool] 任务执行异常: {ex}");
            }
            finally
            {
                task.EndTime = DateTime.Now;
            }
        }
    }

    private void MonitorStatus()
    {
        while (!_isDisposed)
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
                    Title = _name,
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
    public PoolTaskInfo? GetTaskInfo(Guid taskId)
    {
        _taskStatusDict.TryGetValue(taskId, out var task);
        return task;
    }

    /// <summary>
    /// 释放线程池
    /// </summary>
    public void Dispose()
    {
        lock (_disposeLocker)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                _taskQueue.CompleteAdding();
                foreach (var thread in _workers)
                {
                    thread.Join();
                }
                _taskQueue.Dispose();
                // 等待监控线程退出
                if (_monitorThread != null && _monitorThread.IsAlive)
                {
                    _monitorThread.Join();
                }
            }
        }
    }

    /// <summary>
    /// NotImplementedException
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Guid Enqueue(Func<Task> task)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// NotImplementedException
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    PoolTaskInfo2? ISimplePool.GetTaskInfo(Guid taskId)
    {
        throw new NotImplementedException();
    }
}
