namespace LuBan.AIFlow.Models.Common;

/// <summary>
/// Token 使用情况统计模型
/// </summary>
public class TokenUsage
{
    /// <summary>
    /// 提示词数量
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    /// <summary>
    /// 完成词数量
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    /// <summary>
    /// 总词数
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }

    /// <summary>
    /// 完成词详情
    /// </summary>
    [JsonPropertyName("completion_tokens_details")]
    public CompletionTokensDetails? CompletionTokensDetails { get; set; }
}

/// <summary>
/// 完成词详情模型
/// </summary>
public class CompletionTokensDetails
{
    /// <summary>
    /// 接受的预测词数量
    /// </summary>
    [JsonPropertyName("accepted_prediction_tokens")]
    public int AcceptedPredictionTokens { get; set; }

    /// <summary>
    /// 推理词数量
    /// </summary>
    [JsonPropertyName("reasoning_tokens")]
    public int ReasoningTokens { get; set; }

    /// <summary>
    /// 拒绝的预测词数量
    /// </summary>
    [JsonPropertyName("rejected_prediction_tokens")]
    public int RejectedPredictionTokens { get; set; }
}