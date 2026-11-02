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
*命名空间：LuBan.AIAgent.Attachments
*文件名： AttachmentMessageBuilder
*版本号： V1.0.0.0
*唯一标识：a150e669-b8f4-4a36-ac5b-aab4ed2d4b96
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/18
*描述：附件消息构建器：把附件内容合并进一条 User 消息。 文本文件内联到正文；大文本仅注入指引；图片作为 <see cref="DataContent"/> 并列。 CLI 与 Codex 两条通路共用，保证格式一致。
*
*=================================================
*修改标记
*修改时间：2026/9/18
*修改人： yswenli
*版本号： V1.0.0.0
*描述：附件消息构建器：把附件内容合并进一条 User 消息。 文本文件内联到正文；大文本仅注入指引；图片作为 <see cref="DataContent"/> 并列。 CLI 与 Codex 两条通路共用，保证格式一致。
*
*****************************************************************************/

namespace LuBan.AIAgent.Attachments;

/// <summary>
/// 附件消息构建器：把附件内容合并进一条 User 消息。
/// 文本文件内联到正文；大文本仅注入指引；图片作为 <see cref="DataContent"/> 并列。
/// CLI 与 Codex 两条通路共用，保证格式一致。
/// </summary>
public static class AttachmentMessageBuilder
{
    /// <summary>
    /// 构建含附件的用户消息。
    /// </summary>
    /// <param name="input">用户输入文本。</param>
    /// <param name="attachments">已处理的附件列表。</param>
    /// <returns>可直接发送的 <see cref="ChatMessage"/>。</returns>
    public static ChatMessage Build(string input, IReadOnlyList<ProcessedAttachment> attachments)
    {
        var body = new StringBuilder(input ?? "");
        var media = new List<AIContent>();

        foreach (var a in attachments)
        {
            switch (a)
            {
                case { Info.Kind: AttachmentKind.TextFile, IsLargeText: false, Content: TextContent tc }:
                    body.AppendLine().AppendLine();
                    body.AppendLine($"--- 附件 {a.Info.FileName} ---");
                    body.AppendLine(tc.Text);
                    body.AppendLine("--- 附件结束 ---");
                    break;
                case { Info.Kind: AttachmentKind.TextFile, IsLargeText: true, Content: TextContent guide }:
                    body.AppendLine().AppendLine(guide.Text);
                    break;
                default:
                    media.Add(a.Content);
                    break;
            }
        }

        var contents = new List<AIContent> { new TextContent(body.ToString().TrimEnd()) };
        contents.AddRange(media);
        return new ChatMessage(ChatRole.User, contents);
    }
}