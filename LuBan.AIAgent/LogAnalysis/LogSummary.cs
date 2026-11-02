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
*文件名： LogSummary
*版本号： V1.0.0.0
*唯一标识：de287ea4-3e98-4f97-bf98-93da76963e20
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/9
*描述：日志聚合概览。
*
*=================================================
*修改标记
*修改时间：2026/10/9
*修改人： yswenli
*版本号： V1.0.0.0
*描述：日志聚合概览。
*
*****************************************************************************/

namespace LuBan.AIAgent.LogAnalysis;

/// <summary>日志聚合概览。</summary>
/// <param name="FilePath">文件绝对路径。</param>
/// <param name="FileSizeBytes">文件字节数。</param>
/// <param name="TotalLines">已处理行数。</param>
/// <param name="Truncated">是否因超时/超限提前结束。</param>
/// <param name="TruncationReason">提前结束原因："timeout"/"size"/null。</param>
/// <param name="FirstTimestamp">首个探测到的时间戳。</param>
/// <param name="LastTimestamp">最后一个探测到的时间戳。</param>
/// <param name="LevelCounts">各级别计数。</param>
/// <param name="TopErrors">Top-N 错误签名。</param>
/// <param name="OtherErrorCount">超出签名上限被归并的错误计数。</param>
public sealed record LogSummary(
    string FilePath,
    long FileSizeBytes,
    long TotalLines,
    bool Truncated,
    string? TruncationReason,
    DateTimeOffset? FirstTimestamp,
    DateTimeOffset? LastTimestamp,
    IReadOnlyDictionary<LogSeverity, long> LevelCounts,
    IReadOnlyList<ErrorSignature> TopErrors,
    long OtherErrorCount);
