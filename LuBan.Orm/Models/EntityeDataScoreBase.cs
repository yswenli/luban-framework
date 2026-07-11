/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Entities
*文件名： EntityBaseData
*版本号： V1.0.0.0
*唯一标识：0506282b-82ec-4dd9-aede-99d02655c571
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/29 18:53:58
*描述：业务数据实体基类(数据权限)
*
*=================================================
*修改标记
*修改时间：2023/12/29 18:53:58
*修改人： yswenli
*版本号： V1.0.0.0
*描述：业务数据实体基类(数据权限)
*
*****************************************************************************/
namespace LuBan.Orm.Models;

/// <summary>
/// 业务数据实体基类（数据权限）
/// </summary>
public abstract class EntityeDataScoreBase : EntityBase, IOrgIdFilter
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", IsOnlyIgnoreUpdate = true)]
    public virtual long? TenantId { get; set; }
    /// <summary>
    /// 创建者部门Id
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者部门Id", IsOnlyIgnoreUpdate = true)]
    [OwnerOrg]
    public virtual long? CreateOrgId { get; set; }

    /// <summary>
    /// 创建者部门
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(CreateOrgId))]
    public virtual DbOrg CreateOrg { get; set; }

    /// <summary>
    /// 创建者部门名称
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者部门名称", Length = 64, IsOnlyIgnoreUpdate = true)]
    public virtual string? CreateOrgName { get; set; }
}
