namespace LuBan.AIAgent.Attachments;

/// <summary>
/// 默认附件处理器：图片走 ImageSharp 缩放，文本按大小内联或生成指引。
/// </summary>
public sealed class DefaultAttachmentProcessor : IAttachmentProcessor
{
    private static readonly Dictionary<string, string> ExtensionToMediaType = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".webp"] = "image/webp",
        [".gif"] = "image/gif",
        [".bmp"] = "image/bmp",
        [".tiff"] = "image/tiff",
        [".tif"] = "image/tiff",
        [".heic"] = "image/heic",
        [".heif"] = "image/heif",
        [".svg"] = "image/svg+xml",
        [".txt"] = "text/plain",
        [".md"] = "text/markdown",
        [".json"] = "application/json",
        [".xml"] = "application/xml",
        [".yaml"] = "text/yaml",
        [".yml"] = "text/yaml",
        [".toml"] = "text/plain",
        [".ini"] = "text/plain",
        [".cfg"] = "text/plain",
        [".conf"] = "text/plain",
        [".log"] = "text/plain",
        [".sql"] = "text/plain",
        [".cs"] = "text/plain",
        [".py"] = "text/plain",
        [".js"] = "text/plain",
        [".ts"] = "text/plain",
        [".java"] = "text/plain",
        [".go"] = "text/plain",
        [".rs"] = "text/plain",
        [".rb"] = "text/plain",
        [".php"] = "text/plain",
        [".swift"] = "text/plain",
        [".kt"] = "text/plain",
        [".c"] = "text/plain",
        [".cpp"] = "text/plain",
        [".h"] = "text/plain",
        [".hpp"] = "text/plain",
        [".css"] = "text/plain",
        [".html"] = "text/plain",
        [".htm"] = "text/plain",
        [".sh"] = "text/plain"
    };

    /// <summary>图片类扩展名（.svg/.heic/.heif 亦按图片 MIME 归类，但 .svg 走文本处理）。</summary>
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".webp", ".gif", ".bmp", ".tiff", ".tif", ".heic", ".heif"
    };

    /// <summary>无法用 ImageSharp 解码、需提示转换的扩展名。</summary>
    private static readonly HashSet<string> HeicLikeExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".heic", ".heif"
    };

    /// <inheritdoc />
    public IReadOnlyList<string> SupportedExtensions { get; } = ExtensionToMediaType.Keys.ToList();

    /// <inheritdoc />
    public bool IsSupported(string filePath)
        => !string.IsNullOrWhiteSpace(filePath)
           && ExtensionToMediaType.ContainsKey(Path.GetExtension(filePath));

    /// <inheritdoc />
    public string ResolveMediaType(string filePath)
    {
        var ext = Path.GetExtension(filePath);
        return ExtensionToMediaType.TryGetValue(ext, out var mt) ? mt : "text/plain";
    }

    /// <inheritdoc />
    public Task<ProcessedAttachment> ProcessAsync(string filePath, bool generateThumbnail = false, CancellationToken ct = default)
        => throw new NotImplementedException();
}