using System.Text.Json;
using System.Text.Json.Serialization;

namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 模型信息，包含模型的元数据
/// </summary>
public record ModelInfo
{
    /// <summary>
    /// 模型 ID
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// 模型名称
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// 模型所有者
    /// </summary>
    public string? OwnedBy { get; init; }

    /// <summary>
    /// 对象类型（如 "model"）
    /// </summary>
    public string? Object { get; init; }

    /// <summary>
    /// 创建时间戳（Unix 时间戳）
    /// </summary>
    public long? Created { get; init; }

    /// <summary>
    /// 模型版本
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// 模型状态（如 Active、Retiring、Shutdown）
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// 模型领域（如 LLM、VLM、Embedding、ImageGeneration、VideoGeneration、3DGeneration、Router）
    /// </summary>
    public string? Domain { get; init; }

    /// <summary>
    /// 任务类型列表（如 TextGeneration、VisualQuestionAnswering、TextEmbedding）
    /// </summary>
    public IReadOnlyList<string>? TaskType { get; init; }

    /// <summary>
    /// 模型特性
    /// </summary>
    public ModelFeatures? Features { get; init; }

    /// <summary>
    /// Token 限制信息
    /// </summary>
    public TokenLimits? TokenLimits { get; init; }

    /// <summary>
    /// 模态信息（输入/输出模态）
    /// </summary>
    public ModelModalities? Modalities { get; init; }
}

/// <summary>
/// 模型特性
/// </summary>
public record ModelFeatures
{
    /// <summary>
    /// 结构化输出支持
    /// </summary>
    [JsonPropertyName("structured_outputs")]
    public StructuredOutputSupport? StructuredOutputs { get; init; }

    /// <summary>
    /// 工具调用支持
    /// </summary>
    public ToolSupport? Tools { get; init; }

    /// <summary>
    /// 缓存支持
    /// </summary>
    public CacheSupport? Cache { get; init; }

    /// <summary>
    /// 批处理支持
    /// </summary>
    public BatchSupport? Batch { get; init; }
}

/// <summary>
/// 结构化输出支持
/// </summary>
public record StructuredOutputSupport
{
    /// <summary>
    /// 是否支持 JSON 对象
    /// </summary>
    [JsonPropertyName("json_object")]
    public bool? JsonObject { get; init; }

    /// <summary>
    /// 是否支持 JSON Schema
    /// </summary>
    [JsonPropertyName("json_schema")]
    public bool? JsonSchema { get; init; }
}

/// <summary>
/// 工具调用支持
/// </summary>
public record ToolSupport
{
    /// <summary>
    /// 是否支持函数调用
    /// </summary>
    [JsonPropertyName("function_calling")]
    public bool? FunctionCalling { get; init; }
}

/// <summary>
/// 缓存支持
/// </summary>
public record CacheSupport
{
    /// <summary>
    /// 是否支持前缀缓存
    /// </summary>
    [JsonPropertyName("prefix_cache")]
    public bool? PrefixCache { get; init; }

    /// <summary>
    /// 是否支持会话缓存
    /// </summary>
    [JsonPropertyName("session_cache")]
    public bool? SessionCache { get; init; }
}

/// <summary>
/// 批处理支持
/// </summary>
public record BatchSupport
{
    /// <summary>
    /// 是否支持批处理聊天
    /// </summary>
    [JsonPropertyName("batch_chat")]
    public bool? BatchChat { get; init; }

    /// <summary>
    /// 是否支持批处理作业
    /// </summary>
    [JsonPropertyName("batch_job")]
    public bool? BatchJob { get; init; }
}

/// <summary>
/// Token 限制信息
/// </summary>
public record TokenLimits
{
    /// <summary>
    /// 上下文窗口大小
    /// </summary>
    [JsonPropertyName("context_window")]
    public int? ContextWindow { get; init; }

    /// <summary>
    /// 最大输入 Token 长度
    /// </summary>
    [JsonPropertyName("max_input_token_length")]
    public int? MaxInputTokenLength { get; init; }

    /// <summary>
    /// 最大输出 Token 长度
    /// </summary>
    [JsonPropertyName("max_output_token_length")]
    public int? MaxOutputTokenLength { get; init; }

    /// <summary>
    /// 最大推理 Token 长度
    /// </summary>
    [JsonPropertyName("max_reasoning_token_length")]
    public int? MaxReasoningTokenLength { get; init; }
}

/// <summary>
/// 模型模态信息
/// </summary>
public record ModelModalities
{
    /// <summary>
    /// 输入模态列表（如 text、image、video、audio）
    /// </summary>
    [JsonPropertyName("input_modalities")]
    public IReadOnlyList<string>? InputModalities { get; init; }

    /// <summary>
    /// 输出模态列表（如 text、image、video、audio）
    /// </summary>
    [JsonPropertyName("output_modalities")]
    public IReadOnlyList<string>? OutputModalities { get; init; }
}
