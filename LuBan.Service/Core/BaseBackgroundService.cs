/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.AspnetCore
*文件名： BaseBackgroundService
*版本号： V1.0.0.0
*唯一标识：b6c190aa-2b43-4d71-9ef7-8adc69d1ddef
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/10/11 16:29:36
*描述：LuBan.Framework 后台工作基类
*
*=================================================
*修改标记
*修改时间：2022/10/11 16:29:36
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBan.Framework 后台工作基类
*
*****************************************************************************/

namespace LuBan.Service.Core;

/// <summary>
/// LuBan.Framework 后台工作基类
/// </summary>
public abstract class BaseBackgroundService : BaseService, IJob
{
    private CancellationTokenSource _cancellationTokenSource;
    private ActionBlock<Func<Task>> _taskQueue;
    private readonly bool _sequentially;
    private readonly bool _userLog;

    /// <summary>
    /// 是否运行
    /// </summary>
    public bool IsRunning { get; private set; } = false;

    private volatile bool _gotoStop = false;

    private volatile bool _stoped = false;

    // 任务调度参数
    private int _intervalTime = 60 * 1000;
    private int _hour = 0;
    private int _minute = 0;
    private int _second = 0;
    private bool _once = false;
    private bool _isTimePointTask = false;

    /// <summary>
    /// LuBan.Framework 后台工作基类 构造函数：按间隔时间执行任务
    /// </summary>
    /// <param name="intervalTime">间隔时长(ms)，小于1的间隔表示只执行一次</param>
    /// <param name="sequentially">是否按顺序执行</param>
    /// <param name="userLog">启用日志</param>
    public BaseBackgroundService(int intervalTime = 60 * 1000, bool sequentially = true, bool userLog = false)
    {
        _sequentially = sequentially;
        _userLog = userLog;
        _intervalTime = intervalTime;
    }

    /// <summary>
    /// LuBan.Framework 后台工作基类 构造函数：按时间点执行任务
    /// </summary>
    /// <param name="hour">小时</param>
    /// <param name="minute">分钟</param>
    /// <param name="second">秒</param>
    /// <param name="once">是否只执行一次</param>
    /// <param name="sequentially">是否按顺序执行</param>
    /// <param name="userLog">启用日志</param>
    public BaseBackgroundService(int hour, int minute, int second, bool once = false, bool sequentially = true, bool userLog = false)
        : this(0, sequentially, userLog)
    {
        _isTimePointTask = true;
        _hour = hour;
        _minute = minute;
        _second = second;
        _once = once;
    }

    /// <summary>
    /// LuBan.Framework 后台工作基类 构造函数：按时间点字符串执行任务
    /// </summary>
    /// <param name="hourMinuteSeconds">时间点字符串（格式：HH:mm:ss）</param>
    /// <param name="once">是否只执行一次</param>
    /// <param name="sequentially">是否按顺序执行</param>
    /// <param name="userLog">启用日志</param>
    public BaseBackgroundService(string hourMinuteSeconds, bool once = false, bool sequentially = true, bool userLog = false)
        : this(0, sequentially, userLog)
    {
        if (hourMinuteSeconds.TryParseHourMiniteSecond(out int hour, out int minute, out int second))
        {
            _isTimePointTask = true;
            _hour = hour;
            _minute = minute;
            _second = second;
            _once = once;
        }
        else
        {
            throw new ArgumentException("时间格式不正确，应为 HH:mm:ss", nameof(hourMinuteSeconds));
        }
    }

    /// <summary>
    /// 启动服务
    /// </summary>
    public void Start()
    {
        if (IsRunning) return;

        // 重置状态
        IsRunning = true;
        _gotoStop = false;
        _stoped = false;

        // 初始化取消令牌源
        _cancellationTokenSource = new CancellationTokenSource();

        // 创建任务队列
        _taskQueue = new ActionBlock<Func<Task>>(async task =>
        {
            try
            {
                await task();
            }
            catch (Exception ex)
            {
                if (_userLog)
                {
                    Logger.Error(ex);
                }
            }
        }, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = _sequentially ? 1 : Environment.ProcessorCount,
            CancellationToken = _cancellationTokenSource.Token
        });

        // 启动任务调度
        if (_isTimePointTask)
        {
            ThreadUtil.ThreadRun(() => ScheduleAtTimeAsync(_hour, _minute, _second, _once, _cancellationTokenSource.Token));
        }
        else
        {
            ThreadUtil.ThreadRun(() => ScheduleTaskAsync(_intervalTime, _cancellationTokenSource.Token));
        }

        // 打印启动信息
        var attr = GetType().GetCustomAttribute<JobInfoAttribute>();
        if (attr != null)
        {
            ConsoleUtil.WriteLine($"[Background Job]{attr.Name}[{attr.Description}]正在启动中...", color: ConsoleColor.Green);
        }
        else
        {
            ConsoleUtil.WriteLine($"[Background Job]{GetType().Name}正在启动中...", color: ConsoleColor.Green);
        }
    }

    /// <summary>
    /// 停止服务
    /// </summary>
    public void Stop()
    {
        // 调用带有默认超时的Stop方法
        Stop(CancellationToken.None);
    }

    /// <summary>
    /// 停止服务（支持取消）
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    public void Stop(CancellationToken cancellationToken)
    {
        IsRunning = false;
        _cancellationTokenSource.Cancel();
        _gotoStop = true;

        // 释放任务队列资源
        if (_taskQueue != null)
        {
            _taskQueue.Complete();
            try
            {
                _taskQueue.Completion.GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                // 忽略等待异常
            }
        }

        // 等待任务停止，支持超时
        var waitTask = Task.Run(async () =>
        {
            while (!_stoped)
            {
                await Task.Delay(10, cancellationToken);
            }
        }, cancellationToken);

        using var timeoutCts = new CancellationTokenSource(5000);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        
        try
        {
            waitTask.Wait(linkedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            Logger.Warn($"[Background Job]{GetType().Name}停止超时");
        }
        catch (OperationCanceledException)
        {
            // 用户取消，不记录警告
        }

        // 释放取消令牌资源
        try
        {
            _cancellationTokenSource.Dispose();
        }
        catch (Exception)
        {
            // 忽略释放异常
        }

        var attr = GetType().GetCustomAttribute<JobInfoAttribute>();
        if (attr != null)
        {
            ConsoleUtil.WriteLine($"[Background Job]{attr.Name}[{attr.Description}]已停止", color: ConsoleColor.Yellow);
        }
        else
        {
            ConsoleUtil.WriteLine($"[Background Job]{GetType().Name}已停止", color: ConsoleColor.Yellow);
        }

    }

    #region 需要自定义的业务方法

    /// <summary>
    /// 自定义业务逻辑
    /// </summary>
    public virtual void Run()
    {
        return;
    }

    /// <summary>
    /// 异步自定义业务逻辑
    /// </summary>
    /// <returns></returns>
    public virtual Task RunAsync()
    {
        return Task.CompletedTask;
    }

    #endregion



    /// <summary>
    /// 获取作业名称
    /// </summary>
    /// <returns></returns>
    private string GetJobName()
    {
        var attr = GetType().GetCustomAttribute<JobInfoAttribute>();
        return attr?.Name ?? GetType().Name;
    }

    /// <summary>
    /// 带日志记录的RunAsync方法执行
    /// </summary>
    private async Task RunAsyncWithLog()
    {
        var jobName = GetJobName();
        long logId = 0;

        try
        {
            // 记录作业开始
            logId = JobLogService.Instance.LogJobStart(jobName);

            // 记录运行次数
            JobInfosCache.Instance.RecordRun(jobName);

            // 执行实际的作业逻辑
            await RunAsync();

            // 执行实际的作业逻辑
            Run();

            // 记录作业成功
            JobLogService.Instance.LogJobSuccess(logId, "作业执行成功");
        }
        catch (Exception ex)
        {
            // 记录作业失败
            JobLogService.Instance.LogJobFailed(logId, ex.ToString());

            // 记录错误次数和最后一次错误信息
            JobInfosCache.Instance.RecordError(jobName, ex.ToString());

            if (_userLog)
            {
                Logger.Error(ex);
            }
        }
    }

    /// <summary>
    /// 按间隔时间调度任务
    /// </summary>
    /// <param name="intervalTime">小于1表示只执一次</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task ScheduleTaskAsync(int intervalTime, CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();
        try
        {
            while (!cancellationToken.IsCancellationRequested && !_gotoStop)
            {
                if (intervalTime > 100)
                {
                    var elapsed = stopWatch.ElapsedMilliseconds;
                    if (elapsed < intervalTime)
                    {
                        // 计算剩余时间，直接等待剩余时间，避免频繁检查
                        var remaining = intervalTime - elapsed;
                        await TaskUtil.Delay((int)remaining, cancellationToken);
                        continue;
                    }
                    stopWatch.Restart();
                }
                else
                {
                    // 对于没有间隔时间的任务，使用更长的延迟
                    await TaskUtil.Delay(100, cancellationToken);
                }
                if (IsRunning)
                {
                    await _taskQueue.SendAsync(RunAsyncWithLog, cancellationToken);
                    if (intervalTime < 1) break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 捕获取消异常，不做处理
        }
        finally
        {
            _stoped = true;
        }
    }

    /// <summary>
    /// 按时间点调度任务
    /// </summary>
    private async Task ScheduleAtTimeAsync(int hour, int minute, int second, bool once, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && !_gotoStop)
            {
                var now = DateTime.Now;
                var targetTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, second);

                if (targetTime <= now)
                {
                    targetTime = targetTime.AddDays(1);
                }
                var delay = targetTime - now;

                // 直接等待到目标时间附近，避免频繁检查
                if (delay.TotalMilliseconds > 5000) // 如果大于5秒，先等待较长时间
                {
                    await TaskUtil.Delay(5000, cancellationToken);
                    continue;
                }
                else if (delay.TotalMilliseconds > 0) // 如果小于等于5秒，等待剩余时间
                {
                    await TaskUtil.Delay((int)delay.TotalMilliseconds, cancellationToken);
                }

                if (IsRunning)
                {
                    // 只执行异步方法，避免重复执行导致CPU占用过高
                    await _taskQueue.SendAsync(RunAsyncWithLog, cancellationToken);
                    if (once) break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 捕获取消异常，不做处理
        }
        finally
        {
            _stoped = true;
        }
    }
}
