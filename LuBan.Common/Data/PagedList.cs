/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common.Data
*文件名： PagedList
*版本号： V1.0.0.0
*唯一标识：3a0499e4-68ef-4244-8707-17ecc8bef9df
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 18:21:32
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/4 18:21:32
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Common.Data
{

    /// <summary>
    /// 分页泛型集合
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class PagedList<TEntity>
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 页容量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总条数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 当前页集合
        /// </summary>
        public IEnumerable<TEntity> Items { get; set; }

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPrevPage { get; set; }

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNextPage { get; set; }
    }

    /// <summary>
    /// 将集合转换为分页集合
    /// </summary>
    public static class PagedListExtension
    {
        /// <summary>
        /// 将集合转换为分页集合
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="items"></param>
        /// <param name="total"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static PagedList<TModel> ToPagedList<TModel>(this IEnumerable<TModel> items, int total, int pageIndex, int pageSize)
        {
            int num = ((pageSize > 0) ? ((int)Math.Ceiling((double)total / pageSize)) : 0);
            return new PagedList<TModel>
            {
                Page = pageIndex,
                PageSize = pageSize,
                Items = items,
                Total = total,
                TotalPages = num,
                HasNextPage = (pageIndex < num),
                HasPrevPage = (pageIndex - 1 > 0)
            };
        }
    }
}
