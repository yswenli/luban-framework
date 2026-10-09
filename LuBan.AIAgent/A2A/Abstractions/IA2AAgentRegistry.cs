/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.A2A.Abstractions
*文件名： IA2AAgentRegistry
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/8
*描述：A2A Agent 注册表，按名称解析描述符与处理句柄
*
*****************************************************************************/
using LuBan.AIAgent.A2A.Models;

namespace LuBan.AIAgent.A2A.Abstractions;

/// <summary>
/// A2A Agent 注册表。维护「名称 → 描述符 + 处理句柄」映射，
/// 供进程内传输（<see cref="IA2ATransport"/>）按 Agent 名路由。线程安全由实现保证。
/// </summary>
public interface IA2AAgentRegistry
{
    /// <summary>
    /// 注册一个 Agent 及其处理句柄。同名覆盖。
    /// </summary>
    /// <param name="descriptor">Agent 描述符。</param>
    /// <param name="handler">处理句柄。</param>
    void Register(A2AAgentDescriptor descriptor, IA2AHandler handler);

    /// <summary>
    /// 按名称解析 Agent。
    /// </summary>
    /// <param name="name">Agent 名称。</param>
    /// <param name="descriptor">解析到的描述符。</param>
    /// <param name="handler">解析到的处理句柄。</param>
    /// <returns>是否解析成功。</returns>
    bool TryResolve(string name, out A2AAgentDescriptor descriptor, out IA2AHandler handler);

    /// <summary>
    /// 获取全部已注册的 Agent 描述符。
    /// </summary>
    /// <returns>描述符集合。</returns>
    IReadOnlyCollection<A2AAgentDescriptor> GetAll();
}