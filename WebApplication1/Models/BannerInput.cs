namespace Models.Dto;

/// <summary>
/// Banner状态枚举
/// </summary>
[Description("Banner状态枚举")]
public enum EnumBannerStatus
{
    /// <summary>
    /// 发布
    /// </summary>
    [Description("发布")]
    Release = 1,

    /// <summary>
    /// 关闭
    /// </summary>
    [Description("关闭")]
    Draft = 2,




}

/// <summary>
/// banner搜索分页
/// </summary>
public class BannerInput : BasePageInput
{
    /// <summary>
    /// 关键字
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// 位置
    /// </summary>
    public int? Position { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public EnumBannerStatus? Status { get; set; }

}



public class BannerRequest
{
    /// <summary>
    /// id
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Required(ErrorMessage = "请输入标题")]
    public string Title { get; set; }

    /// <summary>
    /// banner图
    /// </summary>
    [Required(ErrorMessage = "请输入banner图")]
    public string TitleImg { get; set; }

    /// <summary>
    /// 跳转链接
    /// </summary>
    public string? JumpLink { get; set; }

    /// <summary>
    /// 位置
    /// </summary>
    public int? Position { get; set; } = 1;


    /// <summary>
    /// 排序
    /// </summary>
    public int? Sort { get; set; } = 100;


    /// <summary>
    /// 状态
    /// </summary>
    public EnumBannerStatus? Status { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    public bool? IsDelete { get; set; }

}


