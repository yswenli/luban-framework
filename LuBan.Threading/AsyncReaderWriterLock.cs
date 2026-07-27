namespace LuBan.Threading;

/// <summary>
/// 异步读写锁：允许多个读者并发，写者独占
/// </summary>
public class AsyncReaderWriterLock : IDisposable
{
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly SemaphoreSlim _readLock = new(1, 1);
    private int _readCount;
    private volatile bool _isDisposed;

    /// <summary>
    /// 异步获取读锁
    /// </summary>
    public async Task<IDisposable> ReadLockAsync(CancellationToken cancellationToken = default)
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(AsyncReaderWriterLock));

        await _readLock.WaitAsync(cancellationToken);
        try
        {
            _readCount++;
            if (_readCount == 1)
            {
                try
                {
                    await _writeLock.WaitAsync(cancellationToken);
                }
                catch
                {
                    _readCount--;
                    throw;
                }
            }
        }
        finally
        {
            _readLock.Release();
        }

        return new ReaderLockReleaser(this);
    }

    /// <summary>
    /// 异步获取写锁
    /// </summary>
    public async Task<IDisposable> WriteLockAsync(CancellationToken cancellationToken = default)
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(AsyncReaderWriterLock));

        await _writeLock.WaitAsync(cancellationToken);
        return new WriterLockReleaser(this);
    }

    private void ReleaseReadLock()
    {
        _readLock.Wait();
        try
        {
            _readCount--;
            if (_readCount == 0)
                _writeLock.Release();
        }
        finally
        {
            _readLock.Release();
        }
    }

    private void ReleaseWriteLock()
    {
        _writeLock.Release();
    }

    /// <summary>
    /// 释放读写锁资源
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _writeLock.Dispose();
        _readLock.Dispose();
    }

    private class ReaderLockReleaser : IDisposable
    {
        private AsyncReaderWriterLock? _lock;

        public ReaderLockReleaser(AsyncReaderWriterLock rwLock)
        {
            _lock = rwLock;
        }

        public void Dispose()
        {
            var rwLock = _lock;
            if (rwLock != null && !rwLock._isDisposed)
            {
                rwLock.ReleaseReadLock();
                _lock = null;
            }
        }
    }

    private class WriterLockReleaser : IDisposable
    {
        private AsyncReaderWriterLock? _lock;

        public WriterLockReleaser(AsyncReaderWriterLock rwLock)
        {
            _lock = rwLock;
        }

        public void Dispose()
        {
            var rwLock = _lock;
            if (rwLock != null && !rwLock._isDisposed)
            {
                rwLock.ReleaseWriteLock();
                _lock = null;
            }
        }
    }
}
