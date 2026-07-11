/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Models
*文件名： BasePageOrder
*版本号： V1.0.0.0
*唯一标识：1b0458ce-179f-4556-a294-ba8783649e99
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/9/25 10:13:21
*描述：排序信息
*
*=================================================
*修改标记
*修改时间：2025/9/25 10:13:21
*修改人： yswenli
*版本号： V1.0.0.0
*描述：排序信息
*
*****************************************************************************/
namespace LuBan.Orm.Models;

/// <summary>
/// 排序信息
/// </summary>
public class BasePageOrder
{
    /// <summary>
    /// 排序字段名称
    /// </summary>
    public string FieldName { get; set; }
    /// <summary>
    /// 是否顺序
    /// </summary>
    public bool IsAsc { get; set; } = true;
}
