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
namespace LuBan.Orm.Models;


/// <summary>
/// 全局分页查询输入参数
/// </summary>
public class BasePageInput : IBasePageInput
{
    /// <summary>
    /// 当前页码
    /// </summary>
    /// <example>1</example>
    [Required, Range(1, long.MaxValue, ErrorMessage = "页数不能小于1")]
    public virtual int Page { get; set; } = 1;

    /// <summary>
    /// 页码容量
    /// </summary>
    /// <example>10</example>
    [Required, Range(1, long.MaxValue, ErrorMessage = "页码不能小于1")]
    public virtual int PageSize { get; set; } = 20;

    /// <summary>
    /// 排序
    /// </summary>
    public List<BasePageOrder>? Orders { get; set; } = [];
}