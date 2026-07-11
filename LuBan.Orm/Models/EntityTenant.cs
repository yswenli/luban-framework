/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Entities
*文件名： EntityTenant
*版本号： V1.0.0.0
*唯一标识：c2bad655-e8c2-486b-8241-572d27ae9555
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/29 18:20:33
*描述：租户基类实体
*
*=================================================
*修改标记
*修改时间：2023/12/29 18:20:33
*修改人： yswenli
*版本号： V1.0.0.0
*描述：租户基类实体
*
*****************************************************************************/
namespace LuBan.Orm.Models;


/// <summary>
/// 租户基类实体
/// </summary>
public abstract class EntityTenant : EntityBase, ITenantIdFilter
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", IsOnlyIgnoreUpdate = true)]
    public virtual long? TenantId { get; set; }
}
