namespace LuBan.AIAgent.Utils.Text;

public static class WildcardMatcher
{
    public static bool Match(string pattern, string value)
    {
        if (string.IsNullOrEmpty(pattern) || pattern == "*")
            return true;
        if (string.IsNullOrEmpty(value))
            return false;

        pattern = pattern.ToLowerInvariant();
        value = value.ToLowerInvariant();

        var parts = pattern.Split('*');
        if (parts.Length == 1)
            return value == pattern;

        var pos = 0;
        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part.Length == 0) continue;

            var idx = value.IndexOf(part, pos, StringComparison.Ordinal);
            if (idx < 0) return false;
            if (i == 0 && idx != 0) return false;
            pos = idx + part.Length;
        }

        var lastPart = parts[^1];
        if (lastPart.Length > 0 && !value.EndsWith(lastPart, StringComparison.Ordinal))
            return false;

        return true;
    }
}
