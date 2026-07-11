namespace LuBan.AIAgent.Providers.ERNIE;

/// <summary>
/// ERNIE 配置选项，用于配置 ERNIE 模型提供者的行为
/// </summary>
public class ERNIEOptions
{
    /// <summary>
    /// ERNIE API 密钥
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// ERNIE API 端点 URL
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// 访问令牌获取端点 URL
    /// </summary>
    public string TokenEndpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// 请求超时时间，默认为30秒
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// 最大重试次数，默认为3次
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;
    
    /// <summary>
    /// 初始重试延迟，默认为500毫秒
    /// </summary>
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);
}