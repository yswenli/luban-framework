/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.AspNetCore
*文件名： SseStream
*版本号： V1.0.0.0
*唯一标识：25e4efb5-93d1-4ead-b7b2-bed593562ea9
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/6/13 15:09:56
*描述：sse流处理类
*
*=================================================
*修改标记
*修改时间：2025/6/13 15:09:56
*修改人： yswenli
*版本号： V1.0.0.0
*描述：sse流处理类
*
*****************************************************************************/
namespace LuBan.Web.Core.AspNetCore;

/// <summary>
/// sse流处理类
/// </summary>
public class SseStream : IDisposable
{
    HttpResponse _response;
    Stream _stream;

    /// <summary>
    /// sse流处理类
    /// </summary>
    /// <param name="stream"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public SseStream(Stream? stream)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream), "stream cannot be null.");
        var contenxt = WebApp.HttpContext;
        if (contenxt == null) throw new ArgumentNullException(nameof(WebApp.HttpContext), "HttpContext cannot be null. Ensure this is called within an HTTP request context.");
        _response = contenxt.Response;
        _stream = stream;
    }

    /// <summary>
    /// sse流处理类
    /// </summary>
    /// <param name="msg"></param>
    public SseStream(string msg) : this(msg.ToStream())
    {

    }
    /// <summary>
    /// sse流处理类
    /// </summary>
    /// <param name="sp"></param>
    public SseStream(StringPlus sp) : this(sp.ToStream())
    {

    }
    /// <summary>
    /// sse流处理类
    /// </summary>
    /// <param name="sb"></param>
    public SseStream(StringBuilder sb) : this(sb.ToString())
    {

    }

    /// <summary>
    /// 发送SSE流数据
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SendAsync(CancellationToken cancellationToken)
    {
        try
        {
            // 设置SSE所需的响应头
            _response.ContentType = "text/event-stream";
            _response.Headers.Append("Cache-Control", "no-cache");
            _response.Headers.Append("Connection", "keep-alive");
            _response.Headers.Append("X-Accel-Buffering", "no");

            // 如果流支持 Seek，重置到起始位置
            if (_stream.CanSeek)
            {
                _stream.Seek(0, SeekOrigin.Begin);
            }

            // 使用缓冲区大小优化传输
            const int bufferSize = 81920; // 80KB buffer
            var buffer = new byte[bufferSize];
            int bytesRead;

            // 循环读取网络流
            while ((bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                // 将读取的数据写入响应
                await _response.Body.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                await _response.Body.FlushAsync(cancellationToken);

                // 检查是否取消
                cancellationToken.ThrowIfCancellationRequested();
            }

            // 发送结束标志
            //var endMessage = "data: [END]\n\n";
            //await _response.Body.WriteAsync(Encoding.UTF8.GetBytes(endMessage), cancellationToken);
            //await _response.Body.FlushAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Logger.Warn("SSE stream transmission was canceled by the client.");
        }
        catch (IOException ex)
        {
            Logger.Error($"SSE流发送失败: 网络流中断 - {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            Logger.Error($"SSE流发送失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 发送SSE流数据
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public async Task SendAsync(int timeout = 180)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(180));
        await SendAsync(cts.Token);
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        if (_stream.CanRead)
        {
            _stream.Dispose();
        }
    }
}
