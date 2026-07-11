/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Core
*文件名： FlowNodeHandleContext
*版本号： V1.0.0.0
*唯一标识：101d1262-bf30-41c6-97d7-c8175d43d396
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30 13:50:48
*描述：节点处理上下文：封装当前执行环境。
*
*=================================================
*修改标记
*修改时间：2025/10/30 13:50:48
*修改人： yswenli
*版本号： V1.0.0.0
*描述：节点处理上下文：封装当前执行环境。
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Core;

/// <summary>
/// 节点处理上下文：封装当前执行环境。
/// 在节点处理器处理节点时提供必要的上下文信息。
/// </summary>
public class FlowNodeHandleContext
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; }

    /// <summary>
    /// 当前要处理的图节点。
    /// </summary>
    public GraphNode Node { get; }

    /// <summary>
    /// 流程定义。
    /// </summary>
    public GraphFlowDefinition Definition { get; }

    /// <summary>
    /// 流程运行时状态。
    /// </summary>
    public FlowRuntimeState State { get; }

    /// <summary>
    /// 流程构建器实例。
    /// </summary>
    public FlowBuilder Builder { get; }

    /// <summary>
    /// 推进请求（可选）。
    /// </summary>
    public AdvanceRequest? Request { get; }

    /// <summary>
    /// 流程记录（可选）。
    /// </summary>
    public FlowRecord? Record { get; }

    /// <summary>
    /// 构造节点处理上下文。
    /// </summary>
    /// <param name="recordId">流程记录ID。</param>
    /// <param name="node">当前图节点。</param>
    /// <param name="definition">流程定义。</param>
    /// <param name="state">流程运行时状态。</param>
    /// <param name="builder">流程构建器实例。</param>
    /// <param name="request">推进请求（可选）。</param>
    /// <param name="record">流程记录（可选）。</param>
    public FlowNodeHandleContext(
        long recordId,
        GraphNode node,
        GraphFlowDefinition definition,
        FlowRuntimeState state,
        FlowBuilder builder,
        AdvanceRequest? request,
        FlowRecord? record)
    {
        RecordId = recordId;
        Node = node;
        Definition = definition;
        State = state;
        Builder = builder;
        Request = request;
        Record = record;
    }
}