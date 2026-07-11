/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common.IPToRegion
*文件名： Region
*版本号： V1.0.0.0
*唯一标识：d13f644f-0058-4283-a703-9924e2b93cf2
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/10/19 9:47:39
*描述：Region
*
*=====================================================================
*修改标记
*修改时间：2021/10/19 9:47:39
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：Region
*
*****************************************************************************/

namespace LuBan.Common.IPToRegion
{
    /// <summary>
    /// 区域
    /// </summary>
    public static class Region
    {
        static Searcher _searcher;

        /// <summary>
        /// 区域
        /// </summary>
        static Region()
        {
            //var dbPath = "";
            //var cBuff = Searcher.LoadContentFromFile(dbPath);
            var cBuff = Resource1.ip2region;
            _searcher = Searcher.NewWithBuffer(cBuff);
        }

        /// <summary>
        /// 根据ip查找区域
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string Get(string ip)
        {
            return _searcher.Search(ip);
        }
    }
}
