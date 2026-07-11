/*****************************************************************************
 * Copyright (c) YSWenli All Rights Reserved.
 * CLR版本： .net8.0
 * 机器名称：WALLE
 * Author：yswenli
 * 命名空间：LuBan.ApprovalFlow.Core.Handlers
 * 文件名： TaskNodeHandler
 * 版本号： V1.0.0.0
 * 唯一标识：101d1262-bf30-41c6-97d7-c8175d43d396
 * 当前的用户域：WALLE
 * 创建人： yswenli
 * 电子邮箱：yswenli@outlook.com
 * 创建时间：2025/10/30 13:50:48
 * 描述：服务节点处理器
 *
 * =================================================
 * 修改标记
 * 修改时间：2025/10/30 13:50:48
 * 修改人： yswenli
 * 版本号： V1.0.0.0
 * 描述：服务节点处理器
 *****************************************************************************/

namespace LuBan.ApprovalFlow.Core.Handlers;

/// <summary>
/// 服务节点处理器：按配置调用目标服务方法并返回结果，可更新表单模型字段。
/// </summary>
public class TaskNodeHandler : BaseNodeHandler
{
    /// <summary>
    /// 判断是否能处理指定节点
    /// </summary>
    /// <param name="node">图节点</param>
    /// <returns>是否能处理</returns>
    public override bool CanHandle(GraphNode node)
        => string.Equals(node?.Type, "service-node", StringComparison.OrdinalIgnoreCase);

/// <summary>
/// 处理服务节点：仅触发事件，不执行业务代码反射调用
/// </summary>
/// <param name="ctx">节点处理上下文</param>
/// <returns>处理结果</returns>
public override async Task<object?> HandleAsync(FlowNodeHandleContext ctx)
{
    // TaskNodeHandler（service-node）现在变为空处理
    // 仅触发事件，不执行业务逻辑
    // 业务逻辑完全由 HTTP 回调或 Listener 实现

    await Task.CompletedTask;
    return null;
}
}
