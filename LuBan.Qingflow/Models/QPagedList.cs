/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Qingflow.Models
*文件名： QPagedList
*版本号： V1.0.0.0
*唯一标识：9f28f9ea-26d9-4d45-83ee-650b44082bcb
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/23 18:10:30
*描述：分页数据
*
*=================================================
*修改标记
*修改时间：2024/12/23 18:10:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：分页数据
*
*****************************************************************************/
namespace LuBan.Qingflow.Models;

/// <summary>
/// 分页数据
/// </summary>
[DataContract]
public class QPagedList<T>
{
    /// <summary>
    /// 页数
    /// </summary>
    [DataMember(Name = "pageAmount")]
    public int PageAmount { get; set; }
    /// <summary>
    /// 起始页码
    /// </summary>
    [DataMember(Name = "pageNum")]
    public int PageNum { get; set; }

    /// <summary>
    /// 每页数量
    /// </summary>
    [DataMember(Name = "pageSize")]
    public int PageSize { get; set; } = 200; //注意pageSize最大可填200
    /// <summary>
    /// 数据
    /// </summary>
    [DataMember(Name = "result")]
    public List<T> Result { get; set; }
    /// <summary>
    /// 结果总数
    /// </summary>
    [DataMember(Name = "resultAmount")]
    public int ResultAmount { get; set; }
}
