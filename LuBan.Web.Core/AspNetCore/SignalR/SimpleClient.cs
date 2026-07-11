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
    /// <param name="timeOut"></param>
    public SimpleClient(string wsUrl = "wss://localhost:7000/hubs/common", int timeOut = 30)
    {
        connection = new HubConnectionBuilder()
            .WithKeepAliveInterval(TimeSpan.FromSeconds(30))
            .WithKeepAliveInterval(TimeSpan.FromSeconds(30))
            .WithServerTimeout(TimeSpan.FromSeconds(30))
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
