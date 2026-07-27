namespace LuBan.AIAgent.Configuration;

/// <summary>
/// Web 工具配置
/// </summary>
public class WebToolOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 最大响应字符数
    /// </summary>
    public int MaxCharacters { get; set; } = 12000;
}