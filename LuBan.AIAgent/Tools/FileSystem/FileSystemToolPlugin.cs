/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.FileSystem
*文件名： FileSystemToolPlugin
*版本号： V1.0.0.0
*唯一标识：9e92b095-1784-4d60-b9c4-64d23145481d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：文件系统工具插件
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：文件系统工具插件
*
*****************************************************************************/
using System.Text.RegularExpressions;
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Tools.FileSystem;

/// <summary>
/// 文件系统工具插件
/// </summary>
public class FileSystemToolPlugin : ILuBanToolPlugin
{
    private readonly FileSystemToolOptions _options;
    private readonly PathGuard _pathGuard;

    /// <summary>
    /// 创建 FileSystemToolPlugin 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="pathGuard">路径守卫</param>
    public FileSystemToolPlugin(IOptions<LuBanAgentOptions> options, PathGuard pathGuard)
    {
        _options = options.Value.Tools.FileSystem;
        _pathGuard = pathGuard;
    }

    /// <summary>
    /// 工具分组名称
    /// </summary>
    public string GroupName => "filesystem";

    /// <summary>
    /// 工具分组描述
    /// </summary>
    public string? Description => "文件系统操作工具，支持读取、写入、搜索文件等操作";

    /// <summary>
    /// 获取工具函数列表
    /// </summary>
    /// <param name="sp">服务提供者</param>
    /// <returns>工具函数列表</returns>
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp, ToolGroupOptions? toolsOptions = null)
    {
        var confirmationService = sp.GetRequiredService<IToolConfirmationService>();
        var toolGroup = new FileSystemToolGroup(_pathGuard, confirmationService);
        var options = sp.GetService<IOptions<LuBanAgentOptions>>();
        Func<string?> workspaceRootProvider = () => options?.Value.WorkspaceRoot;
        // 工作区根兜底仅用于"只读发现类"工具：模型漏传路径时用工作区根做默认扫描范围才有意义。
        // 面向具体文件/目录的读写删改工具一律不兜底——否则漏传 path 会被静默替换成工作区根，
        // 产生"路径被误识别为目录""返回工作区统计"等误导结果；缺参由 RequiredArgumentGuardAIFunction 统一提示。
        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.ReadFileAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.WriteFileAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.ListDirectoryAsync), workspaceRootProvider),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.GetWorkspaceOverviewAsync), workspaceRootProvider),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.DeleteFileAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.DeleteDirectoryAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.SearchFilesAsync), workspaceRootProvider),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.GrepAsync), workspaceRootProvider),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.CreateDirectoryAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.CopyFileAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.MoveFileAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.GetFileInfoAsync))
        };
    }

    /// <summary>
    /// 判断插件是否启用
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <returns>是否启用</returns>
    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.FileSystem.Enabled;
}

/// <summary>
/// 文件系统工具分组
/// </summary>
public class FileSystemToolGroup
{
    private readonly PathGuard _pathGuard;
    private readonly IToolConfirmationService _confirmationService;

    /// <summary>
    /// 创建 FileSystemToolGroup 实例
    /// </summary>
    /// <param name="pathGuard">路径守卫</param>
    /// <param name="confirmationService">工具调用确认服务</param>
    public FileSystemToolGroup(PathGuard pathGuard, IToolConfirmationService confirmationService)
    {
        _pathGuard = pathGuard;
        _confirmationService = confirmationService;
    }

    /// <summary>
    /// 统一的确认前置检查。返回非 null 表示调用方应立即把该结果返回给 LLM：
    /// Plan 模式下返回"已记录计划、未执行"，被拒绝时返回"用户取消"，两者语义不可混用。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <param name="path">操作目标路径。</param>
    /// <param name="arguments">工具参数。</param>
    /// <returns>允许执行时返回 null，否则返回应直接回给 LLM 的结果。</returns>
    private async Task<ToolResult<string>?> ConfirmOrBlockAsync(
        string toolName, string path, Dictionary<string, object?> arguments)
    {
        var outcome = await _confirmationService.EvaluateAsync(toolName, path, arguments)
            .ConfigureAwait(false);

        return outcome switch
        {
            EnumConfirmationOutcome.Allowed => null,
            EnumConfirmationOutcome.Planned => ToolResult.Plan<string>(),
            _ => ToolResult.Denied<string>()
        };
    }

    /// <summary>
    /// 生成「传入的是目录、但该工具需要文件」的可操作提示，
    /// 避免模型把目录当文件反复重试（历史上曾因「未找到文件」误导导致死循环）。
    /// </summary>
    /// <param name="path">传入的目录路径。</param>
    /// <param name="requirement">该工具对路径的要求描述。</param>
    private static string DirectoryPathMessage(string path, string requirement)
        => $"路径是目录: {path}。{requirement}，请改为传入目录下的具体文件（可用 ListDirectory 查看目录内容）。";

    private static readonly HashSet<string> BinaryFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".dll", ".exe", ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico", ".tif", ".tiff",
        ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".woff", ".woff2", ".ttf", ".eot", ".otf",
        ".mp3", ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".wav",
        ".bin", ".dat", ".db", ".sqlite",
        ".so", ".dylib", ".a", ".lib", ".obj", ".o"
    };

    private static readonly HashSet<string> ExcludedDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", "node_modules", "target", "bin", "obj", "dist", "build",
        ".idea", ".vs", ".vscode", "__pycache__", ".gradle"
    };

    private static readonly HashSet<string> KeyFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "pom.xml", "package.json", "build.gradle", "build.gradle.kts",
        "Cargo.toml", "go.mod", "requirements.txt", "pyproject.toml",
        "appsettings.json", "web.config", "app.config",
        "application.yml", "application.yaml", "application.properties",
        "readme.md", "readme.en.md", "readme.txt", "agents.md",
        ".gitignore", "dockerfile", "docker-compose.yml",
        "makefile", "cmakelists.txt"
    };

    private static IEnumerable<string> EnumerateFilesSafe(string rootPath)
    {
        var dirs = new Queue<string>();
        dirs.Enqueue(rootPath);

        while (dirs.Count > 0)
        {
            var current = dirs.Dequeue();

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(current);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Debug("目录遍历跳过：权限不足", ex, current);
                continue;
            }
            catch (DirectoryNotFoundException ex)
            {
                Logger.Debug("目录遍历跳过：目录不存在", ex, current);
                continue;
            }

            // 物化当前目录的文件列表（在 try 内完成枚举，避免迭代器 yield 与 catch 冲突 CS1626）；
            // 单个文件枚举失败（超长路径/中文路径/重解析点）仅跳过该目录，不中断整轮搜索。
            List<string>? fileList;
            try
            {
                fileList = new List<string>(files);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Debug("文件枚举跳过：权限不足", ex, current);
                fileList = null;
            }
            catch (IOException ex)
            {
                Logger.Debug("文件枚举跳过：IO 错误", ex, current);
                fileList = null;
            }

            if (fileList is not null)
            {
                foreach (var file in fileList)
                    yield return file;
            }

            IEnumerable<string> subDirs;
            try
            {
                subDirs = Directory.EnumerateDirectories(current);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Debug("子目录遍历跳过：权限不足", ex, current);
                continue;
            }
            catch (DirectoryNotFoundException ex)
            {
                Logger.Debug("子目录遍历跳过：目录不存在", ex, current);
                continue;
            }
            catch (IOException ex)
            {
                Logger.Debug("子目录遍历跳过：IO 错误", ex, current);
                continue;
            }

            foreach (var subDir in subDirs)
            {
                var name = Path.GetFileName(subDir);
                if (!ExcludedDirectoryNames.Contains(name))
                    dirs.Enqueue(subDir);
            }
        }
    }

    private static class GlobMatcher
    {
        public static bool IsMatch(string path, Regex regex, string rootPath, bool matchFullPath)
        {
            if (!matchFullPath)
            {
                var fileName = Path.GetFileName(path);
                return regex.IsMatch(fileName);
            }

            var relativePath = Path.GetRelativePath(rootPath, path).Replace('\\', '/');
            return regex.IsMatch(relativePath);
        }

        public static Regex CompileGlobPattern(string pattern)
        {
            var regexPattern = GlobToRegex(pattern);
            return new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public static bool IsFileNameOnly(string pattern)
            => !pattern.Contains('/') && !pattern.Contains('\\');

        private static string GlobToRegex(string glob)
        {
            var regex = new StringBuilder("^");
            for (int i = 0; i < glob.Length; i++)
            {
                var c = glob[i];
                switch (c)
                {
                    case '*':
                        if (i + 1 < glob.Length && glob[i + 1] == '*')
                        {
                            regex.Append(".*");
                            i++;
                            if (i + 1 < glob.Length && (glob[i + 1] == '/' || glob[i + 1] == '\\'))
                                i++;
                        }
                        else
                        {
                            regex.Append("[^/\\\\]*");
                        }
                        break;
                    case '?':
                        regex.Append("[^/\\\\]");
                        break;
                    case '.':
                    case '+':
                    case '(':
                    case ')':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                    case '^':
                    case '$':
                    case '|':
                        regex.Append('\\');
                        regex.Append(c);
                        break;
                    case '\\':
                    case '/':
                        regex.Append("[/\\\\]");
                        break;
                    default:
                        regex.Append(c);
                        break;
                }
            }
            regex.Append("$");
            return regex.ToString();
        }
    }

    /// <summary>
    /// 读取文件内容
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>文件内容</returns>
    [Description("读取文件内容")]
    public async Task<ToolResult<string>> ReadFileAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return ToolResult.Fail<string>($"错误：路径 {path} 不在允许访问的范围内");

        // 工作区外读取需确认
        if (await ConfirmOrBlockAsync(nameof(ReadFileAsync), path,
            new Dictionary<string, object?> { ["path"] = path }) is { } blocked)
        {
            return blocked;
        }

        if (Directory.Exists(path))
            return ToolResult.Fail<string>(DirectoryPathMessage(path, "读取文件需要文件路径"));

        try
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Length > 50 * 1024 * 1024)
                return ToolResult.Fail<string>($"错误：文件过大 ({fileInfo.Length / 1024 / 1024}MB)，最大支持 50MB");

            // 行级切片：避免把大文件（日志/源码）整体塞进对话上下文撑爆 token。
            // 单次最多返回 readFileMaxLines 行 / readFileMaxChars 字符，超出则截断并提示改用 Grep 或 RAG。
            const int readFileMaxLines = 2000;
            const int readFileMaxChars = 256 * 1024;

            var sb = new StringBuilder();
            var truncated = false;
            var lineNo = 0;
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineNo++;
                if (lineNo > readFileMaxLines)
                {
                    truncated = true;
                    break;
                }

                if (sb.Length + line.Length + Environment.NewLine.Length > readFileMaxChars)
                {
                    var budget = readFileMaxChars - sb.Length;
                    if (budget > 0)
                        sb.AppendLine(line.Substring(0, Math.Min(line.Length, budget)));
                    truncated = true;
                    break;
                }

                sb.AppendLine(line);
            }

            if (truncated)
            {
                sb.AppendLine();
                sb.AppendLine($"[内容已截断] 源文件大小 {fileInfo.Length / 1024}KB，本次仅返回前 {Math.Min(lineNo, readFileMaxLines)} 行（约 {readFileMaxChars / 1024}KB）。");
                sb.AppendLine("如需检索特定内容请用 Grep 工具（按正则/关键字匹配，按行返回）；超大文件建议先建立 RAG 索引再提问。");
            }

            return ToolResult.Ok<string>(sb.ToString());
        }
        catch (FileNotFoundException ex)
        {
            Logger.Error("文件读取异常：文件不存在", ex, path);
            return ToolResult.Fail<string>($"未找到文件: {path}。请检查路径是否正确；若不确定文件位置，请先用 ListDirectory 查看目录内容，再传入目录下的具体文件路径。");
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("文件读取异常：目录不存在", ex, path);
            return ToolResult.Fail<string>($"未找到目录: {path}。请检查路径是否正确；若不确定目录位置，请先用 ListDirectory 查看上级目录内容。");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("文件读取异常：权限不足", ex, path);
            return ToolResult.Fail<string>($"无法访问文件: {path}，权限不足。请检查权限或尝试其他文件。");
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("文件读取异常：路径过长", ex, path);
            return ToolResult.Fail<string>($"路径过长: {path}。请缩短路径或尝试其他路径。");
        }
        catch (ArgumentException ex)
        {
            Logger.Error("文件读取异常：路径无效", ex, path);
            return ToolResult.Fail<string>($"路径无效: {path}。{ex.Message}");
        }
        catch (IOException ex)
        {
            Logger.Error("文件读取异常：文件可能被独占锁定", ex, path);
            return ToolResult.Fail<string>($"无法读取文件: {path}。文件可能被其他程序独占锁定，请关闭占用该文件的程序后重试。");
        }
        catch (Exception ex)
        {
            Logger.Error("文件读取异常", ex, path);
            return ToolResult.Fail<string>($"操作失败: {ex.Message}");
        }
    }

/// <summary>
/// 写入文件内容到指定路径
/// </summary>
/// <param name="path">文件路径（相对或绝对）</param>
/// <param name="content">文件内容</param>
/// <returns>操作结果</returns>
[Description("写入文件内容到指定路径。")]
    public async Task<ToolResult<string>> WriteFileAsync(string path, string content)
    {
        if (!_pathGuard.IsAllowed(path))
            return ToolResult.Fail<string>($"错误：路径 {path} 不在允许访问的范围内");

        // 工作区内写入免确认，工作区外需确认
        if (await ConfirmOrBlockAsync(nameof(WriteFileAsync), path,
            new Dictionary<string, object?> { ["path"] = path, ["content"] = content }) is { } blocked)
        {
            return blocked;
        }

        if (Directory.Exists(path))
            return ToolResult.Fail<string>(DirectoryPathMessage(path, "写入文件需要包含文件名的文件路径"));

        try
        {
            await File.WriteAllTextAsync(path, content);
            return ToolResult.Ok<string>($"已写入文件 {path}");
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("文件写入异常：目录不存在", ex, path);
            return ToolResult.Fail<string>($"未找到目录: {path}。请检查路径是否正确，或先创建目录。");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("文件写入异常：权限不足", ex, path);
            return ToolResult.Fail<string>($"无法写入文件: {path}，权限不足。请检查权限或尝试其他路径。");
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("文件写入异常：路径过长", ex, path);
            return ToolResult.Fail<string>($"路径过长: {path}。请缩短路径或尝试其他路径。");
        }
        catch (ArgumentException ex)
        {
            Logger.Error("文件写入异常：路径无效", ex, path);
            return ToolResult.Fail<string>($"路径无效: {path}。{ex.Message}");
        }
        catch (IOException ex)
        {
            Logger.Error("文件写入异常：IO 错误", ex, path);
            return ToolResult.Fail<string>($"IO 错误: {path}。{ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("文件写入异常", ex, path);
            return ToolResult.Fail<string>($"操作失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 列出目录内容
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <returns>目录内容</returns>
    [Description("列出目录内容")]
    public async Task<ToolResult<string>> ListDirectoryAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return ToolResult.Fail<string>($"错误：路径 {path} 不在允许访问的范围内");

        // 工作区外列目录需确认
        if (await ConfirmOrBlockAsync(nameof(ListDirectoryAsync), path,
            new Dictionary<string, object?> { ["path"] = path }) is { } blocked)
        {
            return blocked;
        }

        try
        {
            var entries = Directory.EnumerateFileSystemEntries(path);
            return ToolResult.Ok<string>(string.Join("\n", entries));
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("列出目录异常：目录不存在", ex, path);
            return ToolResult.Fail<string>($"未找到目录: {path}。请检查路径是否正确，或尝试其他路径。");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("列出目录异常：权限不足", ex, path);
            return ToolResult.Fail<string>($"无法访问目录: {path}，权限不足。请检查权限或尝试其他目录。");
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("列出目录异常：路径过长", ex, path);
            return ToolResult.Fail<string>($"路径过长: {path}。请缩短路径或尝试其他路径。");
        }
        catch (ArgumentException ex)
        {
            Logger.Error("列出目录异常：路径无效", ex, path);
            return ToolResult.Fail<string>($"路径无效: {path}。{ex.Message}");
        }
        catch (IOException ex)
        {
            Logger.Error("列出目录异常：IO 错误", ex, path);
            return ToolResult.Fail<string>($"IO 错误: {path}。{ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("列出目录异常", ex, path);
            return ToolResult.Fail<string>($"操作失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取工作区概览：目录树（限 3 层）+ 文件类型统计 + 关键文件列表。
    /// 一次调用即可让 AI 了解工作区整体结构，避免多次 ListDirectory 探索。
    /// </summary>
    /// <param name="rootPath">工作区根目录</param>
    /// <returns>工作区概览信息</returns>
    [Description("获取工作区概览：目录树（限3层）+ 文件类型统计 + 关键文件。一次调用了解工作区结构，避免多次ListDirectory。")]
    public async Task<ToolResult<string>> GetWorkspaceOverviewAsync(string rootPath)
    {
        var resolvedPath = string.IsNullOrEmpty(rootPath) || rootPath == "."
            ? Path.GetFullPath(".")
            : rootPath;

        if (await ConfirmOrBlockAsync(nameof(GetWorkspaceOverviewAsync), resolvedPath,
            new Dictionary<string, object?> { ["rootPath"] = resolvedPath }) is { } blocked)
        {
            return blocked;
        }

        if (!_pathGuard.IsAllowed(resolvedPath))
            return ToolResult.Fail<string>($"错误：路径 {resolvedPath} 不在允许访问的范围内");

        try
        {
            var fullPath = Path.GetFullPath(resolvedPath);
            if (!Directory.Exists(fullPath))
                return ToolResult.Fail<string>($"未找到工作区: {resolvedPath}。请检查路径是否正确。");

            var sb = new StringBuilder();
            sb.AppendLine($"# 工作区概览: {Path.GetFileName(fullPath)}");
            sb.AppendLine($"根目录: {fullPath}");
            sb.AppendLine();

            // 1. 目录树（限 3 层）
            sb.AppendLine("## 目录结构（3 层）");
            sb.AppendLine("```");
            BuildDirectoryTree(fullPath, 0, 3, sb, fullPath);
            sb.AppendLine("```");
            sb.AppendLine();

            // 2. 文件类型统计 + 3. 关键文件（合并为一次遍历）
            var extCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var keyFiles = new List<string>();
            CollectWorkspaceStats(fullPath, fullPath, extCounts, keyFiles, maxKeyDepth: 2, currentDepth: 0);

            sb.AppendLine("## 文件类型统计");
            var totalFiles = extCounts.Values.Sum();
            sb.AppendLine($"总文件数: {totalFiles}");
            sb.AppendLine();
            sb.AppendLine("| 扩展名 | 数量 | 占比 |");
            sb.AppendLine("|--------|------|------|");
            foreach (var kv in extCounts.OrderByDescending(k => k.Value).Take(15))
            {
                var pct = totalFiles > 0 ? (kv.Value * 100.0 / totalFiles).ToString("F1") : "0";
                sb.AppendLine($"| {kv.Key} | {kv.Value} | {pct}% |");
            }
            sb.AppendLine();

            sb.AppendLine("## 关键文件");
            if (keyFiles.Count > 0)
            {
                foreach (var f in keyFiles)
                    sb.AppendLine($"- {f}");
            }
            else
            {
                sb.AppendLine("-（未发现关键配置文件）");
            }

            return ToolResult.Ok<string>(sb.ToString().TrimEnd());
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("工作区概览异常：权限不足", ex, rootPath);
            return ToolResult.Fail<string>($"无法访问工作区: {rootPath}，权限不足。请检查权限。");
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("工作区概览异常：目录不存在", ex, rootPath);
            return ToolResult.Fail<string>($"未找到工作区: {rootPath}。请检查路径是否正确。");
        }
        catch (Exception ex)
        {
            Logger.Error("工作区概览异常", ex, rootPath);
            return ToolResult.Fail<string>($"操作失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 递归构建目录树字符串。
    /// </summary>
    private static void BuildDirectoryTree(string dir, int depth, int maxDepth, StringBuilder sb, string rootPath)
    {
        if (depth > maxDepth) return;

        var indent = depth == 0 ? "" : new string(' ', depth * 2);
        var dirName = depth == 0 ? Path.GetFileName(rootPath) : Path.GetFileName(dir);
        sb.AppendLine($"{indent}{dirName}/");

        if (depth >= maxDepth) return;

        string[] subdirs;
        try
        {
            subdirs = Directory.GetDirectories(dir);
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Debug("目录树构建跳过子目录：权限不足", ex, dir);
            return;
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Debug("目录树构建跳过子目录：目录不存在", ex, dir);
            return;
        }

        string[] files;
        try
        {
            files = Directory.GetFiles(dir);
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Debug("目录树构建跳过文件：权限不足", ex, dir);
            return;
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Debug("目录树构建跳过文件：目录不存在", ex, dir);
            return;
        }

        var visibleSubdirs = subdirs
            .Where(d => !ExcludedDirectoryNames.Contains(Path.GetFileName(d)))
            .OrderBy(d => d)
            .Take(20)
            .ToList();

        var visibleFiles = files
            .OrderBy(f => f)
            .Take(10)
            .ToList();

        var childIndent = new string(' ', (depth + 1) * 2);

        foreach (var subdir in visibleSubdirs)
        {
            BuildDirectoryTree(subdir, depth + 1, maxDepth, sb, rootPath);
        }

        foreach (var file in visibleFiles)
        {
            sb.AppendLine($"{childIndent}{Path.GetFileName(file)}");
        }

        if (subdirs.Length > visibleSubdirs.Count)
            sb.AppendLine($"{childIndent}... ({subdirs.Length - visibleSubdirs.Count} 个目录已省略)");
        if (files.Length > visibleFiles.Count)
            sb.AppendLine($"{childIndent}... ({files.Length - visibleFiles.Count} 个文件已省略)");
    }

    /// <summary>
    /// 单次遍历同时统计文件扩展名分布和收集关键配置文件。
    /// </summary>
    private static void CollectWorkspaceStats(
        string dir, string rootPath,
        Dictionary<string, int> extCounts, List<string> keyFiles,
        int maxKeyDepth, int currentDepth)
    {
        string[] entries;
        try
        {
            entries = Directory.GetFileSystemEntries(dir);
        }
        catch (UnauthorizedAccessException) { return; }
        catch (DirectoryNotFoundException) { return; }

        foreach (var entry in entries)
        {
            var attr = File.GetAttributes(entry);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var name = Path.GetFileName(entry);
                if (ExcludedDirectoryNames.Contains(name)) continue;
                CollectWorkspaceStats(entry, rootPath, extCounts, keyFiles, maxKeyDepth, currentDepth + 1);
            }
            else
            {
                var ext = Path.GetExtension(entry);
                if (string.IsNullOrEmpty(ext)) ext = "(无扩展名)";
                extCounts.TryGetValue(ext, out var count);
                extCounts[ext] = count + 1;

                if (currentDepth <= maxKeyDepth && KeyFileNames.Contains(Path.GetFileName(entry)))
                {
                    var relativePath = Path.GetRelativePath(rootPath, entry).Replace('\\', '/');
                    keyFiles.Add(relativePath);
                }
            }
        }
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>删除结果</returns>
    [Description("删除文件（无论是否在工作区内，都必须确认）")]
    public async Task<ToolResult<string>> DeleteFileAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return ToolResult.Fail<string>($"错误：路径 {path} 不在允许访问的范围内");

        // 删除操作：始终需要确认
        if (await ConfirmOrBlockAsync(nameof(DeleteFileAsync), path,
            new Dictionary<string, object?> { ["path"] = path }) is { } blocked)
        {
            return blocked;
        }

        try
        {
            if (Directory.Exists(path))
                return ToolResult.Fail<string>(DirectoryPathMessage(path, "删除文件需要文件路径；如需删除目录请改用 DeleteDirectory"));

            if (!File.Exists(path))
                return ToolResult.Fail<string>($"错误：文件不存在 ({path})");

            File.Delete(path);
            return ToolResult.Ok<string>($"已删除文件 {path}");
        }
        catch (FileNotFoundException ex)
        {
            Logger.Error("文件删除异常：文件不存在", ex, path);
            return ToolResult.Fail<string>($"未找到文件: {path}。请检查路径是否正确。");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("文件删除异常：权限不足", ex, path);
            return ToolResult.Fail<string>($"无法删除文件: {path}，权限不足。请检查权限。");
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("文件删除异常：路径过长", ex, path);
            return ToolResult.Fail<string>($"路径过长: {path}。请缩短路径或尝试其他路径。");
        }
        catch (ArgumentException ex)
        {
            Logger.Error("文件删除异常：路径无效", ex, path);
            return ToolResult.Fail<string>($"路径无效: {path}。{ex.Message}");
        }
        catch (IOException ex)
        {
            Logger.Error("文件删除异常：IO 错误", ex, path);
            return ToolResult.Fail<string>($"IO 错误: {path}。{ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("文件删除异常", ex, path);
            return ToolResult.Fail<string>($"操作失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除目录（递归删除所有子项）
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <returns>删除结果</returns>
    [Description("删除目录及其所有内容（无论是否在工作区内，都必须确认）")]
    public async Task<ToolResult<string>> DeleteDirectoryAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return ToolResult.Fail<string>($"错误：路径 {path} 不在允许访问的范围内");

        // 删除操作：始终需要确认
        if (await ConfirmOrBlockAsync(nameof(DeleteDirectoryAsync), path,
            new Dictionary<string, object?> { ["path"] = path }) is { } blocked)
        {
            return blocked;
        }

        try
        {
            if (!Directory.Exists(path))
                return ToolResult.Fail<string>($"错误：目录不存在 ({path})");

            Directory.Delete(path, recursive: true);
            return ToolResult.Ok<string>($"已删除目录 {path}");
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("目录删除异常：目录不存在", ex, path);
            return ToolResult.Fail<string>($"未找到目录: {path}。请检查路径是否正确。");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("目录删除异常：权限不足", ex, path);
            return ToolResult.Fail<string>($"无法删除目录: {path}，权限不足。请检查权限。");
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("目录删除异常：路径过长", ex, path);
            return ToolResult.Fail<string>($"路径过长: {path}。请缩短路径或尝试其他路径。");
        }
        catch (ArgumentException ex)
        {
            Logger.Error("目录删除异常：路径无效", ex, path);
            return ToolResult.Fail<string>($"路径无效: {path}。{ex.Message}");
        }
        catch (IOException ex)
        {
            Logger.Error("目录删除异常：IO 错误", ex, path);
            return ToolResult.Fail<string>($"IO 错误: {path}。{ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("目录删除异常", ex, path);
            return ToolResult.Fail<string>($"操作失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 按文件名模式搜索文件
    /// </summary>
    /// <param name="rootPath">搜索根目录</param>
    /// <param name="pattern">glob 模式</param>
    /// <param name="maxResults">最大返回数量</param>
    /// <returns>匹配的文件列表</returns>
    [Description("按文件名模式搜索文件，支持 glob 通配符（如 *.cs、**/*.cs）")]
    public Task<ToolResult<string>> SearchFilesAsync(string rootPath, string pattern, int maxResults = 100)
    {
        if (!_pathGuard.IsAllowed(rootPath))
            return Task.FromResult(ToolResult.Fail<string>($"错误：路径 {rootPath} 不在允许访问的范围内"));

        try
        {
            var globRegex = GlobMatcher.CompileGlobPattern(pattern);
            var matchFullPath = !GlobMatcher.IsFileNameOnly(pattern);
            var results = new List<string>();
            foreach (var file in EnumerateFilesSafe(rootPath))
            {
                var ext = Path.GetExtension(file);
                if (!string.IsNullOrEmpty(ext) && BinaryFileExtensions.Contains(ext))
                    continue;

                if (GlobMatcher.IsMatch(file, globRegex, rootPath, matchFullPath))
                {
                    results.Add(file);
                    if (results.Count >= maxResults)
                        break;
                }
            }

            if (results.Count == 0)
                return Task.FromResult(ToolResult.Ok<string>("未找到匹配的文件"));

            var output = new StringBuilder();
            output.AppendLine($"找到 {results.Count} 个匹配文件：");
            foreach (var file in results)
                output.AppendLine(file);

            return Task.FromResult(ToolResult.Ok<string>(output.ToString().TrimEnd()));
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("搜索文件异常：目录不存在", ex, rootPath);
            return Task.FromResult(ToolResult.Fail<string>($"未找到目录: {rootPath}。请检查路径是否正确。"));
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("搜索文件异常：权限不足", ex, rootPath);
            return Task.FromResult(ToolResult.Fail<string>($"无法访问目录: {rootPath}，权限不足。请检查权限。"));
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("搜索文件异常：路径过长", ex, rootPath);
            return Task.FromResult(ToolResult.Fail<string>($"路径过长: {rootPath}。请缩短路径或尝试其他路径。"));
        }
        catch (ArgumentException ex)
        {
            Logger.Error("搜索文件异常：路径无效", ex, rootPath);
            return Task.FromResult(ToolResult.Fail<string>($"路径无效: {rootPath}。{ex.Message}"));
        }
        catch (IOException ex)
        {
            Logger.Error("搜索文件异常：IO 错误", ex, rootPath);
            return Task.FromResult(ToolResult.Fail<string>($"IO 错误: {rootPath}。{ex.Message}"));
        }
        catch (Exception ex)
        {
            Logger.Error("搜索文件异常", ex, rootPath);
            return Task.FromResult(ToolResult.Fail<string>($"操作失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 按正则表达式搜索文件内容
    /// </summary>
    /// <param name="rootPath">搜索根目录</param>
    /// <param name="pattern">正则表达式</param>
    /// <param name="filePattern">文件名 glob 过滤</param>
    /// <param name="maxResults">最大返回匹配行数</param>
    /// <returns>匹配的文件路径、行号和行内容</returns>
    [Description("按正则表达式搜索文件内容，返回匹配的文件路径、行号和行内容")]
    public async Task<ToolResult<string>> GrepAsync(string rootPath, string pattern, string? filePattern = null, int maxResults = 100)
    {
        if (!_pathGuard.IsAllowed(rootPath))
            return ToolResult.Fail<string>($"错误：路径 {rootPath} 不在允许访问的范围内");

        try
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));
            Regex? filePatternRegex = null;
            bool matchFullPath = false;
            if (filePattern != null)
            {
                filePatternRegex = GlobMatcher.CompileGlobPattern(filePattern);
                matchFullPath = !GlobMatcher.IsFileNameOnly(filePattern);
            }

            var results = new List<string>();

            foreach (var file in EnumerateFilesSafe(rootPath))
            {
                var ext = Path.GetExtension(file);
                if (!string.IsNullOrEmpty(ext) && BinaryFileExtensions.Contains(ext))
                    continue;

                if (filePatternRegex != null && !GlobMatcher.IsMatch(file, filePatternRegex, rootPath, matchFullPath))
                    continue;

                try
                {
                    using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    if (stream.Length > 1024 * 1024)
                        continue;

                    using var reader = new StreamReader(stream);
                    var lineNumber = 0;
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lineNumber++;
                        if (regex.IsMatch(line))
                        {
                            results.Add($"{file}:{lineNumber}: {line.Trim()}");
                            if (results.Count >= maxResults)
                                break;
                        }
                    }
                }
                catch (IOException)
                {
                    continue;
                }

                if (results.Count >= maxResults)
                    break;
            }

            if (results.Count == 0)
                return ToolResult.Ok<string>("未找到匹配的内容");

            var output = new StringBuilder();
            output.AppendLine($"找到 {results.Count} 处匹配：");
            foreach (var match in results)
                output.AppendLine(match);

            if (results.Count >= maxResults)
                output.AppendLine($"\n结果已截断，当前显示前 {maxResults} 处匹配。缩小搜索范围或增大 maxResults 参数查看更多。");

            return ToolResult.Ok<string>(output.ToString().TrimEnd());
        }
        catch (RegexMatchTimeoutException ex)
        {
            Logger.Error("搜索内容异常：正则匹配超时", ex, pattern);
            return ToolResult.Fail<string>($"正则匹配超时: {pattern}。请简化正则表达式。");
        }
        catch (ArgumentException ex)
        {
            Logger.Error("搜索内容异常：正则表达式无效", ex, pattern);
            return ToolResult.Fail<string>($"正则表达式无效: {pattern}。{ex.Message}");
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("搜索内容异常：目录不存在", ex, rootPath);
            return ToolResult.Fail<string>($"未找到目录: {rootPath}。请检查路径是否正确。");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("搜索内容异常：权限不足", ex, rootPath);
            return ToolResult.Fail<string>($"无法访问目录: {rootPath}，权限不足。请检查权限。");
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("搜索内容异常：路径过长", ex, rootPath);
            return ToolResult.Fail<string>($"路径过长: {rootPath}。请缩短路径或尝试其他路径。");
        }
        catch (IOException ex)
        {
            Logger.Error("搜索内容异常：IO 错误", ex, rootPath);
            return ToolResult.Fail<string>($"IO 错误: {rootPath}。{ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("搜索内容异常", ex, rootPath);
            return ToolResult.Fail<string>($"操作失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <returns>创建结果</returns>
    [Description("创建目录，支持递归创建父目录")]
    public async Task<ToolResult<string>> CreateDirectoryAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return ToolResult.Fail<string>($"错误：路径 {path} 不在允许访问的范围内");

        if (await ConfirmOrBlockAsync(nameof(CreateDirectoryAsync), path,
            new Dictionary<string, object?> { ["path"] = path }) is { } blocked)
        {
            return blocked;
        }

        try
        {
            if (Directory.Exists(path))
                return ToolResult.Ok<string>($"目录已存在: {path}");

            Directory.CreateDirectory(path);
            return ToolResult.Ok<string>($"已创建目录 {path}");
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("创建目录异常：父目录不存在", ex, path);
            return ToolResult.Fail<string>($"父目录不存在: {path}。请检查路径是否正确。");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("创建目录异常：权限不足", ex, path);
            return ToolResult.Fail<string>($"无法创建目录: {path}，权限不足。请检查权限。");
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("创建目录异常：路径过长", ex, path);
            return ToolResult.Fail<string>($"路径过长: {path}。请缩短路径或尝试其他路径。");
        }
        catch (ArgumentException ex)
        {
            Logger.Error("创建目录异常：路径无效", ex, path);
            return ToolResult.Fail<string>($"路径无效: {path}。{ex.Message}");
        }
        catch (IOException ex)
        {
            Logger.Error("创建目录异常：IO 错误", ex, path);
            return ToolResult.Fail<string>($"IO 错误: {path}。{ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("创建目录异常", ex, path);
            return ToolResult.Fail<string>($"操作失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 复制文件
    /// </summary>
    /// <param name="sourcePath">源文件路径</param>
    /// <param name="destPath">目标路径</param>
    /// <param name="overwrite">是否覆盖</param>
    /// <returns>复制结果</returns>
    [Description("复制文件到目标路径")]
    public async Task<ToolResult<string>> CopyFileAsync(string sourcePath, string destPath, bool overwrite = false)
    {
        if (!_pathGuard.IsAllowed(sourcePath))
            return ToolResult.Fail<string>($"错误：源路径 {sourcePath} 不在允许访问的范围内");

        if (!_pathGuard.IsAllowed(destPath))
            return ToolResult.Fail<string>($"错误：目标路径 {destPath} 不在允许访问的范围内");

        var confirmArgs = new Dictionary<string, object?>
        {
            ["sourcePath"] = sourcePath,
            ["destPath"] = destPath,
            ["overwrite"] = overwrite
        };

        // 源、目标路径分别评估；Plan 模式下首次命中即返回，避免重复记录计划项
        if (await ConfirmOrBlockAsync(nameof(CopyFileAsync), sourcePath, confirmArgs) is { } blockedSource)
        {
            return blockedSource;
        }

        if (await ConfirmOrBlockAsync(nameof(CopyFileAsync), destPath, confirmArgs) is { } blockedDest)
        {
            return blockedDest;
        }

        try
        {
            if (Directory.Exists(sourcePath))
                return ToolResult.Fail<string>(DirectoryPathMessage(sourcePath, "复制源必须是文件"));

            if (!File.Exists(sourcePath))
                return ToolResult.Fail<string>($"错误：源文件不存在 ({sourcePath})");

            if (Directory.Exists(destPath))
                return ToolResult.Fail<string>(DirectoryPathMessage(destPath, "复制目标是文件路径（含文件名），不能是目录"));

            var sourceInfo = new FileInfo(sourcePath);
            if (sourceInfo.Length > 500 * 1024 * 1024)
                return ToolResult.Fail<string>($"错误：源文件过大 ({sourceInfo.Length / 1024 / 1024}MB)，复制操作最大支持 500MB");

            if (!overwrite && File.Exists(destPath))
                return ToolResult.Fail<string>($"错误：目标文件已存在 ({destPath})，如需覆盖请设置 overwrite=true");

            using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await sourceStream.CopyToAsync(destStream).ConfigureAwait(false);
            return ToolResult.Ok<string>($"已复制文件 {sourcePath} -> {destPath}");
        }
        catch (FileNotFoundException ex)
        {
            Logger.Error("复制文件异常：源文件不存在", ex, sourcePath);
            return ToolResult.Fail<string>($"未找到源文件: {sourcePath}。请检查路径是否正确。");
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("复制文件异常：目录不存在", ex, destPath);
            return ToolResult.Fail<string>($"未找到目标目录: {destPath}。请检查路径是否正确。");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("复制文件异常：权限不足", ex, destPath);
            return ToolResult.Fail<string>($"无法访问路径: {destPath}，权限不足。请检查权限。");
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("复制文件异常：路径过长", ex, destPath);
            return ToolResult.Fail<string>($"路径过长: {destPath}。请缩短路径或尝试其他路径。");
        }
        catch (ArgumentException ex)
        {
            Logger.Error("复制文件异常：路径无效", ex, destPath);
            return ToolResult.Fail<string>($"路径无效: {destPath}。{ex.Message}");
        }
        catch (IOException ex)
        {
            Logger.Error("复制文件异常：文件可能被独占锁定", ex, destPath);
            return ToolResult.Fail<string>($"无法复制文件: 文件可能被其他程序独占锁定，请关闭占用该文件的程序后重试。详细信息: {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("复制文件异常", ex, destPath);
            return ToolResult.Fail<string>($"操作失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 移动或重命名文件
    /// </summary>
    /// <param name="sourcePath">源文件路径</param>
    /// <param name="destPath">目标路径</param>
    /// <returns>移动结果</returns>
    [Description("移动或重命名文件")]
    public async Task<ToolResult<string>> MoveFileAsync(string sourcePath, string destPath)
    {
        if (!_pathGuard.IsAllowed(sourcePath))
            return ToolResult.Fail<string>($"错误：源路径 {sourcePath} 不在允许访问的范围内");

        if (!_pathGuard.IsAllowed(destPath))
            return ToolResult.Fail<string>($"错误：目标路径 {destPath} 不在允许访问的范围内");

        var confirmArgs = new Dictionary<string, object?>
        {
            ["sourcePath"] = sourcePath,
            ["destPath"] = destPath
        };

        // 源、目标路径分别评估；Plan 模式下首次命中即返回，避免重复记录计划项
        if (await ConfirmOrBlockAsync(nameof(MoveFileAsync), sourcePath, confirmArgs) is { } blockedSource)
        {
            return blockedSource;
        }

        if (await ConfirmOrBlockAsync(nameof(MoveFileAsync), destPath, confirmArgs) is { } blockedDest)
        {
            return blockedDest;
        }

        try
        {
            if (Directory.Exists(sourcePath))
                return ToolResult.Fail<string>(DirectoryPathMessage(sourcePath, "移动源必须是文件"));

            if (!File.Exists(sourcePath))
                return ToolResult.Fail<string>($"错误：源文件不存在 ({sourcePath})");

            if (Directory.Exists(destPath))
                return ToolResult.Fail<string>(DirectoryPathMessage(destPath, "移动目标是文件路径（含文件名），不能是目录"));

            if (File.Exists(destPath))
                return ToolResult.Fail<string>($"错误：目标文件已存在 ({destPath})，无法覆盖");

            File.Move(sourcePath, destPath);
            return ToolResult.Ok<string>($"已移动文件 {sourcePath} -> {destPath}");
        }
        catch (FileNotFoundException ex)
        {
            Logger.Error("移动文件异常：源文件不存在", ex, sourcePath);
            return ToolResult.Fail<string>($"未找到源文件: {sourcePath}。请检查路径是否正确。");
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("移动文件异常：目录不存在", ex, destPath);
            return ToolResult.Fail<string>($"未找到目标目录: {destPath}。请检查路径是否正确。");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("移动文件异常：权限不足", ex, destPath);
            return ToolResult.Fail<string>($"无法访问路径: {destPath}，权限不足。请检查权限。");
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("移动文件异常：路径过长", ex, destPath);
            return ToolResult.Fail<string>($"路径过长: {destPath}。请缩短路径或尝试其他路径。");
        }
        catch (ArgumentException ex)
        {
            Logger.Error("移动文件异常：路径无效", ex, destPath);
            return ToolResult.Fail<string>($"路径无效: {destPath}。{ex.Message}");
        }
        catch (IOException ex)
        {
            Logger.Error("移动文件异常：IO 错误", ex, destPath);
            return ToolResult.Fail<string>($"IO 错误: {destPath}。{ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("移动文件异常", ex, destPath);
            return ToolResult.Fail<string>($"操作失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取文件或目录信息
    /// </summary>
    /// <param name="path">文件或目录路径</param>
    /// <returns>详细信息</returns>
    [Description("获取文件或目录的详细信息，包括大小、修改时间等")]
    public Task<ToolResult<string>> GetFileInfoAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return Task.FromResult(ToolResult.Fail<string>($"错误：路径 {path} 不在允许访问的范围内"));

        try
        {
            if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                var output = new StringBuilder();
                output.AppendLine($"文件: {fileInfo.FullName}");
                output.AppendLine($"大小: {fileInfo.Length:N0} 字节");
                output.AppendLine($"创建时间: {fileInfo.CreationTime:yyyy-MM-dd HH:mm:ss}");
                output.AppendLine($"修改时间: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
                output.AppendLine($"扩展名: {fileInfo.Extension}");
                output.AppendLine($"属性: {fileInfo.Attributes}");
                return Task.FromResult(ToolResult.Ok<string>(output.ToString().TrimEnd()));
            }
            else if (Directory.Exists(path))
            {
                var dirInfo = new DirectoryInfo(path);
                
                int fileCount = 0;
                try
                {
                    foreach (var _ in dirInfo.EnumerateFiles())
                    {
                        if (++fileCount >= 10000) break;
                    }
                }
                catch (UnauthorizedAccessException ex) { Logger.Debug($"访问被拒绝: {ex.Message}"); }
                catch (DirectoryNotFoundException ex) { Logger.Debug($"目录未找到: {ex.Message}"); }

                int dirCount = 0;
                try
                {
                    foreach (var _ in dirInfo.EnumerateDirectories())
                    {
                        if (++dirCount >= 10000) break;
                    }
                }
                catch (UnauthorizedAccessException ex) { Logger.Debug($"访问被拒绝: {ex.Message}"); }
                catch (DirectoryNotFoundException ex) { Logger.Debug($"目录未找到: {ex.Message}"); }

                var output = new StringBuilder();
                output.AppendLine($"目录: {dirInfo.FullName}");
                output.AppendLine($"文件数: {(fileCount >= 10000 ? "10000+" : fileCount.ToString())}");
                output.AppendLine($"子目录数: {(dirCount >= 10000 ? "10000+" : dirCount.ToString())}");
                output.AppendLine($"创建时间: {dirInfo.CreationTime:yyyy-MM-dd HH:mm:ss}");
                output.AppendLine($"修改时间: {dirInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
                return Task.FromResult(ToolResult.Ok<string>(output.ToString().TrimEnd()));
            }
            else
            {
                return Task.FromResult(ToolResult.Fail<string>($"错误：路径不存在 ({path})"));
            }
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("获取信息异常：目录不存在", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"未找到目录: {path}。请检查路径是否正确。"));
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("获取信息异常：权限不足", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"无法访问路径: {path}，权限不足。请检查权限。"));
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("获取信息异常：路径过长", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"路径过长: {path}。请缩短路径或尝试其他路径。"));
        }
        catch (ArgumentException ex)
        {
            Logger.Error("获取信息异常：路径无效", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"路径无效: {path}。{ex.Message}"));
        }
        catch (IOException ex)
        {
            Logger.Error("获取信息异常：IO 错误", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"IO 错误: {path}。{ex.Message}"));
        }
        catch (Exception ex)
        {
            Logger.Error("获取信息异常", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"操作失败: {ex.Message}"));
        }
    }
}
