/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core.AspNetCore
*文件名： SseEventWriter
*版本号： V1.0.0.0
*唯一标识：a2c7f5d9-1b64-4e08-9d73-2f5c8a06b4e1
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：SSE 事件帧写入器（命名事件 + 注释心跳）
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：SSE 事件帧写入器（命名事件 + 注释心跳）
*
*****************************************************************************/

using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Http;

namespace LuBan.Web.Core.AspNetCore;

/// <summary>
/// SSE 帧写入器：负责响应头、命名事件帧、注释心跳。
/// </summary>
/// <remarks>
/// 与 <see cref="SseStream"/> 的分工：后者写「整段文本流」，本类写「事件流」。
/// <c>data:</c> 必须是单行 JSON，故序列化口径必须 <c>WriteIndented = false</c>。
/// </remarks>
public sealed class SseEventWriter
{
    private readonly HttpResponse _response;
    private readonly JsonSerializerOptions _json;

    /// <summary>创建写入器。</summary>
    /// <param name="response">当前响应。</param>
    /// <param name="json">序列化口径；null 用与 MVC 一致的内置口径（camelCase + 不转义中文）。</param>
    public SseEventWriter(HttpResponse response, JsonSerializerOptions? json = null)
    {
        _response = response ?? throw new ArgumentNullException(nameof(response));
        _json = json ?? LuBan.AIAgent.Protocol.AgentEventProtocol.PayloadJsonOptions;
    }

    /// <summary>设置 SSE 响应头并 flush，之后才能写帧。</summary>
    /// <param name="ct">取消令牌。</param>
    public async Task StartAsync(CancellationToken ct)
    {
        _response.ContentType = "text/event-stream; charset=utf-8";
        _response.Headers.CacheControl = "no-cache";
        _response.Headers.Connection = "keep-alive";
        _response.Headers["X-Accel-Buffering"] = "no";
        await _response.Body.FlushAsync(ct).ConfigureAwait(false);
    }

    /// <summary>写一个命名事件帧。</summary>
    /// <param name="name">事件名（如 <c>text.delta</c>）。</param>
    /// <param name="payload">载荷对象（按 camelCase 序列化）。</param>
    /// <param name="ct">取消令牌。</param>
    public Task WriteEventAsync(string name, object payload, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(payload);

        var data = JsonSerializer.Serialize(payload, payload.GetType(), _json);
        return WriteRawAsync($"event: {name}\ndata: {data}\n\n", ct);
    }

    /// <summary>写一行 SSE 注释（心跳），前端 <c>EventSource</c> 会忽略。</summary>
    /// <param name="comment">注释内容（不得含换行）。</param>
    /// <param name="ct">取消令牌。</param>
    public Task WriteCommentAsync(string comment, CancellationToken ct)
        => WriteRawAsync($": {comment}\n\n", ct);

    private async Task WriteRawAsync(string text, CancellationToken ct)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        await _response.Body.WriteAsync(bytes, ct).ConfigureAwait(false);
        await _response.Body.FlushAsync(ct).ConfigureAwait(false);
    }
}