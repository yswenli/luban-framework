namespace LuBan.ApprovalFlow.Core
{
    /// <summary>
    /// 流程节点处理器接口（责任链模式）。
    /// 按节点类型或特征决定是否处理，并执行具体逻辑。
    /// 支持扩展自定义节点处理器以实现特定业务需求。
    /// </summary>
    public interface IFlowNodeHandler
    {
        /// <summary>
        /// 判断当前处理器是否可以处理该节点。
        /// </summary>
        /// <param name="node">图式节点。</param>
        /// <returns>可处理返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        bool CanHandle(GraphNode node);

        /// <summary>
        /// 处理节点逻辑：执行节点的具体业务操作。
        /// </summary>
        /// <param name="context">处理上下文，包含记录ID、节点、状态等信息。</param>
        /// <returns>处理过程产生的结果（服务节点一般为方法返回值；用户节点通常为空）。</returns>
        Task<object?> HandleAsync(FlowNodeHandleContext context);
    }
}