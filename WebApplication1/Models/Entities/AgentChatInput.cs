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
*文件名： AgentChatInput
*版本号： V1.0.0.0
*唯一标识：Agent 对话入参
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/28
*描述：Agent 对话接口的请求体
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：新增人设、附件来源与会话标识
*
*****************************************************************************/

namespace WebApplication1.Models.Entities;

/// <summary>
/// Agent 对话入参（chat 与 chatstream 共用）。
/// </summary>
public class AgentChatInput
{
    /// <summary>用户本轮输入。</summary>
    public string Prompt { get; set; } = "";

    /// <summary>
    /// 前端人设/规则；null 或空白用后端 Profile 默认提示词。
    /// 不持久化，只作用于本轮。
    /// </summary>
    public string? System { get; set; }

    /// <summary>
    /// 附件来源：http(s) URL / 本机绝对路径 / 相对路径。
    /// 单项失败只跳过，不下线本轮。
    /// </summary>
    public List<string> Attachments { get; set; } = [];

    /// <summary>会话标识；不传则复用该用户最后一次，没有就新建。</summary>
    public string? SessionId { get; set; }
}