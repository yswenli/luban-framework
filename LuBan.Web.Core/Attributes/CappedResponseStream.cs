/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core.Attributes
*文件名： CappedResponseStream
*版本号： V1.0.0.0
*唯一标识：f4e2ba0a-063f-4ce2-9311-a0fd9715c57e
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/11
*描述：封顶响应缓冲流：在指定字节上限内缓冲写入，超限后直通原流。
*
*=================================================
*修改标记
*修改时间：2026/9/11
*修改人： yswenli
*版本号： V1.0.0.0
*描述：封顶响应缓冲流：在指定字节上限内缓冲写入，超限后直通原流。
*
*****************************************************************************/

namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 封顶响应缓冲流：在指定字节上限内缓冲写入，超限后直通原流。
/// 区分两种状态：
/// <list type="bullet">
/// <item><see cref="IsCommitted"/>：缓冲内容是否已交付给底层流（已交付则中间件不再写回，避免重复输出）。</item>
/// <item><see cref="IsTruncated"/>：日志捕获是否已不完整（内容超过上限）。</item>
/// </list>
/// </summary>
internal sealed class CappedResponseStream : Stream
{
    private readonly Stream _inner;
    private readonly MemoryStream _buffer;
    private readonly long _limit;
    private bool _committed;
    private bool _truncated;

    /// <summary>
    /// 日志捕获是否已不完整（内容超过上限）。
    /// </summary>
    public bool IsTruncated => _truncated;

    /// <summary>
    /// 缓冲内容是否已交付给底层流。已交付时中间件不再写回，避免重复输出。
    /// </summary>
    public bool IsCommitted => _committed;

    /// <summary>
    /// 包装底层响应流，在指定字节上限内缓冲写入，超限后改为直通底层流。
    /// </summary>
    /// <param name="inner">被包装的底层响应流。</param>
    /// <param name="limit">缓冲捕获的最大字节数，超出即视为截断。</param>
    public CappedResponseStream(Stream inner, int limit)
    {
        _inner = inner;
        _buffer = new MemoryStream(limit);
        _limit = limit;
    }

    /// <summary>
    /// 上限以内的响应内容（即使已交付也会保留，供日志记录使用）。
    /// </summary>
    public byte[] GetBufferedContent() => _buffer.ToArray();

    /// <summary>
    /// 该包装流仅用于写出响应，不支持读取，恒为 <c>false</c>。
    /// </summary>
    public override bool CanRead => false;
    /// <summary>
    /// 该包装流不支持定位，恒为 <c>false</c>。
    /// </summary>
    public override bool CanSeek => false;
    /// <summary>
    /// 该包装流支持写入，恒为 <c>true</c>；写入内容先进入缓冲或直通底层流。
    /// </summary>
    public override bool CanWrite => true;
    /// <summary>
    /// 该包装流不维护长度，访问即抛出不支持操作异常。
    /// </summary>
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// 同步刷新保持空实现，避免在禁止同步 IO 的宿主上写底层流导致响应被截断。
    /// </summary>
    public override void Flush()
    {
        // 禁止对底层流执行同步 Flush/Write：Kestrel 默认 AllowSynchronousIO=false，
        // 同步操作会抛 InvalidOperationException，此时响应头通常已提交，表现为响应被截断。
        // 数据在 FlushAsync 或中间件末尾交付，此处保持空实现。
    }

    /// <summary>
    /// 异步刷新时把尚未交付的缓冲内容写入底层流并切换为直写模式，随后刷新底层流。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        // 显式异步刷新代表调用方要求数据立即送达客户端（如 SSE / 分块传输）。
        // 交付尚未写出的缓冲内容，之后改为直写底层流；缓冲区继续保留用于日志捕获。
        if (!_committed)
        {
            _committed = true;
            if (_buffer.Length > 0)
            {
                await _inner.WriteAsync(GetBufferMemory(), cancellationToken).ConfigureAwait(false);
            }
        }
        await _inner.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 该包装流不支持读取，调用即抛出不支持操作异常。
    /// </summary>
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <summary>
    /// 该包装流不支持定位，调用即抛出不支持操作异常。
    /// </summary>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <summary>
    /// 该包装流不支持设置长度，调用即抛出不支持操作异常。
    /// </summary>
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <summary>
    /// 同步写入转调异步实现执行，规避宿主禁止同步 IO 的限制。
    /// </summary>
    /// <param name="buffer">待写入的字节数组。</param>
    /// <param name="offset">写入数据在数组中的起始下标。</param>
    /// <param name="count">写入的字节数。</param>
    public override void Write(byte[] buffer, int offset, int count)
    {
        // 统一委托到异步实现：同步写底层流会触发 AllowSynchronousIO=false 限制并中断响应。
        WriteAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 在上限内累计写入缓冲区；超出上限时标记截断并把全部内容写入底层流。
    /// </summary>
    /// <param name="buffer">待写入的字节数组。</param>
    /// <param name="offset">写入数据在数组中的起始下标。</param>
    /// <param name="count">写入的字节数。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (count == 0) return;

        // 已交付：直写底层流，同时在上限内继续捕获用于日志
        if (_committed)
        {
            CaptureForLog(buffer, offset, count);
            await _inner.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            return;
        }

        var room = (int)(_limit - _buffer.Length);
        if (count <= room)
        {
            await _buffer.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            return;
        }

        // 超过上限：写入可捕获部分并标记截断，随后把全部内容交付底层流
        if (room > 0)
            await _buffer.WriteAsync(buffer, offset, room, cancellationToken).ConfigureAwait(false);
        _truncated = true;
        _committed = true;
        await _inner.WriteAsync(GetBufferMemory(), cancellationToken).ConfigureAwait(false);
        await _inner.WriteAsync(new ReadOnlyMemory<byte>(buffer, offset + room, count - room), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 在上限内追加内容用于日志，超过上限则标记截断并停止捕获。
    /// </summary>
    private void CaptureForLog(byte[] buffer, int offset, int count)
    {
        if (_truncated) return;
        var room = (int)(_limit - _buffer.Length);
        if (count <= room)
        {
            _buffer.Write(buffer, offset, count);
        }
        else
        {
            if (room > 0) _buffer.Write(buffer, offset, room);
            _truncated = true;
        }
    }

    private ReadOnlyMemory<byte> GetBufferMemory()
    {
        if (_buffer.TryGetBuffer(out var segment) && segment.Array != null)
            return new ReadOnlyMemory<byte>(segment.Array, segment.Offset, segment.Count);
        return new ReadOnlyMemory<byte>(_buffer.ToArray());
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _buffer.Dispose();
        base.Dispose(disposing);
    }
}