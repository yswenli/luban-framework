/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common.IPToRegion
*文件名： IPRegion
*版本号： V1.0.0.0
*唯一标识：d13f644f-0058-4283-a703-9924e2b93cf2
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/10/19 9:47:39
*描述：ip区域
*
*=====================================================================
*修改标记
*修改时间：2021/10/19 9:47:39
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：ip区域
*
*****************************************************************************/

namespace LuBan.Common.IPToRegion
{
    /// <summary>
    /// ip区域
    /// </summary>
    public class IPRegion
    {
        /// <summary>
        /// ip地址
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 区域
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="remoteIp"></param>
        public static IPRegion Parse(string remoteIp)
        {
            if (remoteIp.IsNullOrEmpty()) throw new ArgumentNullException(nameof(remoteIp));
            var index = remoteIp.IndexOf(",");
            if (index > -1)
            {
                remoteIp = remoteIp.Substring(0, index);
            }
            var region = remoteIp.IpToRegion();
            return new IPRegion()
            {
                IP = remoteIp,
                Region = region
            };
        }
    }


}
