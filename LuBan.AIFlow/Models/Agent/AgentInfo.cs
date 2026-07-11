namespace LuBan.AIFlow.Models.Agent;

/// <summary>
/// Agent 信息模型
/// </summary>
public class AgentInfo
{
    /// <summary>
    /// Agent ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Agent 标题
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Agent 描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Agent 的 Canvas DSL 对象
    /// </summary>
    [JsonPropertyName("dsl")]
    public object Dsl { get; set; } = new();

    /// <summary>
    /// 创建时间戳
    /// </summary>
    [JsonPropertyName("create_time")]
    public long CreateTime { get; set; }

    /// <summary>
    /// 更新时间戳
    /// </summary>
    [JsonPropertyName("update_time")]
    public long UpdateTime { get; set; }

    /// <summary>
    /// 创建日期
    /// </summary>
    [JsonPropertyName("create_date")]
    public string CreateDate { get; set; } = string.Empty;

    /// <summary>
    /// 更新日期
    /// </summary>
    [JsonPropertyName("update_date")]
    public string UpdateDate { get; set; } = string.Empty;

    /// <summary>
    /// 用户 ID
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Agent 头像
    /// </summary>
    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    /// <summary>
    /// Canvas 类型
    /// </summary>
    [JsonPropertyName("canvas_type")]
    public string? CanvasType { get; set; }
}