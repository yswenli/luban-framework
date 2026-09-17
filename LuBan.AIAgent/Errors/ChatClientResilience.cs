/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Errors
*文件名： ChatClientResilience
*唯一标识：容错客户端包装入口
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