/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Models.Entities
*文件名： DbBlock.cs
*版本号： V1.0.0.0
*唯一标识：e3b8816c-d1da-4fa7-be65-d324806f31f3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：DbBlock 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：DbBlock 类
*
*****************************************************************************/

using WebApplication1.Models.Enums;

namespace WebApplication1.Models.Entities;

[SugarTable("db_block", "栏目表")]
public class DbBlock : EntityDataScoreBase
{
    /// <summary>
    /// 上级ID
    /// </summary>
    [SugarColumn(ColumnDescription = "上级ID")]
    public long Pid { get; set; }

    /// <summary>
    /// 栏目名称
    /// </summary>
    [SugarColumn(ColumnDescription = "栏目名称", Length = 100)]
    [MaxLength(100)]
    public string? BlockName { get; set; }

    /// <summary>
    /// 栏目封面
    /// </summary>
    [SugarColumn(ColumnDescription = "栏目封面", Length = 1000)]
    [MaxLength(1000)]
    public string? BlockImg { get; set; }

    /// <summary>
    /// 级别
    /// </summary>
    [SugarColumn(ColumnDescription = "级别")]
    public int? Level { get; set; }
    /// <summary>
    /// 栏目类型
    /// </summary>
    [SugarColumn(ColumnDescription = "栏目类型")]
    public string BlockType { get; set; }
    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int? Sort { get; set; } = 100;

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public EnumBlockStatus? Status { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 200)]
    [MaxLength(200)]
    public string? Remark { get; set; }

    [Navigate(NavigateType.OneToMany, nameof(Pid), nameof(DbBlock.Id))]
    public List<DbBlock> SubBlocks { get; set; }
}
