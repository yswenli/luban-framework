/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Core.Handlers
*文件名： UserNodeHandler
*版本号： V1.0.0.0
*唯一标识：101d1262-bf30-41c6-97d7-c8175d43d396
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30 13:50:48
*描述：用户节点处理器
*
*=================================================
*修改标记
*修改时间：2025/10/30 13:50:48
*修改人： yswenli
*版本号： V1.0.0.0
*描述：用户节点处理器
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Core.Handlers;

/// <summary>
/// 用户节点处理器：校验角色并按配置调用目标服务方法，可更新表单模型字段。
/// </summary>
public class UserNodeHandler : BaseNodeHandler
{
    /// <summary>
    /// 判断是否能处理指定节点
    /// </summary>
    /// <param name="node">图节点</param>
    /// <returns>是否能处理</returns>
    public override bool CanHandle(GraphNode node)
        => string.Equals(node?.Type, "user-node", StringComparison.OrdinalIgnoreCase);

/// <summary>
/// 处理用户节点：仅触发事件，不执行业务代码反射调用
/// </summary>
/// <param name="ctx">节点处理上下文</param>
/// <returns>处理结果</returns>
public override async Task<object?> HandleAsync(FlowNodeHandleContext ctx)
{
    // UserNodeHandler 现在变为空处理：
    // 1. 触发节点进入/离开事件由 ApprovalFlowEngine 调用
    // 2. 审批记录由 Engine 写入 state.History
    // 3. 会签逻辑由 AggregationEvaluator 处理
    // 业务逻辑完全由 HTTP 回调或 Listener 实现

    await Task.CompletedTask;
    return null;
}

}