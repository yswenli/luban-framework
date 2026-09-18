namespace LuBan.AIAgent.Attachments;

/// <summary>
/// 附件类型。
/// </summary>
public enum AttachmentKind
{
    /// <summary>图片，走多模态。</summary>
    Image,

    /// <summary>文本文件，注入或按需读取。</summary>
    TextFile
}