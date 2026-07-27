namespace LuBan.AIAgent.Retrieval.Chunkers;

/// <summary>
/// 切块器工厂：按扩展名路由 + 文件排除
/// </summary>
public class ChunkerFactory
{
    private static readonly string[] ExcludedDirs = { ".git", "bin", "obj", "node_modules", "dist", "packages", ".vs", ".idea", "target" };
    private static readonly HashSet<string> BinaryExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".dll", ".exe", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".zip", ".7z", ".rar", ".gz", ".tar",
        ".pdf", ".woff", ".woff2", ".ttf", ".eot", ".mp3", ".mp4", ".mov", ".pdb", ".nupkg", ".snupkg",
        ".so", ".dylib", ".class", ".jar", ".war", ".bin", ".dat", ".db", ".sqlite"
    };

    private readonly Dictionary<string, ICodeChunker> _byExtension = new(StringComparer.OrdinalIgnoreCase);
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
    /// 判断文件是否应被索引
    /// </summary>
    public bool ShouldIndex(string fullPath, string rootPath, long maxFileSizeBytes)
    {
        var rel = Path.GetRelativePath(rootPath, fullPath);
        foreach (var seg in rel.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            if (ExcludedDirs.Any(d => string.Equals(d, seg, StringComparison.OrdinalIgnoreCase))) return false;
        var name = Path.GetFileName(fullPath);
        if (name.EndsWith(".min.js", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".min.css", StringComparison.OrdinalIgnoreCase)) return false;
        var ext = Path.GetExtension(name);
        if (BinaryExtensions.Contains(ext)) return false;
        if (new FileInfo(fullPath).Length > maxFileSizeBytes) return false;
        if (LooksBinary(fullPath)) return false;
        return true;
    }

    private static bool LooksBinary(string path)
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
        new HeaderChunker("latex", new[] { ".tex" }, @"^\\(chapter|section|subsection|subsubsection)\{([^}]*)\}", 1, 2),
        new SectionChunker("ini", new[] { ".ini", ".cfg" }),
        new SectionChunker("toml", new[] { ".toml" }),
        new StatementChunker("sql", new[] { ".sql" }),
        new StatementChunker("prisma", new[] { ".prisma" }),
        new JsonChunker("json", new[] { ".json", ".jsonc", ".ipynb" }),
        new RuleBlockChunker("css", new[] { ".css", ".scss", ".less" }),
    };
}
