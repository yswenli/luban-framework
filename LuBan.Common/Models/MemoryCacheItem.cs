/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Models
*文件名： MemoryCacheItem
*版本号： V1.0.0.0
*唯一标识：ac489865-3565-4ba4-8e35-1f6aa52de424
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 15:22:10
*描述：缓存项
*
*=================================================
*修改标记
*修改时间：2022/6/21 15:22:10
*修改人： yswenli
*版本号： V1.0.0.0
*描述：缓存项
*
*****************************************************************************/

namespace LuBan.Common.Models
{
    /// <summary>
    /// 缓存项
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MemoryCacheItem
    {
        /// <summary>
        /// 键
        /// </summary>
        public string Key
        {
            get; set;
        }
        /// <summary>
        /// 值
        /// </summary>
        public dynamic Value
        {
            get; set;
        }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime Expired
        {
            get; set;
        }
    }
}
