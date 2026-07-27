namespace LuBan.AIAgent.Configuration;

/// <summary>
/// Agent 应用配置
/// </summary>
public class AppConfig
{
    /// <summary>
    /// Provider 配置列表
    /// </summary>
    public List<ProviderConfig> Providers { get; set; } = new();

    /// <summary>
    /// 当前选择的模型（格式: provider:model）
    /// </summary>
    public string? SelectedModel { get; set; }
}