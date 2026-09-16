/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Models
*文件名： NodeActivityItem
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/16
*描述：子 Agent 节点内部活动条目，随时间轴上报给上层 UI
*
*****************************************************************************/
namespace LuBan.AIAgent.Orchestration.Models;

/// <summary>
/// 子 Agent 节点内部活动条目。节点执行期（最长可达节点超时阈值）内的
/// 思考、正文、工具调用与工具结果都会转为本类型，随
/// <see cref="OrchestrationProgress"/> 实时上报，供上层 UI 展示子代理细节。
/// </summary>
public class NodeActivityItem
{
    /// <summary>
    /// 获取或设置活动类型。
    /// </summary>
    public NodeActivityKind Kind { get; set; }

    /// <summary>
    /// 获取或设置活动文本内容（思考/正文内容，或工具调用的参数摘要、工具结果的返回摘要）。
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 获取或设置工具名（<see cref="NodeActivityKind.ToolCall"/> 时提供）。
    /// </summary>
    public string? ToolName { get; set; }

    /// <summary>
    /// 获取或设置工具调用标识（用于关联调用与结果）。
    /// </summary>
    public string? CallId { get; set; }

    /// <summary>
    /// 获取或设置活动发生时间。
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// 创建思考活动。
    /// </summary>
    /// <param name="text">思考内容（增量）。</param>
    /// <returns>活动条目。</returns>
    public static NodeActivityItem Thinking(string text) => new()
    {
        Kind = NodeActivityKind.Thinking,
        Content = text
    };

    /// <summary>
    /// 创建正文活动（增量片段，上层可合并连续片段）。
    /// </summary>
    /// <param name="text">正文内容（增量）。</param>
    /// <returns>活动条目。</returns>
    public static NodeActivityItem Text(string text) => new()
    {
        Kind = NodeActivityKind.Text,
        Content = text
    };

    /// <summary>
    /// 创建工具调用活动。
    /// </summary>
    /// <param name="toolName">工具名。</param>
    /// <param name="arguments">参数摘要。</param>
    /// <param name="callId">调用标识。</param>
    /// <returns>活动条目。</returns>
    public static NodeActivityItem ToolCall(string toolName, string? arguments, string? callId) => new()
    {
        Kind = NodeActivityKind.ToolCall,
        ToolName = toolName,
        Content = arguments,
        CallId = callId
    };

    /// <summary>
    /// 创建工具结果活动。
    /// </summary>
    /// <param name="content">结果摘要（失败时为错误信息）。</param>
    /// <param name="callId">调用标识。</param>
    /// <returns>活动条目。</returns>
    public static NodeActivityItem ToolResult(string? content, string? callId) => new()
    {
        Kind = NodeActivityKind.ToolResult,
        Content = content,
        CallId = callId
    };
}