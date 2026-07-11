namespace LuBan.ApprovalFlow.Models;

public class AssigneeInfo
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Role { get; set; }
}