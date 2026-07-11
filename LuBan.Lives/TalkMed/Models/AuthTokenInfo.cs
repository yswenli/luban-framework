namespace LuBan.Lives.TalkMed.Models;

[DataContract]
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
