/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： TaskUtil
*版本号： V1.0.0.0
*唯一标识：3b303699-75e3-40b5-a980-2e38966e9c7d
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/14 10:55:41
*描述：Task 工具类
*
*=====================================================================
*修改标记
*修改时间：2022/7/14 10:55:41
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：Task 工具类
*
*****************************************************************************/

namespace LuBan.Threading;

/// <summary>
/// Task 工具类
/// </summary>
public static class TaskUtil
{
    /// <summary>
    /// Run
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static Task RunAsync(this Action action)
    {
        return Task.Run(action);
    }

    /// <summary>
    /// task catch
    /// </summary>
    /// <param name="task"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static async Task Catch(this Task task, Action<Exception> action)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            action.Invoke(ex);
            throw;
        }
    }
    /// <summary>
    /// task catch
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <param name="fun"></param>
    /// <returns></returns>
    public static async Task<T?> Catch<T>(this Task<T> task, Func<Exception, T> fun)
    {
        try
        {
            return await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return fun.Invoke(ex);
        }
    }

    /// <summary>
    /// LongRunning
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static Task LongRunning(this Action action)
    {
        return Task.Factory.StartNew(action, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// 单独线程循环执行
    /// </summary>
    /// <param name="action"></param>
    /// <param name="period"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task WhileAsync(this Action action, int period = 0, CancellationToken cancellationToken = default)
    {
        const int minSleepTime = 100;

        return LongRunning(() =>
        {
            var nextRunTime = Environment.TickCount64;
            while (!cancellationToken.IsCancellationRequested)
            {
                if (period > 0)
                {
                    ThreadUtil.Sleep(minSleepTime, cancellationToken);
                    if (Environment.TickCount64 < nextRunTime) continue;
                    nextRunTime = Environment.TickCount64 + period;
                }
                action?.Invoke();
            }
        });
    }


    /// <summary>
    /// 单独线程循环执行
    /// </summary>
    /// <param name="fun">根据返回值决定是否继续，true表示继续</param>
    /// <param name="period">0表示不间隔</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task WhileAsync(this Func<bool> fun, int period, CancellationToken cancellationToken = default)
    {
        const int minSleepTime = 100;

        return LongRunning(() =>
        {
            var nextRunTime = Environment.TickCount64;
            while (!cancellationToken.IsCancellationRequested)
            {
                if (period > 0)
                {
                    ThreadUtil.Sleep(minSleepTime, cancellationToken);
                    if (Environment.TickCount64 < nextRunTime) continue;
                    nextRunTime = Environment.TickCount64 + period;
                }
                var result = fun?.Invoke();
                if (result != true) return;
            }
        });
    }

    /// <summary>
    /// 指定时间内重复执行操作
    /// </summary>
    /// <param name="action"></param>
    /// <param name="period"></param>
    /// <param name="expired"></param>
    public static void While(this Action action, int period = 1000, int expired = 60 * 1000)
    {
        var endTime = Environment.TickCount64 + expired;
        while (Environment.TickCount64 < endTime)
        {
            action?.Invoke();
            ThreadUtil.Sleep(period);
        }
    }
    /// <summary>
    /// 指定时间内达到指定条件重复执行操作
    /// </summary>
    /// <param name="func"></param>
    /// <param name="period"></param>
    /// <param name="expired"></param>
    public static void While(this Func<bool> func, int period = 1000, int expired = 60 * 1000)
    {
        var endTime = Environment.TickCount64 + expired;
        while (Environment.TickCount64 < endTime)
        {
            if (func?.Invoke() != true)
            {
                break;
            }
            ThreadUtil.Sleep(period);
        }
    }

    /// <summary>
    /// 直到条件满足执行操作
    /// </summary>
    /// <param name="action"></param>
    /// <param name="milliseconds"></param>
    /// <param name="once"></param>
    public static Task Until(this Action action, int milliseconds = 100, bool once = false, CancellationToken cancellationToken = default)
    {
        var safeMilliseconds = milliseconds < 10 ? 10 : milliseconds;

        return LongRunning(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                action?.Invoke();
                ThreadUtil.Sleep(safeMilliseconds, cancellationToken);
                if (once) break;
            }
        });
    }

    /// <summary>
    /// 直到条件满足执行操作
    /// </summary>
    /// <param name="fun"></param>
    /// <param name="milliseconds"></param>
    public static Task Until(this Func<bool> fun, int milliseconds = 100, CancellationToken cancellationToken = default)
    {
        var safeMilliseconds = milliseconds < 10 ? 10 : milliseconds;

        return LongRunning(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var isOver = fun?.Invoke() ?? false;
                    if (isOver) break;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                }
                ThreadUtil.Sleep(safeMilliseconds, cancellationToken);
            }
        });
    }

    /// <summary>
    /// 指定超时任务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <param name="timeOut"></param>
    /// <returns></returns>
    public static async Task<T> RunAsync<T>(this Func<CancellationToken, Task<T>> func, int timeOut = 3 * 1000)
    {
        using CancellationTokenSource cts = new(timeOut);
        return await Task.Run(async () =>
        {
            return await func.Invoke(cts.Token);

        }, cts.Token);
    }

    /// <summary>
    /// 任务超时
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<T> WithCancellationAsync<T>(this Task<T> task, CancellationToken token)
    {
        TaskCompletionSource<bool> tcs = new();

        CancellationTokenRegistration registration = token.Register(src =>
        {
            if (src is TaskCompletionSource<bool> completionSource)
            {
                completionSource.TrySetResult(true);
            }
        }, tcs);

        using (registration)
        {
            if (await Task.WhenAny(task, tcs.Task) != task)
            {
                throw new OperationCanceledException(token);
            }
        }

        return await task;
    }

    /// <summary>
    /// 任务超时
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<T> WithCancellationTimeout<T>(this Task<T> task, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource timeoutSource = new(timeout);
        using CancellationTokenSource linkSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken);
        return await task.WithCancellationAsync(linkSource.Token);
    }

    /// <summary>
    /// 任务超时
    /// </summary>
    /// <param name="action"></param>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task TimeoutAfterAsync(Func<CancellationToken, Task> action, TimeSpan timeout, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
        try
        {
            await action.Invoke(linkedCts.Token);
        }
        catch (OperationCanceledException exception)
        {
            var timeoutReached = timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested;

            if (timeoutReached)
            {
                throw new TimeoutException(exception.Message);
            }
            else
            {
                throw;
            }
        }

    }

    /// <summary>
    /// 任务超时
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="action"></param>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<TResult> TimeoutAfterAsync<TResult>(Func<CancellationToken, Task<TResult>> action, TimeSpan timeout, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
        try
        {
            return await action.Invoke(linkedCts.Token);
        }
        catch (OperationCanceledException exception)
        {
            var timeoutReached = timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested;
            if (timeoutReached)
            {
                throw new TimeoutException(exception.Message);
            }
            else
            {
                throw;
            }
        }
    }

    /// <summary>
    /// 等待任务列表
    /// </summary>
    /// <param name="timeOut"></param>
    /// <param name="tasks"></param>
    /// <returns></returns>
    public static async Task<IEnumerable<object?>> WaitForTasks(int timeOut, params Task<object>[] tasks)
    {
        List<object?> list = [];

        if (tasks != null && tasks.Length > 0)
        {
            if (timeOut > 0)
            {
                using var cts = new CancellationTokenSource(timeOut);
                foreach (Task<object> task in tasks)
                {
                    if (cts.IsCancellationRequested)
                    {
                        list.Add(null);
                    }
                    else
                    {
                        list.Add(await task);
                    }
                }
            }
            else
            {
                foreach (Task<object> task in tasks)
                {
                    list.Add(await task);
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 等待任务列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="action"></param>
    /// <param name="timeOut"></param>
    /// <returns></returns>
    public static void WaitForTasksByList<T>(this List<T> list, Action<T> action, int timeOut = -1)
    {
        if (list == null || list.Count < 1) return;

        List<Task> tasks = [];

        foreach (var item in list)
        {
            tasks.Add(Task.Run(() => action?.Invoke(item)));
        }

        if (timeOut > 0)
        {
            Task.WaitAll([.. tasks], timeOut);
        }
        else
        {
            Task.WaitAll([.. tasks]);
        }
    }

    /// <summary>
    /// 阻塞时长
    /// </summary>
    /// <param name="time"></param>
    public static void Sleep(int time)
    {
        ThreadUtil.Sleep(time);
    }

    /// <summary>
    /// 非阻塞
    /// </summary>
    /// <param name="millisecondsDelay"></param>
    /// <returns></returns>
    public static async Task Delay(int millisecondsDelay)
    {
        if (millisecondsDelay < 50)
        {
            millisecondsDelay = 50;
        }
        await Task.Delay(millisecondsDelay);
    }

    /// <summary>
    /// delay
    /// </summary>
    /// <param name="millisecondsDelay"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
    {
        // 如果取消已经请求，立即抛出异常
        cancellationToken.ThrowIfCancellationRequested();

        if (millisecondsDelay < 50)
        {
            millisecondsDelay = 50;
        }
        await Task.Delay(millisecondsDelay, cancellationToken);
    }
}
