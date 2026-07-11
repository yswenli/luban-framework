namespace LuBan.AIAgent.Providers.AzureOpenAI;

/// <summary>
/// Azure OpenAI 配置选项，用于配置 Azure OpenAI 模型提供者的行为
/// </summary>
public class AzureOpenAIOptions
{
    /// <summary>
    /// Azure OpenAI API 密钥
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Azure OpenAI 端点 URL
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// Azure OpenAI API 版本，默认为2024-02-01
    /// </summary>
    public string ApiVersion { get; set; } = "2024-02-01";

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
