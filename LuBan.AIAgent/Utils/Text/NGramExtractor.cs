/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Utils.Text
*文件名： NGramExtractor
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：字符 n-gram 提取与确定性哈希工具
*
*****************************************************************************/
namespace LuBan.AIAgent.Utils.Text;

/// <summary>
/// 字符 n-gram 提取与确定性哈希工具。
/// 注意：不能用 string.GetHashCode()——.NET 默认对字符串启用哈希随机化，
/// 跨进程同一字符串会得到不同哈希，导致向量跨重启不一致。
/// </summary>
public static class NGramExtractor
{
    /// <summary>FNV-1a 32 位确定性哈希。</summary>
    /// <param name="s">输入字符串</param>
    /// <returns>FNV-1a 32 位哈希值</returns>
    public static uint Fnv1a32(string s)
    {
        uint h = 2166136261u;
        foreach (var c in s)
        {
            h ^= c;
            h *= 16777619u;
        }
        return h;
    }

    /// <summary>规范化：转小写，仅保留 ASCII 字母数字与 CJK 字符，去除空白与标点。</summary>
    /// <param name="text">原始文本</param>
    /// <returns>规范化后的文本</returns>
    public static string Normalize(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        var sb = new System.Text.StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (IsIndexable(c))
                sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }

    /// <summary>
    /// 判断字符是否可作为索引内容（ASCII 字母数字或 CJK 字符）
    /// </summary>
    /// <param name="c">待判断的字符</param>
    /// <returns>可索引时返回 true，否则返回 false</returns>
    private static bool IsIndexable(char c)
    {
        if (c >= 'a' && c <= 'z' || c >= '0' && c <= '9') return true;
        if (c >= 'A' && c <= 'Z') return true;
        if (c >= 0x4E00 && c <= 0x9FFF) return true;
        return false;
    }

    /// <summary>提取字符 bigram + trigram。</summary>
    /// <param name="text">原始文本</param>
    /// <returns>字符 n-gram 序列</returns>
    public static IEnumerable<string> Extract(string text)
    {
        var normalized = Normalize(text);
        if (normalized.Length < 2)
        {
            if (normalized.Length == 1) yield return normalized;
            yield break;
        }
        for (var i = 0; i < normalized.Length; i++)
        {
            if (i + 1 < normalized.Length) yield return normalized.Substring(i, 2);
            if (i + 2 < normalized.Length) yield return normalized.Substring(i, 3);
        }
    }

    /// <summary>提取每个 n-gram 的 FNV-1a 哈希。</summary>
    /// <param name="text">原始文本</param>
    /// <returns>n-gram 的 FNV-1a 哈希序列</returns>
    public static IEnumerable<uint> ExtractHashes(string text)
        => Extract(text).Select(Fnv1a32);
}
