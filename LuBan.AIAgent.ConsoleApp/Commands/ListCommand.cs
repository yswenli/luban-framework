/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.ConsoleApp.Commands
*文件名： ListCommand
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：列出 Provider 命令
*
*****************************************************************************/
using System.Threading.Tasks;
using LuBan.AIAgent.Configuration;
using Microsoft.Extensions.Configuration;

namespace LuBan.AIAgent.ConsoleApp.Commands;

/// <summary>
/// 列出 Provider 命令
/// </summary>
public class ListCommand : CommandBase
{
    /// <summary>
    /// 命令名称
    /// </summary>
    public override string Name => "list";

    /// <summary>
    /// 命令描述
    /// </summary>
    public override string Description => "列出所有 Provider 和当前选择";

    /// <summary>
    /// 创建命令实例
    /// </summary>
    public ListCommand(ConfigManager configManager, IConfiguration configuration)
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
            WriteInfo("暂无配置的 Provider，请使用 add-provider 添加");
            return Task.CompletedTask;
        }

        Console.WriteLine("已配置的 Provider:");
        foreach (var p in ConfigManager.Providers)
        {
            var selected = ConfigManager.SelectedModel?.StartsWith(p.Name + ":") == true ? " (已选)" : "";
            Console.WriteLine($"  - {p.Name}{selected}");
            if (!string.IsNullOrEmpty(p.BaseUrl))
            {
                Console.WriteLine($"      Base URL: {p.BaseUrl}");
            }
        }

        Console.WriteLine();

        if (!string.IsNullOrEmpty(ConfigManager.SelectedModel))
        {
            WriteSuccess($"当前选择的模型: {ConfigManager.SelectedModel}");
        }
        else
        {
            WriteInfo("当前选择的模型: 未选择 (请使用 select 命令选择)");
        }

        return Task.CompletedTask;
    }
}