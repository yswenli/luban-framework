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
    /// <summary>
    /// 登录后返回的访问令牌字符串（接口字段 authToken）。
    /// </summary>
    [DataMember(Name = "authToken")]
    public string AuthToken { get; set; }

    /// <summary>
    /// 令牌对应的登录用户信息（接口字段 userInfo）。
    /// </summary>
    [DataMember(Name = "userInfo")]
    public UserInfo UserInfo { get; set; }
}


/// <summary>
/// TalkMed 平台返回的登录用户资料。
/// </summary>
[DataContract]
public class UserInfo
{
    /// <summary>
    /// 用户主键 Id（接口字段 id）。
    /// </summary>
    [DataMember(Name = "id")]
    public int Id { get; set; }

    /// <summary>
    /// 用户角色 Id（接口字段 role_id）。
    /// </summary>
    [DataMember(Name = "role_id")]
    public int RoleId { get; set; }

    /// <summary>
    /// 用户所在房间标识（接口字段 room_id）。
    /// </summary>
    [DataMember(Name = "room_id")]
    public string RoomId { get; set; }

    /// <summary>
    /// 用户昵称（接口字段 nickname）。
    /// </summary>
    [DataMember(Name = "nickname")]
    public string Nickname { get; set; }

    /// <summary>
    /// 用户真实姓名（接口字段 realname）。
    /// </summary>
    [DataMember(Name = "realname")]
    public string Realname { get; set; }

    /// <summary>
    /// 用户手机号码（接口字段 mobile）。
    /// </summary>
    [DataMember(Name = "mobile")]
    public string Mobile { get; set; }

    /// <summary>
    /// 用户电子邮箱（接口字段 email）。
    /// </summary>
    [DataMember(Name = "email")]
    public string Email { get; set; }

    /// <summary>
    /// 用户头像地址（接口字段 avatar）。
    /// </summary>
    [DataMember(Name = "avatar")]
    public string Avatar { get; set; }

    /// <summary>
    /// 用户所属公司 Id（接口字段 company_id）。
    /// </summary>
    [DataMember(Name = "company_id")]
    public int CompanyId { get; set; }
}
