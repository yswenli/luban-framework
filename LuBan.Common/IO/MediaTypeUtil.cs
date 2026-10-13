/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common.IO
*文件名： MediaTypeUtil
*版本号： V1.0.0.0
*唯一标识：96e636ff-3373-42cf-8bce-eddae8281c46
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/13
*描述：
*
*=================================================
*修改标记
*修改时间：2026/10/13
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Common.IO;

/// <summary>
/// 附件/图片媒体类型单一来源：扩展名→MIME 映射、是否图片判定、HEIC 类判定。
/// </summary>
public static class MediaTypeUtil
{
    /// <summary>
    /// 扩展名→媒体类型（含点、小写、大小写不敏感）。
    /// 同时充当"受支持附件扩展名集合"，**不含 `.pdf`**（PDF 非受支持附件类型）。
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> ExtensionToMediaType =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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

    /// <summary>图片类扩展名（`.svg` 不在此列，仍按文本处理）。</summary>
    public static readonly IReadOnlySet<string> ImageExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".webp", ".gif", ".bmp", ".tiff", ".tif", ".heic", ".heif"
        };

    /// <summary>无法解码、需提示转换的扩展名。</summary>
    public static readonly IReadOnlySet<string> HeicLikeExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".heic", ".heif" };

    /// <summary>按扩展名判断是否属于图片集。</summary>
    public static bool IsImage(string pathOrFileName)
        => ImageExtensions.Contains(Path.GetExtension(pathOrFileName));

    /// <summary>按扩展名判断是否属于 HEIC 类。</summary>
    public static bool IsHeicLike(string pathOrFileName)
        => HeicLikeExtensions.Contains(Path.GetExtension(pathOrFileName));

    /// <summary>解析扩展名对应的媒体类型；未命中返回 <paramref name="defaultMediaType"/>。</summary>
    public static string ResolveMediaType(string pathOrFileName, string defaultMediaType = "text/plain")
        => pathOrFileName is null
            ? defaultMediaType
            : ExtensionToMediaType.TryGetValue(Path.GetExtension(pathOrFileName), out var mt) ? mt : defaultMediaType;
}
