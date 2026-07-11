/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.AspNetCore
*文件名： BaseHub
*版本号： V1.0.0.0
*唯一标识：17948d22-5c33-4ce6-9eb4-87bddc7d4bb4
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/20 13:41:12
*描述：SignalR Hub
*
*=================================================
*修改标记
*修改时间：2024/8/20 13:41:12
*修改人： yswenli
*版本号： V1.0.0.0
*描述：SignalR Hub
*
*****************************************************************************/
namespace LuBan.Web.Core.AspNetCore.SignalR;

/// <summary>
/// SignalR Hub,
/// jwt放在请求地址的 query string [access_token]
/// </summary>
//[Authorize]
public class CommonHub : Hub<IHubClient>, IHubServer
{
    /// <summary>
    /// 连接事件
    /// </summary>
    /// <returns></returns>
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        await JoinGroupAsync("hubGroup");
    }

    /// <summary>
    /// 断连事件
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await LeaveGroupAsync("hubGroup");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// 加入组
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    public async Task JoinGroupAsync(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// 离开组
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    public async Task LeaveGroupAsync(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// 发送消息给所有客户端
    /// </summary>
    /// <param name="user"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendMessageAsync(string user, string message)
    {
        await Clients.All.ReceiveMessage(user, message);
    }

    /// <summary>
    /// 发送消息给当前客户端
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task ReplayMessageAsync(string message)
    {
        await Clients.Caller.ReplayMessage(message);
    }

    /// <summary>
    /// 发送消息给指定组
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="user"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendGroupMessageAsync(string groupName, string user, string message)
    {
        await Clients.Group(groupName).ReceiveMessage(groupName, user, message);
    }

}



