/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Infrastructure
*文件名： SecretMasker
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：敏感文本脱敏工具（URL 内嵌凭据 / 明文令牌），供工具确认弹窗与会话记录统一脱敏
*
*****************************************************************************/
using System.Text.RegularExpressions;

namespace LuBan.AIAgent.Infrastructure;

/// <summary>
/// 敏感文本脱敏工具。
/// </summary>
/// <remarks>
/// 背景（PRD §4.9.3 v0.15 补充）：Git 克隆/推送会把含 PAT 的完整 URL
/// （<c>https://oauth2:&lt;PAT&gt;@host/...</c>）放进工具确认弹窗的参数字典，
/// 并随 <c>ToolResult.commandLine</c> 写入会话消息记录，导致明文令牌泄漏。
/// 故凡对外暴露的命令行 / 参数 / 消息，统一经本类脱敏。
/// </remarks>
public static class SecretMasker
{
    /// <summary>
    /// URL 内嵌凭据：<c>scheme://user:pass@host</c> 或 <c>scheme://token@host</c>。
    /// </summary>
    private static readonly Regex UrlCredentialPattern = new(
        @"(?<scheme>[A-Za-z][A-Za-z0-9+.\-]*://)(?<creds>[^/@\s]+)@",
        RegexOptions.Compiled);

    /// <summary>
    /// 将 API Key 脱敏显示，只保留前 4 位与后 4 位；长度不足或为空时返回 <c>****</c>。
    /// </summary>
    /// <param name="apiKey">原始 API Key。</param>
    /// <returns>脱敏后的字符串。</returns>
    public static string MaskApiKey(string? apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length <= 8)
            return "****";
        return $"{apiKey[..4]}...{apiKey[^4..]}";
    }

    /// <summary>
    /// 脱敏文本中所有 URL 内嵌凭据（形如 <c>https://oauth2:&lt;PAT&gt;@host</c> → <c>https://***@host</c>）。
    /// </summary>
    /// <param name="text">原始文本，可为空。</param>
    /// <returns>脱敏后的文本；入参为空时原样返回。</returns>
    public static string MaskUrlCredentials(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        return UrlCredentialPattern.Replace(text, "${scheme}***@");
    }

    /// <summary>
    /// 在文本中替换指定明文令牌为 <c>****</c>（用于 MCP 工具 arguments 等已知令牌的场景）。
    /// </summary>
    /// <param name="text">原始文本，可为空。</param>
    /// <param name="secret">明文令牌；为空则仅做 URL 内嵌凭据脱敏。</param>
    /// <returns>脱敏后的文本。</returns>
    public static string MaskSecret(string? text, string? secret)
    {
        var masked = MaskUrlCredentials(text);
        if (!string.IsNullOrEmpty(secret) && !string.IsNullOrEmpty(masked))
            masked = masked.Replace(secret!, "****", StringComparison.Ordinal);

        return masked;
    }
}