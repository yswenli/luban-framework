/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Entities
*文件名： EntityBase
*版本号： V1.0.0.0
*唯一标识：17fa1c5b-73fd-4858-bf8a-59af83f21aa6
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/29 18:19:17
*描述：框架实体基类
*
*=================================================
*修改标记
*修改时间：2023/12/29 18:19:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：框架实体基类
*
*****************************************************************************/
namespace LuBan.Orm.Models;


/// <summary>
/// 框架实体基类
/// </summary>
public abstract class EntityBase : EntityBaseId, IDeletedFilter
{
    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间", IsOnlyIgnoreUpdate = true)]
    public virtual DateTime CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "更新时间", IsOnlyIgnoreInsert = true)]
    public virtual DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 创建者Id
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者Id", IsOnlyIgnoreUpdate = true)]
    [OwnerUser]
    public virtual long? CreateUserId { get; set; }

    /// <summary>
    /// 创建者
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(CreateUserId)), JsonIgnore]
    public virtual DbUser? CreateUser { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者姓名", Length = 64, IsOnlyIgnoreUpdate = true)]
    public virtual string? CreateUserName { get; set; }

    /// <summary>
    /// 修改者Id
    /// </summary>
    [SugarColumn(ColumnDescription = "修改者Id", IsOnlyIgnoreInsert = true)]
    public virtual long? UpdateUserId { get; set; }

    /// <summary>
    /// 修改者
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(UpdateUserId)), JsonIgnore]
    public virtual DbUser? UpdateUser { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "修改者姓名", Length = 64, IsOnlyIgnoreInsert = true)]
    public virtual string? UpdateUserName { get; set; }

    /// <summary>
    /// 软删除
    /// </summary>
    [SugarColumn(ColumnDescription = "软删除")]
    public virtual bool IsDelete { get; set; } = false;
}
