/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Models.SysEntities
*文件名： SysWechatTemplateMsg
*版本号： V1.0.0.0
*唯一标识：3a460eaf-3412-44ff-8a7f-7e050e28c339
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/5/27 11:22:09
*描述：微信模板消息记录表
*
*=================================================
*修改标记
*修改时间：2024/5/27 11:22:09
*修改人： yswenli
*版本号： V1.0.0.0
*描述：微信模板消息记录表
*
*****************************************************************************/
namespace LuBan.Orm.Entities;

/// <summary>
/// 微信模板消息记录表
/// </summary>
[SugarTable("db_wechat_template_msg", "微信模板消息记录表")]
[SysTable]
public class DbWechatTemplateMsg : EntityeDataScoreBase
{
    /// <summary>
    /// 防重入id。对于同一个openid + client_msg_id, 只发送一条消息,10分钟有效,超过10分钟不保证效果。若无防重入需求，可不填
    /// </summary>
    [SugarColumn(ColumnDescription = "防重入id", Length = 64)]
    [Required, MaxLength(64)]
    public string? ClientMsgId { get; set; }
    /// <summary>
    /// 接收者openid
    /// </summary>
    [SugarColumn(ColumnDescription = "接收者openid", Length = 30)]
    [Required, MaxLength(30)]
    public string ToUser { get; set; }
    /// <summary>
    /// 模板ID
    /// </summary>
    [SugarColumn(ColumnDescription = "模板ID", Length = 50)]
    [Required, MaxLength(50)]
    public string TemplateId { get; set; }
    /// <summary>
    /// 模板跳转链接（海外账号没有跳转能力）
    /// </summary>
    [SugarColumn(ColumnDescription = "模板跳转链接", Length = 1500)]
    [MaxLength(1500)]
    public string? Url { get; set; }

    /// <summary>
    /// 模板数据
    /// </summary>
    [SugarColumn(ColumnDescription = "模板数据", Length = 1500)]
    [MaxLength(1500)]
    public string Data { get; set; }

    /// <summary>
    /// 所需跳转到的小程序appid（该小程序appid必须与发模板消息的公众号是绑定关联关系，暂不支持小游戏）
    /// </summary>
    [SugarColumn(ColumnDescription = "小程序appid", Length = 100)]
    [MaxLength(100)]
    public string? MiniProgramAppId { get; set; }
    /// <summary>
    /// 所需跳转到小程序的具体页面路径，支持带参数,（示例index?foo=bar），要求该小程序已发布，暂不支持小游戏
    /// </summary>
    [SugarColumn(ColumnDescription = "小程序页面路径", Length = 300)]
    [MaxLength(300)]
    public string? MiniProgramPagePath { get; set; }

    /// <summary>
    /// 发送状态
    /// </summary>
    [SugarColumn(ColumnDescription = "发送状态码")]
    public int? ErrCode { get; set; }

    /// <summary>
    /// 发送状态消息
    /// </summary>
    [SugarColumn(ColumnDescription = "发送状态消息", Length = 300)]
    public string? ErrMsg { get; set; }
}
