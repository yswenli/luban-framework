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

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush()
    {
        // 禁止对底层流执行同步 Flush/Write：Kestrel 默认 AllowSynchronousIO=false，
        // 同步操作会抛 InvalidOperationException，此时响应头通常已提交，表现为响应被截断。
        // 数据在 FlushAsync 或中间件末尾交付，此处保持空实现。
    }

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

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
        // 统一委托到异步实现：同步写底层流会触发 AllowSynchronousIO=false 限制并中断响应。
        WriteAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();
    }

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