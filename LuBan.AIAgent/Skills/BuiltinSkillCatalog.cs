/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills
*文件名： BuiltinSkillCatalog
*版本号： V1.0.0.0
*唯一标识：内置 Skill 目录
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/12
*描述：内置（硬编码）Skill 的只读目录，单例持有，供 scoped 注册表与宿主侧服务复用
*
*****************************************************************************/
namespace LuBan.AIAgent.Skills;

/// <summary>
/// 内置（硬编码）Skill 的只读目录。内置 Skill 为纯提示词模板、无状态，故可安全跨作用域共享。
/// 以单例注入可避免宿主侧单例服务（如工作区初始化器）因捕获 scoped 的
/// <see cref="SkillRegistry"/> 而形成 captive dependency。
/// </summary>
public sealed class BuiltinSkillCatalog
{
    private readonly Dictionary<string, ISkill> _skills;

    /// <summary>
    /// 创建内置 Skill 目录。
    /// </summary>
    /// <param name="skills">全部内置 Skill 定义（DI 中的单例集合）。</param>
    public BuiltinSkillCatalog(IEnumerable<ISkill> skills)
    {
        _skills = new Dictionary<string, ISkill>(StringComparer.OrdinalIgnoreCase);
        foreach (var skill in skills)
            _skills[skill.Id] = skill;
    }

    /// <summary>
    /// 全部内置 Skill 定义。
    /// </summary>
    public IReadOnlyCollection<ISkill> All => _skills.Values;

    /// <summary>
    /// 按 Id 获取内置 Skill。
    /// </summary>
    /// <param name="id">Skill 唯一标识，忽略大小写。</param>
    /// <returns>匹配的内置 Skill；未找到时返回 null。</returns>
    public ISkill? Get(string id)
        => _skills.TryGetValue(id, out var skill) ? skill : null;
}