namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 模型端点配置
/// </summary>
public class ModelEndpointOptions
{
    /// <summary>
    /// API 基础 URL
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// API 密钥
    /// </summary>
    public string? ApiKey { get; set; }
}