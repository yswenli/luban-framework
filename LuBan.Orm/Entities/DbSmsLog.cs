/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Models.SysEntities
*文件名： SysSmsLog
*版本号： V1.0.0.0
*唯一标识：4fb0b423-c5d7-4a0a-967c-5d17a23f6015
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/28 11:13:21
*描述：短信息日志表
*
*=================================================
*修改标记
*修改时间：2023/12/28 11:13:21
*修改人： yswenli
*版本号： V1.0.0.0
*描述：短信息日志表
*
*****************************************************************************/
namespace LuBan.Orm.Entities;

/// <summary>
/// 短信息日志表
/// </summary>
[SugarTable("db_sms_log", "短信息日志表")]
[SysTable]
public class DbSmsLog : EntityBase
{
    /// <summary>
    /// 手机号码
    /// </summary>
    [SugarColumn(ColumnDescription = "手机号码", Length = 11)]
    [Required(ErrorMessage = "请输入手机号码"), MaxLength(11)]
    public string PhoneNumber { get; set; }

    /// <summary>
    /// 模板id
    /// </summary>
    [SugarColumn(ColumnDescription = "模板id")]
    [Required(ErrorMessage = "请输入模板id")]
    public long TemplateId { get; set; }

    /// <summary>
    /// 短信内容
    /// </summary>
    [SugarColumn(ColumnDescription = "短信内容", Length = 150)]
    [Required(ErrorMessage = "请输入短信内容"), MaxLength(150)]
    public string Msg { get; set; }

    /// <summary>
    /// 发送状态
    /// </summary>
    [SugarColumn(ColumnDescription = "发送状态")]
    [Required(ErrorMessage = "请输入发送状态")]
    public bool Status { get; set; }

    /// <summary>
    /// 异常信息
    /// </summary>
    [SugarColumn(ColumnDescription = "短信内容", Length = 300)]
    [MaxLength(300)]
    public string? Error { get; set; }
}
