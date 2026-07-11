namespace LuBan.AIFlow.Models.Agent;

/// <summary>
/// Agent 列表响应模型
/// </summary>
public class ListAgentsResponse
{
    /// <summary>
    /// 响应代码，0 表示成功
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// Agent 列表数据
    /// </summary>
    [JsonPropertyName("data")]
    public List<AgentInfo> Data { get; set; } = new();
}