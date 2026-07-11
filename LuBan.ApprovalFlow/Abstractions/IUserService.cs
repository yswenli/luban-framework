namespace LuBan.ApprovalFlow.Abstractions;

/// <summary>
/// 用户服务接口，提供用户信息查询和权限验证功能
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 根据用户ID获取用户信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户信息，不存在则返回 null</returns>
    Task<UserInfo?> GetUserByIdAsync(long userId);

    /// <summary>
    /// 根据角色获取用户列表
    /// </summary>
    /// <param name="role">角色标识</param>
    /// <returns>拥有该角色的用户列表</returns>
    Task<List<UserInfo>> GetUsersByRoleAsync(string role);

    /// <summary>
    /// 获取指定用户所在部门的所有用户
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>同部门用户列表</returns>
    Task<List<UserInfo>> GetDepartmentUsersAsync(long userId);

    /// <summary>
    /// 获取指定用户的上级主管
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>上级主管信息，不存在则返回 null</returns>
    Task<UserInfo?> GetSupervisorAsync(long userId);

    /// <summary>
    /// 验证用户是否有审批权限
    /// </summary>
    /// <param name="recordId">流程记录ID</param>
    /// <param name="userId">用户ID</param>
    /// <param name="roles">角色列表，可选</param>
    /// <returns>是否有审批权限</returns>
    Task<bool> ValidateApprovalPermissionAsync(long recordId, long userId, List<string>? roles);
}