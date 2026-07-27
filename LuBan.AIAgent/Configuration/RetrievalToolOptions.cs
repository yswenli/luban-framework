namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 语义检索工具配置
/// </summary>
public class RetrievalToolOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 嵌入模型标识
    /// </summary>
    public string ModelId { get; set; } = "bge-small-zh-v1.5";

    /// <summary>
    /// 启动时是否自动下载缺失的模型
    /// </summary>
    public bool AutoDownload { get; set; } = true;

    /// <summary>
    /// 索引单文件最大体积（KB）
    /// </summary>
    public int MaxFileSizeKB { get; set; } = 5120;

    /// <summary>
    /// 默认返回结果数
    /// </summary>
    public int DefaultTopK { get; set; } = 5;

    /// <summary>
    /// 单次搜索返回内容最大字符数
    /// </summary>
    public int MaxResultChars { get; set; } = 8000;
}
