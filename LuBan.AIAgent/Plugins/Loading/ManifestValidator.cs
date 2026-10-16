/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Plugins.Loading
*文件名： ManifestValidator
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：插件清单校验（PRD §3.2 / §3.4 加载流程第 2 步）
*
*****************************************************************************/
using System.Text.RegularExpressions;

namespace LuBan.AIAgent.Plugins.Loading;

/// <summary>
/// 插件清单校验器：加载流程第 2 步「校验签名、<c>targetApp</c>、<c>minHostVersion</c>」（PRD §3.4）。
/// </summary>
/// <remarks>
/// 签名校验依赖宿主提供的签名策略，框架层仅做「结构 + 语义」校验；签名留待宿主扩展
/// （<see cref="ManifestValidationOptions.RequireSignature"/> 为保留位）。
/// </remarks>
public sealed class ManifestValidator
{
    /// <summary>
    /// 插件 id 格式：反向域名（字母开头，允许字母/数字/点/中划线/下划线）。
    /// </summary>
    private static readonly Regex PluginIdPattern = new(
        @"^[a-zA-Z][a-zA-Z0-9._\-]{1,127}$",
        RegexOptions.Compiled);

    /// <summary>
    /// Skill id 格式：与框架 <c>SkillManageToolGroup</c> 一致（<b>不含点号</b>）。
    /// </summary>
    private static readonly Regex SkillIdPattern = new(
        @"^[a-z0-9][a-z0-9_\-]{0,63}$",
        RegexOptions.Compiled);

    /// <summary>
    /// 语义化版本格式（宽松：<c>major.minor[.build[.rev]]</c>）。
    /// </summary>
    private static readonly Regex SemVerPattern = new(
        @"^\d+\.\d+(\.\d+){0,2}(-[0-9A-Za-z.\-]+)?$",
        RegexOptions.Compiled);

    private readonly ManifestValidationOptions _options;
    private readonly PluginPermissionEvaluator _permissions;

    /// <summary>
    /// 创建清单校验器。
    /// </summary>
    /// <param name="options">校验选项；为空时取默认值。</param>
    /// <param name="permissions">权限判定器（白名单语义）；为空时取默认白名单。</param>
    public ManifestValidator(
        ManifestValidationOptions? options = null,
        PluginPermissionEvaluator? permissions = null)
    {
        _options = options ?? new ManifestValidationOptions();
        _permissions = permissions ?? new PluginPermissionEvaluator();
    }

    /// <summary>
    /// 校验插件清单。
    /// </summary>
    /// <param name="manifest">清单对象。</param>
    /// <param name="pluginDirectory">插件目录（用于校验入口程序集是否存在）。</param>
    /// <returns>校验结果。</returns>
    public ManifestValidationResult Validate(PluginManifest? manifest, string? pluginDirectory = null)
    {
        var result = new ManifestValidationResult();

        if (manifest is null)
        {
            result.Errors.Add("plugin.json 解析失败或内容为空。");
            return result;
        }

        // 必填字段
        RequireField(result, manifest.Id, "id");
        RequireField(result, manifest.Name, "name");
        RequireField(result, manifest.Version, "version");
        RequireField(result, manifest.EntryAssembly, "entryAssembly");

        // id 格式
        if (!string.IsNullOrWhiteSpace(manifest.Id) && !PluginIdPattern.IsMatch(manifest.Id))
            result.Errors.Add($"id 格式非法（应为反向域名，如 com.tjc.newproject）：{manifest.Id}");

        // 版本格式
        if (!string.IsNullOrWhiteSpace(manifest.Version) && !SemVerPattern.IsMatch(manifest.Version))
            result.Errors.Add($"version 不是合法的语义化版本：{manifest.Version}");

        // minHostVersion 格式
        if (!string.IsNullOrWhiteSpace(manifest.MinHostVersion) && !SemVerPattern.IsMatch(manifest.MinHostVersion!))
            result.Errors.Add($"minHostVersion 不是合法的语义化版本：{manifest.MinHostVersion}");

        // targetApp 校验
        if (!string.IsNullOrWhiteSpace(_options.TargetApp))
        {
            if (string.IsNullOrWhiteSpace(manifest.TargetApp))
                result.Warnings.Add($"未声明 targetApp，按当前宿主 {_options.TargetApp} 处理。");
            else if (!string.Equals(manifest.TargetApp, _options.TargetApp, StringComparison.OrdinalIgnoreCase))
                result.Errors.Add($"targetApp 不匹配：清单为 {manifest.TargetApp}，当前宿主为 {_options.TargetApp}。");
        }

        // minHostVersion 校验
        if (!string.IsNullOrWhiteSpace(manifest.MinHostVersion) &&
            !string.IsNullOrWhiteSpace(_options.HostVersion) &&
            TryParseVersion(manifest.MinHostVersion!, out var minHost) &&
            TryParseVersion(_options.HostVersion!, out var host) &&
            host < minHost)
        {
            result.Errors.Add($"宿主版本过低：要求 >= {manifest.MinHostVersion}，当前为 {_options.HostVersion}。");
        }

        // 入口程序集存在性
        if (!string.IsNullOrWhiteSpace(pluginDirectory) && !string.IsNullOrWhiteSpace(manifest.EntryAssembly))
        {
            if (PluginLocation.ResolveEntryAssemblyPath(pluginDirectory!, manifest.EntryAssembly) is null)
                result.Errors.Add($"入口程序集不存在：lib/{manifest.EntryAssembly}（或插件根目录下同名文件）。");
        }

        // 侧栏条目
        var entryIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in manifest.UiContributions)
        {
            if (string.IsNullOrWhiteSpace(entry.Id))
            {
                result.Errors.Add("uiContributions 存在缺失 id 的条目。");
                continue;
            }

            if (!entryIds.Add(entry.Id))
                result.Errors.Add($"uiContributions 存在重复 id：{entry.Id}");

            if (string.IsNullOrWhiteSpace(entry.Label))
                result.Warnings.Add($"uiContributions[{entry.Id}] 未声明 label，将回退显示 id。");

            if (string.IsNullOrWhiteSpace(entry.PanelType))
                result.Warnings.Add($"uiContributions[{entry.Id}] 未声明 panelType，宿主无法定位面板实现。");
        }

        // 技能
        var skillIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var skill in manifest.Skills)
        {
            if (string.IsNullOrWhiteSpace(skill.Id))
            {
                result.Errors.Add("skills 存在缺失 id 的条目。");
                continue;
            }

            if (!SkillIdPattern.IsMatch(skill.Id))
                result.Errors.Add($"skill id 格式非法（小写字母/数字/下划线/中划线，不含点号）：{skill.Id}");

            if (!skillIds.Add(skill.Id))
                result.Errors.Add($"skills 存在重复 id：{skill.Id}");

            if (string.IsNullOrWhiteSpace(skill.Name))
                result.Warnings.Add($"skills[{skill.Id}] 未声明 name。");
        }

        // MCP 服务器声明
        var mcpNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var server in manifest.McpServers)
        {
            if (string.IsNullOrWhiteSpace(server.Name))
            {
                result.Errors.Add("mcpServers 存在缺失 name 的条目。");
                continue;
            }

            if (!mcpNames.Add(server.Name))
                result.Errors.Add($"mcpServers 存在重复 name：{server.Name}");

            if (!string.IsNullOrWhiteSpace(server.Transport) &&
                !string.Equals(server.Transport, "inproc", StringComparison.OrdinalIgnoreCase))
            {
                result.Errors.Add($"mcpServers[{server.Name}].transport 暂仅支持 inproc，收到：{server.Transport}");
            }
        }

        // 权限（插件级 + 技能级）
        ValidatePermissionsOf(manifest, result);

        // 依赖：自引用非法
        foreach (var dependency in manifest.Dependencies)
        {
            if (string.IsNullOrWhiteSpace(dependency))
            {
                result.Errors.Add("dependencies 存在空项。");
                continue;
            }

            if (string.Equals(dependency, manifest.Id, StringComparison.OrdinalIgnoreCase))
                result.Errors.Add($"dependencies 不允许自引用：{dependency}");
        }

        // 签名（保留位）
        if (_options.RequireSignature)
            result.Warnings.Add("当前框架版本尚未实现插件签名校验，RequireSignature 已被忽略。");

        return result;
    }

    /// <summary>
    /// 单独校验清单的权限声明（插件级 <c>permissions</c> + 技能级 <c>skills[].permissions</c>）。
    /// </summary>
    /// <remarks>
    /// 与 <see cref="Validate"/> 分离的原因：PRD §3.7 要求「权限 / credential 声明不合规」在加载前直接判
    /// <c>Disabled</c>（宿主策略不允许），而清单格式问题（缺 id、入口程序集缺失等）仍应在激活阶段判
    /// <c>Error</c>（插件自身不可用）。二者状态语义不同，故权限校验需可独立调用。
    /// </remarks>
    /// <param name="manifest">清单对象；为 <c>null</c> 时返回空结果（不做判定）。</param>
    /// <returns>仅含权限相关错误的校验结果。</returns>
    public ManifestValidationResult ValidatePermissions(PluginManifest? manifest)
    {
        var result = new ManifestValidationResult();
        if (manifest is null)
            return result;

        ValidatePermissionsOf(manifest, result);
        return result;
    }

    /// <summary>
    /// 校验权限声明：范围非空 + 落在宿主白名单目录内（PRD §3.7，不合规直接判 <c>Disabled</c>）。
    /// </summary>
    private void ValidatePermissionsOf(PluginManifest manifest, ManifestValidationResult result)
    {
        foreach (var skill in manifest.Skills)
        {
            if (string.IsNullOrWhiteSpace(skill.Id))
                continue;

            ValidatePermissions(result, skill.Permissions, $"skills[{skill.Id}].permissions");
        }

        ValidatePermissions(result, manifest.Permissions, "permissions");
    }

    /// <summary>
    /// 校验单个权限声明的每个维度（PRD §3.7，不合规直接判 <c>Disabled</c>）。
    /// </summary>
    private void ValidatePermissions(
        ManifestValidationResult result,
        PluginPermissions? permissions,
        string path)
        => _permissions.ValidateDeclaration(permissions, path, result);

    /// <summary>
    /// 必填字段校验。
    /// </summary>
    private static void RequireField(ManifestValidationResult result, string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            result.Errors.Add($"缺少必填字段：{fieldName}");
    }

    /// <summary>
    /// 宽松解析版本号。
    /// </summary>
    private static bool TryParseVersion(string value, out Version version)
    {
        version = new Version(0, 0);

        var core = value.Split('-', 2)[0];
        var parts = core.Split('.');
        if (parts.Length < 2 || parts.Length > 4)
            return false;

        var numbers = new int[4];
        for (var i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out numbers[i]))
                return false;
        }

        version = parts.Length switch
        {
            2 => new Version(numbers[0], numbers[1]),
            3 => new Version(numbers[0], numbers[1], numbers[2]),
            _ => new Version(numbers[0], numbers[1], numbers[2], numbers[3])
        };

        return true;
    }
}

/// <summary>
/// 清单校验选项。
/// </summary>
public sealed class ManifestValidationOptions
{
    /// <summary>
    /// 当前宿主标识（如 <c>LubanAgentCodex</c>）；为空则跳过 <c>targetApp</c> 校验。
    /// </summary>
    public string? TargetApp { get; set; }

    /// <summary>
    /// 当前宿主版本（如 <c>2026.10.15.1</c>）；为空则跳过 <c>minHostVersion</c> 校验。
    /// </summary>
    public string? HostVersion { get; set; }

    /// <summary>
    /// 是否要求签名（保留位，当前框架未实现签名校验）。
    /// </summary>
    public bool RequireSignature { get; set; }
}

/// <summary>
/// 清单校验结果。
/// </summary>
public sealed class ManifestValidationResult
{
    /// <summary>
    /// 错误列表（非空即校验不通过）。
    /// </summary>
    public List<string> Errors { get; } = new();

    /// <summary>
    /// 警告列表（不阻断加载）。
    /// </summary>
    public List<string> Warnings { get; } = new();

    /// <summary>
    /// 是否校验通过。
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// 汇总为一行文本（日志用）。
    /// </summary>
    /// <returns>汇总文本。</returns>
    public string ToSummary()
    {
        if (IsValid)
            return Warnings.Count == 0 ? "清单校验通过。" : $"清单校验通过（{Warnings.Count} 条警告）。";

        return $"清单校验失败：{string.Join("；", Errors)}";
    }
}