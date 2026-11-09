/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__ _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.FileSystem
*文件名： TextFileCodec
*版本号： V1.0.0.0
*唯一标识：c8f4a0e6-6e5b-4c0f-9a57-1a2b3c4d5e60
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/11/6
*描述：文本文件编码探测与按原编码回写
*
*****************************************************************************/

namespace LuBan.AIAgent.Tools.FileSystem;

/// <summary>
/// 文本文件编码探测与按原编码回写。
/// <para>
/// 探测顺序：BOM（UTF-8/UTF-16LE/UTF-16BE/UTF-32LE/UTF-32BE）→ 严格 UTF-8 → 本地 ANSI 代码页。
/// 中文环境下 UTF-8 解码失败的文件按 GB18030（GBK 超集）处理，避免 GBK 源码被按 UTF-8 读出乱码、
/// 回写时又被整体转成 UTF-8 而损坏原文件。
/// </para>
/// <para>写入一律按探测到的编码与 BOM 原样回写；新建文件使用 UTF-8 无 BOM。</para>
/// </summary>
internal static class TextFileCodec
{
    /// <summary>编码嗅探读取的字节数（只需覆盖 BOM 与首个多字节字符）。</summary>
    private const int SniffSize = 4096;

    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    static TextFileCodec()
    {
        // .NET Core 默认只带 UTF-8/UTF-16/UTF-32；GBK 等代码页需显式注册。
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        catch
        {
            // 平台不支持代码页时保持默认编码集合，UTF-8 仍可用。
        }
    }

    /// <summary>探测到的文本格式：编码 + 应写回的 BOM 前缀。</summary>
    /// <param name="Encoding">用于解码正文的编码（不含 BOM）。</param>
    /// <param name="Preamble">BOM 字节；无 BOM 时为空数组。</param>
    internal sealed record TextFileFormat(Encoding Encoding, byte[] Preamble)
    {
        /// <summary>BOM 字节长度（即解码时应跳过的前缀长度）。</summary>
        public int PreambleLength => Preamble.Length;
    }

    /// <summary>新建文件使用的格式：UTF-8 无 BOM。</summary>
    internal static TextFileFormat Utf8WithoutBom { get; } = new(new UTF8Encoding(false), []);

    /// <summary>
    /// 根据文件头字节探测文本格式。
    /// </summary>
    /// <param name="head">文件头字节（建议 4KB 以上）。</param>
    /// <returns>探测结果；无法判定时返回 UTF-8。</returns>
    internal static TextFileFormat Detect(ReadOnlySpan<byte> head)
    {
        if (head.Length >= 4 && head[0] == 0xFF && head[1] == 0xFE && head[2] == 0x00 && head[3] == 0x00)
            return new TextFileFormat(new UTF32Encoding(false, false), [0xFF, 0xFE, 0x00, 0x00]);
        if (head.Length >= 4 && head[0] == 0x00 && head[1] == 0x00 && head[2] == 0xFE && head[3] == 0xFF)
            return new TextFileFormat(new UTF32Encoding(true, false), [0x00, 0x00, 0xFE, 0xFF]);
        if (head.Length >= 3 && head[0] == 0xEF && head[1] == 0xBB && head[2] == 0xBF)
            return new TextFileFormat(new UTF8Encoding(false), [0xEF, 0xBB, 0xBF]);
        if (head.Length >= 2 && head[0] == 0xFF && head[1] == 0xFE)
            return new TextFileFormat(new UnicodeEncoding(false, false), [0xFF, 0xFE]);
        if (head.Length >= 2 && head[0] == 0xFE && head[1] == 0xFF)
            return new TextFileFormat(new UnicodeEncoding(true, false), [0xFE, 0xFF]);

        return IsValidUtf8(head)
            ? Utf8WithoutBom
            : new TextFileFormat(ResolveAnsiEncoding(), []);
    }

    /// <summary>
    /// 读取文件头并探测文本格式。
    /// </summary>
    /// <param name="path">文件路径。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>探测结果。</returns>
    internal static async Task<TextFileFormat> DetectAsync(string path, CancellationToken cancellationToken = default)
    {
        var head = await ReadHeadAsync(path, cancellationToken).ConfigureAwait(false);
        return Detect(head);
    }

    /// <summary>
    /// 读取整个文本文件，按探测到的编码解码并去掉 BOM。
    /// </summary>
    /// <param name="path">文件路径。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>正文与格式（供回写复用）。</returns>
    internal static async Task<(string Text, TextFileFormat Format)> ReadAllTextAsync(
        string path, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var head = bytes.AsSpan(0, Math.Min(bytes.Length, SniffSize));
        var format = Detect(head);
        var text = bytes.Length <= format.PreambleLength
            ? string.Empty
            : format.Encoding.GetString(bytes, format.PreambleLength, bytes.Length - format.PreambleLength);
        return (text, format);
    }

    /// <summary>
    /// 按指定格式写回文本（BOM 原样保留）。
    /// </summary>
    /// <param name="path">文件路径。</param>
    /// <param name="text">要写入的正文。</param>
    /// <param name="format">目标格式。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    internal static async Task WriteAllTextAsync(
        string path, string text, TextFileFormat format, CancellationToken cancellationToken = default)
    {
        var body = format.Encoding.GetBytes(text);
        if (format.PreambleLength == 0)
        {
            await File.WriteAllBytesAsync(path, body, cancellationToken).ConfigureAwait(false);
            return;
        }

        var bytes = new byte[format.PreambleLength + body.Length];
        format.Preamble.CopyTo(bytes, 0);
        body.CopyTo(bytes, format.PreambleLength);
        await File.WriteAllBytesAsync(path, bytes, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 判断文件头是否呈二进制特征（不含 BOM 的文本文件不应出现 0x00 字节）。
    /// </summary>
    /// <param name="head">文件头字节。</param>
    /// <param name="format">已探测到的格式；UTF-16/UTF-32 天然含 0x00，沿用 BOM 判定为文本。</param>
    /// <returns>二进制返回 true。</returns>
    internal static bool LooksBinary(ReadOnlySpan<byte> head, TextFileFormat format)
        => format.PreambleLength == 0 && head.IndexOf((byte)0) >= 0;

    /// <summary>读取文件头字节（最多 <see cref="SniffSize"/> 字节）。</summary>
    internal static async Task<byte[]> ReadHeadAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var buffer = new byte[SniffSize];
        var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
        return read == buffer.Length ? buffer : buffer[..read];
    }

    /// <summary>
    /// 严格判断一段字节是否为合法 UTF-8；末尾最多 3 个字节的不完整序列不参与判定。
    /// </summary>
    private static bool IsValidUtf8(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0) return true;

        for (var trim = 0; trim <= 3 && trim < bytes.Length; trim++)
        {
            var candidate = bytes[..(bytes.Length - trim)];
            try
            {
                StrictUtf8.GetCharCount(candidate);
                return true;
            }
            catch (DecoderFallbackException)
            {
                // 继续尝试裁掉末尾不完整序列
            }
        }
        return false;
    }

    /// <summary>
    /// 选择 ANSI 回退编码：CJK 区域用本地代码页，其余环境用 GB18030（GBK 超集，
    /// 对中文文件的覆盖最好，且对纯 ASCII 无副作用）。
    /// </summary>
    private static Encoding ResolveAnsiEncoding()
    {
        try
        {
            var codePage = CultureInfo.CurrentCulture.TextInfo.ANSICodePage;
            var isCjk = codePage is 936 or 950 or 932 or 949;
            return Encoding.GetEncoding(isCjk ? codePage : 54936);
        }
        catch
        {
            try
            {
                return Encoding.GetEncoding(54936);
            }
            catch
            {
                return new UTF8Encoding(false);
            }
        }
    }
}