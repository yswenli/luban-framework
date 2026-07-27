/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.ConsoleApp.Commands
*文件名： MCPCommand
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：MCP 命令 - 查看和管理 MCP 客户端
*
*****************************************************************************/
using System;
using System.Threading.Tasks;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.MCP;
using Microsoft.Extensions.Configuration;

namespace LuBan.AIAgent.ConsoleApp.Commands;

/// <summary>
/// MCP 命令 - 查看和管理 MCP 客户端
/// </summary>
public class MCPCommand : CommandBase
{
    private readonly MCPRegistry _mcpRegistry;

    /// <summary>
    /// 命令名称
    /// </summary>
    public override string Name => "mcp";

    /// <summary>
    /// 命令描述
    /// </summary>
    public override string Description => "查看和管理 MCP (Model Context Protocol) 客户端";

    /// <summary>
    /// 创建命令实例
    /// </summary>
    public MCPCommand(ConfigManager configManager, IConfiguration configuration, MCPRegistry mcpRegistry)
        : base(configManager, configuration)
    {
        _mcpRegistry = mcpRegistry;
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    public override async Task ExecuteAsync()
    {
        Console.WriteLine();
        Console.WriteLine("MCP (Model Context Protocol) 客户端：");
        Console.WriteLine();

        var clients = _mcpRegistry.GetAll();
        if (clients.Count == 0)
        {
            Console.WriteLine("  （无）");
        }
        else
        {
            foreach (var client in clients)
            {
                var status = client.IsConnected ? "🟢 已连接" : "⚪ 未连接";
                Console.WriteLine($"  {status} {client.Name}");
                Console.WriteLine($"     {client.Description}");
                Console.WriteLine();
            }
        }

        Console.WriteLine("提示:");
        Console.WriteLine("  - MCP 允许 AI 连接外部工具和数据源");
        Console.WriteLine("  - 使用 'mcp connect <name>' 连接客户端");
        Console.WriteLine("  - 使用 'mcp tools <name>' 查看可用工具");
        Console.WriteLine();

        // 交互式操作
        Console.Write("请输入命令（或按回车返回）: ");
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input))
            return;

        var parts = input.Split(' ', 2);
        var command = parts[0].ToLower();
        var arg = parts.Length > 1 ? parts[1] : "";

        switch (command)
        {
            case "connect":
                await ConnectClientAsync(arg);
                break;

            case "tools":
                await ShowToolsAsync(arg);
                break;

            default:
                Console.WriteLine($"未知命令: {command}");
                break;
        }
    }

    private async Task ConnectClientAsync(string clientName)
    {
        if (string.IsNullOrEmpty(clientName))
        {
            Console.WriteLine("用法: mcp connect <client-name>");
            return;
        }

        var client = _mcpRegistry.Get(clientName);
        if (client == null)
        {
            Console.WriteLine($"未找到客户端: {clientName}");
            return;
        }

        Console.WriteLine($"正在连接 {clientName}...");
        var success = await client.ConnectAsync();
        Console.WriteLine(success ? "✅ 连接成功" : "❌ 连接失败");
    }

    private async Task ShowToolsAsync(string clientName)
    {
        if (string.IsNullOrEmpty(clientName))
        {
            Console.WriteLine("用法: mcp tools <client-name>");
            return;
        }

        var client = _mcpRegistry.Get(clientName);
        if (client == null)
        {
            Console.WriteLine($"未找到客户端: {clientName}");
            return;
        }

        if (!client.IsConnected)
        {
            Console.WriteLine($"客户端 {clientName} 未连接，请先连接");
            return;
        }

        Console.WriteLine($"{clientName} 可用的工具：");
        var tools = await client.ListToolsAsync();
        foreach (var tool in tools)
        {
            Console.WriteLine($"  - {tool.Name}: {tool.Description}");
        }
    }
}