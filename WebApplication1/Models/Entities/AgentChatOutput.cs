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
*命名空间：WebApplication1.Models.Entities
*文件名： AgentChatOutput
*版本号： V1.0.0.0
*唯一标识：Agent 对话响应
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：非流式对话接口的响应正文与附件摘要
*
*****************************************************************************/

namespace WebApplication1.Models.Entities;

/// <summary>chat 响应正文（放在统一 <c>Success.Result</c> 壳里）。</summary>
public sealed record AgentChatOutput(
    string SessionId,
    string Content,
    string Thinking,
    AgentAttachmentOverview Attachments,
    string? Error);

/// <summary>附件汇总：成功进入本轮 / 被跳过。</summary>
public sealed record AgentAttachmentOverview(
    IReadOnlyList<AgentAttachmentAdded> Added,
    IReadOnlyList<AgentAttachmentSkipped> Skipped);

/// <summary>成功附件（source 为调用方给的原始来源）。</summary>
public sealed record AgentAttachmentAdded(string Source, string FileName, string MediaType, long FileSize);

/// <summary>被跳过的附件及原因。</summary>
public sealed record AgentAttachmentSkipped(string Source, string Reason);