namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 浏览器工具配置
/// </summary>
public class BrowserToolOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 是否使用无头模式
    /// </summary>
    public bool Headless { get; set; } = true;

    /// <summary>
    /// 超时时间（毫秒）
    /// </summary>
    public int Timeout { get; set; } = 30000;

    /// <summary>
    /// 浏览器引擎：chromium, firefox, webkit
    /// </summary>
    public string Engine { get; set; } = "chromium";
}