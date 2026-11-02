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
*命名空间：LuBan.AIAgent.Errors
*文件名： ChatClientResilience
*版本号： V1.0.0.0
*唯一标识：0e9a6077-9cbf-4d29-acee-aa94afb185de
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/17
*描述：幂等包装入口，避免重复包裹导致重试次数相乘
*
*****************************************************************************/

namespace LuBan.AIAgent.Errors;

/// <summary>
/// 容错客户端包装入口。
/// </summary>
public static class ChatClientResilience
{
    /// <summary>
    /// 为聊天客户端套上容错重试（已是 <see cref="ResilientChatClient"/> 时原样返回，保证幂等）。
    /// 只允许直接包装原始客户端；不要把已有装饰器（如 SanitizingChatClient）再交给本方法，
    /// 否则会产生嵌套的容错层导致重试次数相乘。
    /// </summary>
    /// <param name="inner">被包装的聊天客户端。</param>
    /// <param name="options">Agent 配置。</param>
    /// <returns>容错聊天客户端。</returns>
    public static IChatClient Wrap(IChatClient inner, LuBanAgentOptions options)
        => inner is ResilientChatClient ? inner : new ResilientChatClient(inner, options);
}