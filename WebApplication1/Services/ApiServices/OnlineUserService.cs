/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Services.ApiServices
*文件名： OnlineUserService.cs
*版本号： V1.0.0.0
*唯一标识：8e21c7c6-7880-4fbe-b6b9-673231f21eca
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：OnlineUserService 服务类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：OnlineUserService 服务类
*
*****************************************************************************/

using LuBan.Web.Core.OnlineUser;

using WebApplication1.Models.Vos;

namespace WebApplication1.Services.ApiServices;

/// <summary>
/// 在线用户服务
/// </summary>
public class OnlineUserService : BaseService<OnlineUserService>
{
    /// <summary>
    /// 获取在线用户分页列表
    /// </summary>
    public async Task<PagedList<OnlineUserSession>> PageAsync(PageOnlineUserInput input)
    {
        long? tenantId = SessionUser.IsSuperAdmin ? null : SessionUser.TenantId;
        return await OnlineUserStoreProvider.Store.PageAsync(
            input.Page, input.PageSize, tenantId, input.UserName);
    }

    /// <summary>
    /// 踢用户下线
    /// </summary>
    public async Task KickAsync(KickOnlineUserInput input)
    {
        if (input.Id == SessionUser.UserId)
            throw FriendlyError.Ex("不能将自己踢下线");

        ValidateTenantAccess(input.TenantId);

        await OnlineUserStoreProvider.Store.RemoveAsync(input.TenantId, input.Id);
        MemoryCache.Instance.Delete($"{CacheConst.KeyUserOnline}{input.TenantId}:{input.Id}");
    }

    /// <summary>
    /// 禁用用户
    /// </summary>
    public async Task DisableAsync(KickOnlineUserInput input)
    {
        if (input.Id == SessionUser.UserId)
            throw FriendlyError.Ex("不能禁用当前登录账号");

        ValidateTenantAccess(input.TenantId);

        await UserService.Instance.SetStatusAsync(new UserInput
        {
            Id = input.Id,
            Status = EnumEnableStatus.Disable
        });
        await KickAsync(input);
    }

    /// <summary>
    /// 启用用户
    /// </summary>
    public async Task EnableAsync(KickOnlineUserInput input)
    {
        if (input.Id == SessionUser.UserId)
            throw FriendlyError.Ex("不能启用当前登录账号，如需启用请联系其他管理员");

        ValidateTenantAccess(input.TenantId);

        await UserService.Instance.SetStatusAsync(new UserInput
        {
            Id = input.Id,
            Status = EnumEnableStatus.Enable
        });
    }

    private static void ValidateTenantAccess(long targetTenantId)
    {
        if (!SessionUser.IsSuperAdmin && targetTenantId != SessionUser.TenantId)
            throw FriendlyError.Ex("无权操作其他租户的用户");
    }
}
