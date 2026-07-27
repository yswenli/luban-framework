/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.ConsoleApp.Commands
*文件名： SessionCommand
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：Session 命令 - 管理会话
*
*****************************************************************************/
using System;
using System.Threading.Tasks;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Sessions;
using Microsoft.Extensions.Configuration;

namespace LuBan.AIAgent.ConsoleApp.Commands;

/// <summary>
/// Session 命令 - 管理会话
/// </summary>
public class SessionCommand : CommandBase
{
    private readonly ISessionManager _sessionManager;

    /// <summary>
    /// 命令名称
    /// </summary>
    public override string Name => "session";

    /// <summary>
    /// 命令描述
    /// </summary>
    public override string Description => "管理对话会话";

    /// <summary>
    /// 创建命令实例
    /// </summary>
    public SessionCommand(ConfigManager configManager, IConfiguration configuration, ISessionManager sessionManager)
        : base(configManager, configuration)
    {
        _sessionManager = sessionManager;
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    public override async Task ExecuteAsync()
    {
        Console.WriteLine();
        Console.WriteLine("会话管理：");
        Console.WriteLine();

        var currentSession = _sessionManager.CurrentSession;
        if (currentSession != null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"当前会话: {currentSession.Title ?? "未命名"}");
            Console.ResetColor();
            Console.WriteLine($"  ID: {currentSession.SessionId}");
            Console.WriteLine($"  消息数: {currentSession.MessageCount}");
            Console.WriteLine($"  Token数: {currentSession.TotalTokens}");
            Console.WriteLine($"  创建时间: {currentSession.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();
        }

        Console.WriteLine("操作:");
        Console.WriteLine("  1. new      - 创建新会话");
        Console.WriteLine("  2. list     - 列出所有会话");
        Console.WriteLine("  3. switch   - 切换到历史会话");
        Console.WriteLine("  4. title    - 修改当前会话标题");
        Console.WriteLine("  5. clear    - 清除当前会话消息");
        Console.WriteLine("  6. delete   - 删除当前会话");
        Console.WriteLine("  7. stats    - 查看会话统计");
        Console.WriteLine();

        Console.Write("请输入操作: ");
        var input = Console.ReadLine()?.Trim().ToLower();

        if (string.IsNullOrEmpty(input))
            return;

        await ExecuteSubCommand(input);
    }

    /// <summary>
    /// 执行带子命令的命令
    /// </summary>
    public override async Task<bool> ExecuteAsync(string[] args)
    {
        if (args.Length == 0)
            return false;

        var subCommand = args[0].ToLower();
        var restArgs = args.Length > 1 ? string.Join(' ', args[1..]) : null;
        await ExecuteSubCommand(subCommand, restArgs);
        return true;
    }

    /// <summary>
    /// 执行子命令
    /// </summary>
    private async Task ExecuteSubCommand(string input, string? extraArg = null)
    {
        switch (input)
        {
            case "1":
            case "new":
                await CreateNewSessionAsync();
                break;

            case "2":
            case "list":
                await ListSessionsAsync();
                break;

            case "3":
            case "switch":
                await SwitchSessionAsync(extraArg);
                break;

            case "4":
            case "title":
                await UpdateTitleAsync();
                break;

            case "5":
            case "clear":
                await ClearMessagesAsync();
                break;

            case "6":
            case "delete":
                await DeleteSessionAsync();
                break;

            case "7":
            case "stats":
                await ShowStatsAsync();
                break;

            default:
                Console.WriteLine($"未知操作: {input}");
                break;
        }
    }

    private async Task CreateNewSessionAsync()
    {
        Console.Write("请输入会话标题（可选）: ");
        var title = Console.ReadLine()?.Trim();

        var session = await _sessionManager.CreateSessionAsync(userId: "default", title: title);
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ 已创建新会话: {session.Title}");
        Console.ResetColor();
        Console.WriteLine($"  ID: {session.SessionId}");
    }

    private async Task ListSessionsAsync()
    {
        var sessions = await _sessionManager.GetUserSessionsAsync("default");
        
        Console.WriteLine();
        Console.WriteLine("历史会话：");

        int index = 1;
        foreach (var session in sessions)
        {
            var isCurrent = _sessionManager.CurrentSession?.SessionId == session.SessionId;
            var marker = isCurrent ? " (当前)" : "";
            
            Console.WriteLine($"  {index}. {session.Title ?? "未命名"}{marker}");
            Console.WriteLine($"     消息: {session.MessageCount} | Token: {session.TotalTokens} | {session.CreatedAt:yyyy-MM-dd HH:mm}");
            index++;
        }

        if (index == 1)
        {
            Console.WriteLine("  （无历史会话）");
        }
    }

    private async Task SwitchSessionAsync(string? extraArg = null)
    {
        var sessions = (await _sessionManager.GetUserSessionsAsync("default")).ToList();
        
        if (sessions.Count == 0)
        {
            Console.WriteLine("没有可切换的会话");
            return;
        }

        // 如果直接传了序号，尝试直接切换
        if (!string.IsNullOrEmpty(extraArg) && int.TryParse(extraArg, out int directIndex))
        {
            if (directIndex < 1 || directIndex > sessions.Count)
            {
                Console.WriteLine($"无效的序号: {directIndex}，有效范围: 1-{sessions.Count}");
                return;
            }

            var selected = sessions[directIndex - 1];
            await _sessionManager.SetCurrentSessionAsync(selected.SessionId);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ 已切换到会话: {selected.Title}");
            Console.ResetColor();
            return;
        }

        // 交互式选择
        Console.WriteLine();
        Console.WriteLine("选择要切换的会话：");
        Console.WriteLine();

        for (int i = 0; i < sessions.Count; i++)
        {
            var session = sessions[i];
            var isCurrent = _sessionManager.CurrentSession?.SessionId == session.SessionId;
            var marker = isCurrent ? " (当前)" : "";
            
            Console.WriteLine($"  {i + 1}. {session.Title ?? "未命名"}{marker}");
            Console.WriteLine($"     消息: {session.MessageCount} | Token: {session.TotalTokens}");
        }

        Console.WriteLine();
        Console.Write("请输入序号 (或 0 取消): ");
        var input = Console.ReadLine()?.Trim();

        if (!int.TryParse(input, out int index) || index < 0 || index > sessions.Count)
        {
            Console.WriteLine("无效的选择");
            return;
        }

        if (index == 0)
        {
            Console.WriteLine("已取消");
            return;
        }

        var selectedSession = sessions[index - 1];
        await _sessionManager.SetCurrentSessionAsync(selectedSession.SessionId);
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ 已切换到会话: {selectedSession.Title}");
        Console.ResetColor();
    }

    private async Task UpdateTitleAsync()
    {
        var currentSession = _sessionManager.CurrentSession;
        if (currentSession == null)
        {
            Console.WriteLine("当前没有活动会话");
            return;
        }

        Console.WriteLine($"当前标题: {currentSession.Title}");
        Console.Write("请输入新标题: ");
        var title = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(title))
        {
            Console.WriteLine("已取消");
            return;
        }

        await _sessionManager.UpdateSessionTitleAsync(currentSession.SessionId, title);
        Console.WriteLine("✓ 标题已更新");
    }

    private async Task ClearMessagesAsync()
    {
        var currentSession = _sessionManager.CurrentSession;
        if (currentSession == null)
        {
            Console.WriteLine("当前没有活动会话");
            return;
        }

        Console.Write("确认清除所有消息？(y/N): ");
        var confirm = Console.ReadLine()?.Trim().ToLower();

        if (confirm == "y")
        {
            await _sessionManager.ClearMessagesAsync(currentSession.SessionId);
            Console.WriteLine("✓ 已清除消息");
        }
        else
        {
            Console.WriteLine("已取消");
        }
    }

    private async Task DeleteSessionAsync()
    {
        var currentSession = _sessionManager.CurrentSession;
        if (currentSession == null)
        {
            Console.WriteLine("当前没有活动会话");
            return;
        }

        Console.Write($"确认删除会话 \"{currentSession.Title}\"？(y/N): ");
        var confirm = Console.ReadLine()?.Trim().ToLower();

        if (confirm == "y")
        {
            await _sessionManager.DeleteSessionAsync(currentSession.SessionId);
            Console.WriteLine("✓ 会话已删除");
        }
        else
        {
            Console.WriteLine("已取消");
        }
    }

    private async Task ShowStatsAsync()
    {
        var currentSession = _sessionManager.CurrentSession;
        if (currentSession == null)
        {
            Console.WriteLine("当前没有活动会话");
            return;
        }

        var stats = await _sessionManager.GetSessionStatsAsync(currentSession.SessionId);
        
        Console.WriteLine();
        Console.WriteLine("会话统计：");
        Console.WriteLine($"  总消息数: {stats.TotalMessages}");
        Console.WriteLine($"  用户消息: {stats.UserMessages}");
        Console.WriteLine($"  AI 消息: {stats.AssistantMessages}");
        Console.WriteLine($"  总 Token: {stats.TotalTokens}");
        Console.WriteLine($"  平均长度: {stats.AverageMessageLength:F1}");
    }
}