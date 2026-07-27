/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.ConsoleApp.Commands
*文件名： RuleCommand
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：Rule 命令 - 查看和管理规则
*
*****************************************************************************/
using System;
using System.Threading.Tasks;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Rules;
using Microsoft.Extensions.Configuration;

namespace LuBan.AIAgent.ConsoleApp.Commands;

/// <summary>
/// Rule 命令 - 查看和管理规则
/// </summary>
public class RuleCommand : CommandBase
{
    private readonly RuleEngine _ruleEngine;

    /// <summary>
    /// 命令名称
    /// </summary>
    public override string Name => "rule";

    /// <summary>
    /// 命令描述
    /// </summary>
    public override string Description => "查看和管理规则";

    /// <summary>
    /// 创建命令实例
    /// </summary>
    public RuleCommand(ConfigManager configManager, IConfiguration configuration, RuleEngine ruleEngine)
        : base(configManager, configuration)
    {
        _ruleEngine = ruleEngine;
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    public override Task ExecuteAsync()
    {
        Console.WriteLine();
        Console.WriteLine("已配置的规则：");
        Console.WriteLine();

        var rules = _ruleEngine.GetAllRules();
        if (rules.Count == 0)
        {
            Console.WriteLine("  （无）");
        }
        else
        {
            foreach (var rule in rules)
            {
                var status = rule.IsEnabled ? "✅" : "❌";
                Console.WriteLine($"  {status} {rule.Id,-20} - {rule.Name}");
                Console.WriteLine($"     {rule.Description}");
                Console.WriteLine($"     优先级: {rule.Priority}");
                Console.WriteLine();
            }
        }

        Console.WriteLine("提示:");
        Console.WriteLine("  - 规则在工具执行前自动应用");
        Console.WriteLine("  - 高优先级规则优先执行");
        Console.WriteLine("  - 拒绝规则会阻止操作继续");
        Console.WriteLine();

        return Task.CompletedTask;
    }
}