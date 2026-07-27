/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.ConsoleApp.Commands
*文件名： AddProviderCommand
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：添加 Provider 命令
*
*****************************************************************************/
using System;
using System.Threading.Tasks;
using LuBan.AIAgent.Configuration;
using Microsoft.Extensions.Configuration;

namespace LuBan.AIAgent.ConsoleApp.Commands;

/// <summary>
/// 添加 Provider 命令
/// </summary>
public class AddProviderCommand : CommandBase
{
    /// <summary>
    /// 命令名称
    /// </summary>
    public override string Name => "add-provider";

    /// <summary>
    /// 命令描述
    /// </summary>
    public override string Description => "添加 AI Provider";

    /// <summary>
    /// 创建命令实例
    /// </summary>
    public AddProviderCommand(ConfigManager configManager, IConfiguration configuration)
        : base(configManager, configuration)
    {
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    public override Task ExecuteAsync()
    {
        Console.WriteLine();
        Console.WriteLine("选择 Provider 类型:");
        Console.WriteLine("  1. OpenAI");
        Console.WriteLine("  2. Azure OpenAI");
        Console.WriteLine("  3. DeepSeek");
        Console.WriteLine("  4. Kimi (Moonshot)");
        Console.WriteLine("  5. 智谱 GLM");
        Console.WriteLine("  6. 通义千问");
        Console.WriteLine("  7. 豆包");
        Console.WriteLine("  8. Claude");
        Console.WriteLine("  9. Google Gemini");
        Console.WriteLine("  10. Ollama (本地)");
        Console.WriteLine("  11. 自定义 OpenAI 兼容 API");
        Console.Write("请选择 (1-11): ");

        var choice = Console.ReadLine()?.Trim();

        string providerName;
        string apiKey;
        string? baseUrl = null;

        switch (choice)
        {
            case "1":
                providerName = "openai";
                Console.Write("请输入 OpenAI API Key: ");
                apiKey = ReadPassword();
                break;

            case "2":
                providerName = "azure";
                Console.Write("请输入 Azure OpenAI API Key: ");
                apiKey = ReadPassword();
                Console.Write("请输入 Azure OpenAI Endpoint (如 https://your-resource.openai.azure.com): ");
                baseUrl = Console.ReadLine()?.Trim();
                break;

            case "3":
                providerName = "deepseek";
                Console.Write("请输入 DeepSeek API Key: ");
                apiKey = ReadPassword();
                baseUrl = "https://api.deepseek.com";
                break;

            case "4":
                providerName = "kimi";
                Console.Write("请输入 Kimi API Key: ");
                apiKey = ReadPassword();
                baseUrl = "https://api.moonshot.cn";
                break;

            case "5":
                providerName = "glm";
                Console.Write("请输入智谱 API Key: ");
                apiKey = ReadPassword();
                baseUrl = "https://open.bigmodel.cn";
                break;

            case "6":
                providerName = "qwen";
                Console.Write("请输入通义千问 API Key: ");
                apiKey = ReadPassword();
                baseUrl = "https://dashscope.aliyuncs.com";
                break;

            case "7":
                providerName = "doubao";
                Console.Write("请输入豆包 API Key: ");
                apiKey = ReadPassword();
                break;

            case "8":
                providerName = "claude";
                Console.Write("请输入 Claude API Key: ");
                apiKey = ReadPassword();
                baseUrl = "https://api.anthropic.com";
                break;

            case "9":
                providerName = "gemini";
                Console.Write("请输入 Google AI API Key: ");
                apiKey = ReadPassword();
                break;

            case "10":
                providerName = "ollama";
                apiKey = "ollama";
                Console.Write("请输入 Ollama 服务地址 (默认 http://localhost:11434): ");
                baseUrl = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(baseUrl))
                    baseUrl = "http://localhost:11434";
                break;

            case "11":
                Console.WriteLine();
                Console.Write("请输入 Provider 名称: ");
                providerName = Console.ReadLine()?.Trim()?.ToLower() ?? "custom";
                Console.Write("请输入 API Key: ");
                apiKey = ReadPassword();
                Console.Write("请输入 API Base URL: ");
                baseUrl = Console.ReadLine()?.Trim();
                break;

            default:
                WriteError("无效选择");
                return Task.CompletedTask;
        }

        if (string.IsNullOrEmpty(apiKey))
        {
            WriteError("API Key 不能为空");
            return Task.CompletedTask;
        }

        try
        {
            ConfigManager.AddProvider(providerName, apiKey, baseUrl);

            var displayName = ProviderModels.GetDisplayName(providerName);
            var models = ProviderModels.GetModels(providerName);

            WriteSuccess($"Provider '{displayName}' 已添加并保存");

            if (models.Count > 0)
            {
                Console.WriteLine($"  支持的模型: {string.Join(", ", models.Take(5))}{(models.Count > 5 ? "..." : "")}");
            }
        }
        catch (Exception ex)
        {
            WriteError(ex.Message);
        }

        return Task.CompletedTask;
    }
}