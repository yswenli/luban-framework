/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.LogAnalysis
*文件名： ILogAggregator
*版本号： V1.0.0.0
*唯一标识：74ed2c9e-69b9-46f2-95ca-00b27863e94e
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/9
*描述：日志聚合器：单遍流式扫描，恒定内存。
*
*=================================================
*修改标记
*修改时间：2026/10/9
*修改人： yswenli
*版本号： V1.0.0.0
*描述：日志聚合器：单遍流式扫描，恒定内存。
*
*****************************************************************************/

namespace LuBan.AIAgent.LogAnalysis;

/// <summary>日志聚合器：单遍流式扫描，恒定内存。</summary>
public interface ILogAggregator
{
    /// <summary>聚合成概览。</summary>
    /// <param name="filePath">日志文件路径。</param>
    /// <param name="ct">外部取消令牌。</param>
    Task<LogSummary> AggregateAsync(string filePath, CancellationToken ct = default);
}
