/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Models.Common
*文件名： TokenUsage.cs
*版本号： V1.0.0.0
*唯一标识：b8494c80-9b9c-4ff0-983e-93845f72a485
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：TokenUsage 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：TokenUsage 类
*
*****************************************************************************/

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