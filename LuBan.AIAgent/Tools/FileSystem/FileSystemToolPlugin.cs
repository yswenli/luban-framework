/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
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
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp)
    {
        var confirmationService = sp.GetService(typeof(Services.IToolConfirmationService)) as Services.IToolConfirmationService
            ?? new Services.ToolConfirmationService(new Services.ToolConfirmationContext());
        var toolGroup = new FileSystemToolGroup(_pathGuard, confirmationService);
        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.ReadFileAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.WriteFileAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.ListDirectoryAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.GetWorkspaceOverviewAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.DeleteFileAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.DeleteDirectoryAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.SearchFilesAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(FileSystemToolGroup.GrepAsync)),
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
    private readonly Services.IToolConfirmationService _confirmationService;

    /// <summary>
    /// 创建 FileSystemToolGroup 实例
    /// </summary>
    /// <param name="pathGuard">路径守卫</param>
    /// <param name="confirmationService">工具调用确认服务</param>
    public FileSystemToolGroup(PathGuard pathGuard, Services.IToolConfirmationService confirmationService)
    {
        _pathGuard = pathGuard;
        _confirmationService = confirmationService;
    }

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
        "readme.md", "readme.en.md", "readme.txt",
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
            catch (UnauthorizedAccessException) { continue; }
            catch (DirectoryNotFoundException) { continue; }

            foreach (var file in files)
                yield return file;

            IEnumerable<string> subDirs;
            try
            {
                subDirs = Directory.EnumerateDirectories(current);
            }
            catch (UnauthorizedAccessException) { continue; }
            catch (DirectoryNotFoundException) { continue; }

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
        if (!_confirmationService.TryConfirmByPath("ReadFileAsync", path,
            new Dictionary<string, object?> { ["path"] = path }))
        {
            return ToolResult.Cancelled<string>();
        }

        try
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Length > 50 * 1024 * 1024)
                return ToolResult.Fail<string>($"错误：文件过大 ({fileInfo.Length / 1024 / 1024}MB)，最大支持 50MB");

            return ToolResult.Ok<string>(await File.ReadAllTextAsync(path));
        }
        catch (FileNotFoundException ex)
        {
            Logger.Error("文件读取异常：文件不存在", ex, path);
            return ToolResult.Fail<string>($"未找到文件: {path}。请检查路径是否正确，或尝试其他路径。");
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("文件读取异常：目录不存在", ex, path);
            return ToolResult.Fail<string>($"未找到目录: {path}。请检查路径是否正确，或尝试其他路径。");
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
            Logger.Error("文件读取异常：IO 错误", ex, path);
            return ToolResult.Fail<string>($"IO 错误: {path}。{ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("文件读取异常", ex, path);
            return ToolResult.Fail<string>($"操作失败: {ex.Message}");
        }
    }

/// <summary>
/// 写入文件内容到指定路径。对于临时文件，建议使用工作区临时目录：.luban-agent/temp/
/// </summary>
/// <param name="path">文件路径（相对或绝对）</param>
/// <param name="content">文件内容</param>
/// <returns>操作结果</returns>
[Description(@"写入文件内容到指定路径。

💡 临时文件建议：
- 使用工作区临时目录：.luban-agent/temp/文件名.扩展名
- 示例：.luban-agent/temp/query_users_20260806.py
- 工作区切换时会自动清理24小时前的临时文件")]
public async Task<ToolResult<string>> WriteFileAsync(string path, string content)
    {
        if (!_pathGuard.IsAllowed(path))
            return ToolResult.Fail<string>($"错误：路径 {path} 不在允许访问的范围内");

        // 工作区内写入免确认，工作区外需确认
        if (!_confirmationService.TryConfirmByPath("WriteFileAsync", path,
            new Dictionary<string, object?> { ["path"] = path, ["content"] = content }))
        {
            return ToolResult.Cancelled<string>();
        }

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
    public Task<ToolResult<string>> ListDirectoryAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return Task.FromResult(ToolResult.Fail<string>($"错误：路径 {path} 不在允许访问的范围内"));

        // 工作区外列目录需确认
        if (!_confirmationService.TryConfirmByPath("ListDirectoryAsync", path,
            new Dictionary<string, object?> { ["path"] = path }))
        {
            return Task.FromResult(ToolResult.Cancelled<string>());
        }

        try
        {
            var entries = Directory.EnumerateFileSystemEntries(path);
            return Task.FromResult(ToolResult.Ok<string>(string.Join("\n", entries)));
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("列出目录异常：目录不存在", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"未找到目录: {path}。请检查路径是否正确，或尝试其他路径。"));
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("列出目录异常：权限不足", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"无法访问目录: {path}，权限不足。请检查权限或尝试其他目录。"));
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("列出目录异常：路径过长", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"路径过长: {path}。请缩短路径或尝试其他路径。"));
        }
        catch (ArgumentException ex)
        {
            Logger.Error("列出目录异常：路径无效", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"路径无效: {path}。{ex.Message}"));
        }
        catch (IOException ex)
        {
            Logger.Error("列出目录异常：IO 错误", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"IO 错误: {path}。{ex.Message}"));
        }
        catch (Exception ex)
        {
            Logger.Error("列出目录异常", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"操作失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 获取工作区概览：目录树（限 3 层）+ 文件类型统计 + 关键文件列表。
    /// 一次调用即可让 AI 了解工作区整体结构，避免多次 ListDirectory 探索。
    /// </summary>
    /// <param name="rootPath">工作区根目录</param>
    /// <returns>工作区概览信息</returns>
    [Description("获取工作区概览：目录树（限3层）+ 文件类型统计 + 关键文件。一次调用了解工作区结构，避免多次ListDirectory。")]
    public Task<ToolResult<string>> GetWorkspaceOverviewAsync(string rootPath)
    {
        if (!_pathGuard.IsAllowed(rootPath))
            return Task.FromResult(ToolResult.Fail<string>($"错误：路径 {rootPath} 不在允许访问的范围内"));

        try
        {
            var fullPath = Path.GetFullPath(rootPath);
            if (!Directory.Exists(fullPath))
                return Task.FromResult(ToolResult.Fail<string>($"未找到工作区: {rootPath}。请检查路径是否正确。"));

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

            return Task.FromResult(ToolResult.Ok<string>(sb.ToString().TrimEnd()));
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("工作区概览异常：权限不足", ex, rootPath);
            return Task.FromResult(ToolResult.Fail<string>($"无法访问工作区: {rootPath}，权限不足。请检查权限。"));
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("工作区概览异常：目录不存在", ex, rootPath);
            return Task.FromResult(ToolResult.Fail<string>($"未找到工作区: {rootPath}。请检查路径是否正确。"));
        }
        catch (Exception ex)
        {
            Logger.Error("工作区概览异常", ex, rootPath);
            return Task.FromResult(ToolResult.Fail<string>($"操作失败: {ex.Message}"));
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
        catch (UnauthorizedAccessException) { return; }
        catch (DirectoryNotFoundException) { return; }

        string[] files;
        try
        {
            files = Directory.GetFiles(dir);
        }
        catch (UnauthorizedAccessException) { return; }
        catch (DirectoryNotFoundException) { return; }

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
    public Task<ToolResult<string>> DeleteFileAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return Task.FromResult(ToolResult.Fail<string>($"错误：路径 {path} 不在允许访问的范围内"));

        // 删除操作：始终需要确认
        if (!_confirmationService.TryConfirmByPath("DeleteFileAsync", path,
            new Dictionary<string, object?> { ["path"] = path }))
        {
            return Task.FromResult(ToolResult.Cancelled<string>());
        }

        try
        {
            if (!File.Exists(path))
                return Task.FromResult(ToolResult.Fail<string>($"错误：文件不存在 ({path})"));

            File.Delete(path);
            return Task.FromResult(ToolResult.Ok<string>($"已删除文件 {path}"));
        }
        catch (FileNotFoundException ex)
        {
            Logger.Error("文件删除异常：文件不存在", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"未找到文件: {path}。请检查路径是否正确。"));
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("文件删除异常：权限不足", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"无法删除文件: {path}，权限不足。请检查权限。"));
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("文件删除异常：路径过长", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"路径过长: {path}。请缩短路径或尝试其他路径。"));
        }
        catch (ArgumentException ex)
        {
            Logger.Error("文件删除异常：路径无效", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"路径无效: {path}。{ex.Message}"));
        }
        catch (IOException ex)
        {
            Logger.Error("文件删除异常：IO 错误", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"IO 错误: {path}。{ex.Message}"));
        }
        catch (Exception ex)
        {
            Logger.Error("文件删除异常", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"操作失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 删除目录（递归删除所有子项）
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <returns>删除结果</returns>
    [Description("删除目录及其所有内容（无论是否在工作区内，都必须确认）")]
    public Task<ToolResult<string>> DeleteDirectoryAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return Task.FromResult(ToolResult.Fail<string>($"错误：路径 {path} 不在允许访问的范围内"));

        // 删除操作：始终需要确认
        if (!_confirmationService.TryConfirmByPath("DeleteDirectoryAsync", path,
            new Dictionary<string, object?> { ["path"] = path }))
        {
            return Task.FromResult(ToolResult.Cancelled<string>());
        }

        try
        {
            if (!Directory.Exists(path))
                return Task.FromResult(ToolResult.Fail<string>($"错误：目录不存在 ({path})"));

            Directory.Delete(path, recursive: true);
            return Task.FromResult(ToolResult.Ok<string>($"已删除目录 {path}"));
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("目录删除异常：目录不存在", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"未找到目录: {path}。请检查路径是否正确。"));
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("目录删除异常：权限不足", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"无法删除目录: {path}，权限不足。请检查权限。"));
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("目录删除异常：路径过长", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"路径过长: {path}。请缩短路径或尝试其他路径。"));
        }
        catch (ArgumentException ex)
        {
            Logger.Error("目录删除异常：路径无效", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"路径无效: {path}。{ex.Message}"));
        }
        catch (IOException ex)
        {
            Logger.Error("目录删除异常：IO 错误", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"IO 错误: {path}。{ex.Message}"));
        }
        catch (Exception ex)
        {
            Logger.Error("目录删除异常", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"操作失败: {ex.Message}"));
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
    public Task<ToolResult<string>> CreateDirectoryAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return Task.FromResult(ToolResult.Fail<string>($"错误：路径 {path} 不在允许访问的范围内"));

        if (!_confirmationService.TryConfirmByPath("CreateDirectoryAsync", path,
            new Dictionary<string, object?> { ["path"] = path }))
        {
            return Task.FromResult(ToolResult.Cancelled<string>());
        }

        try
        {
            if (Directory.Exists(path))
                return Task.FromResult(ToolResult.Ok<string>($"目录已存在: {path}"));

            Directory.CreateDirectory(path);
            return Task.FromResult(ToolResult.Ok<string>($"已创建目录 {path}"));
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("创建目录异常：父目录不存在", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"父目录不存在: {path}。请检查路径是否正确。"));
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("创建目录异常：权限不足", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"无法创建目录: {path}，权限不足。请检查权限。"));
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("创建目录异常：路径过长", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"路径过长: {path}。请缩短路径或尝试其他路径。"));
        }
        catch (ArgumentException ex)
        {
            Logger.Error("创建目录异常：路径无效", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"路径无效: {path}。{ex.Message}"));
        }
        catch (IOException ex)
        {
            Logger.Error("创建目录异常：IO 错误", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"IO 错误: {path}。{ex.Message}"));
        }
        catch (Exception ex)
        {
            Logger.Error("创建目录异常", ex, path);
            return Task.FromResult(ToolResult.Fail<string>($"操作失败: {ex.Message}"));
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
    public Task<ToolResult<string>> CopyFileAsync(string sourcePath, string destPath, bool overwrite = false)
    {
        if (!_pathGuard.IsAllowed(sourcePath))
            return Task.FromResult(ToolResult.Fail<string>($"错误：源路径 {sourcePath} 不在允许访问的范围内"));

        if (!_pathGuard.IsAllowed(destPath))
            return Task.FromResult(ToolResult.Fail<string>($"错误：目标路径 {destPath} 不在允许访问的范围内"));

        if (!_confirmationService.TryConfirmByPath("CopyFileAsync", sourcePath,
            new Dictionary<string, object?> { ["sourcePath"] = sourcePath, ["destPath"] = destPath, ["overwrite"] = overwrite }))
        {
            return Task.FromResult(ToolResult.Cancelled<string>());
        }

        if (!_confirmationService.TryConfirmByPath("CopyFileAsync", destPath,
            new Dictionary<string, object?> { ["sourcePath"] = sourcePath, ["destPath"] = destPath, ["overwrite"] = overwrite }))
        {
            return Task.FromResult(ToolResult.Cancelled<string>());
        }

        try
        {
            if (!File.Exists(sourcePath))
                return Task.FromResult(ToolResult.Fail<string>($"错误：源文件不存在 ({sourcePath})"));

            File.Copy(sourcePath, destPath, overwrite);
            return Task.FromResult(ToolResult.Ok<string>($"已复制文件 {sourcePath} -> {destPath}"));
        }
        catch (FileNotFoundException ex)
        {
            Logger.Error("复制文件异常：源文件不存在", ex, sourcePath);
            return Task.FromResult(ToolResult.Fail<string>($"未找到源文件: {sourcePath}。请检查路径是否正确。"));
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("复制文件异常：目录不存在", ex, destPath);
            return Task.FromResult(ToolResult.Fail<string>($"未找到目标目录: {destPath}。请检查路径是否正确。"));
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("复制文件异常：权限不足", ex, destPath);
            return Task.FromResult(ToolResult.Fail<string>($"无法访问路径: {destPath}，权限不足。请检查权限。"));
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("复制文件异常：路径过长", ex, destPath);
            return Task.FromResult(ToolResult.Fail<string>($"路径过长: {destPath}。请缩短路径或尝试其他路径。"));
        }
        catch (ArgumentException ex)
        {
            Logger.Error("复制文件异常：路径无效", ex, destPath);
            return Task.FromResult(ToolResult.Fail<string>($"路径无效: {destPath}。{ex.Message}"));
        }
        catch (IOException ex)
        {
            Logger.Error("复制文件异常：IO 错误", ex, destPath);
            return Task.FromResult(ToolResult.Fail<string>($"IO 错误: {destPath}。{ex.Message}"));
        }
        catch (Exception ex)
        {
            Logger.Error("复制文件异常", ex, destPath);
            return Task.FromResult(ToolResult.Fail<string>($"操作失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 移动或重命名文件
    /// </summary>
    /// <param name="sourcePath">源文件路径</param>
    /// <param name="destPath">目标路径</param>
    /// <returns>移动结果</returns>
    [Description("移动或重命名文件")]
    public Task<ToolResult<string>> MoveFileAsync(string sourcePath, string destPath)
    {
        if (!_pathGuard.IsAllowed(sourcePath))
            return Task.FromResult(ToolResult.Fail<string>($"错误：源路径 {sourcePath} 不在允许访问的范围内"));

        if (!_pathGuard.IsAllowed(destPath))
            return Task.FromResult(ToolResult.Fail<string>($"错误：目标路径 {destPath} 不在允许访问的范围内"));

        if (!_confirmationService.TryConfirmByPath("MoveFileAsync", sourcePath,
            new Dictionary<string, object?> { ["sourcePath"] = sourcePath, ["destPath"] = destPath }))
        {
            return Task.FromResult(ToolResult.Cancelled<string>());
        }

        if (!_confirmationService.TryConfirmByPath("MoveFileAsync", destPath,
            new Dictionary<string, object?> { ["sourcePath"] = sourcePath, ["destPath"] = destPath }))
        {
            return Task.FromResult(ToolResult.Cancelled<string>());
        }

        try
        {
            if (!File.Exists(sourcePath))
                return Task.FromResult(ToolResult.Fail<string>($"错误：源文件不存在 ({sourcePath})"));

            if (File.Exists(destPath))
                return Task.FromResult(ToolResult.Fail<string>($"错误：目标文件已存在 ({destPath})，无法覆盖"));

            File.Move(sourcePath, destPath);
            return Task.FromResult(ToolResult.Ok<string>($"已移动文件 {sourcePath} -> {destPath}"));
        }
        catch (FileNotFoundException ex)
        {
            Logger.Error("移动文件异常：源文件不存在", ex, sourcePath);
            return Task.FromResult(ToolResult.Fail<string>($"未找到源文件: {sourcePath}。请检查路径是否正确。"));
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("移动文件异常：目录不存在", ex, destPath);
            return Task.FromResult(ToolResult.Fail<string>($"未找到目标目录: {destPath}。请检查路径是否正确。"));
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("移动文件异常：权限不足", ex, destPath);
            return Task.FromResult(ToolResult.Fail<string>($"无法访问路径: {destPath}，权限不足。请检查权限。"));
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("移动文件异常：路径过长", ex, destPath);
            return Task.FromResult(ToolResult.Fail<string>($"路径过长: {destPath}。请缩短路径或尝试其他路径。"));
        }
        catch (ArgumentException ex)
        {
            Logger.Error("移动文件异常：路径无效", ex, destPath);
            return Task.FromResult(ToolResult.Fail<string>($"路径无效: {destPath}。{ex.Message}"));
        }
        catch (IOException ex)
        {
            Logger.Error("移动文件异常：IO 错误", ex, destPath);
            return Task.FromResult(ToolResult.Fail<string>($"IO 错误: {destPath}。{ex.Message}"));
        }
        catch (Exception ex)
        {
            Logger.Error("移动文件异常", ex, destPath);
            return Task.FromResult(ToolResult.Fail<string>($"操作失败: {ex.Message}"));
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
                catch (UnauthorizedAccessException) { }
                catch (DirectoryNotFoundException) { }

                int dirCount = 0;
                try
                {
                    foreach (var _ in dirInfo.EnumerateDirectories())
                    {
                        if (++dirCount >= 10000) break;
                    }
                }
                catch (UnauthorizedAccessException) { }
                catch (DirectoryNotFoundException) { }

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
