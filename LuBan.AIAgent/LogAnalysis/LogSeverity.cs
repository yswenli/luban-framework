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
*文件名： LogSeverity
*版本号： V1.0.0.0
*唯一标识：31044c0c-2b69-466a-a73b-4985221e3b87
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/9
*描述：日志严重级别（Trace..Fatal 顺序由低到高；Unknown 表示未识别，不参与排序）。
*
*=================================================
*修改标记
*修改时间：2026/10/9
*修改人： yswenli
*版本号： V1.0.0.0
*描述：日志严重级别（Trace..Fatal 顺序由低到高；Unknown 表示未识别，不参与排序）。
*
*****************************************************************************/

namespace LuBan.AIAgent.LogAnalysis;

/// <summary>日志严重级别（Trace..Fatal 顺序由低到高；Unknown 表示未识别，不参与排序）。</summary>
public enum LogSeverity
{
    /// <summary>跟踪。</summary>
    Trace,
    /// <summary>调试。</summary>
    Debug,
    /// <summary>信息。</summary>
    Info,
    /// <summary>警告。</summary>
    Warn,
    /// <summary>错误。</summary>
    Error,
    /// <summary>致命。</summary>
    Fatal,
    /// <summary>未知（未匹配到级别）。</summary>
    Unknown
}