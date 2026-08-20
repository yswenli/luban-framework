/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Rules.BuiltIn
*文件名： PathAccessRule
*版本号： V1.0.0.0
*唯一标识：2450f53d-7628-4e50-b99e-a56d9986dd58
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：路径访问规则
*
*=================================================
*修改标记
*修改时间：2026/7/27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：路径访问规则
*
*****************************************************************************/

namespace LuBan.AIAgent.Rules.BuiltIn;

/// <summary>
/// 路径访问规则 - 限制文件系统访问范围
/// </summary>
public class PathAccessRule : RuleBase
{
    private readonly HashSet<string> _allowedPaths;
    private readonly HashSet<string> _deniedPaths;

    /// <summary>
    /// 规则 ID
    /// </summary>
    public override string Id => "path-access";

    /// <summary>
    /// 规则名称
    /// </summary>
    public override string Name => "路径访问规则";

    /// <summary>
    /// 规则描述
    /// </summary>
    public override string Description => "限制文件系统访问，防止访问敏感路径";

    /// <summary>
    /// 规则优先级
    /// </summary>
    public override int Priority => 100;

    /// <summary>
    /// 创建路径访问规则
    /// </summary>
    /// <param name="allowedPaths">允许访问的路径</param>
    /// <param name="deniedPaths">禁止访问的路径</param>
    public PathAccessRule(IEnumerable<string>? allowedPaths = null, IEnumerable<string>? deniedPaths = null)
    {
        _allowedPaths = new HashSet<string>(allowedPaths ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        _deniedPaths = new HashSet<string>(deniedPaths ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 检查规则是否适用
    /// </summary>
    public override bool IsApplicable(RuleContext context)
    {
        return context.ActionType == "file-read" ||
               context.ActionType == "file-write" ||
               context.ActionType == "directory-list";
    }

    /// <summary>
    /// 执行规则
    /// </summary>
    public override Task<RuleResult> ExecuteAsync(RuleContext context)
    {
        var path = context.Arguments.GetValueOrDefault("path")?.ToString();

        if (string.IsNullOrEmpty(path))
            return Task.FromResult(Allow());

        // 检查是否在禁止列表中
        if (IsInDeniedPath(path))
        {
            return Task.FromResult(Deny($"禁止访问路径: {path}"));
        }

        // 如果有允许列表，检查是否在允许列表中
        if (_allowedPaths.Count > 0 && !IsInAllowedPath(path))
        {
            return Task.FromResult(Deny($"路径不在允许访问范围内: {path}"));
        }

        return Task.FromResult(Allow());
    }

    private bool IsInDeniedPath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return _deniedPaths.Any(denied =>
        {
            var normalizedDenied = Path.GetFullPath(denied);
            if (!normalizedDenied.EndsWith(Path.DirectorySeparatorChar.ToString()))
                normalizedDenied += Path.DirectorySeparatorChar;
            return fullPath.StartsWith(normalizedDenied, StringComparison.OrdinalIgnoreCase);
        });
    }

    private bool IsInAllowedPath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return _allowedPaths.Any(allowed =>
        {
            var normalizedAllowed = Path.GetFullPath(allowed);
            if (!normalizedAllowed.EndsWith(Path.DirectorySeparatorChar.ToString()))
                normalizedAllowed += Path.DirectorySeparatorChar;
            return fullPath.StartsWith(normalizedAllowed, StringComparison.OrdinalIgnoreCase) ||
                   fullPath.Equals(Path.GetFullPath(allowed).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
        });
    }
}
