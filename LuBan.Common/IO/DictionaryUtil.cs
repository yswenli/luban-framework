/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： BaseService
*版本号： V1.0.0.0
*唯一标识：b3deeffd-31c4-456b-9e1b-dcc3dfffa06e
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/22 11:46:39
*描述：字典工具类
*
*=================================================
*修改标记
*修改时间：2022/7/22 11:46:39
*修改人： yswenli
*版本号： V1.0.0.0
*描述：字典工具类
*
*****************************************************************************/

namespace LuBan.Common.IO
{
    /// <summary>
    /// 字典工具类
    /// </summary>
    public static class DictionaryUtil
    {
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="target"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TVal> AddRange<TKey, TVal>(this Dictionary<TKey, TVal> target,
            Dictionary<TKey, TVal> data) where TKey : notnull
        {
            if (target == null)
            {
                target = new Dictionary<TKey, TVal>();
            }

            if (target.Count < 1)
            {
                if (data == null || data.Count < 1)
                {
                    return target;
                }
                else
                {
                    foreach (var item in data)
                    {
                        target.Add(item.Key, item.Value);
                    }
                }
            }
            else
            {
                if (data != null && data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        if (item.Key != null)
                            target.TryAdd(item.Key, item.Value);
                    }
                }
            }
            return target;
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="target"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static SortedDictionary<TKey, TVal> AddRange<TKey, TVal>(this SortedDictionary<TKey, TVal> target,
            Dictionary<TKey, TVal> data) where TKey : notnull
        {
            if (target == null)
            {
                target = new SortedDictionary<TKey, TVal>();
            }

            if (target.Count < 1)
            {
                if (data == null || data.Count < 1)
                {
                    return target;
                }
                else
                {
                    foreach (var item in data)
                    {
                        target.Add(item.Key, item.Value);
                    }
                }
            }
            else
            {
                if (data != null && data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        if (item.Key != null)
                            target.TryAdd(item.Key, item.Value);
                    }
                }
            }
            return target;
        }

        /// <summary>
        /// 按指定条件获取元素
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="predicate"></param>
        public static Dictionary<TKey, TVal>? GetDictionary<TKey, TVal>(this Dictionary<TKey, TVal> dic, Func<TKey, bool> predicate) where TKey : notnull
        {
            if (dic == null || dic.Count < 1) return null;
            Dictionary<TKey, TVal> result = new Dictionary<TKey, TVal>();
            dic.Keys.Where(predicate).ToList().ForEach((k) =>
            {
                result[k] = dic[k];
            });
            return result;
        }

        /// <summary>
        /// 按指定条件获取元素
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TVal>? ToDictionary<TKey, TVal>(this Dictionary<TKey, TVal> dic, Func<TKey, bool> predicate) where TKey : notnull
        {
            return dic.GetDictionary(predicate);
        }

        /// <summary>
        /// 返回指定条件的首个值
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static TVal? FirstOrDefault<TKey, TVal>(this Dictionary<TKey, TVal> dic, Func<TKey, bool> predicate) where TKey : notnull
        {
            if (dic == null || dic.Count < 1) return default;
            var key = dic.Keys.Where(predicate).FirstOrDefault();
            if (key == null) return default;
            return dic[key];
        }
        /// <summary>
        /// 返回指定条件的首个值
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static TVal? FirstOrDefault<TKey, TVal>(this SortedDictionary<TKey, TVal> dic, Func<TKey, bool> predicate) where TKey : notnull
        {
            if (dic == null || dic.Count < 1) return default;
            var key = dic.Keys.Where(predicate).FirstOrDefault();
            if (key == null) return default;
            return dic[key];
        }

        /// <summary>
        /// 返回指定条件的值列表
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static List<TVal>? ToList<TKey, TVal>(this Dictionary<TKey, TVal> dic, Func<TKey, bool> predicate) where TKey : notnull
        {
            if (dic == null || dic.Count < 1) return null;
            var keys = dic.Keys.Where(predicate).ToList();
            if (keys == null || keys.Count < 1) return null;
            List<TVal> result = new List<TVal>();
            foreach (var key in keys)
            {
                result.Add(dic[key]);
            }
            return result;
        }

        /// <summary>
        /// 返回指定条件的值列表
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static List<TVal>? ToList<TKey, TVal>(this SortedDictionary<TKey, TVal> dic, Func<TKey, bool> predicate) where TKey : notnull
        {
            if (dic == null || dic.Count < 1) return null;
            var keys = dic.Keys.Where(predicate).ToList();
            if (keys == null || keys.Count < 1) return null;
            List<TVal> result = new List<TVal>();
            foreach (var key in keys)
            {
                result.Add(dic[key]);
            }
            return result;
        }


        /// <summary>
        /// 返回指定条件的首个值
        /// </summary>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="ignoreCaseKey"></param>
        /// <returns></returns>
        public static TVal? FirstOrDefault<TVal>(this Dictionary<string, TVal> dic, string ignoreCaseKey)
        {
            if (dic == null || dic.Count < 1) return default;
            var findKey = dic.Keys.Where(q => q.Equals(ignoreCaseKey, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (findKey.IsNullOrEmpty()) return default;
            return dic[findKey];
        }
        /// <summary>
        /// 返回指定条件的首个值
        /// </summary>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="ignoreCaseKey"></param>
        /// <returns></returns>
        public static TVal? FirstOrDefault<TVal>(this SortedDictionary<string, TVal> dic, string ignoreCaseKey)
        {
            if (dic == null || dic.Count < 1) return default;
            var findKey = dic.Keys.Where(q => q.Equals(ignoreCaseKey, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (findKey.IsNullOrEmpty()) return default;
            return dic[findKey];
        }

        /// <summary>
        /// 返回指定条件的首个值
        /// </summary>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static TVal? FirstOrDefault<TVal>(this Dictionary<dynamic, TVal> dic, Func<dynamic, bool> predicate)
        {
            if (dic == null || dic.Count < 1) return default;
            var findKey = dic.Keys.Where(predicate).FirstOrDefault();
            if (findKey == null) return default;
            return dic[findKey];
        }
        /// <summary>
        /// 返回指定条件的首个值
        /// </summary>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static TVal? FirstOrDefault<TVal>(this SortedDictionary<dynamic, TVal> dic, Func<dynamic, bool> predicate)
        {
            if (dic == null || dic.Count < 1) return default;
            var findKey = dic.Keys.Where(predicate).FirstOrDefault();
            if (findKey == null) return default;
            return dic[findKey];
        }

        /// <summary>
        /// 返回指定条件的首个值
        /// </summary>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="ignoreCaseKey"></param>
        /// <returns></returns>
        public static TVal? GetVal<TVal>(this Dictionary<string, TVal> dic, string ignoreCaseKey)
        {
            return dic.FirstOrDefault(ignoreCaseKey);
        }

        /// <summary>
        /// 返回指定条件的首个值
        /// </summary>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="ignoreCaseKey"></param>
        /// <returns></returns>
        public static TVal? GetVal<TVal>(this SortedDictionary<string, TVal> dic, string ignoreCaseKey)
        {
            return dic.FirstOrDefault(ignoreCaseKey);
        }

        /// <summary>
        /// 返回指定条件的值列表
        /// </summary>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="ignoreCaseKey"></param>
        /// <returns></returns>
        public static List<TVal>? ToList<TVal>(this Dictionary<string, TVal> dic, string ignoreCaseKey)
        {
            if (dic == null || dic.Count < 1) return null;
            var findKey = dic.Keys.Where(q => q.Equals(ignoreCaseKey, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (findKey == null || findKey.Count < 1) return null;
            List<TVal> result = new List<TVal>();
            foreach (var item in findKey)
            {
                result.Add(dic[item]);
            }
            return result;
        }

        /// <summary>
        /// 返回指定条件的值列表
        /// </summary>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="ignoreCaseKey"></param>
        /// <returns></returns>
        public static List<TVal>? ToList<TVal>(this SortedDictionary<string, TVal> dic, string ignoreCaseKey)
        {
            if (dic == null || dic.Count < 1) return null;
            var findKey = dic.Keys.Where(q => q.Equals(ignoreCaseKey, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (findKey == null || findKey.Count < 1) return null;
            List<TVal> result = new List<TVal>();
            foreach (var item in findKey)
            {
                result.Add(dic[item]);
            }
            return result;
        }

        /// <summary>
        /// 按指定条件移除元素
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="predicate"></param>
        public static void Remove<TKey, TVal>(this Dictionary<TKey, TVal> dic, Func<TKey, bool> predicate) where TKey : notnull
        {
            if (dic == null || dic.Count < 1) return;
            dic.Keys.Where(predicate).ToList().ForEach((k) =>
            {
                dic.Remove(k);
            });
        }

        /// <summary>
        /// 按指定条件移除元素
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="predicate"></param>
        public static void Remove<TKey, TVal>(this SortedDictionary<TKey, TVal> dic, Func<TKey, bool> predicate) where TKey : notnull
        {
            if (dic == null || dic.Count < 1) return;
            dic.Keys.Where(predicate).ToList().ForEach((k) =>
            {
                dic.Remove(k);
            });
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="ignoreCaseKey"></param>
        /// <returns></returns>
        public static bool Exists<TVal>(this Dictionary<string, TVal> dic, string ignoreCaseKey)
        {
            if (dic == null || dic.Count < 1) return false;
            return dic.Keys.Contains(ignoreCaseKey, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="ignoreCaseKey"></param>
        /// <returns></returns>
        public static bool Exists<TVal>(this SortedDictionary<string, TVal> dic, string ignoreCaseKey)
        {
            if (dic == null || dic.Count < 1) return false;
            return dic.Keys.Contains(ignoreCaseKey, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static bool Exists<TKey, TVal>(this Dictionary<TKey, TVal> dic, Func<TKey, bool> predicate) where TKey : notnull
        {
            if (dic == null || dic.Count < 1) return false;
            var keys = dic.Keys.Where(predicate).ToList();
            if (keys == null || keys.Count < 1) return false;
            return true;
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static bool Exists<TKey, TVal>(this SortedDictionary<TKey, TVal> dic, Func<TKey, bool> predicate) where TKey : notnull
        {
            if (dic == null || dic.Count < 1) return false;
            var keys = dic.Keys.Where(predicate).ToList();
            if (keys == null || keys.Count < 1) return false;
            return true;
        }

        /// <summary>
        /// 清理
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        public static void Clear<TKey, TVal>(this Dictionary<TKey, TVal> dic) where TKey : notnull
        {
            if (dic == null || dic.Count < 1) return;
            dic.Keys.ToList().ForEach((k) =>
            {
                dic.Remove(k);
            });
        }

        /// <summary>
        /// 清理
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        public static void Clear<TKey, TVal>(this SortedDictionary<TKey, TVal> dic) where TKey : notnull
        {
            if (dic == null || dic.Count < 1) return;
            dic.Keys.ToList().ForEach((k) =>
            {
                dic.Remove(k);
            });
        }

        /// <summary>
        /// 获取查询字符串
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static string GetQueryString<TKey, TVal>(this IDictionary<TKey, TVal> dic) where TKey : notnull
        {
            if (dic == null || dic.Count < 1) return string.Empty;
            var queryString = new StringPlus();
            dic.Keys.ToList().ForEach((k) =>
            {
                queryString.Append(k);
                queryString.Append("=");
                queryString.Append(k);
                queryString.Append("&");
            });
            queryString.RemoveLast();
            return queryString.ToString();
        }


        /// <summary>
        /// 合并两个字典
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic">字典</param>
        /// <param name="newDic">新字典</param>
        public static void AddOrUpdate<T>(this ConcurrentDictionary<string, T> dic, Dictionary<string, T> newDic)
        {
            foreach (var (key, value) in newDic)
            {
                dic.AddOrUpdate(key, value, (key, old) => value);
            }
        }
    }
}
