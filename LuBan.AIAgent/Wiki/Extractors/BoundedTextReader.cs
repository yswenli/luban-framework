/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki.Extractors
*文件名： BoundedTextReader
*版本号： V1.0.0.0
*唯一标识：e2f0c3d2-1b2c-4e5f-9a01-2b3c4d5e6f76
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/10
*描述：有界流式文本读取（按字符上限截断，恒定内存）
*
*****************************************************************************/
namespace LuBan.AIAgent.Wiki.Extractors;

/// <summary>有界流式读取器：按字符上限读取文本文件，避免一次性把大文件读入内存。</summary>
internal static class BoundedTextReader
{
    /// <summary>读取至多 <paramref name="maxChars"/> 个字符（UTF-8，自动识别 BOM）。</summary>
    /// <param name="filePath">文件路径。</param>
    /// <param name="maxChars">字符上限。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>文本与是否因超限被截断。</returns>
    public static async Task<(string Text, bool Truncated)> ReadAsync(
        string filePath, int maxChars, CancellationToken cancellationToken = default)
    {
        if (maxChars <= 0) throw new ArgumentOutOfRangeException(nameof(maxChars));

        using var stream = new FileStream(
            filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
            bufferSize: 64 * 1024, useAsync: true);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        var sb = new StringBuilder(Math.Min(maxChars, 64 * 1024));
        var buffer = new char[Math.Min(16 * 1024, maxChars)];
        while (sb.Length < maxChars)
        {
            var want = Math.Min(buffer.Length, maxChars - sb.Length);
            var read = await reader.ReadAsync(buffer.AsMemory(0, want), cancellationToken).ConfigureAwait(false);
            if (read == 0) break;
            sb.Append(buffer, 0, read);
        }

        // 恰好读满上限时，再探测一字符判断是否还有剩余内容
        var truncated = false;
        if (sb.Length == maxChars)
        {
            var extra = await reader.ReadAsync(buffer.AsMemory(0, 1), cancellationToken).ConfigureAwait(false);
            truncated = extra > 0;
        }

        return (sb.ToString(), truncated);
    }
}
