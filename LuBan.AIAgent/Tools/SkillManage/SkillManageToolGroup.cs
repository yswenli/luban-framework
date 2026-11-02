/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.SkillManage
*文件名： SkillManageToolGroup
*版本号： V1.0.0.0
*唯一标识：7cfb630d-9dbb-4af6-afb1-c5cf63359a14
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/14
*描述：Skill 管理工具实现（WriteSkillAsync / DeleteSkillAsync）
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.1
*描述：写/删接入 IToolConfirmationService（Default/AcceptEdits 必确认、Plan 记计划）；
*      workspaceRoot 改可选并由宿主工作区兜底；补 [Description] 提升工具可发现性
*
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.2
*描述：删除路径补 Skill Id 正则校验（防 .. 越界删除）；确认弹窗补正文摘要；
*      ResolveWorkspaceRoot/ConfirmOrBlockAsync 收敛到 ManageToolSupport；
*      workspaceRoot 相对路径锚定后必须仍在宿主工作区内（防跨用户穿越）
*
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.3
*描述：新增 requireWorkspaceContainment 开关，隔离宿主下绝对路径也不得越出宿主工作区
*
*****************************************************************************/

namespace LuBan.AIAgent.Tools.SkillManage;

/// <summary>
/// Skill 管理工具实现。
/// </summary>
/// <remarks>
/// 写/删均属"有副作用且需用户知情"的操作，故与 <c>FileSystemToolGroup</c> 一致，
/// 在落盘前调用 <see cref="IToolConfirmationService.EvaluateAsync"/>：
/// Default / AcceptEdits 必弹确认，Plan 模式记计划不执行，BypassPermissions 放行。
/// 确认名沿用框架约定"C# 方法名"（<c>WriteSkillAsync</c> / <c>DeleteSkillAsync</c>），
/// 与 <see cref="ToolConfirmationService"/> 内置的 <c>AlwaysConfirmTools</c> 默认项对齐。
/// </remarks>
public class SkillManageToolGroup
{
    private readonly SkillRegistry _registry;
    private readonly SkillManageToolOptions _options;
    private readonly IAgentReloadTrigger? _reloadTrigger;
    private readonly IToolConfirmationService? _confirmationService;
    private readonly Func<string?>? _defaultWorkspaceRoot;
    private readonly bool _requireWorkspaceContainment;

    // Skill Id 合法性正则：小写字母/数字/下划线/中划线，首字符必须为字母/数字，最长 64
    private static readonly Regex IdPattern = new(@"^[a-z0-9][a-z0-9_-]{0,63}$", RegexOptions.Compiled);

    /// <summary>
    /// 创建 SkillManageToolGroup 实例。
    /// </summary>
    /// <param name="registry">Skill 注册表。</param>
    /// <param name="options">Skill 管理工具选项。</param>
    /// <param name="reloadTrigger">写入/删除后的热重载触发器，可为 null。</param>
    /// <param name="confirmationService">工具调用确认服务；为 null 表示不做确认（仅供程序化直调，宿主插件必须传入）。</param>
    /// <param name="defaultWorkspaceRoot">工作区根提供者（惰性求值）；工具参数省略 workspaceRoot 时用它兜底。</param>
    /// <param name="requireWorkspaceContainment">
    /// 是否要求 workspaceRoot（含绝对路径）必须落在宿主工作区内；宿主开启按用户隔离时必须传 true。
    /// </param>
    public SkillManageToolGroup(
        SkillRegistry registry,
        SkillManageToolOptions options,
        IAgentReloadTrigger? reloadTrigger = null,
        IToolConfirmationService? confirmationService = null,
        Func<string?>? defaultWorkspaceRoot = null,
        bool requireWorkspaceContainment = false)
    {
        _registry = registry;
        _options = options;
        _reloadTrigger = reloadTrigger;
        _confirmationService = confirmationService;
        _defaultWorkspaceRoot = defaultWorkspaceRoot;
        _requireWorkspaceContainment = requireWorkspaceContainment;
    }

    /// <summary>
    /// 创建或覆盖工作区级 Skill。
    /// </summary>
    /// <param name="id">Skill Id（小写字母/数字/下划线/中划线）。</param>
    /// <param name="name">Skill 名称（用于 frontmatter）。</param>
    /// <param name="description">Skill 描述。</param>
    /// <param name="content">SKILL.md 正文内容。</param>
    /// <param name="overwrite">是否覆盖同名 Skill；省略等同 false（已存在会返回可自纠提示）。</param>
    /// <param name="workspaceRoot">工作区根目录；省略时使用宿主当前工作区。</param>
    /// <returns>创建结果。</returns>
    [Description("创建或覆盖一个工作区级 Skill：写入 <工作区>/.luban-agent/skills/<id>/SKILL.md（含 name/description frontmatter）。写入前会请求用户确认；同名 Skill 已存在时须第二次调用并传 overwrite=true。")]
    public async Task<ToolResult<string>> WriteSkillAsync(
        [Description("Skill Id：仅小写字母/数字/下划线/中划线，首字符须为字母或数字，最长 64；如 api-review")]
        string id,
        [Description("Skill 名称，写入 frontmatter 的 name（单行、不能含冒号）")]
        string name,
        [Description("Skill 用途描述，写入 frontmatter 的 description，模型据此判断何时使用该 Skill")]
        string description,
        [Description("SKILL.md 正文（Markdown 指令模板，UTF-8，上限 256KB）")]
        string content,
        [Description("同名 Skill 已存在时必须传 true 才会覆盖；首次创建可省略（默认 false）")]
        bool overwrite = false,
        [Description("工作区根目录；省略时使用宿主当前工作区（推荐省略），相对路径不得越出宿主工作区")]
        string? workspaceRoot = null)
    {
        // 校验 Id
        if (string.IsNullOrWhiteSpace(id) || !IdPattern.IsMatch(id))
            return ToolResult.Fail<string>($"Skill Id 不合法，需匹配: {IdPattern}");

        // 内置 Id 不可覆盖
        if (_registry.IsBuiltin(id))
            return ToolResult.Fail<string>($"Skill Id '{id}' 为内置保留，不可覆盖。");

        // 检查同名
        var existing = _registry.Get(id);
        if (existing != null && !overwrite)
            return ToolResult.Fail<string>($"Skill '{id}' 已存在，需设置 overwrite=true 覆盖。");

        // 内容大小限制
        var totalBytes = Encoding.UTF8.GetByteCount(content);
        if (totalBytes > _options.MaxSkillFileBytes)
            return ToolResult.Fail<string>($"Skill 内容超过上限 {_options.MaxSkillFileBytes / 1024}KB。");

        var (root, rootError) = ManageToolSupport.ResolveWorkspaceRoot(
            workspaceRoot, _defaultWorkspaceRoot, _requireWorkspaceContainment);
        if (rootError is not null)
            return ToolResult.Fail<string>(rootError);

        if (root is null)
            return ToolResult.Fail<string>("未获取到工作区根目录，无法写入 Skill。请传入 workspaceRoot，或确认宿主已设置工作区。");

        var skillDir = Path.Combine(root, ".luban-agent", "skills", id);
        var skillFile = Path.Combine(skillDir, "SKILL.md");

        // 写操作需确认（Default / AcceptEdits 必确认；Plan 记计划；Bypass 放行）
        if (await ManageToolSupport.ConfirmOrBlockAsync(_confirmationService, nameof(WriteSkillAsync), skillDir,
                new Dictionary<string, object?>
                {
                    ["id"] = id,
                    ["name"] = name,
                    ["overwrite"] = overwrite,
                    // 正文可能很长，只给摘要，避免挤掉上面的关键字段
                    ["content"] = ManageToolSupport.BuildContentSummary(content)
                }).ConfigureAwait(false) is { } blocked)
        {
            return blocked;
        }

        // 清洗 frontmatter（去除换行/冒号）
        name = SanitizeFrontmatterValue(name);
        description = SanitizeFrontmatterValue(description);

        // 构建文件内容
        var sb = new StringBuilder();
        sb.AppendLine("---");
        sb.AppendLine($"name: {name}");
        sb.AppendLine($"description: {description}");
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine(content.Trim());

        try
        {
            Directory.CreateDirectory(skillDir);
            await File.WriteAllTextAsync(skillFile, sb.ToString());

            // 即时重新加载
            _registry.LoadFromWorkspace(root);

            // 标记延迟重载
            _reloadTrigger?.RequestReload();

            return ToolResult.Ok(skillFile, $"Skill '{id}' 已写入 {skillFile}");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail<string>($"写入 Skill 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除工作区级 Skill。
    /// </summary>
    /// <param name="id">Skill Id。</param>
    /// <param name="workspaceRoot">工作区根目录；省略时使用宿主当前工作区。</param>
    /// <returns>删除结果。</returns>
    [Description("删除一个工作区级 Skill（递归删除 <工作区>/.luban-agent/skills/<id> 目录）。删除前会请求用户确认；内置 Skill 不可删除。")]
    public async Task<ToolResult<string>> DeleteSkillAsync(
        [Description("要删除的 Skill Id（须为工作区级，非内置）")]
        string id,
        [Description("工作区根目录；省略时使用宿主当前工作区（推荐省略），相对路径不得越出宿主工作区")]
        string? workspaceRoot = null)
    {
        if (string.IsNullOrWhiteSpace(id) || !IdPattern.IsMatch(id))
            return ToolResult.Fail<string>($"Skill Id 不合法，需匹配: {IdPattern}");

        // 内置不可删
        if (_registry.IsBuiltin(id))
            return ToolResult.Fail<string>($"Skill '{id}' 为内置，不可删除。");

        var (root, rootError) = ManageToolSupport.ResolveWorkspaceRoot(
            workspaceRoot, _defaultWorkspaceRoot, _requireWorkspaceContainment);
        if (rootError is not null)
            return ToolResult.Fail<string>(rootError);

        if (root is null)
            return ToolResult.Fail<string>("未获取到工作区根目录，无法删除 Skill。请传入 workspaceRoot，或确认宿主已设置工作区。");

        var skillDir = Path.Combine(root, ".luban-agent", "skills", id);
        if (!Directory.Exists(skillDir))
            return ToolResult.Fail<string>($"Skill '{id}' 不存在。");

        // 删除操作始终需要确认
        if (await ManageToolSupport.ConfirmOrBlockAsync(_confirmationService, nameof(DeleteSkillAsync), skillDir,
                new Dictionary<string, object?>
                {
                    ["id"] = id
                }).ConfigureAwait(false) is { } blocked)
        {
            return blocked;
        }

        try
        {
            await Task.Run(() => Directory.Delete(skillDir, recursive: true));

            _registry.LoadFromWorkspace(root);
            _reloadTrigger?.RequestReload();

            return ToolResult.Ok(skillDir, $"Skill '{id}' 已删除。");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail<string>($"删除 Skill 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 清洗 frontmatter 值（去换行/冒号），避免破坏 YAML 结构。
    /// </summary>
    private static string SanitizeFrontmatterValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        // 移除换行与冒号，避免破坏 YAML 结构
        return value.Replace("\r", " ").Replace("\n", " ").Replace(":", " -");
    }
}