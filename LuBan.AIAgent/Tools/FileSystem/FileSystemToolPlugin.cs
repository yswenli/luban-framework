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
        var toolGroup = new FileSystemToolGroup(_pathGuard);
        var tools = new List<AIFunction>();

        foreach (var method in typeof(FileSystemToolGroup).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            var func = AIFunctionFactory.Create(method, toolGroup);
            tools.Add(func);
        }

        return tools;
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

    /// <summary>
    /// 创建 FileSystemToolGroup 实例
    /// </summary>
    /// <param name="pathGuard">路径守卫</param>
    public FileSystemToolGroup(PathGuard pathGuard)
    {
        _pathGuard = pathGuard;
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

    private static IEnumerable<string> EnumerateFilesSafe(string rootPath)
    {
        IEnumerable<string> files;
        try
        {
            files = Directory.EnumerateFiles(rootPath);
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }
        catch (DirectoryNotFoundException)
        {
            yield break;
        }

        foreach (var file in files)
            yield return file;

        IEnumerable<string> dirs;
        try
        {
            dirs = Directory.EnumerateDirectories(rootPath);
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }
        catch (DirectoryNotFoundException)
        {
            yield break;
        }

        foreach (var dir in dirs)
        {
            foreach (var file in EnumerateFilesSafe(dir))
                yield return file;
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
    public async Task<string> ReadFileAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return $"错误：路径 {path} 不在允许访问的范围内";

        // 工作区外读取需确认
        if (!ToolConfirmationService.TryConfirmByPath("ReadFileAsync", path,
            new Dictionary<string, object?> { ["path"] = path }))
        {
            return "操作已被用户取消";
        }

        try
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Length > 50 * 1024 * 1024)
                return $"错误：文件过大 ({fileInfo.Length / 1024 / 1024}MB)，最大支持 50MB";

            return await File.ReadAllTextAsync(path);
        }
        catch (FileNotFoundException ex)
        {
            Logger.Error("文件读取异常：文件不存在", ex, path);
            return $"读取文件失败: 文件不存在 ({path})。请确认路径是否正确。";
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("文件读取异常：目录不存在", ex, path);
            return $"读取文件失败: 目录不存在 ({path})。请确认路径是否正确。";
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("文件读取异常：权限不足", ex, path);
            return $"读取文件失败: 权限不足，无法访问 ({path})。请检查文件权限或选择其他文件。";
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("文件读取异常：路径过长", ex, path);
            return $"读取文件失败: 路径过长 ({path})。请缩短路径或使用其他路径。";
        }
        catch (ArgumentException ex)
        {
            Logger.Error("文件读取异常：路径无效", ex, path);
            return $"读取文件失败: 路径无效 ({path})。{ex.Message}";
        }
        catch (IOException ex)
        {
            Logger.Error("文件读取异常：IO 错误", ex, path);
            return $"读取文件失败: IO 错误 ({path})。{ex.Message}";
        }
        catch (Exception ex)
        {
            Logger.Error("文件读取异常", ex, path);
            return $"读取文件失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 写入文件
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="content">文件内容</param>
    /// <returns>写入结果</returns>
    [Description("写入文件内容")]
    public async Task<string> WriteFileAsync(string path, string content)
    {
        if (!_pathGuard.IsAllowed(path))
            return $"错误：路径 {path} 不在允许访问的范围内";

        // 工作区内写入免确认，工作区外需确认
        if (!ToolConfirmationService.TryConfirmByPath("WriteFileAsync", path,
            new Dictionary<string, object?> { ["path"] = path, ["content"] = content }))
        {
            return "操作已被用户取消";
        }

        try
        {
            await File.WriteAllTextAsync(path, content);
            return $"已写入文件 {path}";
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("文件写入异常：目录不存在", ex, path);
            return $"写入文件失败: 目录不存在 ({path})。请确认路径是否正确或先创建目录。";
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("文件写入异常：权限不足", ex, path);
            return $"写入文件失败: 权限不足，无法访问 ({path})。请检查文件权限或选择其他路径。";
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("文件写入异常：路径过长", ex, path);
            return $"写入文件失败: 路径过长 ({path})。请缩短路径或使用其他路径。";
        }
        catch (ArgumentException ex)
        {
            Logger.Error("文件写入异常：路径无效", ex, path);
            return $"写入文件失败: 路径无效 ({path})。{ex.Message}";
        }
        catch (IOException ex)
        {
            Logger.Error("文件写入异常：IO 错误", ex, path);
            return $"写入文件失败: IO 错误 ({path})。{ex.Message}";
        }
        catch (Exception ex)
        {
            Logger.Error("文件写入异常", ex, path);
            return $"写入文件失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 列出目录内容
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <returns>目录内容</returns>
    [Description("列出目录内容")]
    public Task<string> ListDirectoryAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return Task.FromResult($"错误：路径 {path} 不在允许访问的范围内");

        // 工作区外列目录需确认
        if (!ToolConfirmationService.TryConfirmByPath("ListDirectoryAsync", path,
            new Dictionary<string, object?> { ["path"] = path }))
        {
            return Task.FromResult("操作已被用户取消");
        }

        try
        {
            var entries = Directory.EnumerateFileSystemEntries(path);
            return Task.FromResult(string.Join("\n", entries));
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("列出目录异常：目录不存在", ex, path);
            return Task.FromResult($"列出目录失败: 目录不存在 ({path})。请确认路径是否正确。");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("列出目录异常：权限不足", ex, path);
            return Task.FromResult($"列出目录失败: 权限不足，无法访问 ({path})。请检查目录权限或选择其他目录。");
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("列出目录异常：路径过长", ex, path);
            return Task.FromResult($"列出目录失败: 路径过长 ({path})。请缩短路径或使用其他路径。");
        }
        catch (ArgumentException ex)
        {
            Logger.Error("列出目录异常：路径无效", ex, path);
            return Task.FromResult($"列出目录失败: 路径无效 ({path})。{ex.Message}");
        }
        catch (IOException ex)
        {
            Logger.Error("列出目录异常：IO 错误", ex, path);
            return Task.FromResult($"列出目录失败: IO 错误 ({path})。{ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("列出目录异常", ex, path);
            return Task.FromResult($"列出目录失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>删除结果</returns>
    [Description("删除文件（无论是否在工作区内，都必须确认）")]
    public Task<string> DeleteFileAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return Task.FromResult($"错误：路径 {path} 不在允许访问的范围内");

        // 删除操作：始终需要确认
        if (!ToolConfirmationService.TryConfirmByPath("DeleteFileAsync", path,
            new Dictionary<string, object?> { ["path"] = path }))
        {
            return Task.FromResult("操作已被用户取消");
        }

        try
        {
            if (!File.Exists(path))
                return Task.FromResult($"错误：文件不存在 ({path})");

            File.Delete(path);
            return Task.FromResult($"已删除文件 {path}");
        }
        catch (FileNotFoundException ex)
        {
            Logger.Error("文件删除异常：文件不存在", ex, path);
            return Task.FromResult($"删除文件失败: 文件不存在 ({path})。请确认路径是否正确。");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("文件删除异常：权限不足", ex, path);
            return Task.FromResult($"删除文件失败: 权限不足，无法删除 ({path})。请检查文件权限。");
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("文件删除异常：路径过长", ex, path);
            return Task.FromResult($"删除文件失败: 路径过长 ({path})。请缩短路径或使用其他路径。");
        }
        catch (ArgumentException ex)
        {
            Logger.Error("文件删除异常：路径无效", ex, path);
            return Task.FromResult($"删除文件失败: 路径无效 ({path})。{ex.Message}");
        }
        catch (IOException ex)
        {
            Logger.Error("文件删除异常：IO 错误", ex, path);
            return Task.FromResult($"删除文件失败: IO 错误 ({path})。{ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("文件删除异常", ex, path);
            return Task.FromResult($"删除文件失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除目录（递归删除所有子项）
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <returns>删除结果</returns>
    [Description("删除目录及其所有内容（无论是否在工作区内，都必须确认）")]
    public Task<string> DeleteDirectoryAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return Task.FromResult($"错误：路径 {path} 不在允许访问的范围内");

        // 删除操作：始终需要确认
        if (!ToolConfirmationService.TryConfirmByPath("DeleteDirectoryAsync", path,
            new Dictionary<string, object?> { ["path"] = path }))
        {
            return Task.FromResult("操作已被用户取消");
        }

        try
        {
            if (!Directory.Exists(path))
                return Task.FromResult($"错误：目录不存在 ({path})");

            Directory.Delete(path, recursive: true);
            return Task.FromResult($"已删除目录 {path}");
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("目录删除异常：目录不存在", ex, path);
            return Task.FromResult($"删除目录失败: 目录不存在 ({path})。请确认路径是否正确。");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("目录删除异常：权限不足", ex, path);
            return Task.FromResult($"删除目录失败: 权限不足，无法删除 ({path})。请检查目录权限。");
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("目录删除异常：路径过长", ex, path);
            return Task.FromResult($"删除目录失败: 路径过长 ({path})。请缩短路径或使用其他路径。");
        }
        catch (ArgumentException ex)
        {
            Logger.Error("目录删除异常：路径无效", ex, path);
            return Task.FromResult($"删除目录失败: 路径无效 ({path})。{ex.Message}");
        }
        catch (IOException ex)
        {
            Logger.Error("目录删除异常：IO 错误", ex, path);
            return Task.FromResult($"删除目录失败: IO 错误 ({path})。{ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("目录删除异常", ex, path);
            return Task.FromResult($"删除目录失败: {ex.Message}");
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
    public Task<string> SearchFilesAsync(string rootPath, string pattern, int maxResults = 100)
    {
        if (!_pathGuard.IsAllowed(rootPath))
            return Task.FromResult($"错误：路径 {rootPath} 不在允许访问的范围内");

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
                return Task.FromResult("未找到匹配的文件");

            var output = new StringBuilder();
            output.AppendLine($"找到 {results.Count} 个匹配文件：");
            foreach (var file in results)
                output.AppendLine(file);

            return Task.FromResult(output.ToString().TrimEnd());
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("搜索文件异常：目录不存在", ex, rootPath);
            return Task.FromResult($"搜索文件失败: 目录不存在 ({rootPath})。请确认路径是否正确。");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("搜索文件异常：权限不足", ex, rootPath);
            return Task.FromResult($"搜索文件失败: 权限不足，无法访问 ({rootPath})。请检查目录权限。");
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("搜索文件异常：路径过长", ex, rootPath);
            return Task.FromResult($"搜索文件失败: 路径过长 ({rootPath})。请缩短路径或使用其他路径。");
        }
        catch (ArgumentException ex)
        {
            Logger.Error("搜索文件异常：路径无效", ex, rootPath);
            return Task.FromResult($"搜索文件失败: 路径无效 ({rootPath})。{ex.Message}");
        }
        catch (IOException ex)
        {
            Logger.Error("搜索文件异常：IO 错误", ex, rootPath);
            return Task.FromResult($"搜索文件失败: IO 错误 ({rootPath})。{ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("搜索文件异常", ex, rootPath);
            return Task.FromResult($"搜索文件失败: {ex.Message}");
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
    public async Task<string> GrepAsync(string rootPath, string pattern, string? filePattern = null, int maxResults = 100)
    {
        if (!_pathGuard.IsAllowed(rootPath))
            return $"错误：路径 {rootPath} 不在允许访问的范围内";

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

                var fileInfo = new FileInfo(file);
                if (fileInfo.Length > 1024 * 1024)
                    continue;

                try
                {
                    using var reader = new StreamReader(file);
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
                return "未找到匹配的内容";

            var output = new StringBuilder();
            output.AppendLine($"找到 {results.Count} 处匹配：");
            foreach (var match in results)
                output.AppendLine(match);

            if (results.Count >= maxResults)
                output.AppendLine($"\n结果已截断，当前显示前 {maxResults} 处匹配。缩小搜索范围或增大 maxResults 参数查看更多。");

            return output.ToString().TrimEnd();
        }
        catch (RegexMatchTimeoutException ex)
        {
            Logger.Error("搜索内容异常：正则匹配超时", ex, pattern);
            return $"搜索内容失败: 正则表达式匹配超时，请简化正则表达式 ({pattern})。";
        }
        catch (ArgumentException ex)
        {
            Logger.Error("搜索内容异常：正则表达式无效", ex, pattern);
            return $"搜索内容失败: 正则表达式无效 ({pattern})。{ex.Message}";
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("搜索内容异常：目录不存在", ex, rootPath);
            return $"搜索内容失败: 目录不存在 ({rootPath})。请确认路径是否正确。";
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("搜索内容异常：权限不足", ex, rootPath);
            return $"搜索内容失败: 权限不足，无法访问 ({rootPath})。请检查目录权限。";
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("搜索内容异常：路径过长", ex, rootPath);
            return $"搜索内容失败: 路径过长 ({rootPath})。请缩短路径或使用其他路径。";
        }
        catch (IOException ex)
        {
            Logger.Error("搜索内容异常：IO 错误", ex, rootPath);
            return $"搜索内容失败: IO 错误 ({rootPath})。{ex.Message}";
        }
        catch (Exception ex)
        {
            Logger.Error("搜索内容异常", ex, rootPath);
            return $"搜索内容失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <returns>创建结果</returns>
    [Description("创建目录，支持递归创建父目录")]
    public Task<string> CreateDirectoryAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return Task.FromResult($"错误：路径 {path} 不在允许访问的范围内");

        if (!ToolConfirmationService.TryConfirmByPath("CreateDirectoryAsync", path,
            new Dictionary<string, object?> { ["path"] = path }))
        {
            return Task.FromResult("操作已被用户取消");
        }

        try
        {
            if (Directory.Exists(path))
                return Task.FromResult($"目录已存在: {path}");

            Directory.CreateDirectory(path);
            return Task.FromResult($"已创建目录 {path}");
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("创建目录异常：父目录不存在", ex, path);
            return Task.FromResult($"创建目录失败: 父目录不存在 ({path})。请确认路径是否正确。");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("创建目录异常：权限不足", ex, path);
            return Task.FromResult($"创建目录失败: 权限不足，无法访问 ({path})。请检查目录权限。");
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("创建目录异常：路径过长", ex, path);
            return Task.FromResult($"创建目录失败: 路径过长 ({path})。请缩短路径或使用其他路径。");
        }
        catch (ArgumentException ex)
        {
            Logger.Error("创建目录异常：路径无效", ex, path);
            return Task.FromResult($"创建目录失败: 路径无效 ({path})。{ex.Message}");
        }
        catch (IOException ex)
        {
            Logger.Error("创建目录异常：IO 错误", ex, path);
            return Task.FromResult($"创建目录失败: IO 错误 ({path})。{ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("创建目录异常", ex, path);
            return Task.FromResult($"创建目录失败: {ex.Message}");
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
    public Task<string> CopyFileAsync(string sourcePath, string destPath, bool overwrite = false)
    {
        if (!_pathGuard.IsAllowed(sourcePath))
            return Task.FromResult($"错误：源路径 {sourcePath} 不在允许访问的范围内");

        if (!_pathGuard.IsAllowed(destPath))
            return Task.FromResult($"错误：目标路径 {destPath} 不在允许访问的范围内");

        if (!ToolConfirmationService.TryConfirmByPath("CopyFileAsync", sourcePath,
            new Dictionary<string, object?> { ["sourcePath"] = sourcePath, ["destPath"] = destPath, ["overwrite"] = overwrite }))
        {
            return Task.FromResult("操作已被用户取消");
        }

        try
        {
            if (!File.Exists(sourcePath))
                return Task.FromResult($"错误：源文件不存在 ({sourcePath})");

            File.Copy(sourcePath, destPath, overwrite);
            return Task.FromResult($"已复制文件 {sourcePath} -> {destPath}");
        }
        catch (FileNotFoundException ex)
        {
            Logger.Error("复制文件异常：源文件不存在", ex, sourcePath);
            return Task.FromResult($"复制文件失败: 源文件不存在 ({sourcePath})。请确认路径是否正确。");
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("复制文件异常：目录不存在", ex, destPath);
            return Task.FromResult($"复制文件失败: 目标目录不存在 ({destPath})。请确认路径是否正确。");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("复制文件异常：权限不足", ex, destPath);
            return Task.FromResult($"复制文件失败: 权限不足，无法访问 ({destPath})。请检查文件权限。");
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("复制文件异常：路径过长", ex, destPath);
            return Task.FromResult($"复制文件失败: 路径过长 ({destPath})。请缩短路径或使用其他路径。");
        }
        catch (ArgumentException ex)
        {
            Logger.Error("复制文件异常：路径无效", ex, destPath);
            return Task.FromResult($"复制文件失败: 路径无效 ({destPath})。{ex.Message}");
        }
        catch (IOException ex)
        {
            Logger.Error("复制文件异常：IO 错误", ex, destPath);
            return Task.FromResult($"复制文件失败: IO 错误 ({destPath})。{ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("复制文件异常", ex, destPath);
            return Task.FromResult($"复制文件失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 移动或重命名文件
    /// </summary>
    /// <param name="sourcePath">源文件路径</param>
    /// <param name="destPath">目标路径</param>
    /// <returns>移动结果</returns>
    [Description("移动或重命名文件")]
    public Task<string> MoveFileAsync(string sourcePath, string destPath)
    {
        if (!_pathGuard.IsAllowed(sourcePath))
            return Task.FromResult($"错误：源路径 {sourcePath} 不在允许访问的范围内");

        if (!_pathGuard.IsAllowed(destPath))
            return Task.FromResult($"错误：目标路径 {destPath} 不在允许访问的范围内");

        if (!ToolConfirmationService.TryConfirmByPath("MoveFileAsync", sourcePath,
            new Dictionary<string, object?> { ["sourcePath"] = sourcePath, ["destPath"] = destPath }))
        {
            return Task.FromResult("操作已被用户取消");
        }

        try
        {
            if (!File.Exists(sourcePath))
                return Task.FromResult($"错误：源文件不存在 ({sourcePath})");

            if (File.Exists(destPath))
                return Task.FromResult($"错误：目标文件已存在 ({destPath})，无法覆盖");

            File.Move(sourcePath, destPath);
            return Task.FromResult($"已移动文件 {sourcePath} -> {destPath}");
        }
        catch (FileNotFoundException ex)
        {
            Logger.Error("移动文件异常：源文件不存在", ex, sourcePath);
            return Task.FromResult($"移动文件失败: 源文件不存在 ({sourcePath})。请确认路径是否正确。");
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("移动文件异常：目录不存在", ex, destPath);
            return Task.FromResult($"移动文件失败: 目标目录不存在 ({destPath})。请确认路径是否正确。");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("移动文件异常：权限不足", ex, destPath);
            return Task.FromResult($"移动文件失败: 权限不足，无法访问 ({destPath})。请检查文件权限。");
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("移动文件异常：路径过长", ex, destPath);
            return Task.FromResult($"移动文件失败: 路径过长 ({destPath})。请缩短路径或使用其他路径。");
        }
        catch (ArgumentException ex)
        {
            Logger.Error("移动文件异常：路径无效", ex, destPath);
            return Task.FromResult($"移动文件失败: 路径无效 ({destPath})。{ex.Message}");
        }
        catch (IOException ex)
        {
            Logger.Error("移动文件异常：IO 错误", ex, destPath);
            return Task.FromResult($"移动文件失败: IO 错误 ({destPath})。{ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("移动文件异常", ex, destPath);
            return Task.FromResult($"移动文件失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取文件或目录信息
    /// </summary>
    /// <param name="path">文件或目录路径</param>
    /// <returns>详细信息</returns>
    [Description("获取文件或目录的详细信息，包括大小、修改时间等")]
    public Task<string> GetFileInfoAsync(string path)
    {
        if (!_pathGuard.IsAllowed(path))
            return Task.FromResult($"错误：路径 {path} 不在允许访问的范围内");

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
                return Task.FromResult(output.ToString().TrimEnd());
            }
            else if (Directory.Exists(path))
            {
                var dirInfo = new DirectoryInfo(path);
                var fileCount = dirInfo.EnumerateFiles().Take(10000).Count();
                var dirCount = dirInfo.EnumerateDirectories().Take(10000).Count();

                var output = new StringBuilder();
                output.AppendLine($"目录: {dirInfo.FullName}");
                output.AppendLine($"文件数: {(fileCount >= 10000 ? "10000+" : fileCount.ToString())}");
                output.AppendLine($"子目录数: {(dirCount >= 10000 ? "10000+" : dirCount.ToString())}");
                output.AppendLine($"创建时间: {dirInfo.CreationTime:yyyy-MM-dd HH:mm:ss}");
                output.AppendLine($"修改时间: {dirInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
                return Task.FromResult(output.ToString().TrimEnd());
            }
            else
            {
                return Task.FromResult($"错误：路径不存在 ({path})");
            }
        }
        catch (DirectoryNotFoundException ex)
        {
            Logger.Error("获取信息异常：目录不存在", ex, path);
            return Task.FromResult($"获取信息失败: 目录不存在 ({path})。请确认路径是否正确。");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.Error("获取信息异常：权限不足", ex, path);
            return Task.FromResult($"获取信息失败: 权限不足，无法访问 ({path})。请检查文件权限。");
        }
        catch (PathTooLongException ex)
        {
            Logger.Error("获取信息异常：路径过长", ex, path);
            return Task.FromResult($"获取信息失败: 路径过长 ({path})。请缩短路径或使用其他路径。");
        }
        catch (ArgumentException ex)
        {
            Logger.Error("获取信息异常：路径无效", ex, path);
            return Task.FromResult($"获取信息失败: 路径无效 ({path})。{ex.Message}");
        }
        catch (IOException ex)
        {
            Logger.Error("获取信息异常：IO 错误", ex, path);
            return Task.FromResult($"获取信息失败: IO 错误 ({path})。{ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("获取信息异常", ex, path);
            return Task.FromResult($"获取信息失败: {ex.Message}");
        }
    }
}
