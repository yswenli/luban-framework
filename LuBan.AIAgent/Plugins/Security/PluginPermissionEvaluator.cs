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
/// <b>声明期</b>（<see cref="ValidateDeclaration"/>）：插件清单里写明的<b>具体范围</b>必须落在白名单目录内，
/// 否则计入错误 → 清单校验不通过 → 加载前直接 <c>Disabled</c>（PRD §3.7）。
/// </para>
/// <para>
/// <b>通配声明</b>（<c>*</c>，含数组简写的裸维度形式，如 <c>"network"</c>）：表示「声明该维度、具体范围由宿主
/// 白名单决定」。声明期<b>放行</b>（仅记警告），运行期以宿主白名单为<b>上限</b>判定。
/// </para>
/// <para>
/// <b>运行期</b>（<see cref="IsNetworkAllowed"/> 等）：以插件<b>声明</b>的范围为准判定单次访问是否越界；
/// 未声明即一律拒绝（默认拒绝）。判定只读，不抛异常。
/// </para>
/// </remarks>
public sealed class PluginPermissionEvaluator
{
    private readonly PluginSecurityOptions _options;
    private readonly ICredentialKeyAliases _aliases;

    /// <summary>
    /// 创建判定器。
    /// </summary>
    /// <param name="options">白名单策略；为空时取默认白名单。</param>
    /// <param name="aliases">凭据键别名来源（可选）；用于把历史拼写解析为规范键后比较。</param>
    public PluginPermissionEvaluator(
        PluginSecurityOptions? options = null,
        ICredentialKeyAliases? aliases = null)
    {
        _aliases = aliases ?? NullCredentialKeyAliases.Instance;
        _options = options ?? new PluginSecurityOptions();
        _options.Normalize(_aliases);
    }

    /// <summary>当前生效的白名单策略。</summary>
    public PluginSecurityOptions Options => _options;

    // ---------------- 运行期判定 ----------------

    /// <summary>
    /// 判定插件是否有权访问指定网络主机。
    /// </summary>
    /// <param name="permissions">插件声明的权限。</param>
    /// <param name="host">目标主机（如 <c>api.example.com</c>）。</param>
    /// <returns>允许时返回 <c>true</c>。</returns>
    public bool IsNetworkAllowed(PluginPermissions? permissions, string? host)
    {
        if (permissions is null || string.IsNullOrWhiteSpace(host))
            return false;

        var scopes = permissions.GetScopes("network");

        // 通配声明不直接放行：其实际范围以宿主网络白名单为上限（PRD §3.7 默认拒绝）
        return scopes.Any(pattern => IsWildcard(pattern)
            ? _options.NetworkWhitelist.Any(allowed => HostMatches(allowed, host))
            : HostMatches(pattern, host));
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

        // 通配声明以宿主进程白名单为上限，而非直接放行
        return scopes.Any(scope => IsWildcard(scope)
            ? _options.ProcessWhitelist.Any(allowed => IsWildcard(allowed) ||
                                                       string.Equals(allowed.Trim(), name, StringComparison.OrdinalIgnoreCase))
            : string.Equals(scope.Trim(), name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 判定插件是否有权读取指定凭据（PRD §3.7：未声明则无法读取）。
    /// </summary>
    /// <param name="permissions">插件声明的权限。</param>
    /// <param name="key">凭据键（如 <c>connector:myconnector:token</c>）。</param>
    /// <returns>允许时返回 <c>true</c>。</returns>
    public bool IsCredentialAllowed(PluginPermissions? permissions, string? key)
    {
        if (permissions is null || string.IsNullOrWhiteSpace(key))
            return false;

        var normalized = _aliases.Resolve(key);
        var scopes = permissions.GetScopes("credential");

        // 通配声明以宿主凭据目录为上限，而非直接放行
        return scopes.Any(scope => IsWildcard(scope)
            ? _options.CredentialCatalog.Any(allowed => IsWildcard(allowed) ||
                                                        string.Equals(_aliases.Resolve(allowed), normalized, StringComparison.OrdinalIgnoreCase))
            : string.Equals(_aliases.Resolve(scope), normalized, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 判定插件是否有权执行指定 Git 操作。
    /// </summary>
    /// <param name="permissions">插件声明的权限。</param>
    /// <param name="operation">操作名（如 <c>clone</c> / <c>push</c> / <c>add</c> / <c>commit</c> / <c>rev-parse</c>）。</param>
    /// <returns>允许时返回 <c>true</c>。</returns>
    public bool IsGitAllowed(PluginPermissions? permissions, string? operation)
    {
        if (permissions is null || string.IsNullOrWhiteSpace(operation))
            return false;

        var op = operation.Trim().ToLowerInvariant();
        var scopes = permissions.GetScopes("git");

        // 通配声明以宿主 Git 能力白名单为上限，而非直接放行
        return scopes.Any(scope => IsWildcard(scope)
            ? _options.GitWhitelist.Any(allowed => GitScopeCovers(allowed, op))
            : GitScopeCovers(scope, op));
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

        // 通配声明以宿主文件系统白名单为上限，而非直接放行
        return scopes.Any(item => IsWildcard(item)
            ? _options.FilesystemWhitelist.Any(allowed => IsWildcard(allowed) ||
                                                         string.Equals(allowed.Trim(), scope.Trim(), StringComparison.OrdinalIgnoreCase))
            : string.Equals(item.Trim(), scope.Trim(), StringComparison.OrdinalIgnoreCase));
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

                var trimmed = scope.Trim();

                // 通配声明（含数组简写的裸维度形式，如 "network"）＝「声明该维度、范围由宿主白名单决定」：
                // 声明期放行并记警告，运行期仍以宿主白名单为上限判定（见 IsXxxAllowed）。
                if (IsWildcard(trimmed))
                {
                    result.Warnings.Add(
                        $"{path}.{dimension} 声明为通配 *，实际可访问范围以宿主白名单为上限（{DescribeWhitelist(dimension)}）。");
                    continue;
                }

                ValidateScope(result, path, dimension, trimmed);
            }
        }
    }

    private void ValidateScope(ManifestValidationResult result, string path, string dimension, string scope)
    {
        switch (dimension)
        {
            case "network":
                // 具体范围必须被白名单「覆盖」；通配 * 已在 ValidateDeclaration 中放行（运行期以白名单为上限）
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
                var normalized = _aliases.Resolve(scope);
                if (!_options.CredentialCatalog.Any(allowed => IsWildcard(allowed) ||
                                                               string.Equals(_aliases.Resolve(allowed), normalized, StringComparison.OrdinalIgnoreCase)))
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
    /// Git 能力覆盖：能力组覆盖其子操作，其余精确匹配。
    /// </summary>
    /// <remarks>
    /// <c>clone_push</c> → <c>clone</c> / <c>push</c>（远端读写）；
    /// <c>commit</c> → <c>add</c> / <c>commit</c>（本地暂存与提交）；
    /// <c>inspect</c> → <c>rev-parse</c> / <c>status</c> / <c>diff</c> / <c>branch</c> / <c>config</c> / <c>checkout</c>
    /// （本地只读检查与切分支）。
    /// </remarks>
    /// <param name="scope">声明或白名单项。</param>
    /// <param name="operation">操作名（小写）。</param>
    /// <returns>覆盖返回 <c>true</c>。</returns>
    private static bool GitScopeCovers(string? scope, string operation)
    {
        var s = (scope ?? string.Empty).Trim().ToLowerInvariant();
        var op = (operation ?? string.Empty).Trim().ToLowerInvariant();

        if (s.Length == 0)
            return false;

        if (s == "*" || s == op)
            return true;

        return s switch
        {
            "clone_push" => op is "clone" or "push",
            "commit" => op is "add" or "commit",
            "inspect" => op is "rev-parse" or "status" or "diff" or "branch" or "config" or "checkout",
            _ => false
        };
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

    /// <summary>
    /// 描述某维度的宿主白名单，用于通配声明的警告文案。
    /// </summary>
    /// <param name="dimension">维度名。</param>
    /// <returns>白名单的可读描述。</returns>
    private string DescribeWhitelist(string dimension) => dimension switch
    {
        "network" => Format(_options.NetworkWhitelist),
        "process" => Format(_options.ProcessWhitelist),
        "credential" => Format(_options.CredentialCatalog),
        "git" => Format(_options.GitWhitelist),
        "filesystem" => Format(_options.FilesystemWhitelist),
        _ => "宿主策略"
    };
}