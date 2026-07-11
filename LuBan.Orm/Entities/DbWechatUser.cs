/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：
*文件名： 
*版本号： V1.0.0.0
*唯一标识：a5bb6173-b22d-4edd-852f-9b02bb075167
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/11/03 14:00:15
*描述：微信用户表
*
*=================================================
*修改标记
*修改时间：2023/11/03 14:00:15
*修改人： yswenli
*版本号： V1.0.0.0
*描述：微信用户表
*
*****************************************************************************/

namespace LuBan.Orm.Entities;

/// <summary>
/// 系统微信用户表
/// </summary>
[SugarTable("db_wechat_user", "系统微信用户表")]
[SysTable]
public class DbWechatUser : EntityeDataScoreBase
{
    /// <summary>
    /// 系统用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "系统用户Id")]
    public long UserId { get; set; }

    /// <summary>
    /// 系统用户
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(UserId)), JsonIgnore]
    public DbUser User { get; set; }

    /// <summary>
    /// 平台类型
    /// </summary>
    [SugarColumn(ColumnDescription = "平台类型")]
    public EnumPlatformType PlatformType { get; set; } = EnumPlatformType.微信公众号;

    /// <summary>
    /// OpenId
    /// </summary>
    [SugarColumn(ColumnDescription = "OpenId", Length = 64)]
    [Required, MaxLength(64)]
    public string OpenId { get; set; }

    /// <summary>
    /// 会话密钥
    /// </summary>
    [SugarColumn(ColumnDescription = "会话密钥", Length = 256)]
    [MaxLength(256)]
    public string? SessionKey { get; set; }

    /// <summary>
    /// UnionId
    /// </summary>
    [SugarColumn(ColumnDescription = "UnionId", Length = 64)]
    [MaxLength(64)]
    public string? UnionId { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [SugarColumn(ColumnDescription = "昵称", Length = 64)]
    [MaxLength(64)]
    public string? NickName { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    [SugarColumn(ColumnDescription = "头像", Length = 256)]
    [MaxLength(256)]
    public string? Avatar { get; set; }

    /// <summary>
    /// AccessToken
    /// </summary>
    [SugarColumn(ColumnDescription = "AccessToken", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? AccessToken { get; set; }

    /// <summary>
    /// RefreshToken
    /// </summary>
    [SugarColumn(ColumnDescription = "RefreshToken", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    [SugarColumn(ColumnDescription = "ExpiresIn")]
    public int? ExpiresIn { get; set; }

    /// <summary>
    /// 用户授权的作用域，使用逗号分隔
    /// </summary>
    [SugarColumn(ColumnDescription = "授权作用域", Length = 64)]
    [MaxLength(64)]
    public string? Scope { get; set; }
    /// <summary>
    /// 是否关注
    /// </summary>
    [SugarColumn(ColumnDescription = "是否关注")]
    public bool? IsSubscribed { get; set; } = false;
    /// <summary>
    /// 关注时间
    /// </summary>
    [SugarColumn(ColumnDescription = "关注时间")]
    public DateTime? SubscribeTime { get; set; }
    /// <summary>
    /// 关注时扫码值
    /// </summary>
    [SugarColumn(ColumnDescription = "关注时扫码值", Length = 64)]
    public string? Scene { get; set; }

}