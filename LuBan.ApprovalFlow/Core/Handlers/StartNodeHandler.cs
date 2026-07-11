namespace LuBan.ApprovalFlow.Core.Handlers;

/// <summary>
/// 开始节点处理器：负责处理审批流程的开始节点，将流程状态设置为处理中。
/// </summary>
public class StartNodeHandler : IFlowNodeHandler
{
    /// <summary>
    /// 判断是否能处理指定节点
    /// </summary>
    /// <param name="node">图节点对象</param>
    /// <returns>如果节点类型为开始节点返回 true，否则返回 false</returns>
    public bool CanHandle(GraphNode node)
        => string.Equals(node?.Type, ConstNodeType.StartNode, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 处理开始节点：将流程状态设置为处理中，并记录当前节点ID
    /// </summary>
    /// <param name="ctx">节点处理上下文，包含节点信息、流程状态等</param>
    /// <returns>始终返回 null</returns>
    public async Task<object?> HandleAsync(FlowNodeHandleContext ctx)
    {
        // 获取当前节点
        var node = ctx.Node;
        if (node == null) return null;

        // 设置流程状态为处理中
        ctx.State.Status = ConstApprovalFlowStatus.Processing;
        // 记录当前处理的节点ID
        ctx.State.CurrentNodeKey = node.Id;

        await Task.CompletedTask;
        return null;
    }
}