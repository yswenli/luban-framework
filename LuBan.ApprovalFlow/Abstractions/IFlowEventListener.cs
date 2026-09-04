/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Abstractions
*文件名： IFlowEventListener.cs
*版本号： V1.0.0.0
*唯一标识：1de4d58b-7811-4cc0-848d-bf955e6d5e72
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：IFlowEventListener 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：IFlowEventListener 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Abstractions;

/// <summary>
/// 流程事件监听器接口，用于监听审批流程中的各类事件
/// </summary>
public interface IFlowEventListener
{
    /// <summary>
    /// 流程初始化完成时触发
    /// </summary>
    /// <param name="args">流程初始化事件参数</param>
    void OnInitialized(FlowInitializedEventArgs args);

    /// <summary>
    /// 流程完成时触发
    /// </summary>
    /// <param name="args">流程完成事件参数</param>
    void OnCompleted(FlowCompletedEventArgs args);

    /// <summary>
    /// 流程被驳回时触发
    /// </summary>
    /// <param name="args">流程驳回事件参数</param>
    void OnRejected(FlowRejectedEventArgs args);

    /// <summary>
    /// 流程被取消时触发
    /// </summary>
    /// <param name="args">流程取消事件参数</param>
    void OnCancelled(FlowCancelledEventArgs args);

    /// <summary>
    /// 流程被退回时触发
    /// </summary>
    /// <param name="args">流程退回事件参数</param>
    void OnReturned(FlowReturnedEventArgs args);

    /// <summary>
    /// 流程发生错误时触发
    /// </summary>
    /// <param name="args">流程错误事件参数</param>
    void OnError(FlowErrorEventArgs args);

    /// <summary>
    /// 进入审批节点时触发
    /// </summary>
    /// <param name="args">节点进入事件参数</param>
    void OnNodeEnter(NodeEnterEventArgs args);

    /// <summary>
    /// 离开审批节点时触发
    /// </summary>
    /// <param name="args">节点离开事件参数</param>
    void OnNodeLeave(NodeLeaveEventArgs args);

    /// <summary>
    /// 执行审批操作时触发
    /// </summary>
    /// <param name="args">审批操作事件参数</param>
    void OnApprovalAction(ApprovalActionEventArgs args);

    /// <summary>
    /// 判断是否监听指定流程的事件
    /// </summary>
    /// <param name="flowKey">流程标识</param>
    /// <returns>是否监听，默认为 true</returns>
    bool ShouldListen(string flowKey) => true;
}