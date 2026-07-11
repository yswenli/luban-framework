namespace LuBan.AIAgent.Providers.Doubao;

/// <summary>
/// Doubao 配置选项，用于配置 Doubao 模型提供者的行为
/// </summary>
public class DoubaoOptions
{
    /// <summary>
    /// Doubao API 密钥
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Doubao API 基础 URL
    /// </summary>
    public string? BaseUrl { get; set; }
    
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