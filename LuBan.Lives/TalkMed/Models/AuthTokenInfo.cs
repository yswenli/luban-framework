/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Lives.TalkMed.Models
*文件名： AuthTokenInfo.cs
*版本号： V1.0.0.0
*唯一标识：d19cd85e-edba-4fd8-ad7a-892672ef9bbf
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：AuthTokenInfo 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：AuthTokenInfo 类
*
*****************************************************************************/

namespace LuBan.Lives.TalkMed.Models;

[DataContract]
/// <summary>
/// AuthTokenInfo 模型类
/// </summary>
public class AuthTokenInfo
{
    [DataMember(Name = "authToken")]
    public string AuthToken { get; set; }

    [DataMember(Name = "userInfo")]
    public UserInfo UserInfo { get; set; }
}


[DataContract]
public class UserInfo
{
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "role_id")]
    public int RoleId { get; set; }

    [DataMember(Name = "room_id")]
    public string RoomId { get; set; }

    [DataMember(Name = "nickname")]
    public string Nickname { get; set; }

    [DataMember(Name = "realname")]
    public string Realname { get; set; }

    [DataMember(Name = "mobile")]
    public string Mobile { get; set; }

    [DataMember(Name = "email")]
    public string Email { get; set; }

    [DataMember(Name = "avatar")]
    public string Avatar { get; set; }

    [DataMember(Name = "company_id")]
    public int CompanyId { get; set; }
}
