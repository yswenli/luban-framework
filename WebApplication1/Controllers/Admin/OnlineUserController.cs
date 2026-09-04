/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Controllers.Admin
*文件名： OnlineUserController.cs
*版本号： V1.0.0.0
*唯一标识：922bbd16-7a55-43ba-abb2-c2d12786d7b6
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：OnlineUserController 控制器
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：OnlineUserController 控制器
*
*****************************************************************************/

using LuBan.Web.Core.OnlineUser;

using WebApplication1.Models.Vos;
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
