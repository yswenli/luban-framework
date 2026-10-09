/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.LogAnalysis
*文件名： LogSignatureNormalizer
*描述：把日志行归一化为稳定模板用于错误聚类
*****************************************************************************/
namespace LuBan.AIAgent.LogAnalysis;

/// <summary>
/// 把日志行归一化为稳定模板（用于错误聚类）。
/// 替换顺序重要：GUID 先于引号/十六进制，IP/十六进制先于数字，最后统一空白。
/// </summary>
internal static partial class LogSignatureNormalizer
{
    private const int DefaultMaxChars = 500;

    /// <summary>归一化单行。</summary>
    /// <param name="line">原始行。</param>
    /// <param name="maxChars">参与归一化的最大字符数（超出截断）。</param>
    public static string Normalize(string line, int maxChars = DefaultMaxChars)
    {
        if (string.IsNullOrEmpty(line)) return string.Empty;
        if (maxChars <= 0) maxChars = DefaultMaxChars;

        var s = line.Length > maxChars ? line[..maxChars] : line;

        s = GuidRegex().Replace(s, "<GUID>");
        s = IpRegex().Replace(s, "<IP>");
        s = HexRegex().Replace(s, "<HEX>");
        s = DoubleQuoteRegex().Replace(s, "<S>");
        s = SingleQuoteRegex().Replace(s, "<S>");
        s = NumberRegex().Replace(s, "<N>");
        s = WhitespaceRegex().Replace(s, " ").Trim();
        return s;
    }

    [GeneratedRegex("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", RegexOptions.CultureInvariant)]
    private static partial Regex GuidRegex();

    [GeneratedRegex("\\b\\d{1,3}(?:\\.\\d{1,3}){3}\\b", RegexOptions.CultureInvariant)]
    private static partial Regex IpRegex();

    [GeneratedRegex("\\b0x[0-9a-fA-F]+\\b", RegexOptions.CultureInvariant)]
    private static partial Regex HexRegex();

    [GeneratedRegex("\"[^\"]*\"", RegexOptions.CultureInvariant)]
    private static partial Regex DoubleQuoteRegex();

    [GeneratedRegex("'[^']*'", RegexOptions.CultureInvariant)]
    private static partial Regex SingleQuoteRegex();

    [GeneratedRegex("\\d+", RegexOptions.CultureInvariant)]
    private static partial Regex NumberRegex();

    [GeneratedRegex("\\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();
}