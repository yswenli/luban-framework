/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Utils.Text
*文件名： TextUtils
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：文本工具类
*
*****************************************************************************/
namespace LuBan.AIAgent.Utils.Text;

/// <summary>
/// 文本工具类
/// </summary>
public static class TextUtils
{
    /// <summary>
    /// 计算规范化文本的 SHA-256 内容哈希
    /// </summary>
    /// <param name="content">原始文本内容</param>
    /// <returns>内容的 SHA-256 十六进制哈希</returns>
    public static string ComputeContentHash(string content)
    {
        var normalized = NGramExtractor.Normalize(content);
        return Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(normalized)));
    }
}
