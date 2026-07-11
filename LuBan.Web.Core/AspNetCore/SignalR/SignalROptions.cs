/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.AspNetCore
*文件名： SignalROptions
*版本号： V1.0.0.0
*唯一标识：17948d22-5c33-4ce6-9eb4-87bddc7d4bb4
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/20 13:41:12
*描述：SignalR options
*
*=================================================
*修改标记
*修改时间：2024/8/20 13:41:12
*修改人： yswenli
*版本号： V1.0.0.0
*描述：SignalR options
*
*****************************************************************************/

namespace LuBan.Web.Core.AspNetCore.SignalR;

/// <summary>
/// SignalR options
/// </summary>
public class SignalROptions
{
    /// <summary>
    /// SignalR hub url
    /// </summary>
    public string HubUrl { get; set; } = "/hubs/common";

    /// <summary>
    /// 握手超时时间
    /// </summary>
    public int HandshakeTimeout { get; set; } = 30; // seconds

    /// <summary>
    /// 心跳间隔时间
    /// </summary>
    public int KeepAliveInterval { get; set; } = 15; // seconds
    /// <summary>
    /// 空连接超时时间
    /// </summary>
    public int FreeTimeout { get; set; } = 60; // seconds

    /// <summary>
    /// 消息包大小
    /// </summary>
    public int MaximumReceiveMessageSize { get; set; } = 32 * 1024; // 32KB

    /// <summary>
    /// 每连接并行处理数量
    /// </summary>
    public int ParallelCount { get; set; } = 1;

}
