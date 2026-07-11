/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
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
    [DataMember(Name = "displayName")]
    public string DisplayName { get; set; }

    [DataMember(Name = "email")]
    public string Email { get; set; }

    [DataMember(Name = "finishAudit")]
    public bool? FinishAudit { get; set; }

    [DataMember(Name = "headImg")]
    public string? HeadImg { get; set; }

    [DataMember(Name = "mobileNum")]
    public string? MobileNum { get; set; }

    [DataMember(Name = "nickName")]
    public string? NickName { get; set; }

    [DataMember(Name = "platform")]
    public string? Platform { get; set; }

    [DataMember(Name = "queId")]
    public string? QueId { get; set; }

    [DataMember(Name = "queTitle")]
    public string? QueTitle { get; set; }

    [DataMember(Name = "remark")]
    public string Remark { get; set; }

    [DataMember(Name = "status")]
    public bool? Status { get; set; }

    [DataMember(Name = "type")]
    public int? Type { get; set; }

    [DataMember(Name = "uid")]
    public int? Uid { get; set; }

    [DataMember(Name = "userId")]
    public string UserId { get; set; }

    [DataMember(Name = "userRepresentation")]
    public string UserRepresentation { get; set; }

    [DataMember(Name = "wechatHeadImg")]
    public string? WechatHeadImg { get; set; }

    [DataMember(Name = "wechatName")]
    public string? WechatName { get; set; }
}
