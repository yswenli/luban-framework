/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Sessions
*文件名： SessionMessageSegment
*描述：一轮助手回复中的有序段落（思考 / 正文 / 工具调用）
*
*****************************************************************************/

namespace LuBan.AIAgent.Sessions;

/// <summary>
/// 一轮助手回复中的有序段落：思考、正文、工具调用按发生顺序排列。
/// </summary>
/// <remarks>
/// 只服务于「按时间铺开」的呈现。Content / Thinking / ToolCalls 仍是模型上下文
/// 与既有客户端的口径，两者并存、互不替代。
/// </remarks>
public sealed record SessionMessageSegment
{
    /// <summary>段落类型，取值见 <see cref="SegmentKinds"/>。</summary>
    public string Kind { get; init; } = "";

    /// <summary>思考或正文内容；tool 段落为空。</summary>
    public string? Text { get; init; }

    /// <summary>工具名；仅 tool 段落有值。</summary>
    public string? Name { get; init; }

    /// <summary>工具调用标识；仅 tool 段落有值。</summary>
    public string? CallId { get; init; }

    /// <summary>工具入参 JSON 字符串；仅 tool 段落有值。</summary>
    public string? Arguments { get; init; }
}

/// <summary>段落类型常量。</summary>
public static class SegmentKinds
{
    /// <summary>思考（reasoning）。</summary>
    public const string Thinking = "thinking";

    /// <summary>正文。</summary>
    public const string Text = "text";

    /// <summary>工具调用。</summary>
    public const string Tool = "tool";
}