/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： WebProxyUtil
*版本号： V1.0.0.0
*唯一标识：2c3d63ba-6dfb-4533-b839-ec91751f5444
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/12/27 10:53:09
*描述：http代理工具类
*
*=====================================================================
*修改标记
*修改时间：2021/12/27 10:53:09
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：http代理工具类
*
*****************************************************************************/
namespace LuBan.Common
{
    /// <summary>
    /// http代理工具类
    /// </summary>
    public static class WebProxyUtil
    {
        static ConcurrentDictionary<string, WebProxy> _cache;

        /// <summary>
        /// http代理工具类
        /// </summary>
        static WebProxyUtil()
        {
            _cache = new ConcurrentDictionary<string, WebProxy>();
        }

        /// <summary>
        /// 获取在ie中设置的代理
        /// </summary>
        /// <returns></returns>
        public static WebProxy GetDefaultProxy()
        {
            return WebProxy.GetDefaultProxy();
        }

        /// <summary>
        /// 创建代理
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="userName"></param>
        /// <param name="passwords"></param>
        /// <returns></returns>
        public static WebProxy CreateWebProxy(string ip = "192.168.11.6", int port = 3128, string userName = "", string passwords = "")
        {
            var key = $"{ip}{port}{userName}{passwords}";

            return _cache.GetOrAdd(key, (k) =>
            {
                var webProxy = new WebProxy(ip, port);

                if (string.IsNullOrEmpty(userName))
                {
                    webProxy.UseDefaultCredentials = true;
                }
                else
                {
                    webProxy.UseDefaultCredentials = false;
                    webProxy.Credentials = new NetworkCredential(userName, passwords);
                }
                webProxy.BypassProxyOnLocal = false;

                return webProxy;
            });
        }
        /// <summary>
        /// 创建代理
        /// </summary>
        /// <param name="url"></param>
        /// <param name="userName"></param>
        /// <param name="passwords"></param>
        /// <returns></returns>
        public static WebProxy CreateWebProxy(string url, string userName, string passwords)
        {
            var key = $"{url}{userName}{passwords}";

            return _cache.GetOrAdd(key, (k) =>
            {
                var webProxy = new WebProxy(new Uri(url));

                if (string.IsNullOrEmpty(userName))
                {
                    webProxy.UseDefaultCredentials = true;
                }
                else
                {
                    webProxy.UseDefaultCredentials = false;
                    webProxy.Credentials = new NetworkCredential(userName, passwords);
                }
                webProxy.BypassProxyOnLocal = false;

                return webProxy;
            });
        }
    }
}
