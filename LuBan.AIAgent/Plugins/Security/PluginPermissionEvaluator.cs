/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Plugins.Security
*文件名： PluginPermissionEvaluator
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/16
*描述：插件权限判定器 - 声明期静态校验 + 运行期越界判定（PRD §3.7）
*
*****************************************************************************/
using LuBan.AIAgent.Plugins.Loading;

namespace LuBan.AIAgent.Plugins.Security;

/// <summary>
/// 插件权限判定器：把 <see cref="PluginSecurityOptions"/> 白名单语义落到「声明是否合规」与「访问是否越界」两件事上。
/// </summary>
/// <remarks>
/// <para>
/// <b>声明期</b>（<see cref="ValidateDeclaration"/>）：插件清单里的每一处权限范围都必须落在白名单目录内，
/// 否则计入错误 → 清单校验不通过 → 加载前直接 <c>Disabled</c>（PRD §3.7）。
/// </para>
/// <para>
/// <b>运行期</b>（<see cref="IsNetworkAllowed"/> 等）：以插件<b>声明</b>的范围为准判定单次访问是否越界；
/// 未声明即一律拒绝（默认拒绝）。判定只读，不抛异常。
/// </para>
/// </remarks>
public sealed class PluginPermissionEvaluator
{
    private readonly PluginSecurityOptions _options;

    /// <summary>
    /// 创建判定器。
    /// </summary>
    /// <param name="options">白名单策略；为空时取默认白名单。</param>
    public PluginPermissionEvaluator(PluginSecurityOptions? options = null)
    {
        _options = options ?? new PluginSecurityOptions();
        _options.Normalize();
    }

    /// <summary>当前生效的白名单策略。</summary>
    public PluginSecurityOptions Options => _options;

    // ---------------- 运行期判定 ----------------

    /// <summary>
    /// 判定插件是否有权访问指定网络主机。
    /// </summary>
    /// <param name="permissions">插件声明的权限。</param>
    /// <param name="host">目标主机（如 <c>codeup.aliyun.com</c>）。</param>
    /// <returns>允许时返回 <c>true</c>。</returns>
    public bool IsNetworkAllowed(PluginPermissions? permissions, string? host)
    {
        if (permissions is null || string.IsNullOrWhiteSpace(host))
            return false;

        var scopes = permissions.GetScopes("network");
        return scopes.Any(pattern => HostMatches(pattern, host));
    }

    /// <summary>
    /// 判定插件是否有权执行指定进程命令。
    /// </summary>
    /// <param name="permissions">插件声明的权限。</param>
    /// <param name="command">命令名（如 <c>git</c>）。</param>
    /// <returns>允许时返回 <c>true</c>。</returns>
    public bool IsProcessAllowed(PluginPermissions? permissions, string? command)
    {
        if (permissions is null || string.IsNullOrWhiteSpace(command))
            return false;

        var name = NormalizeCommandName(command);
        var scopes = permissions.GetScopes("process");

        return scopes.Any(scope => IsWildcard(scope) ||
                                   string.Equals(scope.Trim(), name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 判定插件是否有权读取指定凭据（PRD §3.7：未声明则无法读取）。
    /// </summary>
    /// <param name="permissions">插件声明的权限。</param>
    /// <param name="key">凭据键（如 <c>yunxiao_pat</c>）。</param>
    /// <returns>允许时返回 <c>true</c>。</returns>
    public bool IsCredentialAllowed(PluginPermissions? permissions, string? key)
    {
        if (permissions is null || string.IsNullOrWhiteSpace(key))
            return false;

        var normalized = CredentialKeys.Normalize(key);
        var scopes = permissions.GetScopes("credential");

        return scopes.Any(scope => IsWildcard(scope) ||
                                   string.Equals(CredentialKeys.Normalize(scope), normalized, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 判定插件是否有权执行指定 Git 操作。
    /// </summary>
    /// <param name="permissions">插件声明的权限。</param>
    /// <param name="operation">操作名（<c>clone</c> / <c>push</c>）。</param>
    /// <returns>允许时返回 <c>true</c>。</returns>
    public bool IsGitAllowed(PluginPermissions? permissions, string? operation)
    {
        if (permissions is null || string.IsNullOrWhiteSpace(operation))
            return false;

        var op = operation.Trim().ToLowerInvariant();
        var scopes = permissions.GetScopes("git");

        return scopes.Any(scope => GitScopeCovers(scope, op));
    }

    /// <summary>
    /// 判定插件是否有权访问指定文件系统范围（当前仅声明期使用）。
    /// </summary>
    /// <param name="permissions">插件声明的权限。</param>
    /// <param name="scope">范围（如 <c>workspace</c>）。</param>
    /// <returns>允许时返回 <c>true</c>。</returns>
    public bool IsFilesystemAllowed(PluginPermissions? permissions, string? scope)
    {
        if (permissions is null || string.IsNullOrWhiteSpace(scope))
            return false;

        var scopes = permissions.GetScopes("filesystem");

        return scopes.Any(item => IsWildcard(item) ||
                                  string.Equals(item.Trim(), scope.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    // ---------------- 声明期校验 ----------------

    /// <summary>
    /// 校验权限声明是否落在白名单目录内，不合规的错误计入 <paramref name="result"/>。
    /// </summary>
    /// <param name="permissions">权限声明。</param>
    /// <param name="path">错误信息中的路径前缀（如 <c>permissions</c> 或 <c>skills[x].permissions</c>）。</param>
    /// <param name="result">校验结果累加器。</param>
    public void ValidateDeclaration(PluginPermissions? permissions, string path, ManifestValidationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (permissions is null || permissions.IsEmpty)
            return;

        foreach (var dimension in PluginPermissions.KnownDimensions)
        {
            var scopes = permissions.GetScopes(dimension);
            foreach (var scope in scopes)
            {
                if (string.IsNullOrWhiteSpace(scope))
                {
                    result.Errors.Add($"{path}.{dimension} 存在空范围项。");
                    continue;
                }

                ValidateScope(result, path, dimension, scope.Trim());
            }
        }
    }

    private void ValidateScope(ManifestValidationResult result, string path, string dimension, string scope)
    {
        switch (dimension)
        {
            case "network":
                // 声明必须被白名单「覆盖」：裸 * 只有在白名单本身含 * 时才被授权（PRD §3.7 不得放行全网）
                if (!_options.NetworkWhitelist.Any(allowed => PatternCovers(allowed, scope)))
                {
                    result.Errors.Add(
                        $"{path}.network 越界：{scope} 不在宿主网络白名单内（{Format(_options.NetworkWhitelist)}）。");
                }
                break;

            case "process":
                if (!_options.ProcessWhitelist.Any(allowed => IsWildcard(allowed) ||
                                                              string.Equals(allowed.Trim(), scope, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Errors.Add(
                        $"{path}.process 越界：{scope} 不在宿主进程白名单内（{Format(_options.ProcessWhitelist)}）。");
                }
                break;

            case "credential":
                var normalized = CredentialKeys.Normalize(scope);
                if (!_options.CredentialCatalog.Any(allowed => IsWildcard(allowed) ||
                                                               string.Equals(CredentialKeys.Normalize(allowed), normalized, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Errors.Add(
                        $"{path}.credential 越界：{scope} 不在宿主凭据目录内（{Format(_options.CredentialCatalog)}）。");
                }
                break;

            case "git":
                if (!_options.GitWhitelist.Any(allowed => GitScopeCovers(allowed, scope.Trim().ToLowerInvariant())))
                {
                    result.Errors.Add(
                        $"{path}.git 越界：{scope} 不在宿主 Git 能力白名单内（{Format(_options.GitWhitelist)}）。");
                }
                break;

            case "filesystem":
                if (!_options.FilesystemWhitelist.Any(allowed => IsWildcard(allowed) ||
                                                                 string.Equals(allowed.Trim(), scope, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Errors.Add(
                        $"{path}.filesystem 越界：{scope} 不在宿主文件系统白名单内（{Format(_options.FilesystemWhitelist)}）。");
                }
                break;

            case "database":
                // PRD §3.7 未给定 database 白名单目录（建库/执行脚本自由范围），仅校验非空。
                break;
        }
    }

    // ---------------- 匹配原语 ----------------

    /// <summary>
    /// 通配主机匹配：<c>*</c> 匹配任意主机；<c>*.foo.com</c> 匹配 <c>foo.com</c> 的任意子域（不含裸域）；
    /// 其余为精确匹配（忽略大小写）。
    /// </summary>
    /// <param name="pattern">模式（来自插件声明或白名单）。</param>
    /// <param name="host">目标主机。</param>
    /// <returns>匹配返回 <c>true</c>。</returns>
    public static bool HostMatches(string? pattern, string? host)
    {
        var p = (pattern ?? string.Empty).Trim().ToLowerInvariant().TrimEnd('.');
        var h = (host ?? string.Empty).Trim().ToLowerInvariant().TrimEnd('.');

        if (p.Length == 0 || h.Length == 0)
            return false;

        if (p == "*")
            return true;

        if (p.StartsWith("*.", StringComparison.Ordinal))
        {
            var suffix = p[2..];
            return h.EndsWith("." + suffix, StringComparison.Ordinal);
        }

        return string.Equals(p, h, StringComparison.Ordinal);
    }

    /// <summary>
    /// 判定白名单项能否覆盖（授权）一个声明项。
    /// </summary>
    /// <param name="allowed">白名单项。</param>
    /// <param name="declared">插件声明项。</param>
    /// <returns>被覆盖返回 <c>true</c>。</returns>
    private static bool PatternCovers(string allowed, string declared)
    {
        var a = (allowed ?? string.Empty).Trim().ToLowerInvariant();
        var d = (declared ?? string.Empty).Trim().ToLowerInvariant();

        if (a == "*")
            return true;

        if (d == "*")
            return false;

        if (d.StartsWith("*.", StringComparison.Ordinal))
        {
            if (!a.StartsWith("*.", StringComparison.Ordinal))
                return false;

            var declaredSuffix = d[2..];
            var allowedSuffix = a[2..];

            // *.aliyuncs.com 覆盖 *.aliyuncs.com 与 *.foo.aliyuncs.com
            return string.Equals(declaredSuffix, allowedSuffix, StringComparison.Ordinal) ||
                   declaredSuffix.EndsWith("." + allowedSuffix, StringComparison.Ordinal);
        }

        // 精确声明：白名单须以通配或精确方式匹配它
        return HostMatches(a, d);
    }

    /// <summary>
    /// Git 能力覆盖：<c>clone_push</c> 覆盖 <c>clone</c> / <c>push</c>，其余精确匹配。
    /// </summary>
    /// <param name="scope">声明或白名单项。</param>
    /// <param name="operation">操作名（小写）。</param>
    /// <returns>覆盖返回 <c>true</c>。</returns>
    private static bool GitScopeCovers(string? scope, string operation)
    {
        var s = (scope ?? string.Empty).Trim().ToLowerInvariant();

        if (s.Length == 0)
            return false;

        if (s == "*" || s == operation)
            return true;

        return s == "clone_push" && (operation == "clone" || operation == "push");
    }

    private static bool IsWildcard(string? value) => (value ?? string.Empty).Trim() == "*";

    private static string NormalizeCommandName(string command)
    {
        var name = command.Trim();

        try
        {
            name = Path.GetFileName(name);
            name = Path.GetFileNameWithoutExtension(name);
        }
        catch (ArgumentException)
        {
            // 非法路径字符：原样比较
        }

        return name;
    }

    private static string Format(IEnumerable<string> values) => string.Join(", ", values);
}