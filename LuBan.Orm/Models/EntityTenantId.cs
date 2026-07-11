/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm
*文件名： EntityTenantId
*版本号： V1.0.0.0
*唯一标识：1a1ad372-e48f-4e62-818e-e2cac258fdb7
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:09:12
*描述：租户基类实体Id
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:09:12
*修改人： yswenli
*版本号： V1.0.0.0
*描述：租户基类实体Id
*
*****************************************************************************/
namespace LuBan.Orm.Models;

/// <summary>
/// 租户基类实体Id
/// </summary>
public abstract class EntityTenantId : EntityBaseId, ITenantIdFilter
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", IsOnlyIgnoreUpdate = true)]
    public virtual long? TenantId { get; set; }
}
