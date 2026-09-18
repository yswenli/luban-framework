namespace LuBan.AIAgent.Sessions;

/// <summary>历史附件重建所需的最小记录。</summary>
public sealed class AttachmentRecord
{
    /// <summary>文件名。</summary>
    public string FileName { get; set; } = "";

    /// <summary>MIME 类型。</summary>
    public string MediaType { get; set; } = "";

    /// <summary>原始字节数。</summary>
    public long FileSize { get; set; }

    /// <summary>源文件绝对路径。</summary>
    public string SourcePath { get; set; } = "";

    /// <summary>"Image" 或 "TextFile"。</summary>
    public string Kind { get; set; } = "";
}