/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Lives.TalkMed
*文件名： SignatureHelper.cs
*版本号： V1.0.0.0
*唯一标识：c7e9eaba-fc65-4203-8376-d8e5f577a658
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：SignatureHelper 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：SignatureHelper 类
*
*****************************************************************************/

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
