/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Rules
*文件名： BuiltinRuleCatalog
*版本号： V1.0.0.0
*唯一标识：内置 Rule 目录
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/12
*描述：内置（硬编码）Rule 的只读目录，单例持有，供 scoped 注册表与宿主侧服务复用
*
*****************************************************************************/
namespace LuBan.AIAgent.Rules;

/// <summary>
/// 内置（硬编码）Rule 的只读目录。内置 Rule 构造后只读（<c>IsEnabled</c> 无写入路径），
/// 故可安全跨作用域共享。以单例注入可避免宿主侧单例服务因捕获 scoped 的
/// <see cref="RuleEngine"/> 而形成 captive dependency。
/// </summary>
public sealed class BuiltinRuleCatalog
{
    private readonly Dictionary<string, IRule> _rules;

    /// <summary>
    /// 创建内置 Rule 目录。
    /// </summary>
    /// <param name="rules">全部内置 Rule 定义（DI 中的单例集合）。</param>
    public BuiltinRuleCatalog(IEnumerable<IRule> rules)
    {
        _rules = new Dictionary<string, IRule>(StringComparer.OrdinalIgnoreCase);
        foreach (var rule in rules)
            _rules[rule.Id] = rule;
    }

    /// <summary>
    /// 全部内置 Rule 定义。
    /// </summary>
    public IReadOnlyCollection<IRule> All => _rules.Values;

    /// <summary>
    /// 按 Id 获取内置 Rule。
    /// </summary>
    /// <param name="id">Rule 唯一标识，忽略大小写。</param>
    /// <returns>匹配的内置 Rule；未找到时返回 null。</returns>
    public IRule? Get(string id)
        => _rules.TryGetValue(id, out var rule) ? rule : null;
}