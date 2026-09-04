/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Core.Handlers
*文件名： EndNodeHandler.cs
*版本号： V1.0.0.0
*唯一标识：3e3c9a43-e485-4720-8dce-2d10caf3a684
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：EndNodeHandler 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：EndNodeHandler 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Core.Handlers;

/// <summary>
/// 结束节点处理器：负责处理审批流程的结束节点，将流程状态设置为已完成。
/// </summary>
public class EndNodeHandler : IFlowNodeHandler
{
    /// <summary>
    /// 判断是否能处理指定节点
    /// </summary>
    /// <param name="node">图节点对象</param>
    /// <returns>如果节点类型为结束节点返回 true，否则返回 false</returns>
    public bool CanHandle(GraphNode node)
        => string.Equals(node?.Type, ConstNodeType.EndNode, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 处理结束节点：将流程状态设置为已完成，并清空当前节点ID
    /// </summary>
    /// <param name="ctx">节点处理上下文，包含节点信息、流程状态等</param>
    /// <returns>始终返回 null</returns>
    public async Task<object?> HandleAsync(FlowNodeHandleContext ctx)
    {
        // 获取当前节点
        var node = ctx.Node;
        if (node == null) return null;

        // 设置流程状态为已完成
        ctx.State.Status = ConstApprovalFlowStatus.Completed;
        // 清空当前节点ID，表示流程已结束
        ctx.State.CurrentNodeKey = null;

        await Task.CompletedTask;
        return null;
    }
}