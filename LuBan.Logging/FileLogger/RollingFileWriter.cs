namespace LuBan.Logging.FileLogger;

/// <summary>
/// 滚动文件写入器，按 100MB 或跨天滚动，UTF-8 编码，最多保留指定数量的备份。
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
        catch
        {
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

                var line = text + Environment.NewLine;
                using (var fs = new FileStream(_currentFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    sw.Write(line);
                }
                _currentFileSize += System.Text.Encoding.UTF8.GetByteCount(line);
                _currentFileDate = now.Date;
            }
            catch
            {
            }
        }
    }

    private void RollFile(DateTime today)
    {
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
        catch
        {
            _currentFileSize = 0;
            _currentFileDate = today;
        }
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

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}
