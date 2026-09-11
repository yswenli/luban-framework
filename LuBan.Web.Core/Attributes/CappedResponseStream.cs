namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 封顶响应缓冲流：在指定字节上限内缓冲写入，超限后直通原流并标记溢出。
/// </summary>
internal sealed class CappedResponseStream : Stream
{
    private readonly Stream _inner;
    private readonly MemoryStream _buffer;
    private readonly long _limit;
    private bool _overflow;

    public bool IsOverflow => _overflow;

    public CappedResponseStream(Stream inner, int limit)
    {
        _inner = inner;
        _buffer = new MemoryStream(limit);
        _limit = limit;
    }

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
        if (_overflow) _inner.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (count == 0) return;

        if (_overflow)
        {
            _inner.Write(buffer, offset, count);
            return;
        }

        if (_buffer.Length + count > _limit)
        {
            _overflow = true;
            var remaining = (int)(_limit - _buffer.Length);
            if (remaining > 0)
                _buffer.Write(buffer, offset, remaining);
            var buffered = _buffer.ToArray();
            _inner.Write(buffered, 0, buffered.Length);
            _inner.Write(buffer, offset + remaining, count - remaining);
        }
        else
        {
            _buffer.Write(buffer, offset, count);
        }
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (count == 0) return;

        if (_overflow)
        {
            await _inner.WriteAsync(buffer, offset, count, cancellationToken);
            return;
        }

        if (_buffer.Length + count > _limit)
        {
            _overflow = true;
            var remaining = (int)(_limit - _buffer.Length);
            if (remaining > 0)
                await _buffer.WriteAsync(buffer, offset, remaining, cancellationToken);
            var buffered = _buffer.ToArray();
            await _inner.WriteAsync(buffered, 0, buffered.Length, cancellationToken);
            await _inner.WriteAsync(buffer, offset + remaining, count - remaining, cancellationToken);
        }
        else
        {
            await _buffer.WriteAsync(buffer, offset, count, cancellationToken);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _buffer.Dispose();
        base.Dispose(disposing);
    }
}