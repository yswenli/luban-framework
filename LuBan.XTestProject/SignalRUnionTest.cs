using LuBan.Common;
using LuBan.Web.Core.AspNetCore.SignalR;

using Microsoft.AspNetCore.SignalR.Client;

using System.Diagnostics;

namespace LuBan.XTestProject
{

    /// <summary>
    /// 测试SignalR功能
    /// </summary>
    [TestClass]
    public class SignalRUnionTest
    {

        /// <summary>
        /// 测试signalR 1
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestMethod1Async()
        {
            HubConnection connection = new HubConnectionBuilder()
                .WithKeepAliveInterval(TimeSpan.FromSeconds(30))
                .WithKeepAliveInterval(TimeSpan.FromSeconds(30))
                .WithServerTimeout(TimeSpan.FromSeconds(30))
                .WithUrl("wss://localhost:7000/hubs/common")
                .WithAutomaticReconnect()
                .WithStatefulReconnect()
                .Build();

            //重连
            connection.Reconnecting += (ex) =>
            {
                ConsoleUtil.WriteLine("Reconnecting...");
                return Task.CompletedTask;
            };

            //接收消息
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                var newMessage = $"{user}: {message}";
                ConsoleUtil.WriteLine(newMessage);
            });

            //断开事件
            connection.Closed += async (ex) =>
            {
                ConsoleUtil.WriteLine($"Connection closed.{ex}");
                await Task.CompletedTask;
            };


            //连接
            try
            {
                await connection.StartAsync();
                Debug.Assert(connection.State == HubConnectionState.Connected);
                ConsoleUtil.WriteLine("Connection started");
            }
            catch (Exception ex)
            {
                ConsoleUtil.WriteLine($"{ex}");
            }

            try
            {
                //发送消息
                await connection.InvokeAsync("SendMessageAsync", "yswenli", "Hello World!");
                await connection.InvokeAsync("ReplayMessageAsync", "Hello World You!");
                await connection.InvokeAsync("SendGroupMessageAsync", "hubGroup", "yswenli", "Hello World Group!");

                ConsoleUtil.WriteLine("SendMessage");
            }
            catch (Exception ex)
            {
                ConsoleUtil.WriteLine($"{ex}");
            }


            //断开连接
            await connection.StartAsync();
            await connection.DisposeAsync();

            Assert.IsTrue(connection.State == HubConnectionState.Disconnected);
        }

        /// <summary>
        /// 测试signalR 2
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestMethodAsync2()
        {
            using (var client = new SimpleClient("wss://localhost:7000/hubs/common", 60))
            {
                await client.ConnectAsync();
                await client.JoinGroupAsync("hubGroup");
                await client.SendMessageAsync("yswenli", "Hello World!");
                await client.ReplayMessageAsync("Hello World You!");
                await client.SendGroupMessageAsync("hubGroup", "yswenli", "Hello World Group!");
                await client.LeaveGroupAsync("hubGroup");
            }
        }

    }


}
