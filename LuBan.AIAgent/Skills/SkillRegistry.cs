/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Skills
*文件名： SkillRegistry
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：Skill 注册表
*
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuBan.AIAgent.Skills;

/// <summary>
/// Skill 注册表，管理所有可用的 Skill
/// </summary>
public class SkillRegistry
{
    private readonly Dictionary<string, ISkill> _skills = new();

    /// <summary>
    /// 创建 SkillRegistry 实例
    /// </summary>
    /// <param name="skills">所有注册的 Skill</param>
    public SkillRegistry(IEnumerable<ISkill> skills)
    {
        foreach (var skill in skills)
        {
            _skills[skill.Id] = skill;
        }
    }

    /// <summary>
    /// 获取所有 Skill
    /// </summary>
    public IReadOnlyList<ISkill> GetAll() => _skills.Values.ToList();

    /// <summary>
    /// 根据分类获取 Skill
    /// </summary>
    public IReadOnlyList<ISkill> GetByCategory(string category)
        => _skills.Values.Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

    /// <summary>
    /// 根据 ID 获取 Skill
    /// </summary>
    public ISkill? Get(string id) => _skills.TryGetValue(id, out var skill) ? skill : null;

    /// <summary>
    /// 搜索 Skill
    /// </summary>
    public IReadOnlyList<ISkill> Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return GetAll();

        return _skills.Values
            .Where(s => s.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                       s.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// 获取所有分类
    /// </summary>
    public IReadOnlyList<string> GetCategories()
        => _skills.Values.Select(s => s.Category).Distinct().ToList();
}