using WebApplication1.Models.Vos;

namespace WebApplication1.Models.Entities;

[SugarTable("db_banner", "banner表")]
public class DbBanner : EntityeDataScoreBase
{

    /// <summary>
    /// 标题
    /// </summary>
    [SugarColumn(ColumnDescription = "标题", Length = 500)]
    [MaxLength(500)]
    public string? Title { get; set; }

    /// <summary>
    /// banner图
    /// </summary>
    [SugarColumn(ColumnDescription = "banner图", Length = 1000)]
    [MaxLength(1000)]
    public string? TitleImg { get; set; }


    /// <summary>
    /// 跳转链接
    /// </summary>
    [SugarColumn(ColumnDescription = "跳转链接", Length = 500)]
    [MaxLength(500)]
    public string? JumpLink { get; set; }



    /// <summary>
    /// 位置
    /// </summary>
    [SugarColumn(ColumnDescription = "位置")]
    public int? Position { get; set; } = 1;



    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int? Sort { get; set; } = 100;



    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public EnumBannerStatus? Status { get; set; }


}
