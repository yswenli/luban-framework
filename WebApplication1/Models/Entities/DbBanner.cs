/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Models.Entities
*文件名： DbBanner.cs
*版本号： V1.0.0.0
*唯一标识：c890953e-7d45-4700-99f6-115ae4971ca8
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：DbBanner 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：DbBanner 类
*
*****************************************************************************/

using WebApplication1.Models.Vos;

namespace WebApplication1.Models.Entities;

[SugarTable("db_banner", "banner表")]
public class DbBanner : EntityDataScoreBase
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
