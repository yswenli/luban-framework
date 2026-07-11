/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： UriUtil
*版本号： V1.0.0.0
*唯一标识：1eaebfd9-f888-4d80-b10c-63c56b8509dc
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/3/6 11:31:19
*描述：uri工具类
*
*=================================================
*修改标记
*修改时间：2023/3/6 11:31:19
*修改人： yswenli
*版本号： V1.0.0.0
*描述：uri工具类
*
*****************************************************************************/
namespace LuBan.Common
{
    /// <summary>
    /// uri工具类
    /// </summary>
    public static class UriUtil
    {
        /// <summary>
        /// 获取基础地址和资源地址
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Tuple<string, string> GetBaseUrlAndSource(this Uri uri)
        {
            var baseUrl = $"{uri.Scheme}://{uri.Host}";
            if (uri.Port != 80 && uri.Port != 443)
            {
                baseUrl += ":" + uri.Port;
            }
            return new Tuple<string, string>(baseUrl, uri.PathAndQuery);
        }

        /// <summary>
        /// 获取基础地址和资源地址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Tuple<string, string>? GetBaseUrlAndSource(this string url)
        {
            if (url.IsNullOrEmpty()) return null;
            return new Uri(url).GetBaseUrlAndSource();
        }

        /// <summary>
        /// 将地栏参数换成字典
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetQueryForDic(this string url)
        {
            var dic = new Dictionary<string, string>();
            var queryArr = url.Substring(url.IndexOf("?") + 1).Split('&', StringSplitOptions.RemoveEmptyEntries);
            if (queryArr != null && queryArr.Length > 0)
            {
                foreach (var item in queryArr)
                {
                    if (item.IsNotNullOrEmpty() && item.IndexOf("=") > -1)
                    {
                        var arr = item.Split('=', StringSplitOptions.TrimEntries);
                        if (arr != null && arr.Length == 2)
                        {
                            dic.TryAdd(arr[0], arr[1]);
                        }
                    }
                }
            }
            return dic;
        }

        /// <summary>
        /// /将地栏参数换成字典
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static SortedDictionary<string, string> GetQueryForSortedDic(this string url)
        {
            if (url.IsNullOrEmpty()) return [];
            var dic = new SortedDictionary<string, string>();
            var queryArr = url.Substring(url.IndexOf("?") + 1).Split('&', StringSplitOptions.RemoveEmptyEntries);
            if (queryArr != null && queryArr.Length > 0)
            {
                foreach (var item in queryArr)
                {
                    if (item.IsNotNullOrEmpty() && item.IndexOf("=") > -1)
                    {
                        var arr = item.Split('=', StringSplitOptions.TrimEntries);
                        if (arr != null && arr.Length == 2)
                        {
                            dic.TryAdd(arr[0], arr[1]);
                        }
                    }
                }
            }
            return dic;
        }

    }
}
