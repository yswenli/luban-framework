/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core.AspNetCore.SignalR
*文件名： SimpleClient.cs
*版本号： V1.0.0.0
*唯一标识：563af4a0-d0ad-4cc3-9ae8-92580f84b9d3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:29
*描述：SimpleClient 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：SimpleClient 类
*
*****************************************************************************/

namespace LuBan.Web.Core.AspNetCore.SignalR;

/// <summary>
/// Simple SignalR client
/// </summary>
public class SimpleClient : IDisposable, IAsyncDisposable
{
    readonly HubConnection connection;

    public event Action<string, string> OnMessageReceived;

    /// <summary>
    /// Simple SignalR client
    /// </summary>
    /// <param name="wsUrl"></param>
    /// <param name="timeOut">心跳间隔与服务端超时时间（秒）。服务端超时至少应为心跳间隔的 2 倍。</param>
    public SimpleClient(string wsUrl = "wss://localhost:7000/hubs/common", int timeOut = 30)
    {
        // 服务端超时设为心跳间隔的 2 倍，避免网络抖动导致误判断连
        var keepAlive = TimeSpan.FromSeconds(timeOut > 0 ? timeOut : 30);
        var serverTimeout = TimeSpan.FromSeconds(keepAlive.TotalSeconds * 2);

        connection = new HubConnectionBuilder()
            .WithKeepAliveInterval(keepAlive)
            .WithServerTimeout(serverTimeout)
            .WithUrl(wsUrl)
            .WithAutomaticReconnect()
            .WithStatefulReconnect()
            .Build();

        //接收消息
        connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            var newMessage = $"{user}: {message}";
            ConsoleUtil.WriteLine(newMessage);
            OnMessageReceived?.Invoke(user, message);
        });

        //断开事件
        connection.Closed += async (ex) =>
        {
            ConsoleUtil.WriteLine($"Connection closed.{ex}");
            await Task.CompletedTask;
        };
    }

    /// <summary>
    /// 连接Hub
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        await connection.StartAsync();
    }

    /// <summary>
    /// 加入群组
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task JoinGroupAsync(string groupName, CancellationToken cancellationToken = default)
    {
        await connection.InvokeAsync("JoinGroupAsync", groupName, cancellationToken);
    }

    /// <summary>
    /// 离开群组
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task LeaveGroupAsync(string groupName, CancellationToken cancellationToken = default)
    {
        await connection.InvokeAsync("LeaveGroupAsync", groupName, cancellationToken);
    }

    /// <summary>
    /// 发送全体消息
    /// </summary>
    /// <param name="user"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SendMessageAsync(string user, string message, CancellationToken cancellationToken = default)
    {
        await connection.InvokeAsync("SendMessageAsync", user, message, cancellationToken);
    }

    /// <summary>
    /// 回复消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ReplayMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        await connection.InvokeAsync("ReplayMessageAsync", message, cancellationToken);
    }

    /// <summary>
    /// 发送群组消息
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="user"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SendGroupMessageAsync(string groupName, string user, string message, CancellationToken cancellationToken = default)
    {
        await connection.InvokeAsync("SendGroupMessageAsync", groupName, user, message, cancellationToken);
    }



    /// <summary>
    /// 释放资源
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        if (connection.State != HubConnectionState.Disconnected)
        {
            await connection.StopAsync();
        }
        await connection.DisposeAsync();
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }
}
