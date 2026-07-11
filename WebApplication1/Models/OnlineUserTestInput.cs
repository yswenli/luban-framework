namespace WebApplication1.Models;

/// <summary>
/// 写入在线用户会话测试输入
/// </summary>
public class WriteOnlineUserTestInput
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }

    /// <summary>
    /// 用户姓名
    /// </summary>
    public string? UserName { get; set; }
}

/// <summary>
/// 分页查询在线用户测试输入
/// </summary>
public class PageOnlineUserTestInput : BasePageInput
{
    /// <summary>
    /// 租户Id（可选，不传则查全部）
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    /// 用户姓名（可选）
    /// </summary>
    public string? UserName { get; set; }
}
