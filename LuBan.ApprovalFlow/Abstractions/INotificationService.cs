namespace LuBan.ApprovalFlow.Abstractions;

/// <summary>
/// 通知服务接口，用于发送审批流程相关的通知消息
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// 发送通知消息
    /// </summary>
    /// <param name="request">通知请求对象</param>
    Task SendAsync(NotificationRequest request);
}

/// <summary>
/// 通知请求对象，包含通知的接收者、标题、内容和类型等信息
/// </summary>
public class NotificationRequest
{
    /// <summary>
    /// 接收通知的用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 通知标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 通知内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 通知类型，如：待办、已办、催办等
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 通知附加数据，可包含业务相关信息
    /// </summary>
    public object? Data { get; set; }
}