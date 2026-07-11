/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AI.Models.Chat
*文件名： ChatResponse
*版本号： V1.0.0.0
*唯一标识：02633465-b71c-4437-be55-686172a9e968
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/7/1 14:21:32
*描述：聊天响应模型
*
*=================================================
*修改标记
*修改时间：2025/7/1 14:21:32
*修改人： yswenli
*版本号： V1.0.0.0
*描述：聊天响应模型
*
*****************************************************************************/
namespace LuBan.AIFlow.Models.Chat;

/// <summary>
/// 聊天响应模型
/// </summary>
public class ChatResponse
{
    /// <summary>
    /// 事件类型
    /// </summary>
    [JsonPropertyName("event")]
    public string Event { get; set; }

    /// <summary>
    /// 任务ID
    /// </summary>
    [JsonPropertyName("task_id")]
    public string TaskId { get; set; }

    /// <summary>
    /// 响应ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// 消息ID
    /// </summary>
    [JsonPropertyName("message_id")]
    public string MessageId { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    [JsonPropertyName("conversation_id")]
    public string ConversationId { get; set; }

    /// <summary>
    /// 模式
    /// </summary>
    [JsonPropertyName("mode")]
    public string Mode { get; set; }

    /// <summary>
    /// 回答内容
    /// </summary>
    [JsonPropertyName("answer")]
    public string Answer { get; set; }

    /// <summary>
    /// 元数据信息
    /// </summary>
    [JsonPropertyName("metadata")]
    public ChatMetadata Metadata { get; set; }

    /// <summary>
    /// 创建时间戳
    /// </summary>
    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }
}

/// <summary>
/// 聊天元数据信息
/// </summary>
public class ChatMetadata
{
    /// <summary>
    /// 使用量信息
    /// </summary>
    [JsonPropertyName("usage")]
    public UsageInfo Usage { get; set; }

    /// <summary>
    /// 检索资源列表
    /// </summary>
    [JsonPropertyName("retriever_resources")]
    public List<RetrieverResource> RetrieverResources { get; set; }
}

/// <summary>
/// 使用量信息
/// </summary>
public class UsageInfo
{
    /// <summary>
    /// 提示词token数量
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    /// <summary>
    /// 提示词单价
    /// </summary>
    [JsonPropertyName("prompt_unit_price")]
    public string PromptUnitPrice { get; set; }

    /// <summary>
    /// 提示词价格单位
    /// </summary>
    [JsonPropertyName("prompt_price_unit")]
    public string PromptPriceUnit { get; set; }

    /// <summary>
    /// 提示词总价
    /// </summary>
    [JsonPropertyName("prompt_price")]
    public string PromptPrice { get; set; }

    /// <summary>
    /// 补全token数量
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    /// <summary>
    /// 补全单价
    /// </summary>
    [JsonPropertyName("completion_unit_price")]
    public string CompletionUnitPrice { get; set; }

    /// <summary>
    /// 补全价格单位
    /// </summary>
    [JsonPropertyName("completion_price_unit")]
    public string CompletionPriceUnit { get; set; }

    /// <summary>
    /// 补全总价
    /// </summary>
    [JsonPropertyName("completion_price")]
    public string CompletionPrice { get; set; }

    /// <summary>
    /// 总token数量
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }

    /// <summary>
    /// 总价格
    /// </summary>
    [JsonPropertyName("total_price")]
    public string TotalPrice { get; set; }

    /// <summary>
    /// 货币类型
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    /// <summary>
    /// 延迟时间
    /// </summary>
    [JsonPropertyName("latency")]
    public double Latency { get; set; }
}

/// <summary>
/// 检索资源信息
/// </summary>
public class RetrieverResource
{
    /// <summary>
    /// 位置
    /// </summary>
    [JsonPropertyName("position")]
    public int Position { get; set; }

    /// <summary>
    /// 数据集ID
    /// </summary>
    [JsonPropertyName("dataset_id")]
    public string DatasetId { get; set; }

    /// <summary>
    /// 数据集名称
    /// </summary>
    [JsonPropertyName("dataset_name")]
    public string DatasetName { get; set; }

    /// <summary>
    /// 文档ID
    /// </summary>
    [JsonPropertyName("document_id")]
    public string DocumentId { get; set; }

    /// <summary>
    /// 文档名称
    /// </summary>
    [JsonPropertyName("document_name")]
    public string DocumentName { get; set; }

    /// <summary>
    /// 段落ID
    /// </summary>
    [JsonPropertyName("segment_id")]
    public string SegmentId { get; set; }

    /// <summary>
    /// 相关度评分
    /// </summary>
    [JsonPropertyName("score")]
    public double Score { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; }
}

/// <summary>
/// 枚举回答类型
/// </summary>
public enum EnumAnswerType
{
    /// <summary>
    /// 默认值
    /// </summary>
    [Description("默认值")]
    Default = 0,
    /// <summary>
    /// 验光单
    /// </summary>
    [Description("验光单")]
    IsEyePrescription = 1,
    /// <summary>
    /// IOLMaster
    /// </summary>
    [Description("IOLMaster")]
    IsIOLMaster = 2,
    /// <summary>
    /// 全部
    /// </summary>
    [Description("全部")]
    All = 3
}
