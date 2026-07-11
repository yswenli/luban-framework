namespace LuBan.AIFlow.Models.Chat;

/// <summary>
/// 聊天助手响应
/// </summary>
public class ChatAssistantResponse
{
    /// <summary>
    /// 响应代码
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 响应数据
    /// </summary>
    [JsonPropertyName("data")]
    public ChatAssistantResponseData Data { get; set; }
}

/// <summary>
/// 聊天助手响应数据
/// </summary>
public class ChatAssistantResponseData
{
    /// <summary>
    /// 回答内容
    /// </summary>
    [JsonPropertyName("answer")]
    public string Answer { get; set; }

    /// <summary>
    /// 引用信息
    /// </summary>
    [JsonPropertyName("reference")]
    public ReferenceInfo Reference { get; set; }

    /// <summary>
    /// 音频二进制数据
    /// </summary>
    [JsonPropertyName("audio_binary")]
    public string AudioBinary { get; set; }

    /// <summary>
    /// 响应ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; }

    /// <summary>
    /// 提示词
    /// </summary>
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; }
}

/// <summary>
/// 引用信息
/// </summary>
public class ReferenceInfo
{
    /// <summary>
    /// 总数
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>
    /// 文本块列表
    /// </summary>
    [JsonPropertyName("chunks")]
    public List<ReferenceChunkInfo> Chunks { get; set; }

    /// <summary>
    /// 文档聚合信息
    /// </summary>
    [JsonPropertyName("doc_aggs")]
    public List<DocAggInfo> DocAggs { get; set; }
}

/// <summary>
/// 引用文本块信息
/// </summary>
public class ReferenceChunkInfo
{
    /// <summary>
    /// 文本块ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; }

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
    /// 数据集ID
    /// </summary>
    [JsonPropertyName("dataset_id")]
    public string DatasetId { get; set; }

    /// <summary>
    /// 图片ID
    /// </summary>
    [JsonPropertyName("image_id")]
    public string ImageId { get; set; }

    /// <summary>
    /// 相似度
    /// </summary>
    [JsonPropertyName("similarity")]
    public double Similarity { get; set; }

    /// <summary>
    /// 向量相似度
    /// </summary>
    [JsonPropertyName("vector_similarity")]
    public double VectorSimilarity { get; set; }

    /// <summary>
    /// 词项相似度
    /// </summary>
    [JsonPropertyName("term_similarity")]
    public double TermSimilarity { get; set; }

    /// <summary>
    /// 位置信息
    /// </summary>
    [JsonPropertyName("positions")]
    public List<string> Positions { get; set; }
}

/// <summary>
/// 文档聚合信息
/// </summary>
public class DocAggInfo
{
    /// <summary>
    /// 文档名称
    /// </summary>
    [JsonPropertyName("doc_name")]
    public string DocName { get; set; }

    /// <summary>
    /// 文档ID
    /// </summary>
    [JsonPropertyName("doc_id")]
    public string DocId { get; set; }

    /// <summary>
    /// 计数
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }
}