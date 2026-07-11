/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Core.Handlers
*文件名： GatewayNodeHandler
*版本号： V1.0.0.0
*唯一标识：101d1262-bf30-41c6-97d7-c8175d43d396
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30 13:50:48
*描述：Gateway 节点处理器
*
*=================================================
*修改标记
*修改时间：2025/10/30 13:50:48
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Gateway 节点处理器
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Core.Handlers;

/// <summary>
/// Gateway 节点处理器：根据请求动作或上下文路由键选择一条出边并推进到目标节点。
/// 支持在节点属性中配置 routeKey（默认使用 "edgeText"）来从运行态上下文中取值匹配边文本。
/// </summary>
public class GatewayNodeHandler : IFlowNodeHandler
{
    /// <summary>
    /// 判断是否能处理指定节点
    /// </summary>
    /// <param name="node">图节点</param>
    /// <returns>是否能处理</returns>
    public bool CanHandle(GraphNode node)
        => string.Equals(node?.Type, "gateway-node", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 处理网关节点：根据请求动作或上下文路由键选择一条出边并推进到目标节点
    /// </summary>
    /// <param name="ctx">节点处理上下文</param>
    /// <returns>处理结果</returns>
    public async Task<object?> HandleAsync(FlowNodeHandleContext ctx)
    {
        try
        {
            var node = ctx.Node;
            if (node == null)
            {
                return null;
            }

            // 获取当前节点的所有出边
            var outs = ctx.Definition.Edges?.Where(e => e.SourceNodeId == node.Id).ToList() ?? new();
            if (outs.Count == 0)
            {
                return null;
            }

            // 获取路由键配置，默认为 "edgeText"
            var routeKey = (node.Properties != null && node.Properties.TryGetValue("routeKey", out var rk) && rk != null)
                ? rk.ToString()
                : "edgeText";

            // 确定路由选择值：优先使用请求动作，其次从上下文中获取
            string? selection = null;
            if (!string.IsNullOrWhiteSpace(ctx.Request?.Action))
            {
                // 使用请求中的动作作为路由选择
                selection = ctx.Request!.Action;
            }
            else if (ctx.State.Context != null && !string.IsNullOrWhiteSpace(routeKey) && ctx.State.Context.TryGetValue(routeKey!, out var val) && val != null)
            {
                // 从流程上下文中获取路由值
                selection = val.ToString();
            }

            // 根据选择值匹配出边
            GraphEdge? chosen = null;
            if (!string.IsNullOrWhiteSpace(selection))
            {
                // 根据边文本匹配对应的出边
                chosen = outs.FirstOrDefault(e => string.Equals(e.Text?.Value, selection, StringComparison.OrdinalIgnoreCase));
            }
            // 如果没有匹配的边且只有一条出边，则默认选择该边
            if (chosen == null && outs.Count == 1)
            {
                chosen = outs[0];
            }

            // 设置下一个节点
            if (chosen != null)
            {
                // 将选中的边文本保存到上下文
                if (ctx.State.Context != null)
                {
                    FlowContextAccessor.SetEdgeText(ctx.State.Context, chosen.Text?.Value ?? "auto");
                }
                // 设置下一个要处理的节点ID
                ctx.State.CurrentNodeKey = chosen.TargetNodeId;
            }
            else
            {
                // 无法选择任何边时记录警告
                Logger.Warn("GatewayNodeHandler Error", new Exception("No edge could be selected for node {NodeId} with selection '{Selection}'"), node.Id, selection ?? "null");
            }

            await Task.CompletedTask;
            return null;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "GatewayNodeHandler: Error during execution");
            throw;
        }
    }
}