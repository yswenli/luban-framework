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
*文件名： ErrorSignature
*版本号： V1.0.0.0
*唯一标识：3bd95695-a4fa-4058-8c64-60ae2674b1b9
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/9
*描述：归一化后的错误签名及其出现统计。
*
*=================================================
*修改标记
*修改时间：2026/10/9
*修改人： yswenli
*版本号： V1.0.0.0
*描述：归一化后的错误签名及其出现统计。
*
*****************************************************************************/

namespace LuBan.AIAgent.LogAnalysis;

/// <summary>归一化后的错误签名及其出现统计。</summary>
/// <param name="Template">归一化模板。</param>
/// <param name="Count">出现次数。</param>
/// <param name="SampleLineNumbers">样本行号（≤ SampleLinesPerSignature）。</param>
public sealed record ErrorSignature(
    string Template,
    long Count,
    IReadOnlyList<long> SampleLineNumbers);
