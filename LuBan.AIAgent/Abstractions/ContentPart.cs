namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 内容部分抽象记录，用于表示聊天消息中的不同类型的内容
/// </summary>
public abstract record ContentPart;

/// <summary>
/// 文本内容部分，包含文本信息
/// </summary>
/// <param name="Text">文本内容</param>
public record TextPart(string Text) : ContentPart;

/// <summary>
/// 图片URL内容部分，包含图片的URL
/// </summary>
/// <param name="Url">图片URL</param>
public record ImageUrlPart(string Url) : ContentPart;

/// <summary>
/// 二进制内容部分，包含二进制数据和媒体类型
/// </summary>
/// <param name="Data">二进制数据</param>
/// <param name="MediaType">媒体类型</param>
public record BinaryPart(byte[] Data, string MediaType) : ContentPart;
