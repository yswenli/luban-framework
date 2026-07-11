/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.AspNetCore.SignalR
*文件名： IHubClient
*版本号： V1.0.0.0
*唯一标识：624e2ec3-984e-4e0f-8b64-e99ac4342eef
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/11 13:57:22
*描述：SignalR Hub 客户端接口
*
*=====================================================================
*修改标记
*修改时间：2024/8/9 13:57:22
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：SignalR Hub 客户端接口
*
*****************************************************************************/
namespace LuBan.Web.Core.AspNetCore.SignalR;


/// <summary>
/// SignalR Hub 客户端接口
/// </summary>
public interface IHubClient
{
    /// <summary>
    /// 接收群组消息
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="user"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task ReceiveMessage(string groupName, string user, string message);

    /// <summary>
    /// 接收消息
    /// </summary>
    /// <param name="user"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task ReceiveMessage(string user, string message);
    /// <summary>
    /// 接收消息
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task ReplayMessage(string message);
}
