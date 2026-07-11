/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Data
*文件名： DataSplitter
*版本号： V1.0.0.0
*唯一标识：c4ecebff-1324-437c-a19e-1fe24cfdb16b
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2023/2/7 10:22:38
*描述：数据拆分处理
*
*=================================================
*修改标记
*修改时间：2023/2/7 10:22:38
*修改人： yswen
*版本号： V1.0.0.0
*描述：数据拆分处理
*
*****************************************************************************/

namespace LuBan.Common.Data;

/// <summary>
/// 数据拆分处理
/// </summary>
public static class DataSplitter
{

    /// <summary>
    /// 指定大小分批执行
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="action"></param>
    /// <param name="maxCount"></param>
    public static void Execute<T>(this T[] data, Action<T[]> action, int maxCount = 100)
    {
        if (data == null || data.Length < 1 || data[0] == null) return;
        if (maxCount < 1)
        {
            maxCount = 100;
        }
        if (data.Length < maxCount)
        {
            action.Invoke(data);
        }
        else
        {
            var offset = 0;
            do
            {
                var splitData = data.Skip(offset * maxCount).Take(maxCount).ToArray();
                if (splitData == null || splitData.Length < 1) break;
                offset += 1;
                action.Invoke(splitData);
            }
            while (true);
        }
    }
    /// <summary>
    /// 指定大小分批执行
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="action"></param>
    /// <param name="maxCount"></param>
    /// <returns></returns>
    public static async Task ExecuteAsync<T>(this T[] data, Func<T[], Task> action, int maxCount = 100)
    {

        if (data == null || data.Length < 1 || data[0] == null) return;
        if (maxCount < 1)
        {
            maxCount = 100;
        }
        if (data.Length < maxCount)
        {
            await action.Invoke(data);
        }
        else
        {
            var offset = 0;
            do
            {
                var splitData = data.Skip(offset * maxCount).Take(maxCount).ToArray();
                if (splitData == null || splitData.Length < 1) break;
                offset += 1;
                await action.Invoke(splitData);
            }
            while (true);
        }
    }



    /// <summary>
    /// 指定大小分批执行
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="action"></param>
    /// <param name="maxCount"></param>
    public static void Execute<T>(this T[] data, Func<T[], bool> action, int maxCount = 100)
    {
        if (data == null || data.Length < 1 || data[0] == null) return;
        if (maxCount < 1)
        {
            maxCount = 100;
        }
        if (data.Length < maxCount)
        {
            action.Invoke(data);
        }
        else
        {
            var offset = 0;
            do
            {
                var splitData = data.Skip(offset * maxCount).Take(maxCount).ToArray();
                if (splitData == null || splitData.Length < 1) break;
                offset += 1;
                if (!action.Invoke(splitData)) break;
            }
            while (true);
        }
    }


    /// <summary>
    /// 指定大小分批执行
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="action"></param>
    /// <param name="maxCount"></param>
    public static void Execute<T>(this List<T> data, Action<List<T>> action, int maxCount = 100)
    {
        if (data == null || data.Count < 1 || data[0] == null) return;
        if (maxCount < 1)
        {
            maxCount = 100;
        }
        if (data.Count < maxCount)
        {
            action.Invoke(data);
        }
        else
        {
            var offset = 0;
            do
            {
                var splitData = data.Skip(offset * maxCount).Take(maxCount).ToList();
                if (splitData == null || splitData.Count < 1) break;
                offset += 1;
                action.Invoke(splitData);
            }
            while (true);
        }
    }
    /// <summary>
    /// 指定大小分批执行
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="action"></param>
    /// <param name="maxCount"></param>
    /// <returns></returns>
    public static async Task ExecuteAsync<T>(this List<T> data, Func<List<T>, Task> action, int maxCount = 100)
    {
        if (data == null || data.Count < 1 || data[0] == null) return;
        if (maxCount < 1)
        {
            maxCount = 100;
        }
        if (data.Count < maxCount)
        {
            await action.Invoke(data);
        }
        else
        {
            var offset = 0;
            do
            {
                var splitData = data.Skip(offset * maxCount).Take(maxCount).ToList();
                if (splitData == null || splitData.Count < 1) break;
                offset += 1;
                await action.Invoke(splitData);
            }
            while (true);
        }
    }

    /// <summary>
    /// 指定大小分批执行
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="action"></param>
    /// <param name="maxCount"></param>
    public static void Execute<T>(this List<T> data, Func<List<T>, bool> action, int maxCount = 100)
    {
        if (data == null || data.Count < 1 || data[0] == null) return;
        if (maxCount < 1)
        {
            maxCount = 100;
        }
        if (data.Count < maxCount)
        {
            action.Invoke(data);
        }
        else
        {
            var offset = 0;
            do
            {
                var splitData = data.Skip(offset * maxCount).Take(maxCount).ToList();
                if (splitData == null || splitData.Count < 1) break;
                offset += 1;
                if (!action.Invoke(splitData))
                {
                    break;
                }
            }
            while (true);
        }
    }


    /// <summary>
    /// 指定大小分批执行
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TVal"></typeparam>
    /// <param name="dic"></param>
    /// <param name="action"></param>
    /// <param name="maxCount"></param>
    public static void Execute<TKey, TVal>(this Dictionary<TKey, TVal> dic, Action<List<KeyValuePair<TKey, TVal>>> action, int maxCount = 100) where TKey : notnull
    {
        if (dic == null || dic.Count < 1) return;
        if (maxCount < 1)
        {
            maxCount = 100;
        }
        if (dic.Count < maxCount)
        {
            action.Invoke(dic.ToList());
        }
        else
        {
            var offset = 0;
            do
            {
                var splitData = dic.Skip(offset * maxCount).Take(maxCount).ToList();
                if (splitData == null || splitData.Count < 1) break;
                offset += 1;
                action.Invoke(splitData);
            }
            while (true);
        }
    }

    /// <summary>
    /// 指定大小分批执行
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TVal"></typeparam>
    /// <param name="dic"></param>
    /// <param name="action"></param>
    /// <param name="maxCount"></param>
    /// <returns></returns>
    public static async Task ExecuteAsync<TKey, TVal>(this Dictionary<TKey, TVal> dic, Func<List<KeyValuePair<TKey, TVal>>, Task> action, int maxCount = 100) where TKey : notnull
    {
        if (dic == null || dic.Count < 1) return;
        if (maxCount < 1)
        {
            maxCount = 100;
        }
        if (dic.Count < maxCount)
        {
            await action.Invoke(dic.ToList());
        }
        else
        {
            var offset = 0;
            do
            {
                var splitData = dic.Skip(offset * maxCount).Take(maxCount).ToList();
                if (splitData == null || splitData.Count < 1) break;
                offset += 1;
                await action.Invoke(splitData);
            }
            while (true);
        }
    }

    /// <summary>
    /// 指定大小分批执行
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TVal"></typeparam>
    /// <param name="data"></param>
    /// <param name="action"></param>
    /// <param name="maxCount"></param>
    public static void Execute<TKey, TVal>(this Dictionary<TKey, TVal> data, Func<List<KeyValuePair<TKey, TVal>>, bool> action, int maxCount = 100) where TKey : notnull
    {
        if (data == null || data.Count < 1) return;
        if (maxCount < 1)
        {
            maxCount = 100;
        }
        if (data.Count < maxCount)
        {
            action.Invoke(data.ToList());
        }
        else
        {
            var offset = 0;
            do
            {
                var splitData = data.Skip(offset * maxCount).Take(maxCount).ToList();
                if (splitData == null || splitData.Count < 1) break;
                offset += 1;
                if (!action.Invoke(splitData))
                {
                    break;
                }
            }
            while (true);
        }
    }
}
