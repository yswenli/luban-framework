namespace LuBan.AIAgent.Configuration;

/// <summary>
/// Provider 配置项
/// </summary>
public class ProviderConfig
{
    /// <summary>
    /// Provider 名称（如 openai, azure, deepseek）
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// API 密钥
    /// </summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    /// API 基础 URL（可选，用于自定义端点）
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Provider 显示名称
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 支持的模型列表（运行时填充，不保存到配置文件）
    /// </summary>
    public List<string> SupportedModels { get; set; } = new();
}