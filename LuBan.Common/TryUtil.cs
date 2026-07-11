/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： TryUtil
*版本号： V1.0.0.0
*唯一标识：7e073fb7-a618-4b6d-9d74-11912560f445
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/4/26 11:15:44
*描述：重试操作工具类
*
*=====================================================================
*修改标记
*修改时间：2021/4/26 11:15:44
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：重试操作工具类
*
*****************************************************************************/
namespace LuBan.Common;

/// <summary>
/// 重试操作工具类
/// </summary>
public static class TryUtil
{
    /// <summary>
    /// 重试
    /// </summary>
    /// <param name="action"></param>
    /// <param name="tryTimes"></param>
    /// <param name="priod"></param>
    /// <returns></returns>
    public static List<Exception> TryDo(this Action action, int tryTimes = 3, int priod = 60 * 1000)
    {
        List<Exception> result = new List<Exception>();
        for (int i = 0; i < tryTimes; i++)
        {
            try
            {
                action.Invoke();
                break;
            }
            catch (Exception ex)
            {
                result.Add(ex);
                Logger.Error($"TryUtil.TryDo", ex);
                Thread.Sleep(priod);
            }
        }
        return result;
    }
    /// <summary>
    /// 重试
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fun"></param>
    /// <param name="tryTimes"></param>
    /// <param name="priod"></param>
    /// <returns></returns>
    public static List<Exception> TryDo<T>(this Func<T> fun, int tryTimes = 3, int priod = 60 * 1000)
    {
        List<Exception> result = new List<Exception>();
        for (int i = 0; i < tryTimes; i++)
        {
            try
            {
                _ = fun.Invoke();
                break;
            }
            catch (Exception ex)
            {
                result.Add(ex);
                Logger.Error($"TryUtil.TryDo", ex);
                Thread.Sleep(priod);
            }
        }
        return result;
    }

    /// <summary>
    /// 重试
    /// </summary>
    /// <param name="fun"></param>
    /// <param name="tryTimes"></param>
    /// <param name="priod"></param>
    /// <returns></returns>
    public static List<Exception> TryDo(this Func<bool> fun, int tryTimes = 3, int priod = 60 * 1000)
    {
        List<Exception> result = new();
        for (int i = 0; i < tryTimes; i++)
        {
            try
            {
                if (fun.Invoke())
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                result.Add(ex);
                Logger.Error($"TryUtil.TryDo", ex);
                Thread.Sleep(priod);
            }
        }
        return result;
    }

    /// <summary>
    /// 异步
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <param name="tryTimes"></param>
    /// <param name="priod"></param>
    /// <returns></returns>
    public static async Task<T> TryDoAsync<T>(this Func<Task<T>> func, int tryTimes = 3, int priod = 60 * 1000)
    {
        return await TaskUtil.RunAsync(async (t) =>
        {
            for (int i = 0; i < tryTimes; i++)
            {
                try
                {
                    return await func.Invoke();
                }
                catch
                {
                    if (i >= tryTimes - 1)
                        throw;
                    else
                        await Task.Delay(priod);
                }
            }
            throw new Exception("empty");
        });
    }

    /// <summary>
    /// BlockIO
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <param name="timeOut"></param>
    /// <returns></returns>
    public static T? BlockIO<T>(Func<T?> func, int timeOut)
    {
        var stopWatch = Stopwatch.StartNew();
        while (stopWatch.ElapsedMilliseconds < timeOut)
        {
            var result = func.Invoke();
            if (result != null)
            {
                if (result is DateTime dt && dt == new DateTime())
                {
                    Thread.Sleep(50);
                    continue;
                }
                stopWatch.Stop();
                return result;
            }
            Thread.Sleep(50);
        }
        stopWatch.Stop();
        return default;
    }
}
