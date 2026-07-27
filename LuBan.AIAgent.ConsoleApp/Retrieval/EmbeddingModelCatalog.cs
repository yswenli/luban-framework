namespace LuBan.AIAgent.ConsoleApp.Retrieval;

/// <summary>
/// 模型文件规格
/// </summary>
public record ModelFileSpec(string RemotePath, string LocalName, long MinSizeBytes);

/// <summary>
/// 嵌入模型规格
/// </summary>
public record EmbeddingModelSpec(string ModelId, int Dimension, string RemoteBase, string MirrorBase, IReadOnlyList<ModelFileSpec> Files);

/// <summary>
/// 嵌入模型目录
/// </summary>
public static class EmbeddingModelCatalog
{
    /// <summary>
    /// bge-small-zh-v1.5（384 维，中文+代码混合场景）
    /// </summary>
    public static readonly EmbeddingModelSpec BgeSmallZhV15 = new(
        "bge-small-zh-v1.5", 384,
        "https://huggingface.co/BAAI/bge-small-zh-v1.5/resolve/main/",
        "https://hf-mirror.com/BAAI/bge-small-zh-v1.5/resolve/main/",
        new ModelFileSpec[]
        {
            new("onnx/model.onnx", "model.onnx", 10_000_000),
            new("tokenizer.json", "tokenizer.json", 100_000),
            new("tokenizer_config.json", "tokenizer_config.json", 100),
        });

    /// <summary>
    /// 按模型标识查找
    /// </summary>
    public static EmbeddingModelSpec? Find(string modelId)
        => string.Equals(modelId, BgeSmallZhV15.ModelId, StringComparison.OrdinalIgnoreCase) ? BgeSmallZhV15 : null;
}
