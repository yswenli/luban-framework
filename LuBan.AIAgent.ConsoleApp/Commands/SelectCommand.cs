/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.ConsoleApp.Commands
*文件名： SelectCommand
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：选择模型命令
*
*****************************************************************************/
using System;
using System.Linq;
using System.Threading.Tasks;
using LuBan.AIAgent.Configuration;
using Microsoft.Extensions.Configuration;

namespace LuBan.AIAgent.ConsoleApp.Commands;

/// <summary>
/// 选择模型命令
/// </summary>
public class SelectCommand : CommandBase
{
    /// <summary>
    /// 命令名称
    /// </summary>
    public override string Name => "select";

    /// <summary>
    /// 命令描述
    /// </summary>
    public override string Description => "选择模型";

    /// <summary>
    /// 创建命令实例
    /// </summary>
    public SelectCommand(ConfigManager configManager, IConfiguration configuration)
        : base(configManager, configuration)
    {
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    public override Task ExecuteAsync()
    {
        Console.WriteLine();

        if (ConfigManager.Providers.Count == 0)
        {
            WriteError("暂无配置的 Provider，请先使用 add-provider 添加");
            return Task.CompletedTask;
        }

        // 显示 Provider 列表
        Console.WriteLine("已配置的 Provider:");
        var providerList = ConfigManager.Providers.ToList();
        for (int i = 0; i < providerList.Count; i++)
        {
            var p = providerList[i];
            var displayName = ProviderModels.GetDisplayName(p.Name);
            var selected = ConfigManager.SelectedModel?.StartsWith(p.Name + ":") == true ? " (已选)" : "";
            Console.WriteLine($"  {i + 1}. {displayName}{selected}");
        }

        Console.WriteLine();
        Console.Write("请选择 Provider 编号: ");
        var providerChoice = Console.ReadLine()?.Trim();

        if (!int.TryParse(providerChoice, out var providerIndex) || providerIndex < 1 || providerIndex > providerList.Count)
        {
            WriteError("无效选择");
            return Task.CompletedTask;
        }

        var selectedProvider = providerList[providerIndex - 1];

        // 获取该 Provider 支持的模型列表
        var supportedModels = ProviderModels.GetModels(selectedProvider.Name);

        Console.WriteLine();
        Console.WriteLine($"{ProviderModels.GetDisplayName(selectedProvider.Name)} 支持的模型:");

        if (supportedModels.Count > 0)
        {
            // 显示预定义的模型列表
            for (int i = 0; i < supportedModels.Count; i++)
            {
                var isSelected = ConfigManager.SelectedModel == $"{selectedProvider.Name}:{supportedModels[i]}";
                var selected = isSelected ? " (已选)" : "";
                Console.WriteLine($"  {i + 1}. {supportedModels[i]}{selected}");
            }
            Console.WriteLine($"  {supportedModels.Count + 1}. 手动输入其他模型");

            Console.WriteLine();
            Console.Write("请选择模型编号: ");
            var modelChoice = Console.ReadLine()?.Trim();

            if (!int.TryParse(modelChoice, out var modelIndex) || modelIndex < 1 || modelIndex > supportedModels.Count + 1)
            {
                WriteError("无效选择");
                return Task.CompletedTask;
            }

            string modelName;
            if (modelIndex == supportedModels.Count + 1)
            {
                // 手动输入
                Console.Write("请输入模型名称: ");
                modelName = Console.ReadLine()?.Trim() ?? "";
            }
            else
            {
                modelName = supportedModels[modelIndex - 1];
            }

            if (string.IsNullOrEmpty(modelName))
            {
                WriteError("模型名称不能为空");
                return Task.CompletedTask;
            }

            try
            {
                var fullModel = $"{selectedProvider.Name}:{modelName}";
                ConfigManager.SetSelectedModel(fullModel);
                WriteSuccess($"已选择模型: {fullModel}");
            }
            catch (Exception ex)
            {
                WriteError(ex.Message);
            }
        }
        else
        {
            // 没有预定义的模型列表，手动输入
            Console.WriteLine("  该 Provider 没有预定义的模型列表，请手动输入");
            Console.WriteLine();
            Console.Write($"请输入模型名称: ");
            var modelName = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(modelName))
            {
                WriteError("模型名称不能为空");
                return Task.CompletedTask;
            }

            try
            {
                var fullModel = $"{selectedProvider.Name}:{modelName}";
                ConfigManager.SetSelectedModel(fullModel);
                WriteSuccess($"已选择模型: {fullModel}");
            }
            catch (Exception ex)
            {
                WriteError(ex.Message);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 执行带子命令的命令，支持 /select provider:model 格式
    /// </summary>
    public override Task<bool> ExecuteAsync(string[] args)
    {
        if (args.Length == 0)
            return Task.FromResult(false);

        var modelId = string.Join(' ', args);
        try
        {
            // 验证格式
            if (!modelId.Contains(':'))
            {
                WriteError("模型格式错误，应为 provider:model，例如 openai:gpt-4o");
                return Task.FromResult(true);
            }

            ConfigManager.SetSelectedModel(modelId);
            WriteSuccess($"已选择模型: {modelId}");
        }
        catch (Exception ex)
        {
            WriteError(ex.Message);
        }

        return Task.FromResult(true);
    }
}