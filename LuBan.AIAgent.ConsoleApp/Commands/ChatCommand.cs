/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.ConsoleApp.Commands
*文件名： ChatCommand
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：智能对话命令，支持工具调用和 thinking 显示
*
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Services;
using LuBan.AIAgent.Sessions;
using LuBan.Common;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace LuBan.AIAgent.ConsoleApp.Commands;

/// <summary>
/// 智能对话命令，支持工具调用和 thinking 显示
/// </summary>
public class ChatCommand : CommandBase
{
    private readonly ISessionManager _sessionManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<string, Task<bool>>? _executeCommandAsync;

    /// <summary>
    /// 命令名称
    /// </summary>
    public override string Name => "chat";

    /// <summary>
    /// 命令描述
    /// </summary>
    public override string Description => "智能对话（支持工具调用）";

    /// <summary>
    /// 创建命令实例
    /// </summary>
    public ChatCommand(ConfigManager configManager, IConfiguration configuration, ISessionManager sessionManager, IServiceProvider serviceProvider, Func<string, Task<bool>>? executeCommandAsync = null)
        : base(configManager, configuration)
    {
        _sessionManager = sessionManager;
        _serviceProvider = serviceProvider;
        _executeCommandAsync = executeCommandAsync;
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    public override async Task ExecuteAsync()
    {
        if (!ConfigManager.HasSelectedModel)
        {
            WriteError("请先使用 select 命令选择模型");
            return;
        }

        // 创建或获取当前会话
        var currentSession = _sessionManager.CurrentSession;
        if (currentSession == null)
        {
            currentSession = await _sessionManager.CreateSessionAsync(userId: "default", title: "新对话");
            Console.WriteLine($"已创建新会话: {currentSession.SessionId}");
        }

        // 使用注入的 ServiceProvider，而不是创建新的
        // 这样可以确保所有服务（包括 IRetrievalService）都可用
        var serviceProvider = _serviceProvider;

        Console.WriteLine();
        Console.WriteLine("可用工具: 文件系统、脚本执行、浏览器、数据库、Redis、Web请求");
        Console.WriteLine("提示: AI 会自动判断是否需要使用工具来回答你的问题");
        Console.WriteLine("      危险操作（写入、删除、执行脚本）需要用户确认");
        Console.WriteLine("      输入 / 命令可执行操作，如 /session switch 1");
        Console.WriteLine("示例: 帮我查一下D盘下面有哪些目录");
        Console.WriteLine($"当前会话: {currentSession.Title ?? "未命名"}");
        Console.WriteLine("开始对话 (输入 'exit' 返回主菜单)");
        Console.WriteLine();

        try
        {
            // 设置工具确认回调
            ToolConfirmationService.ConfirmationCallback = (toolName, args) =>
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[yellow]⚠️  [bold]危险操作请求: {Markup.Escape(toolName)}[/][/]");
                AnsiConsole.MarkupLine("[yellow]参数:[/]");
                var formattedArgs = ToolConfirmationService.FormatArguments(args, 500);
                foreach (var line in formattedArgs.Split('\n'))
                {
                    AnsiConsole.WriteLine(line);
                }
                AnsiConsole.WriteLine();

                AnsiConsole.Markup("[yellow]是否执行此操作？(y/N): [/]");
                var input = Console.ReadLine()?.Trim().ToLower();
                var confirmed = input == "y" || input == "yes";

                if (confirmed)
                {
                    AnsiConsole.MarkupLine("[green]✓ 已确认执行[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]✗ 已取消执行[/]");
                }

                return confirmed;
            };

            var agentFactory = serviceProvider.GetRequiredService<ILuBanAgentFactory>();
            
            // 创建 Agent 时不指定 toolGroups，启用所有工具
            var agent = await agentFactory.CreateAsync(
                modelName: ConfigManager.SelectedModel,
                systemPrompt: @"你是一个智能助手，拥有以下工具能力：

1. **文件系统操作**：可以读取文件、写入文件、列出目录内容
2. **脚本执行**：可以执行 Shell、Python、Lua 等脚本
3. **浏览器自动化**：可以打开网页、点击元素、输入文本、截图
4. **数据库操作**：可以执行 SQL 查询
5. **Redis 操作**：可以执行 Redis 命令
6. **Web 请求**：可以发送 HTTP 请求

当用户的请求涉及上述操作时，**必须使用相应的工具**来完成，不要说'我无法访问'或'我没有这个能力'。

例如：
- 用户说'帮我看看 D 盘有什么文件' -> 使用 list_directory 工具
- 用户说'读取某个文件的内容' -> 使用 read_file 工具
- 用户说'执行这个命令' -> 使用 run_shell 工具
- 用户说'打开某个网页' -> 使用浏览器工具

请立即使用工具来帮助用户完成任务。");

            Console.WriteLine("✓ 工具插件已加载（根据 appsettings.json 配置启用）");
            Console.WriteLine();

            await RunChatLoop(agent);
        }
        catch (Exception ex)
        {
            WriteError(ex.Message);
        }
        finally
        {
            // 清理确认回调
            ToolConfirmationService.ConfirmationCallback = null;
        }
    }

    /// <summary>
    /// 运行对话循环，支持工具调用显示、ESC 取消和 / 命令
    /// </summary>
    private async Task RunChatLoop(LuBanAgent agent)
    {
        var currentSession = _sessionManager.CurrentSession;
        
        while (true)
        {
            Console.Write("你: ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
                continue;

            if (input.ToLower() == "exit")
                break;

            // 处理 / 命令
            if (input.StartsWith('/') && _executeCommandAsync != null)
            {
                var handled = await _executeCommandAsync(input);
                if (handled)
                    continue;
            }

            try
            {
                string? finalResponse = null;
                var cancelled = false;
                var toolCalls = new System.Collections.Generic.List<string>();
                var thinkingContents = new System.Collections.Generic.List<string>();

                cancelled = await ConsoleUtil.RunWithStatusAsync(async (updateStatus, cancellationToken) =>
                {
                    updateStatus("AI 正在思考中...");

                    var response = await agent.RunAsync(input, cancellationToken);

                    if (response.Messages != null)
                    {
                        foreach (var message in response.Messages)
                        {
                            if (message.Role == ChatRole.Assistant && message.Contents != null)
                            {
                                foreach (var content in message.Contents)
                                {
                                    if (content is Microsoft.Extensions.AI.FunctionCallContent functionCall)
                                    {
                                        var toolInfo = $"调用工具: {functionCall.Name}";
                                        toolCalls.Add(toolInfo);
                                    }
                                }

                                var textContents = message.Contents
                                    .OfType<TextContent>()
                                    .Where(t => !string.IsNullOrWhiteSpace(t.Text))
                                    .ToList();

                                foreach (var text in textContents)
                                {
                                    var isThinking = false;
                                    if (text.AdditionalProperties != null)
                                    {
                                        foreach (var key in text.AdditionalProperties.Keys)
                                        {
                                            if (key.Contains("thinking", StringComparison.OrdinalIgnoreCase) ||
                                                key.Contains("thought", StringComparison.OrdinalIgnoreCase) ||
                                                key.Contains("reasoning", StringComparison.OrdinalIgnoreCase))
                                            {
                                                isThinking = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (isThinking)
                                    {
                                        thinkingContents.Add(text.Text!);
                                    }
                                }
                            }
                        }

                        if (toolCalls.Count > 0)
                        {
                            updateStatus($"已调用 {toolCalls.Count} 个工具，正在生成回答...");
                        }
                    }

                    finalResponse = response.Text;

                    if (!string.IsNullOrEmpty(finalResponse))
                    {
                        updateStatus("生成回答完成");
                        await Task.Delay(200, cancellationToken);
                    }
                }, "正在处理... (按 ESC 取消)", "cyan");
                
                if (cancelled)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("已取消当前操作");
                    Console.ResetColor();
                    continue;
                }
                
                // 保存消息到 Session
                if (currentSession != null && !cancelled)
                {
                    await _sessionManager.AddMessageAsync(currentSession.SessionId, "user", input);
                    if (!string.IsNullOrEmpty(finalResponse))
                    {
                        await _sessionManager.AddMessageAsync(currentSession.SessionId, "assistant", finalResponse);
                    }
                }
                
                // 显示工具调用过程
                if (toolCalls.Count > 0)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("工具调用过程:");
                    foreach (var toolCall in toolCalls)
                    {
                        Console.WriteLine($"  {toolCall}");
                    }
                    Console.ResetColor();
                }

                // 显示思考内容
                if (thinkingContents.Count > 0)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("💭 思考过程:");
                    foreach (var thinking in thinkingContents)
                    {
                        var lines = thinking.Split('\n');
                        foreach (var line in lines)
                        {
                            Console.WriteLine($"  {line}");
                        }
                    }
                    Console.ResetColor();
                }
                
                // 显示最终回答
                if (!string.IsNullOrEmpty(finalResponse))
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("AI: ");
                    Console.ResetColor();
                    Console.WriteLine(finalResponse);
                }
                else if (!cancelled)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("（无响应）");
                    Console.ResetColor();
                }
                
                Console.WriteLine();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("操作已取消");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                WriteError(GetFriendlyErrorMessage(ex));
            }
        }
    }

    /// <summary>
    /// 获取友好的错误信息
    /// </summary>
    private static string GetFriendlyErrorMessage(Exception ex)
    {
        var message = ex.Message;
        
        // 检查是否是 API 相关错误
        if (message.Contains("404") || message.Contains("Not Found"))
        {
            return "API 请求失败：模型不存在或 API 端点配置错误。请检查：\n" +
                   "  1. 选择的模型是否支持\n" +
                   "  2. API 端点配置是否正确\n" +
                   "  3. API Key 是否有效";
        }
        
        if (message.Contains("401") || message.Contains("Unauthorized"))
        {
            return "API 认证失败：API Key 无效或已过期。请检查 API Key 配置。";
        }
        
        if (message.Contains("403") || message.Contains("Forbidden"))
        {
            return "API 访问被拒绝：没有权限访问该模型或 API。请检查 API Key 权限。";
        }
        
        if (message.Contains("429") || message.Contains("Too Many Requests"))
        {
            return "API 请求过于频繁：已达到速率限制。请稍后再试。";
        }
        
        if (message.Contains("500") || message.Contains("Internal Server Error"))
        {
            return "API 服务器错误：服务端出现问题。请稍后再试或联系服务商。";
        }
        
        if (message.Contains("503") || message.Contains("Service Unavailable"))
        {
            return "API 服务不可用：服务暂时不可用。请稍后再试。";
        }
        
        // 如果是 ClientResultException，提取更友好的信息
        if (ex is System.ClientModel.ClientResultException clientEx)
        {
            return $"API 调用失败：{clientEx.Message}\n请检查模型配置和 API Key。";
        }
        
        // 默认返回原始错误信息
        return ex.Message;
    }

    /// <summary>
    /// 显示对话过程，包括工具调用
    /// </summary>
    private static void DisplayConversation(IEnumerable<ChatMessage> messages, Action<string> updateStatus)
    {
        foreach (var message in messages)
        {
            if (message.Role == ChatRole.Assistant)
            {
                // 检查是否有工具调用
                var functionCalls = message.Contents?
                    .OfType<FunctionCallContent>()
                    .ToList() ?? new List<FunctionCallContent>();
                
                var textContents = message.Contents?
                    .OfType<TextContent>()
                    .ToList() ?? new List<TextContent>();

                // 显示 thinking 文本（如果有）
                foreach (var text in textContents)
                {
                    if (!string.IsNullOrWhiteSpace(text.Text))
                    {
                        // 检查是否是 thinking 内容
                        if (text.Text.Contains("thinking") || 
                            text.Text.Contains("考虑") ||
                            text.Text.Contains("分析"))
                        {
                            updateStatus("正在思考...");
                        }
                    }
                }

                // 显示工具调用
                if (functionCalls.Count > 0)
                {
                    foreach (var call in functionCalls)
                    {
                        updateStatus($"正在调用工具: {call.Name}");
                    }
                }
            }
            else if (message.Role == ChatRole.Tool)
            {
                // 工具返回结果
                var functionResults = message.Contents?
                    .OfType<FunctionResultContent>()
                    .ToList() ?? new List<FunctionResultContent>();

                if (functionResults.Count > 0)
                {
                    updateStatus("工具执行完成，正在生成回答...");
                }
            }
        }
    }

}