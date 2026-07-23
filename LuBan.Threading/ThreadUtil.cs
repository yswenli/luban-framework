/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Threading
*文件名： ThreadUtil
*版本号： V1.0.0.0
*唯一标识：508fdc9d-4eb2-4f18-b120-37a1e41718fd
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/5/19 13:43:46
*描述：线程工具类
*
*=================================================
*修改标记
*修改时间：2025/5/19 13:43:46
*修改人： yswenli
*版本号： V1.0.0.0
*描述：线程工具类
*
*****************************************************************************/
namespace LuBan.Threading;

/// <summary>
/// 线程工具类
/// </summary>
public static class ThreadUtil
{
    /// <summary>
    /// 独立线程运行（使用线程池或任务，减少线程创建开销）
    /// </summary>
    /// <param name="action"></param>
    public static void ThreadRun(Action action)
    {
        // 使用线程池代替直接创建线程，减少资源消耗
        ThreadPool.QueueUserWorkItem(_ => action?.Invoke());
    }

    /// <summary>
    /// 独立线程运行（使用任务，减少线程创建开销）
    /// </summary>
    /// <param name="action"></param>
    public static void ThreadRun(Func<Task?> action)
    {
        // 使用Task.Run代替直接创建线程，更高效地管理异步操作
        _ = Task.Run(async () =>
        {
            var task = action?.Invoke();
            if (task != null) await task;
        });
    }



    /// <summary>
    /// 线程中循环处理
    /// </summary>
    /// <param name="action"></param>
    /// <param name="milliseconds"></param>
    public static void ThreadWhile(Action action, int milliseconds = -1, CancellationToken cancellationToken = default)
    {
        var safeMilliseconds = EnsureMinimumSleep(milliseconds);

        ThreadRun(() =>
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                action?.Invoke();
                Sleep(safeMilliseconds, cancellationToken);
            }
        });
    }

    /// <summary>
    /// 线程中循环处理
    /// </summary>
    /// <param name="fun">返回 true 继续循环，返回 false 或 null 退出循环</param>
    /// <param name="milliseconds"></param>
    public static void ThreadWhile(this Func<bool> fun, int milliseconds = -1, CancellationToken cancellationToken = default)
    {
        var safeMilliseconds = EnsureMinimumSleep(milliseconds);

        ThreadRun(() =>
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                var result = fun?.Invoke() ?? false;
                if (!result) break;
                Sleep(safeMilliseconds, cancellationToken);
            }
        });
    }

    /// <summary>
    /// 确保最小睡眠时间，防止忙等待
    /// </summary>
    /// <param name="milliseconds">原始睡眠时间</param>
    /// <returns>安全的睡眠时间</returns>
    private static int EnsureMinimumSleep(int milliseconds)
    {
        if (milliseconds <= 0 && milliseconds != -1) return 10;
        return milliseconds;
    }

    /// <summary>
    /// 线程睡眠工具方法（支持分段唤醒与取消，避免长时间阻塞无法中断）
    /// </summary>
    /// <param name="milliseconds">睡眠时长（毫秒），默认-1表示无限期睡眠（需配合取消令牌使用）</param>
    /// <param name="cancellationToken">取消令牌（用于中途中断睡眠，如程序退出、任务取消场景）</param>
    /// <exception cref="ArgumentOutOfRangeException">当睡眠时长小于-1时抛出</exception>
    /// <exception cref="OperationCanceledException">当取消令牌触发取消时抛出</exception>
    public static void Sleep(int milliseconds = -1, CancellationToken cancellationToken = default)
    {
        if (milliseconds < 1)
        {
            if (cancellationToken == default)
            {
                Thread.Sleep(Timeout.Infinite);
            }
            else
            {
                cancellationToken.WaitHandle.WaitOne();
                cancellationToken.ThrowIfCancellationRequested();
            }
            return;
        }

        if (milliseconds <= 5000)
        {
            Thread.Sleep(milliseconds);
            cancellationToken.ThrowIfCancellationRequested();
            return;
        }

        cancellationToken.WaitHandle.WaitOne(milliseconds);
        cancellationToken.ThrowIfCancellationRequested();
    }
}
