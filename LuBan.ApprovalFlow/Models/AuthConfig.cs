namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 认证配置，用于HTTP请求的身份验证设置。
/// </summary>
public class AuthConfig
{
    /// <summary>
    /// 认证类型：none/basic/bearer/apikey/custom。
    /// </summary>
    public string Type { get; set; } = ConstAuthType.None;
    /// <summary>
    /// Basic认证用户名。
    /// </summary>
    public string? BasicUsername { get; set; }
    /// <summary>
    /// Basic认证密码。
    /// </summary>
    public string? BasicPassword { get; set; }
    /// <summary>
    /// Bearer Token令牌。
    /// </summary>
    public string? BearerToken { get; set; }
    /// <summary>
    /// API Key名称。
    /// </summary>
    public string? ApiKeyName { get; set; }
    /// <summary>
    /// API Key值。
    /// </summary>
    public string? ApiKeyValue { get; set; }
    /// <summary>
    /// API Key位置：header/query/cookie，默认header。
    /// </summary>
    public string ApiKeyLocation { get; set; } = "header";
    /// <summary>
    /// 自定义请求头字典。
    /// </summary>
    public Dictionary<string, string>? CustomHeaders { get; set; }
}