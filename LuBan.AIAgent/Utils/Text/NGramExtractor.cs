namespace LuBan.AIAgent.Utils.Text;

/// <summary>
/// 字符 n-gram 提取与确定性哈希工具。
/// 注意：不能用 string.GetHashCode()——.NET 默认对字符串启用哈希随机化，
/// 跨进程同一字符串会得到不同哈希，导致向量跨重启不一致。
/// </summary>
public static class NGramExtractor
{
    /// <summary>FNV-1a 32 位确定性哈希。</summary>
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

    private static bool IsIndexable(char c)
    {
        if (c >= 'a' && c <= 'z' || c >= '0' && c <= '9') return true;
        if (c >= 'A' && c <= 'Z') return true;
        if (c >= 0x4E00 && c <= 0x9FFF) return true;
        return false;
    }

    /// <summary>提取字符 bigram + trigram。</summary>
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
    public static IEnumerable<uint> ExtractHashes(string text)
        => Extract(text).Select(Fnv1a32);
}
