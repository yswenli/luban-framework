/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Logging.FileLogger
*文件名： RollingFileWriter.cs
*版本号： V1.0.0.0
*唯一标识：7d0055ac-41d2-48b3-bae0-1de5c56e3ec6
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31 14:35:50
*描述：RollingFileWriter 类
*
*=================================================
*修改标记
*修改时间：2026/7/31 14:35:50
*修改人： yswenli
*版本号： V1.0.0.0
*描述：RollingFileWriter 类
*
*****************************************************************************/

namespace LuBan.Logging.FileLogger;

/// <summary>
/// 滚动文件写入器，按 100MB 或跨天滚动，UTF-8 编码，最多保留指定数量的备份。
/// 复用常驻 StreamWriter 并定时 Flush，避免每行日志开关文件句柄。
/// </summary>
internal sealed class RollingFileWriter : IDisposable
{
    private readonly string _fileName;
    private readonly string _directory;
    private readonly long _maxFileSizeBytes;
    private readonly int _maxRollBackups;
    private readonly object _lock = new();
    private DateTime _currentFileDate = DateTime.MinValue;
    private string _currentFilePath;
    private long _currentFileSize;
    private StreamWriter? _streamWriter;
    private DateTime _lastFlushTime;
    private static readonly TimeSpan _flushInterval = TimeSpan.FromSeconds(1);
    private static int _firstErrorLogged;

    /// <summary>
    /// 初始化滚动文件写入器。
    /// </summary>
    /// <param name="fileName">文件名（如 info.txt）。</param>
    /// <param name="options">文件日志配置。</param>
    public RollingFileWriter(string fileName, FileLoggerOptions options)
    {
        _fileName = fileName;
        _directory = Path.Combine(PathUtil.CurrentPath, options.Directory);
        _maxFileSizeBytes = options.MaxFileSizeBytes;
        _maxRollBackups = options.MaxRollBackups;
        _currentFilePath = Path.Combine(_directory, fileName);
        EnsureDirectoryAndProbeSize();
    }

    private void EnsureDirectoryAndProbeSize()
    {
        try
        {
            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }
            var fi = new FileInfo(_currentFilePath);
            if (fi.Exists)
            {
                _currentFileSize = fi.Length;
                _currentFileDate = fi.LastWriteTime.Date;
            }
            else
            {
                _currentFileSize = 0;
                _currentFileDate = DateTime.Now.Date;
            }
        }
        catch (Exception ex)
        {
            LogFirstError("EnsureDirectoryAndProbeSize", ex);
            _currentFileSize = 0;
            _currentFileDate = DateTime.Now.Date;
        }
    }

    /// <summary>
    /// 写入一行日志文本（追加换行符）。
    /// </summary>
    /// <param name="text">日志文本。</param>
    public void WriteLine(string text)
    {
        lock (_lock)
        {
            try
            {
                var now = DateTime.Now;
                var needRoll = false;

                if (now.Date != _currentFileDate)
                {
                    needRoll = true;
                }
                else if (_currentFileSize >= _maxFileSizeBytes)
                {
                    needRoll = true;
                }

                if (needRoll)
                {
                    RollFile(now.Date);
                }

                EnsureWriterOpen();

                var line = text + Environment.NewLine;
                _streamWriter!.Write(line);
                _currentFileSize += System.Text.Encoding.UTF8.GetByteCount(line);
                _currentFileDate = now.Date;

                // 定时 Flush，平衡性能与崩溃时的数据丢失风险
                if (now - _lastFlushTime >= _flushInterval)
                {
                    _streamWriter.Flush();
                    _lastFlushTime = now;
                }
            }
            catch (Exception ex)
            {
                LogFirstError("WriteLine", ex);
            }
        }
    }

    private void EnsureWriterOpen()
    {
        if (_streamWriter != null) return;

        var fs = new FileStream(_currentFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        _streamWriter = new StreamWriter(fs, System.Text.Encoding.UTF8);
    }

    private void RollFile(DateTime today)
    {
        // 先关闭现有 writer，确保缓冲区落盘
        CloseWriter();

        try
        {
            if (!File.Exists(_currentFilePath))
            {
                _currentFileSize = 0;
                _currentFileDate = today;
                return;
            }

            var baseName = Path.GetFileNameWithoutExtension(_fileName);
            var ext = Path.GetExtension(_fileName);
            var dateStr = _currentFileDate.ToString("yyyyMMdd");
            var backupName = $"{baseName}_{dateStr}{ext}";
            var backupPath = Path.Combine(_directory, backupName);

            var counter = 1;
            while (File.Exists(backupPath))
            {
                backupName = $"{baseName}_{dateStr}_{counter}{ext}";
                backupPath = Path.Combine(_directory, backupName);
                counter++;
            }

            File.Move(_currentFilePath, backupPath);

            CleanupOldBackups(baseName, ext);

            _currentFileSize = 0;
            _currentFileDate = today;
        }
        catch (Exception ex)
        {
            LogFirstError("RollFile", ex);
            // 滚动失败时：重置日期避免当天重复尝试，但重新探测实际大小
            _currentFileDate = today;
            try
            {
                var fi = new FileInfo(_currentFilePath);
                _currentFileSize = fi.Exists ? fi.Length : 0;
            }
            catch
            {
                _currentFileSize = 0;
            }
        }
    }

    private void CloseWriter()
    {
        try
        {
            _streamWriter?.Flush();
            _streamWriter?.Dispose();
        }
        catch
        {
        }
        _streamWriter = null;
    }

    private void CleanupOldBackups(string baseName, string ext)
    {
        try
        {
            var pattern = $"{baseName}_*";
            var backups = Directory.GetFiles(_directory, pattern)
                .Select(p => new FileInfo(p))
                .OrderBy(f => f.CreationTime)
                .ToList();

            while (backups.Count > _maxRollBackups)
            {
                var oldest = backups[0];
                backups.RemoveAt(0);
                oldest.Delete();
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// 仅在首次错误时输出到 Console.Error，避免错误刷屏，同时保留可观测性。
    /// </summary>
    private static void LogFirstError(string context, Exception ex)
    {
        if (Interlocked.Exchange(ref _firstErrorLogged, 1) == 0)
        {
            try
            {
                Console.Error.WriteLine($"[LuBan.Logging] 文件日志写入失败 ({context})，后续错误将不再提示: {ex}");
            }
            catch
            {
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (_lock)
        {
            CloseWriter();
        }
    }
}
