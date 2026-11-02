/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Qingflow.Models
*文件名： AuditUser
*版本号： V1.0.0.0
*唯一标识：915894e1-e3e2-447a-a130-09ddd757108c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/24 16:56:12
*描述：申请人信息
*
*=================================================
*修改标记
*修改时间：2024/12/24 16:56:12
*修改人： yswenli
*版本号： V1.0.0.0
*描述：申请人信息
*
*****************************************************************************/
namespace LuBan.Qingflow.Models;

/// <summary>
/// 申请人信息
/// </summary>
[DataContract]
public class AuditUser
{
    /// <summary>
    /// 审批人的显示名称（接口字段 displayName）。
    /// </summary>
    [DataMember(Name = "displayName")]
    public string DisplayName { get; set; }

    /// <summary>
    /// 审批人的邮箱地址（接口字段 email）。
    /// </summary>
    [DataMember(Name = "email")]
    public string Email { get; set; }

    /// <summary>
    /// 该审批人是否已完成审批（接口字段 finishAudit）。
    /// </summary>
    [DataMember(Name = "finishAudit")]
    public bool? FinishAudit { get; set; }

    /// <summary>
    /// 审批人的头像地址（接口字段 headImg）。
    /// </summary>
    [DataMember(Name = "headImg")]
    public string? HeadImg { get; set; }

    /// <summary>
    /// 审批人的手机号码（接口字段 mobileNum）。
    /// </summary>
    [DataMember(Name = "mobileNum")]
    public string? MobileNum { get; set; }

    /// <summary>
    /// 审批人的昵称（接口字段 nickName）。
    /// </summary>
    [DataMember(Name = "nickName")]
    public string? NickName { get; set; }

    /// <summary>
    /// 审批人所属的平台标识（接口字段 platform）。
    /// </summary>
    [DataMember(Name = "platform")]
    public string? Platform { get; set; }

    /// <summary>
    /// 审批人对应的字段ID（接口字段 queId）。
    /// </summary>
    [DataMember(Name = "queId")]
    public string? QueId { get; set; }

    /// <summary>
    /// 审批人对应的字段标题（接口字段 queTitle）。
    /// </summary>
    [DataMember(Name = "queTitle")]
    public string? QueTitle { get; set; }

    /// <summary>
    /// 审批意见或说明（接口字段 remark）。
    /// </summary>
    [DataMember(Name = "remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 审批人的状态（接口字段 status）。
    /// </summary>
    [DataMember(Name = "status")]
    public bool? Status { get; set; }

    /// <summary>
    /// 审批人的类型（接口字段 type）。
    /// </summary>
    [DataMember(Name = "type")]
    public int? Type { get; set; }

    /// <summary>
    /// 审批人在轻流中的数字用户ID（接口字段 uid）。
    /// </summary>
    [DataMember(Name = "uid")]
    public int? Uid { get; set; }

    /// <summary>
    /// 审批人的用户ID（接口字段 userId）。
    /// </summary>
    [DataMember(Name = "userId")]
    public string UserId { get; set; }

    /// <summary>
    /// 审批人的展示形式，用于界面呈现（接口字段 userRepresentation）。
    /// </summary>
    [DataMember(Name = "userRepresentation")]
    public string UserRepresentation { get; set; }

    /// <summary>
    /// 审批人的微信头像地址（接口字段 wechatHeadImg）。
    /// </summary>
    [DataMember(Name = "wechatHeadImg")]
    public string? WechatHeadImg { get; set; }

    /// <summary>
    /// 审批人的微信名称（接口字段 wechatName）。
    /// </summary>
    [DataMember(Name = "wechatName")]
    public string? WechatName { get; set; }
}
