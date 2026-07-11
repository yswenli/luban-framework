namespace LuBan.Lives.TalkMed;

/// <summary>
/// 签名
/// </summary>
internal class SignatureHelper
{
    /// <summary>
    /// 获取sha256
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string GetHash256(string str)
    {
        return SHAUtil.GetSHA256(str);
    }

    /// <summary>
    /// 获取签名
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="appSecret"></param>
    /// <returns></returns>
    public static string GetOpenApiSignature(string appId, string appSecret, string timestamp)
    {
        return GetHash256($"{appId}-{appSecret}-{timestamp}");
    }

    /// <summary>
    /// 签名
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="appSecret"></param>
    /// <param name="autoToken"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static string GetSignatureForAuthorize(string appId, string appSecret, string autoToken, string timestamp)
    {
        return GetHash256($"{appId}-{appSecret}-{autoToken}-{timestamp}");
    }
}
