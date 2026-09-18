using Microsoft.Extensions.AI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System.Text;

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

    private const int MaxWidth = 2000;
    private const int MaxHeight = 2000;
    private const long MaxImageBytes = 5 * 1024 * 1024;   // 5MB
    private const long LargeTextThreshold = 50 * 1024;    // 50KB
    private const long MaxTextFileBytes = 10 * 1024 * 1024; // 10MB

    /// <inheritdoc />
    public async Task<ProcessedAttachment> ProcessAsync(string filePath, bool generateThumbnail = false, CancellationToken ct = default)
    {
        if (!IsSupported(filePath))
            throw new InvalidOperationException($"不支持的文件类型: {Path.GetExtension(filePath)}");
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"文件不存在: {filePath}", filePath);

        var ext = Path.GetExtension(filePath);
        var info = new FileInfo(filePath);
        var mediaType = ResolveMediaType(filePath);

        // SVG 是 XML 文本，按文本处理
        var isText = !ImageExtensions.Contains(ext) || ext.Equals(".svg", StringComparison.OrdinalIgnoreCase);
        if (isText)
            return await ProcessTextAsync(filePath, info, mediaType, ct).ConfigureAwait(false);

        if (HeicLikeExtensions.Contains(ext))
            throw new NotSupportedException("当前环境无法解码 HEIC，请先转换为 PNG/JPEG");

        return await ProcessImageAsync(filePath, info, ext, mediaType, generateThumbnail, ct).ConfigureAwait(false);
    }

    private async Task<ProcessedAttachment> ProcessImageAsync(
        string filePath, FileInfo info, string ext, string mediaType, bool generateThumbnail, CancellationToken ct)
    {
        using var source = await Image.LoadAsync(filePath, ct).ConfigureAwait(false);
        var width = source.Width;
        var height = source.Height;
        var pixelSize = $"{width}x{height}";

        byte[] bytes;
        if (width > MaxWidth || height > MaxHeight)
        {
            var ratio = Math.Min((double)MaxWidth / width, (double)MaxHeight / height);
            var targetW = Math.Max(1, (int)(width * ratio));
            var targetH = Math.Max(1, (int)(height * ratio));
            source.Mutate(x => x.Resize(targetW, targetH, KnownResamplers.Lanczos3));
            bytes = await EncodeUnderLimitAsync(source, ct).ConfigureAwait(false);
        }
        else
        {
            bytes = await File.ReadAllBytesAsync(filePath, ct).ConfigureAwait(false);
            if (bytes.LongLength > MaxImageBytes)
                bytes = await EncodeUnderLimitAsync(source, ct).ConfigureAwait(false);
        }

        var outMedia = bytes.Length > 0 && IsPng(bytes) ? "image/png" : "image/jpeg";
        string? thumb = generateThumbnail ? await SaveThumbnailAsync(source, ct).ConfigureAwait(false) : null;

        return new ProcessedAttachment
        {
            Info = new AttachmentInfo(info.Name, outMedia, info.Length, AttachmentKind.Image, Path.GetFullPath(filePath)),
            Content = new DataContent(bytes, outMedia),
            PixelSize = pixelSize,
            ThumbnailPath = thumb
        };
    }

    private static async Task<byte[]> EncodeUnderLimitAsync(Image image, CancellationToken ct)
    {
        // 先试 PNG，再按质量档降级 JPEG
        var png = await EncodeAsync(image, new PngEncoder(), ct).ConfigureAwait(false);
        if (png.LongLength <= MaxImageBytes)
            return png;

        foreach (var quality in new[] { 85, 70, 55, 40 })
        {
            var jpeg = await EncodeAsync(image, new JpegEncoder { Quality = quality }, ct).ConfigureAwait(false);
            if (jpeg.LongLength <= MaxImageBytes)
                return jpeg;
        }

        // 仍超限：等比缩小再试
        var work = image.Clone(x => x.Resize((int)(image.Width * 0.75), (int)(image.Height * 0.75), KnownResamplers.Lanczos3));
        try
        {
            return await EncodeUnderLimitAsync(work, ct).ConfigureAwait(false);
        }
        finally { work.Dispose(); }
    }

    private static async Task<byte[]> EncodeAsync(Image image, SixLabors.ImageSharp.Formats.IImageEncoder encoder, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await image.SaveAsync(ms, encoder, ct).ConfigureAwait(false);
        return ms.ToArray();
    }

    private static bool IsPng(byte[] bytes)
        => bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47;

    private static async Task<string> SaveThumbnailAsync(Image image, CancellationToken ct)
    {
        var dir = LuBan.Common.IO.TempDirectory.GetTempDir();
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".jpg");
        using var thumb = image.Clone(x => x.Resize(new ResizeOptions
        {
            Size = new Size(64, 64),
            Mode = ResizeMode.Max
        }));
        await thumb.SaveAsync(path, new JpegEncoder { Quality = 80 }, ct).ConfigureAwait(false);
        return path;
    }

    private Task<ProcessedAttachment> ProcessTextAsync(
        string filePath, FileInfo info, string mediaType, CancellationToken ct)
        => throw new NotImplementedException();
}