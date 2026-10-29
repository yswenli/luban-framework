/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Attachments.Ingestion
*文件名： AttachmentPathClassifier
*版本号： V1.0.0.0
*唯一标识：d3b8e6f2-5a19-4c70-8b3e-9f64c2a17d05
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：附件来源分类。
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：附件来源分类。
*
*****************************************************************************/

namespace LuBan.AIAgent.Attachments.Ingestion;

/// <summary>附件来源类别。</summary>
public enum AttachmentSourceKind
{
    /// <summary>http(s) URL。</summary>
    Http,

    /// <summary>file:// URI。</summary>
    FileUri,

    /// <summary>本机绝对路径。</summary>
    LocalAbsolute,

    /// <summary>相对路径（工作区 / 内容根）。</summary>
    Relative
}

/// <summary>
/// 来源分类。判定顺序固定：URL → file:// → 盘符/UNC → 非 Windows 的 '/' → 相对。
/// </summary>
/// <remarks>
/// Windows 上 <see cref="Path.IsPathRooted(string)"/> 对 <c>/x</c> 返回 true，
/// 不能直接拿来判绝对路径，否则 <c>/fonts/a.png</c> 会变成 <c>C:\fonts\a.png</c>。
/// </remarks>
public static class AttachmentPathClassifier
{
    /// <summary>分类。</summary>
    /// <param name="source">调用方给的来源。</param>
    /// <returns>来源类别。</returns>
    public static AttachmentSourceKind Classify(string source)
    {
        if (string.IsNullOrWhiteSpace(source)) return AttachmentSourceKind.Relative;
        var s = source.Trim();

        if (Uri.TryCreate(s, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return AttachmentSourceKind.Http;
        }

        // 盘符/UNC 必须先于 file:// 判定：Windows 上 Uri.TryCreate(@"D:\a.txt") 会把它解析成 file URI
        if (OperatingSystem.IsWindows())
        {
            // 盘符：D:\ 或 D:/
            if (s.Length >= 2 && char.IsAsciiLetter(s[0]) && s[1] == ':')
                return AttachmentSourceKind.LocalAbsolute;

            // UNC：\\server\share
            if (s.StartsWith(@"\\", StringComparison.Ordinal))
                return AttachmentSourceKind.LocalAbsolute;
        }

        if (uri is not null && uri.Scheme == Uri.UriSchemeFile)
            return AttachmentSourceKind.FileUri;

        if (!OperatingSystem.IsWindows() && s.StartsWith('/'))
            return AttachmentSourceKind.LocalAbsolute;

        return AttachmentSourceKind.Relative;
    }

    /// <summary>相对路径去掉前导分隔符（Windows 上 <c>/fonts/a.png</c> 要按站点相对处理）。</summary>
    /// <param name="source">调用方给的来源。</param>
    /// <returns>去掉前导分隔符的相对路径。</returns>
    public static string ToRelative(string source) => source.Trim().TrimStart('/', '\\');
}