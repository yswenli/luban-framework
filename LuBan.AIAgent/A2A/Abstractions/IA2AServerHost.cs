/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.A2A.Abstractions
*文件名： IA2AServerHost
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/8
*描述：A2A 服务端宿主抽象（框架侧中性定义，不依赖 ASP.NET Core 类型）
*
*****************************************************************************/
using LuBan.AIAgent.A2A.Models;

namespace LuBan.AIAgent.A2A.Abstractions;

/// <summary>
/// A2A 服务端宿主抽象。框架侧保持中性，不引用 ASP.NET Core 类型；
/// 由 <c>LubanAgentA2A</c> 基于 ASP.NET Core + A2A SDK 实现，负责把注册到
/// <see cref="IA2AAgentRegistry"/> 的 Agent 暴露为 A2A HTTP 端点
/// （含 <c>/.well-known/agent-card.json</c>）。
/// </summary>
public interface IA2AServerHost
{
    /// <summary>
    /// 获取服务端是否已启动。
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 获取对外端点基址；未启动时为 null。
    /// </summary>
    string? Endpoint { get; }

    /// <summary>
    /// 获取当前对外暴露的 Agent 描述符集合。
    /// </summary>
    /// <returns>描述符集合。</returns>
    IReadOnlyCollection<A2AAgentDescriptor> GetDescriptors();

    /// <summary>
    /// 启动服务端。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    Task StartAsync(CancellationToken ct = default);

    /// <summary>
    /// 停止服务端。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    Task StopAsync(CancellationToken ct = default);
}