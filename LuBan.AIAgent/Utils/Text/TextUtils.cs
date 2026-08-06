namespace LuBan.AIAgent.Utils.Text;

public static class TextUtils
{
    public static string ComputeContentHash(string content)
    {
        var normalized = NGramExtractor.Normalize(content);
        return Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(normalized)));
    }
}
