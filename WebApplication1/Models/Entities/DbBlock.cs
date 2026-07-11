using WebApplication1.Models.Enums;

namespace WebApplication1.Models.Entities;

[SugarTable("db_block", "栏目表")]
public class DbBlock : EntityeDataScoreBase
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
