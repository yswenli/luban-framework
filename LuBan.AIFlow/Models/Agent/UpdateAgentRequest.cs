namespace LuBan.AIFlow.Models.Agent;

/// <summary>
/// Agent 更新请求模型
/// </summary>
public class UpdateAgentRequest
{
    /// <summary>
    /// Agent 标题
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Agent 描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Agent 的 Canvas DSL 对象
    /// </summary>
    [JsonPropertyName("dsl")]
    public object? Dsl { get; set; }
}