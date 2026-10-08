/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*命名空间：LuBan.AIAgent.Orchestration
*文件名： IOrchestrationProgressSink
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/23
*描述：编排进度回调出口。工具化后编排由主 Agent 经工具调用触发，
*      执行发生在 FunctionInvokingChatClient 内部，外层流拿不到 update，
*      故进度经本接口由宿主实现直接投递到 UI。
*
*****************************************************************************/
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.AIAgent.Orchestration;

/// <summary>
/// 编排进度回调出口。框架仅定义契约并默认注册 <see cref="NullOrchestrationProgressSink"/>（no-op），
/// 宿主（如 TUI/GUI）可覆盖注册实现以实时渲染规划与节点执行进度。
/// </summary>
public interface IOrchestrationProgressSink
{
    /// <summary>
    /// 发布一次编排进度事件。实现需保证线程安全（同层节点并行上报）。
    /// </summary>
    /// <param name="progress">进度事件。</param>
    void Publish(OrchestrationProgress progress);
}

/// <summary>
/// 默认空实现：宿主未注册 UI Sink 时静默丢弃进度，不影响编排执行。
/// </summary>
public sealed class NullOrchestrationProgressSink : IOrchestrationProgressSink
{
    /// <inheritdoc/>
    public void Publish(OrchestrationProgress progress)
    {
    }
}