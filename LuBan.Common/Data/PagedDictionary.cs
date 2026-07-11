/****************************************************************************
*Copyright @ 2023-2024 RiverLand All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：RiverLand
*命名空间：LuBan.Common.Data
*文件名： PagedDictionary
*版本号： V1.0.0.0
*唯一标识：761a6ccf-2e7e-458e-ba73-af97fc0414d1
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 17:39:40
*描述：分页字典信息类
*
*=================================================
*修改标记
*修改时间：2022/6/21 17:39:40
*修改人： yswenli
*版本号： V1.0.0.0
*描述：分页字典信息类
*
*****************************************************************************/

namespace LuBan.Common.Data
{
    /// <summary>
    /// 分页字典信息类
    /// </summary>
    public class PagedDictionary<T1, T2> where T1 : notnull
    {
        /// <summary>
        /// 分页信息类
        /// </summary>
        public PagedDictionary()
        {
            Page = 1;
            PageSize = 20;
            Total = 0;
            Dictionary = [];
        }
        /// <summary>
        /// 页号
        /// </summary>
        [JsonProperty("pageIndex")]
        public int Page { get; set; }
        /// <summary>
        /// 分页条数
        /// </summary>
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        [JsonProperty("total")]
        public long Total { get; set; }

        /// <summary>
        /// 分页数据内容
        /// </summary>
        [JsonProperty("list")]
        public Dictionary<T1, T2> Dictionary { get; set; } = [];
    }
}
