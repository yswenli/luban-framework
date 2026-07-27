/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.ConsoleApp.Commands
*文件名： BrowseCommand
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：浏览网站命令
*
*****************************************************************************/
using LuBan.AIAgent.Configuration;
using LuBan.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuBan.AIAgent.ConsoleApp.Commands;

/// <summary>
/// 浏览网站命令，用自然语言操作网站
/// </summary>
public class BrowseCommand : CommandBase
{
    private readonly Func<string, Task<bool>>? _executeCommandAsync;

    /// <summary>
    /// 命令名称
    /// </summary>
    public override string Name => "browse";

    /// <summary>
    /// 命令描述
    /// </summary>
    public override string Description => "用自然语言操作网站";

    /// <summary>
    /// 创建命令实例
    /// </summary>
    public BrowseCommand(ConfigManager configManager, IConfiguration configuration, Func<string, Task<bool>>? executeCommandAsync = null)
        : base(configManager, configuration)
    {
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

        Console.WriteLine();
        Console.Write("请输入目标网站 URL: ");
        var url = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(url))
        {
            WriteError("URL 不能为空");
            return;
        }

        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "https://" + url;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            WriteError("无效的 URL，仅支持 http:// 和 https://");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("输入自然语言指令来操作网站 (输入 'done' 结束):");
        Console.WriteLine("例如: '导航到登录页面', '点击提交按钮', '在搜索框输入关键词'");
        Console.WriteLine();

        var systemPrompt = BuildSystemPrompt(url);
        using var serviceProvider = BuildServiceProvider();

        try
        {
            var agentFactory = serviceProvider.GetRequiredService<ILuBanAgentFactory>();
            var agent = await agentFactory.CreateAsync(
                modelName: ConfigManager.SelectedModel,
                systemPrompt: systemPrompt,
                toolGroups: new[] { "browser" });

            Console.WriteLine($"正在连接 {url}...");
            await RunInteractionLoop(agent);
        }
        catch (Exception ex)
        {
            WriteError(ex.Message);
        }
    }

    /// <summary>
    /// 构建系统提示词
    /// </summary>
    private static string BuildSystemPrompt(string url)
    {
        return $@"你是一个浏览器自动化助手。用户会用自然语言描述他们想要在网站上执行的操作。
            当前目标网站: {url}

            你可以使用以下工具来操作浏览器:
            - NavigateAsync: 导航到指定 URL
            - ClickAsync: 点击页面元素
            - TypeTextAsync: 在输入框中输入文本
            - ScreenshotAsync: 截取页面截图
            - GetContentAsync: 获取页面内容

            请根据用户的自然语言描述，使用合适的工具来完成任务。";
    }

    /// <summary>
    /// 运行交互循环，支持 ESC 取消、实时状态显示和 / 命令
    /// </summary>
    private async Task RunInteractionLoop(LuBanAgent agent)
    {
        while (true)
        {
            Console.WriteLine();
            Console.Write("指令 (或输入 'done' 结束，/ 命令可用): ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input) || input.ToLower() == "done")
                break;

            // 处理 / 命令
            if (input.StartsWith('/') && _executeCommandAsync != null)
            {
                var handled = await _executeCommandAsync(input);
                if (handled)
                    continue;
            }

            Console.WriteLine();

            try
            {
                string? finalResponse = null;
                var cancelled = false;
                
                cancelled = await ConsoleUtil.RunWithStatusAsync(async (updateStatus, cancellationToken) =>
                {
                    updateStatus("AI 正在执行...");
                    
                    var response = await agent.RunAsync(input, cancellationToken);
                    finalResponse = response.Text;
                    updateStatus("执行完成");
                    await Task.Delay(200, cancellationToken);
                }, "正在执行... (按 ESC 取消)", "cyan");
                
                if (cancelled)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("已取消当前操作");
                    Console.ResetColor();
                    continue;
                }
                
                Console.WriteLine();
                Console.WriteLine("结果:");
                if (!string.IsNullOrEmpty(finalResponse))
                {
                    Console.WriteLine(finalResponse);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("（无响应）");
                    Console.ResetColor();
                }
            }
            catch (OperationCanceledException)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("操作已取消");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                WriteError(ex.Message);
            }
        }
    }
}