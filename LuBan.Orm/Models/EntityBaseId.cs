/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm
*文件名： EntityBaseId
*版本号： V1.0.0.0
*唯一标识：06251086-6f82-45c8-97a3-975a63cba12f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 13:32:00
*描述：框架实体基类Id
*
*=================================================
*修改标记
*修改时间：2023/12/4 13:32:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：框架实体基类Id
*
*****************************************************************************/
namespace LuBan.Orm.Models;

/// <summary>
/// 框架实体基类Id
/// </summary>
public abstract class EntityBaseId
{
    /// <summary>
    /// 雪花Id
    /// </summary>
    [SugarColumn(ColumnName = "Id", ColumnDescription = "主键Id", IsPrimaryKey = true, IsIdentity = false)]
    public virtual long Id { get; set; }
}
