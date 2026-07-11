/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm
*文件名： BasePageInput
*版本号： V1.0.0.0
*唯一标识：cf2a42c2-70dc-4983-ad3c-a3c3bb3b8483
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 18:26:53
*描述：全局分页查询输入参数
*
*=================================================
*修改标记
*修改时间：2023/12/4 18:26:53
*修改人： yswenli
*版本号： V1.0.0.0
*描述：全局分页查询输入参数
*
*****************************************************************************/

namespace LuBan.Orm.Models
{
    /// <summary>
    /// 全局分页查询输入参数
    /// </summary>
    public interface IBasePageInput
    {
        /// <summary>
        /// 排序字段
        /// </summary>
        List<BasePageOrder>? Orders { get; set; }
        /// <summary>
        /// 页数
        /// </summary>
        int Page { get; set; }
        /// <summary>
        /// 每页条数
        /// </summary>
        int PageSize { get; set; }
    }
}