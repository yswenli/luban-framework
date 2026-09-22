/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Retrieval.Chunkers
*文件名： ChunkerFactory
*版本号： V1.0.0.0
*唯一标识：411d4a15-64a7-4dc8-950a-da1f1018f67c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：切块器工厂
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：切块器工厂
*
*****************************************************************************/
namespace LuBan.AIAgent.Retrieval.Chunkers;

/// <summary>
/// 切块器工厂：按扩展名路由 + 文件排除
/// </summary>
public class ChunkerFactory
{
    private static readonly string[] ExcludedDirs = { ".git", "bin", "obj", "node_modules", "dist", "packages", ".vs", ".idea", "target" };

    /// <summary>
    /// 除切块器已注册扩展名外，额外允许索引的扩展名：
    /// 纯文本（.txt/.csv/.tsv/.log/.properties）与表格（.xlsx，读取时走 Excel 提取）。
    /// 这些扩展名没有专用切块器，走滑动窗口兜底。
    /// 白名单之外的扩展名一律跳过（含 .docx/.pdf/无扩展名文件），扫描阶段不读取内容。
    /// </summary>
    private static readonly string[] ExtraIndexableExtensions = { ".txt", ".csv", ".tsv", ".log", ".properties", ".xlsx" };

    private readonly Dictionary<string, ICodeChunker> _byExtension = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _indexableExtensions;
    private readonly SlidingWindowChunker _fallback = new();

    /// <summary>
    /// 创建切块器工厂
    /// </summary>
    public ChunkerFactory(IEnumerable<ICodeChunker>? customChunkers = null)
    {
        var defaults = CreateDefaults();
        foreach (var c in defaults.Concat(customChunkers ?? Enumerable.Empty<ICodeChunker>()))
            foreach (var ext in c.Extensions)
                _byExtension[ext] = c;
        _indexableExtensions = new HashSet<string>(_byExtension.Keys.Concat(ExtraIndexableExtensions), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 目录名是否属于索引排除目录（按名称匹配，任意层级生效）
    /// </summary>
    public static bool IsExcludedDirectory(string directoryName) => ExcludedDirs.Contains(directoryName, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 是否为重解析点（符号链接/联接点/云盘占位目录）。此类目录不递归，避免目录环与云盘水合下载。
    /// 无法读取属性时按“是”处理（跳过）。
    /// </summary>
    public static bool IsReparsePoint(string directoryPath)
    {
        try { return (File.GetAttributes(directoryPath) & FileAttributes.ReparsePoint) != 0; }
        catch { return true; }
    }

    /// <summary>
    /// 逐层安全枚举文件：跳过排除目录与重解析点，单层不可访问时跳过该层而不中断整体枚举。
    /// 索引（<c>RetrievalService</c>）与预扫（CLI/桌面端）共用，保证两侧口径一致。
    /// 同一路径只产出一次（Windows 文件系统大小写不敏感，按不区分大小写去重）。
    /// </summary>
    public static IEnumerable<string> EnumerateFiles(string root, string pattern)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Queue<string> dirs = new();
        dirs.Enqueue(root);
        while (dirs.Count > 0)
        {
            var dir = dirs.Dequeue();
            string[] files;
            try { files = Directory.GetFiles(dir, pattern); }
            catch { continue; }
            foreach (var f in files)
                if (seen.Add(f)) yield return f;
            string[] subDirs;
            try { subDirs = Directory.GetDirectories(dir); }
            catch { continue; }
            foreach (var d in subDirs)
            {
                if (IsExcludedDirectory(Path.GetFileName(d)) || IsReparsePoint(d)) continue;
                dirs.Enqueue(d);
            }
        }
    }

    /// <summary>
    /// 按文件路径获取切块器
    /// </summary>
    public ICodeChunker GetChunker(string filePath)
    {
        var ext = Path.GetExtension(filePath);
        return ext.Length > 0 && _byExtension.TryGetValue(ext, out var c) ? c : _fallback;
    }

    /// <summary>
    /// 获取文件语言标识
    /// </summary>
    public string GetLanguage(string filePath) => GetChunker(filePath).Language;

    /// <summary>
    /// 判断文件是否应被索引：只做扩展名白名单 + 文件大小判断，**不读取文件内容**。
    /// 内容级二进制判断（NUL 字节）推迟到真正读取时，避免扫描阶段打开海量文件导致云盘水合与杀软扫描的长耗时。
    /// </summary>
    public bool ShouldIndex(string fullPath, string rootPath, long maxFileSizeBytes)
    {
        var rel = Path.GetRelativePath(rootPath, fullPath);
        foreach (var seg in rel.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            if (IsExcludedDirectory(seg)) return false;
        var name = Path.GetFileName(fullPath);
        if (name.EndsWith(".min.js", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".min.css", StringComparison.OrdinalIgnoreCase)) return false;
        var ext = Path.GetExtension(name);
        if (ext.Length == 0 || !_indexableExtensions.Contains(ext)) return false;
        try { return new FileInfo(fullPath).Length <= maxFileSizeBytes; }
        catch (IOException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
    }

    internal static bool LooksBinary(string path)
    {
        try
        {
            var buffer = new byte[8192];
            using var fs = File.OpenRead(path);
            int read = fs.Read(buffer, 0, buffer.Length);
            for (int i = 0; i < read; i++) if (buffer[i] == 0) return true;
            return false;
        }
        catch { return true; }
    }

    private static List<ICodeChunker> CreateDefaults() => new()
    {
        new BracePairingChunker("csharp", new[] { ".cs" }, @"\b(class|struct|interface|enum|record|namespace)\b"),
        new BracePairingChunker("javascript", new[] { ".js", ".jsx", ".mjs", ".cjs" }, @"\b(class|function)\b"),
        new BracePairingChunker("typescript", new[] { ".ts", ".tsx" }, @"\b(class|interface|enum|function|namespace)\b"),
        new BracePairingChunker("go", new[] { ".go" }, @"\b(func|type)\b"),
        new BracePairingChunker("java", new[] { ".java" }, @"\b(class|interface|enum|record)\b"),
        new BracePairingChunker("kotlin", new[] { ".kt", ".kts" }, @"\b(class|interface|object|fun)\b"),
        new BracePairingChunker("scala", new[] { ".scala" }, @"\b(class|trait|object)\b"),
        new BracePairingChunker("c", new[] { ".c", ".h" }, @"\b(struct|enum|typedef)\b"),
        new BracePairingChunker("cpp", new[] { ".cpp", ".cc", ".cxx", ".hpp" }, @"\b(class|struct|enum|namespace)\b"),
        new BracePairingChunker("rust", new[] { ".rs" }, @"\b(struct|enum|trait|impl|fn|mod)\b"),
        new BracePairingChunker("swift", new[] { ".swift" }, @"\b(class|struct|enum|protocol|extension|func)\b"),
        new BracePairingChunker("php", new[] { ".php" }, @"\b(class|interface|trait|function)\b"),
        new BracePairingChunker("dart", new[] { ".dart" }, @"\b(class|enum|mixin|extension)\b"),
        new BracePairingChunker("groovy", new[] { ".groovy" }, @"\b(class|interface|enum|trait)\b"),
        new BracePairingChunker("graphql", new[] { ".graphql", ".gql" }, @"\b(type|interface|enum|input|schema)\b"),
        new BracePairingChunker("protobuf", new[] { ".proto" }, @"\b(message|service|enum)\b"),
        new IndentChunker("python", new[] { ".py" }, @"^\s*(async\s+def|def|class)\s+"),
        new IndentChunker("yaml", new[] { ".yaml", ".yml" }, @"^[A-Za-z_][\w.\-]*\s*:"),
        new KeywordEndChunker("ruby", new[] { ".rb" }, @"^\s*(def|class|module|if|unless|case|begin|while|until|for)\b|\bdo\b", @"^\s*end\b"),
        new KeywordEndChunker("lua", new[] { ".lua" }, @"\b(function|if|for|while)\b|\bdo\b", @"^\s*end\b"),
        new KeywordEndChunker("vb", new[] { ".vb" }, @"^\s*(Public\s+|Private\s+|Protected\s+)?(Sub|Function|Class|Module|Property)\b", @"^\s*End\s+(Sub|Function|Class|Module|Property)\b"),
        new MarkupChunker("html", new[] { ".html", ".htm" }),
        new MarkupChunker("xml", new[] { ".xml", ".xaml", ".csproj", ".config", ".resx", ".svg", ".props", ".targets" }),
        new MarkupChunker("razor", new[] { ".razor", ".cshtml" }),
        new MarkupChunker("vue", new[] { ".vue" }),
        new HeaderChunker("markdown", new[] { ".md", ".markdown" }, @"^(#{1,6})\s+(.*?)\s*#*\s*$", 1, 2),
        // .xlsx 提取后为 “## <sheet 名> + markdown 表格”，按标题分节切块（一个 sheet 一节，sheet 名进 SymbolName）
        new HeaderChunker("excel", new[] { ".xlsx" }, @"^(#{1,6})\s+(.*?)\s*#*\s*$", 1, 2),
        new HeaderChunker("latex", new[] { ".tex" }, @"^\\(chapter|section|subsection|subsubsection)\{([^}]*)\}", 1, 2),
        new SectionChunker("ini", new[] { ".ini", ".cfg" }),
        new SectionChunker("toml", new[] { ".toml" }),
        new StatementChunker("sql", new[] { ".sql" }),
        new StatementChunker("prisma", new[] { ".prisma" }),
        new JsonChunker("json", new[] { ".json", ".jsonc", ".ipynb", ".jsonl", ".ndjson" }),
        new RuleBlockChunker("css", new[] { ".css", ".scss", ".less" }),
    };
}
