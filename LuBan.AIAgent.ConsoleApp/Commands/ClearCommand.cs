/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.ConsoleApp.Commands
*文件名： ClearCommand
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：清除配置命令
*
*****************************************************************************/
using System.Threading.Tasks;
using LuBan.AIAgent.Configuration;
using Microsoft.Extensions.Configuration;

namespace LuBan.AIAgent.ConsoleApp.Commands;

/// <summary>
/// 清除配置命令
/// </summary>
public class ClearCommand : CommandBase
{
    /// <summary>
    /// 命令名称
    /// </summary>
    public override string Name => "clear";

    /// <summary>
    /// 命令描述
    /// </summary>
    public override string Description => "清除所有配置";

    /// <summary>
    /// 创建命令实例
    /// </summary>
    public ClearCommand(ConfigManager configManager, IConfiguration configuration)
        : base(configManager, configuration)
    {
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    public override Task ExecuteAsync()
    {
        Console.WriteLine();
        Console.Write("确定要清除所有配置吗? (y/N): ");
        var confirm = Console.ReadLine()?.Trim().ToLower();

        if (confirm == "y" || confirm == "yes")
        {
            ConfigManager.Clear();
            WriteSuccess("配置已清除");
        }
        else
        {
            WriteInfo("已取消");
        }

        return Task.CompletedTask;
    }
}