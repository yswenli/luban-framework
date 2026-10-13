using LuBan.Common.IO;
using Microsoft.Extensions.AI;
using SkiaSharp;
using System.Text;

namespace LuBan.AIAgent.Attachments;

/// <summary>
/// 默认附件处理器：图片走 SkiaSharp 缩放，文本按大小内联或生成指引。
/// </summary>
public sealed class DefaultAttachmentProcessor : IAttachmentProcessor
{
    /// <inheritdoc />
    public IReadOnlyList<string> SupportedExtensions { get; } = MediaTypeUtil.ExtensionToMediaType.Keys.ToList();

    /// <inheritdoc />
    public bool IsSupported(string filePath)
        => !string.IsNullOrWhiteSpace(filePath)
           && MediaTypeUtil.ExtensionToMediaType.ContainsKey(Path.GetExtension(filePath));

    /// <inheritdoc />
    public string ResolveMediaType(string filePath) => MediaTypeUtil.ResolveMediaType(filePath);

    private const int MaxWidth = 2000;
    private const int MaxHeight = 2000;
    private const long MaxImageBytes = 5 * 1024 * 1024;   // 5MB
    private const long LargeTextThreshold = 50 * 1024;    // 50KB
    private const long MaxTextFileBytes = 10 * 1024 * 1024; // 10MB

    private readonly LogToolOptions _logOptions;
    private readonly ILogAggregator _aggregator;

    private static readonly LogToolOptions DefaultLogOptions = new();

    /// <summary>创建处理器（无参：使用默认日志配置，供仅需 MIME 解析等轻量场景）。</summary>
    public DefaultAttachmentProcessor()
        : this(Options.Create(DefaultLogOptions), new LogAggregator(Options.Create(DefaultLogOptions)))
    {
    }

    /// <summary>创建处理器。</summary>
    /// <param name="logOptions">大日志分析配置（绑定 <c>LuBanAgent:Tools:Log</c>）。</param>
    /// <param name="aggregator">日志聚合器。</param>
    public DefaultAttachmentProcessor(
        IOptions<LogToolOptions> logOptions,
        ILogAggregator aggregator)
    {
        _logOptions = logOptions?.Value ?? new LogToolOptions();
        _logOptions.ApplyHardCaps();
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
    }

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
        var isText = !MediaTypeUtil.IsImage(ext) || ext.Equals(".svg", StringComparison.OrdinalIgnoreCase);
        if (isText)
            return await ProcessTextAsync(filePath, info, mediaType, ct).ConfigureAwait(false);

        if (MediaTypeUtil.IsHeicLike(ext))
            throw new NotSupportedException("当前环境无法解码 HEIC，请先转换为 PNG/JPEG");

        return await ProcessImageAsync(filePath, info, ext, mediaType, generateThumbnail, ct).ConfigureAwait(false);
    }

    private async Task<ProcessedAttachment> ProcessImageAsync(
        string filePath, FileInfo info, string ext, string mediaType, bool generateThumbnail, CancellationToken ct)
    {
        var raw = await File.ReadAllBytesAsync(filePath, ct).ConfigureAwait(false);
        using var source = SKBitmap.Decode(raw);
        if (source == null)
            throw new NotSupportedException($"当前环境无法解码该图片格式（{ext}），请先转换为 PNG/JPEG");

        var width = source.Width;
        var height = source.Height;
        var pixelSize = $"{width}x{height}";

        byte[] bytes;
        string outMedia;
        if (width > MaxWidth || height > MaxHeight)
        {
            var (targetW, targetH) = ImageUtil.ScaleToFit(width, height, MaxWidth, MaxHeight);
            using var resized = ImageUtil.Resize(source, targetW, targetH);
            bytes = EncodeUnderLimit(resized);
            outMedia = IsPng(bytes) ? "image/png" : "image/jpeg";
        }
        else if (raw.LongLength > MaxImageBytes)
        {
            bytes = EncodeUnderLimit(source);
            outMedia = IsPng(bytes) ? "image/png" : "image/jpeg";
        }
        else
        {
            // 原样返回原始字节时，媒体类型必须与载荷真实格式一致（gif/webp/bmp 原格式），
            // 不能一律标 image/jpeg，否则模型侧按声明类型解码会失败。
            bytes = raw;
            outMedia = mediaType;
        }

        string? thumb = generateThumbnail ? ImageUtil.SaveThumbnail(source) : null;

        return new ProcessedAttachment
        {
            Info = new AttachmentInfo(info.Name, outMedia, info.Length, AttachmentKind.Image, Path.GetFullPath(filePath)),
            Content = new DataContent(bytes, outMedia),
            PixelSize = pixelSize,
            ThumbnailPath = thumb
        };
    }

    private static byte[] EncodeUnderLimit(SKBitmap image)
    {
        // 先试 PNG，再按质量档降级 JPEG
        var png = ImageUtil.Encode(image, SKEncodedImageFormat.Png, 100);
        if (png.LongLength <= MaxImageBytes)
            return png;

        foreach (var quality in new[] { 85, 70, 55, 40 })
        {
            var jpeg = ImageUtil.Encode(image, SKEncodedImageFormat.Jpeg, quality);
            if (jpeg.LongLength <= MaxImageBytes)
                return jpeg;
        }

        // 仍超限：等比缩小再试
        var workW = Math.Max(1, (int)(image.Width * 0.75));
        var workH = Math.Max(1, (int)(image.Height * 0.75));
        using var work = ImageUtil.Resize(image, workW, workH);
        return EncodeUnderLimit(work);
    }

    private static bool IsPng(byte[] bytes)
        => bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47;

    private async Task<ProcessedAttachment> ProcessTextAsync(
        string filePath, FileInfo info, string mediaType, CancellationToken ct)
    {
        var fullPath = Path.GetFullPath(filePath);

        // .log 分流必须先于 10MB 拒绝，否则大日志永远到不了聚合分支
        if (_logOptions.Enabled && _logOptions.IsAggregateExtension(filePath))
            return await ProcessLogAsync(filePath, info, mediaType, fullPath, ct).ConfigureAwait(false);

        if (info.Length > MaxTextFileBytes)
            throw new InvalidOperationException($"文本文件过大（{info.Length / 1024 / 1024}MB，上限 10MB），请使用 LLM Wiki 知识库");

        if (info.Length <= LargeTextThreshold)
        {
            var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8, ct).ConfigureAwait(false);
            return new ProcessedAttachment
            {
                Info = new AttachmentInfo(info.Name, mediaType, info.Length, AttachmentKind.TextFile, fullPath),
                Content = new TextContent(content)
            };
        }

        var sizeText = $"{info.Length / 1024.0:F1}KB";
        var guidance =
            $"用户附加了文件 {info.Name}（{sizeText}），内容过长未直接注入。" +
            $"请使用 filesystem 的 ReadFile 工具读取（单次返回前 2000 行/256KB），" +
            $"如需定位特定内容用 Grep 工具按关键字检索。文件路径: {fullPath}";
        return new ProcessedAttachment
        {
            Info = new AttachmentInfo(info.Name, mediaType, info.Length, AttachmentKind.TextFile, fullPath),
            Content = new TextContent(guidance),
            IsLargeText = true
        };
    }

    private async Task<ProcessedAttachment> ProcessLogAsync(
        string filePath, FileInfo info, string mediaType, string fullPath, CancellationToken ct)
    {
        // 超过单次聚合上限：仅指引
        if (info.Length > _logOptions.MaxFileSizeBytes)
        {
            var limitMb = _logOptions.MaxFileSizeBytes / 1024d / 1024d;
            var sizeMb = info.Length / 1024d / 1024d;
            return Guide(fullPath, info, mediaType,
                $"用户附加了日志文件 {info.Name}（{sizeMb:F1}MB），超过单次聚合上限（{limitMb:F0}MB），未自动生成概览。");
        }

        // ≤ 50KB：按普通文本内联
        if (info.Length <= LargeTextThreshold)
        {
            var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8, ct).ConfigureAwait(false);
            return new ProcessedAttachment
            {
                Info = new AttachmentInfo(info.Name, mediaType, info.Length, AttachmentKind.TextFile, fullPath),
                Content = new TextContent(content)
            };
        }

        // 50KB–上限：流式聚合出概览（恒定内存），失败/超时回退指引，不阻断发送
        try
        {
            var summary = await _aggregator.AggregateAsync(fullPath, ct).ConfigureAwait(false);
            var overview = LogSummaryFormatter.Format(summary, _logOptions.MaxResultChars);
            var text = overview + Environment.NewLine + Environment.NewLine + LogDrillGuide(fullPath);
            return new ProcessedAttachment
            {
                Info = new AttachmentInfo(info.Name, mediaType, info.Length, AttachmentKind.TextFile, fullPath),
                Content = new TextContent(text),
                IsLargeText = true
            };
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Guide(fullPath, info, mediaType,
                $"用户附加了日志文件 {info.Name} 自动聚合失败，已跳过概览。");
        }
    }

    private static ProcessedAttachment Guide(string fullPath, FileInfo info, string mediaType, string reason)
        => new()
        {
            Info = new AttachmentInfo(info.Name, mediaType, info.Length, AttachmentKind.TextFile, fullPath),
            Content = new TextContent(reason + LogDrillGuide(fullPath)),
            IsLargeText = true
        };

    private static string LogDrillGuide(string fullPath)
        => $"可用 log 工具钻取：LogStats（概览）、LogGrep（按关键字/级别/时间/行区间检索）、LogSlice（按行号切片）。文件路径: {fullPath}";
}