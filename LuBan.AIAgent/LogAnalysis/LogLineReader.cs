/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.LogAnalysis
*文件名： LogLineReader
*描述：字节块流式行读取器（恒定内存）
*****************************************************************************/
using System.Runtime.CompilerServices;

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

    public LogLineReader(string filePath, int maxLineBytes)
    {
        _filePath = filePath;
        _maxLineBytes = Math.Max(1, maxLineBytes);
    }

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

        int read;
        while ((read = await stream.ReadAsync(readBuffer.AsMemory(0, readBuffer.Length), ct).ConfigureAwait(false)) > 0)
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