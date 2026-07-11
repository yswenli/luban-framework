namespace WebApplication1.Models;

/// <summary>
/// 在线用户分页查询
/// </summary>
public class PageOnlineUserInput : BasePageInput
{
    /// <summary>
    /// 用户姓名
    /// </summary>
    public string? UserName { get; set; }
}

/// <summary>
/// 踢用户下线
/// </summary>
public class KickOnlineUserInput : BaseIdInput
{
    /// <summary>
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }
}
