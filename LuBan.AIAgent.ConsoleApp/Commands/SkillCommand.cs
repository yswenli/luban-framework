/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.ConsoleApp.Commands
*文件名： SkillCommand
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：Skill 命令 - 列出和执行 Skill
*
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Skills;
using LuBan.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuBan.AIAgent.ConsoleApp.Commands;

/// <summary>
/// Skill 命令 - 列出和执行 Skill
/// </summary>
public class SkillCommand : CommandBase
{
    private readonly SkillRegistry _skillRegistry;

    /// <summary>
    /// 命令名称
    /// </summary>
    public override string Name => "skill";

    /// <summary>
    /// 命令描述
    /// </summary>
    public override string Description => "查看和执行 Skill（技能）";

    /// <summary>
    /// 创建命令实例
    /// </summary>
    public SkillCommand(ConfigManager configManager, IConfiguration configuration, SkillRegistry skillRegistry)
        : base(configManager, configuration)
    {
        _skillRegistry = skillRegistry;
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    public override async Task ExecuteAsync()
    {
        Console.WriteLine();
        Console.WriteLine("可用的 Skill (技能)：");
        Console.WriteLine();

        var skills = _skillRegistry.GetAll();
        var categories = _skillRegistry.GetCategories();

        foreach (var category in categories)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[{category}]");
            Console.ResetColor();

            var categorySkills = skills.Where(s => s.Category == category);
            foreach (var skill in categorySkills)
            {
                Console.WriteLine($"  {skill.Id,-20} - {skill.Name}");
                Console.WriteLine($"  {"",-20}   {skill.Description}");
                
                if (skill.Examples.Any())
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"  {"",-20}   示例: {skill.Examples.First()}");
                    Console.ResetColor();
                }
                Console.WriteLine();
            }
        }

        Console.WriteLine("提示: 输入 'skill <id>' 来执行 Skill，或 'skill <id> <参数>' 带参数执行");
        Console.WriteLine("示例: skill brainstorming");
        Console.WriteLine("示例: skill brainstorming 我想实现一个用户登录功能");
        Console.WriteLine();

        // 询问是否要执行某个 Skill
        Console.Write("请输入 Skill ID 或按回车返回: ");
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input))
            return;

        // 解析输入
        var parts = input.Split(' ', 2);
        var skillId = parts[0];
        var skillInput = parts.Length > 1 ? parts[1] : "";

        await ExecuteSkillAsync(skillId, skillInput);
    }

    /// <summary>
    /// 执行带子命令的命令
    /// </summary>
    public override async Task<bool> ExecuteAsync(string[] args)
    {
        if (args.Length == 0)
            return false;

        var skillId = args[0];
        var skillInput = args.Length > 1 ? string.Join(' ', args[1..]) : "";
        await ExecuteSkillAsync(skillId, skillInput);
        return true;
    }

    /// <summary>
    /// 执行指定的 Skill
    /// </summary>
    private async Task ExecuteSkillAsync(string skillId, string input)
    {
        var skill = _skillRegistry.Get(skillId);
        if (skill == null)
        {
            WriteError($"未找到 Skill: {skillId}");
            return;
        }

        if (!ConfigManager.HasSelectedModel)
        {
            WriteError("请先使用 select 命令选择模型");
            return;
        }

        // 如果没有输入参数，提示用户输入
        if (string.IsNullOrEmpty(input))
        {
            Console.WriteLine();
            Console.WriteLine($"执行 Skill: {skill.Name}");
            Console.WriteLine(skill.Description);
            Console.WriteLine();

            if (skill.Examples.Any())
            {
                Console.WriteLine("示例:");
                foreach (var example in skill.Examples)
                {
                    Console.WriteLine($"  - {example}");
                }
                Console.WriteLine();
            }

            Console.Write("请输入内容: ");
            input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("已取消执行");
                return;
            }
        }

        // 构建 ServiceProvider 并执行
        using var serviceProvider = BuildServiceProvider();
        
        try
        {
            var agentFactory = serviceProvider.GetRequiredService<ILuBanAgentFactory>();
            var agent = await agentFactory.CreateAsync(modelName: ConfigManager.SelectedModel);

            var context = new SkillContext
            {
                Agent = agent,
                ServiceProvider = serviceProvider,
                Log = msg => Console.WriteLine($"  {msg}"),
                UpdateStatus = status => Console.Title = $"LuBan Agent - {status}"
            };

            Console.WriteLine();
            Console.WriteLine($"执行 Skill: {skill.Name}");
            Console.WriteLine();

            // 使用动画显示执行状态
            await ConsoleUtil.RunWithStatusAsync(async updateStatus =>
            {
                context.UpdateStatus = updateStatus;
                var result = await skill.ExecuteAsync(context, input);

                if (result.Success)
                {
                    Console.WriteLine();
                    Console.WriteLine(result.Text);
                }
                else
                {
                    WriteError(result.Error ?? "执行失败");
                }
            }, $"正在执行 {skill.Name}...", "cyan");

            Console.WriteLine();
        }
        catch (Exception ex)
        {
            WriteError(ex.Message);
        }
    }
}