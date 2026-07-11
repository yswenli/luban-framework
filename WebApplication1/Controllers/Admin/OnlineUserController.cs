using LuBan.Web.Core.OnlineUser;
using WebApplication1.Models;
using WebApplication1.Services.ApiServices;

namespace WebApplication1.Controllers.Admin;

/// <summary>
/// 在线用户管理
/// </summary>
public class OnlineUserController : BaseAdminController
{
    /// <summary>
    /// 获取在线用户分页列表
    /// </summary>
    [DisplayName("获取在线用户分页列表"), HttpPost]
    public async Task<PagedList<OnlineUserSession>?> PageAsync([FromBody] PageOnlineUserInput input)
    {
        return await OnlineUserService.Instance.PageAsync(input);
    }

    /// <summary>
    /// 踢用户下线
    /// </summary>
    [DisplayName("踢用户下线"), HttpPost]
    public async Task KickAsync([FromBody] KickOnlineUserInput input)
    {
        await OnlineUserService.Instance.KickAsync(input);
    }

    /// <summary>
    /// 禁用用户
    /// </summary>
    [DisplayName("禁用用户"), HttpPost]
    public async Task DisableAsync([FromBody] KickOnlineUserInput input)
    {
        await OnlineUserService.Instance.DisableAsync(input);
    }

    /// <summary>
    /// 启用用户
    /// </summary>
    [DisplayName("启用用户"), HttpPost]
    public async Task EnableAsync([FromBody] KickOnlineUserInput input)
    {
        await OnlineUserService.Instance.EnableAsync(input);
    }
}
