/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.LogAnalysis
*文件名： LogLineReader
*版本号： V1.0.0.0
*唯一标识：0d8bd932-5b22-4b8d-92ad-cc1cc51e42ad
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/9
*描述：字节块流式行读取器（恒定内存）
*
*****************************************************************************/

namespace LuBan.AIAgent.LogAnalysis;

/// <summary>单行读取结果。</summary>
/// <param name="LineNumber">1 起的行号。</param>
/// <param name="Text">解码后的文本（超长时已截断）。</param>
/// <param name="Truncated">该行是否因超过单行字节上限被截断。</param>
internal readonly record struct LogLine(long LineNumber, string Text, bool Truncated);

/// <summary>
/// 字节块流式行读取器：固定 64KB 读缓冲 + 复用的行缓冲，内存恒定；
/// 跳过 UTF-8 BOM；按 0x0A 分行并去除行尾 0x0D；超长行截断但继续扫描。
/// 说明：多字节 UTF-8 序列的每个字节均 &lt; 0x80 或 ≥ 0x80，绝不会等于 0x0A，
/// 因此按 0x0A 切分不会截断多字节字符。
/// </summary>
internal sealed class LogLineReader
{
    private const int ReadBufferSize = 64 * 1024;
    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);

    private readonly string _filePath;
    private readonly int _maxLineBytes;

    /// <summary>
    /// 指定待读取的日志文件与单行字节上限。
    /// </summary>
    /// <param name="filePath">日志文件路径。</param>
    /// <param name="maxLineBytes">单行保留的最大字节数，超出部分丢弃并标记截断。</param>
    public LogLineReader(string filePath, int maxLineBytes)
    {
        _filePath = filePath;
        _maxLineBytes = Math.Max(1, maxLineBytes);
    }

    /// <summary>
    /// 以恒定内存逐行读取文件，超长行截断后继续扫描。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>逐行产出的日志行序列，行号从 1 起。</returns>
    public async IAsyncEnumerable<LogLine> ReadAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        await using var stream = new FileStream(
            _filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
            ReadBufferSize, FileOptions.SequentialScan | FileOptions.Asynchronous);

        var readBuffer = new byte[ReadBufferSize];
        var lineBuffer = new byte[_maxLineBytes];
        var lineLength = 0;
        var truncated = false;
        var lineNumber = 0L;
        var isFirstBlock = true;

        var read = await stream.ReadAtLeastAsync(
            readBuffer.AsMemory(0, readBuffer.Length),
            minimumBytes: 3,
            throwOnEndOfStream: false,
            cancellationToken: ct).ConfigureAwait(false);
        while (read > 0)
        {
            var start = 0;
            if (isFirstBlock)
            {
                isFirstBlock = false;
                if (read >= 3 && readBuffer[0] == 0xEF && readBuffer[1] == 0xBB && readBuffer[2] == 0xBF)
                    start = 3;
            }

            for (var i = start; i < read; i++)
            {
                if (readBuffer[i] == (byte)'\n')
                {
                    lineNumber++;
                    yield return Build(lineNumber, lineBuffer, lineLength, truncated);
                    lineLength = 0;
                    truncated = false;
                }
                else if (lineLength < lineBuffer.Length)
                {
                    lineBuffer[lineLength++] = readBuffer[i];
                }
                else
                {
                    truncated = true;
                }
            }

            read = await stream.ReadAsync(readBuffer.AsMemory(0, readBuffer.Length), ct).ConfigureAwait(false);
        }

        if (lineLength > 0 || truncated)
        {
            lineNumber++;
            yield return Build(lineNumber, lineBuffer, lineLength, truncated);
        }
    }

    private static LogLine Build(long lineNumber, byte[] buffer, int length, bool truncated)
    {
        if (length > 0 && buffer[length - 1] == (byte)'\r')
            length--;
        return new LogLine(lineNumber, Utf8.GetString(buffer, 0, length), truncated);
    }
}