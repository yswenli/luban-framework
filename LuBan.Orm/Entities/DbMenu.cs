/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Entities
*文件名： 系统菜单表
*版本号： V1.0.0.0
*唯一标识：a5bb6173-b22d-4edd-852f-9b02bb075167
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/11/03 14:00:15
*描述：
*
*=================================================
*修改标记
*修改时间：2023/11/03 14:00:15
*修改人： yswenli
*版本号： V1.0.0.0
*描述：系统菜单表
*
*****************************************************************************/

namespace LuBan.Orm.Entities;

/// <summary>
/// 系统菜单表
/// </summary>
[SugarTable("db_menu", "系统菜单表")]
[SysTable]
public class DbMenu : EntityBase
{
    /// <summary>
    /// 父Id
    /// </summary>
    [SugarColumn(ColumnDescription = "父Id")]
    public long Pid { get; set; }

    /// <summary>
    /// 菜单类型（1目录 2菜单 3按钮）
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单类型")]
    public EnumMenuType Type { get; set; }

    /// <summary>
    /// 路由名称
    /// </summary>
    [SugarColumn(ColumnDescription = "路由名称", Length = 64)]
    [MaxLength(64)]
    public string? Name { get; set; }

    /// <summary>
    /// 路由地址
    /// </summary>
    [SugarColumn(ColumnDescription = "路由地址", Length = 128)]
    [MaxLength(128)]
    public string? Path { get; set; }

    /// <summary>
    /// 组件路径
    /// </summary>
    [SugarColumn(ColumnDescription = "组件路径", Length = 128)]
    [MaxLength(128)]
    public string? Component { get; set; }

    /// <summary>
    /// 重定向
    /// </summary>
    [SugarColumn(ColumnDescription = "重定向", Length = 128)]
    [MaxLength(128)]
    public string? Redirect { get; set; }

    /// <summary>
    /// 权限标识
    /// </summary>
    [SugarColumn(ColumnDescription = "权限标识", Length = 128)]
    [MaxLength(128)]
    public string? Permission { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单名称", Length = 64)]
    [Required, MaxLength(64)]
    public string Title { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [SugarColumn(ColumnDescription = "图标", Length = 128)]
    [MaxLength(128)]
    public string? Icon { get; set; }

    /// <summary>
    /// 是否内嵌
    /// </summary>
    [SugarColumn(ColumnDescription = "是否内嵌")]
    public bool IsIframe { get; set; }

    /// <summary>
    /// 外链链接
    /// </summary>
    [SugarColumn(ColumnDescription = "外链链接", Length = 256)]
    [MaxLength(256)]
    public string? OutLink { get; set; }

    /// <summary>
    /// 是否隐藏
    /// </summary>
    [SugarColumn(ColumnDescription = "是否隐藏")]
    public bool IsHide { get; set; }

    /// <summary>
    /// 是否缓存
    /// </summary>
    [SugarColumn(ColumnDescription = "是否缓存")]
    public bool IsKeepAlive { get; set; } = true;

    /// <summary>
    /// 是否固定
    /// </summary>
    [SugarColumn(ColumnDescription = "是否固定")]
    public bool IsAffix { get; set; }

    /// <summary>
    /// 标识(用于某些分层业务中)
    /// </summary>
    [SugarColumn(ColumnDescription = "标识(用于某些分层业务中)", Length = 64)]
    public string? Mark { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public EnumEnableStatus Status { get; set; } = EnumEnableStatus.Enable;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 256)]
    [MaxLength(256)]
    public string? Remark { get; set; }

    /// <summary>
    /// 菜单子项
    /// </summary>
    [SugarColumn(IsIgnore = true), JsonIgnore]
    public List<DbMenu> Children { get; set; } = [];
}