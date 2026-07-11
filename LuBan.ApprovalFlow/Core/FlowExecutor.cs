/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Core
*文件名： FlowExecutor
*版本号： V1.0.0.0
*唯一标识：101d1262-bf30-41c6-97d7-c8175d43d396
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30 13:50:48
*描述：审批流执行器：封装推进逻辑与节点业务执行。
*
*=================================================
*修改标记
*修改时间：2025/10/30 13:50:48
*修改人： yswenli
*版本号： V1.0.0.0
*描述：审批流执行器：封装推进逻辑与节点业务执行。
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Core;

/// <summary>
/// 审批流执行器：封装推进逻辑与节点业务执行。
/// 负责流程状态的推进、节点处理、异常恢复等核心执行功能。
/// </summary>
public class FlowExecutor : IDisposable
{
    private readonly FlowBuilder _builder;

    /// <summary>
    /// 节点处理器链，按注册顺序组成责任链。
    /// </summary>
    private readonly List<IFlowNodeHandler> _handlers;

    /// <summary>
    /// 是否已释放资源。
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// 获取流程执行统计信息。
    /// </summary>
    public FlowExecutionStats Stats => FlowMonitor.Instance.GetStats(FlowDefinition.Key ?? string.Empty);

    /// <summary>
    /// 是否有正在运行的流程实例。
    /// </summary>
    public bool IsRunning => Stats.RunningRecords > 0;

    /// <summary>
    /// 节点配置数据：当前绑定的流程定义。
    /// </summary>
    public GraphFlowDefinition FlowDefinition { get; internal set; }

    /// <summary>
    /// 流程初始化完成事件。
    /// </summary>
    public event EventHandler<FlowInitializedEventArgs>? Initialized;

    /// <summary>
    /// 节点处理完成事件。
    /// </summary>
    public event EventHandler<NodeHandledEventArgs>? NodeHandled;

    /// <summary>
    /// 流程退出事件。
    /// </summary>
    public event EventHandler<FlowExitedEventArgs>? Exited;

    /// <summary>
    /// 流程错误事件。
    /// </summary>
    public event EventHandler<FlowErrorEventArgs>? OnError;

    /// <summary>
    /// 构造函数（用于FlowBuilder创建实例）。
    /// </summary>
    /// <param name="builder">审批流构建器实例。</param>
    public FlowExecutor(FlowBuilder builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _handlers = new List<IFlowNodeHandler>
        {
            new UserNodeHandler(),
            new TaskNodeHandler(),
            new GatewayNodeHandler()
        };
        FlowMonitor.Instance.RegisterExecutor(this);
    }

    /// <summary>
    /// 构造函数（绑定指定流程定义）。
    /// </summary>
    /// <param name="builder">审批流构建器实例。</param>
    /// <param name="definition">绑定的流程定义。</param>
    public FlowExecutor(FlowBuilder builder, GraphFlowDefinition definition)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        FlowDefinition = definition;
        _handlers = new List<IFlowNodeHandler>
        {
            new UserNodeHandler(),
            new TaskNodeHandler(),
            new GatewayNodeHandler()
        };
        FlowMonitor.Instance.RegisterExecutor(this);
    }

    /// <summary>
    /// 构造函数（绑定流程定义并支持注入节点处理器链）。
    /// </summary>
    /// <param name="builder">审批流构建器实例。</param>
    /// <param name="definition">绑定的流程定义。</param>
    /// <param name="handlers">节点处理器集合，按注册顺序组成责任链。</param>
    public FlowExecutor(FlowBuilder builder, GraphFlowDefinition definition, IEnumerable<IFlowNodeHandler> handlers)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        FlowDefinition = definition;
        _handlers = (handlers?.ToList() ?? new List<IFlowNodeHandler>())
            .DefaultIfEmpty(new UserNodeHandler())
            .Append(new TaskNodeHandler())
            .Append(new GatewayNodeHandler())
            .ToList();
        FlowMonitor.Instance.RegisterExecutor(this);
    }

    /// <summary>
    /// 释放资源：注销执行器并标记已释放状态。
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed) return;
        FlowMonitor.Instance.UnregisterExecutor(this);
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 插入流程记录（内存实现）并返回记录主键。
    /// </summary>
    /// <param name="def">流程定义。</param>
    /// <param name="state">初始运行时状态。</param>
    /// <param name="formPayload">表单数据载荷（可选）。</param>
    /// <returns>新建记录的主键ID。</returns>
    public async Task<long> InsertRecordAsync(GraphFlowDefinition def, FlowRuntimeState state, Dictionary<string, object>? formPayload)
    {
        // 从流程定义中获取表单名称
        var formName = def.Nodes.FirstOrDefault(n => string.Equals(n.Type, ConstNodeType.TaskNode, StringComparison.OrdinalIgnoreCase))?.Text?.Value;
        var record = new FlowRecord
        {
            FormName = formName,
            FormStatus = ConstApprovalFlowStatus.NotStarted,
            FormJson = formPayload != null ? SerializeUtil.Serialize(formPayload) : null,
            FlowJson = SerializeUtil.Serialize(def),
            FlowResult = SerializeUtil.Serialize(state)
        };
        // 添加记录到监控器
        var id = FlowMonitor.Instance.AddRecord(FlowDefinition.Key ?? string.Empty, record);
        // 触发初始化事件
        Initialized?.Invoke(this, new FlowInitializedEventArgs(id, def, state, formPayload));
        // 发布 EventBus 初始化事件
        var eventBusPublisher = new FlowEventBusPublisher();
        await eventBusPublisher.PublishInitializedAsync(id, FlowDefinition.Key, formPayload, null);
        return await Task.FromResult(id);
    }

    /// <summary>
    /// 处理节点：根据节点类型调用对应的处理器，执行节点业务逻辑。
    /// </summary>
    /// <param name="recordId">流程记录ID。</param>
    /// <param name="node">当前节点。</param>
    /// <param name="def">流程定义。</param>
    /// <param name="state">流程运行时状态。</param>
    /// <param name="req">推进请求（可为空）。</param>
    /// <returns>节点处理结果。</returns>
    private async Task<object?> HandleNodeAsync(long recordId, GraphNode node, GraphFlowDefinition def, FlowRuntimeState state, AdvanceRequest? req)
    {
        try
        {
            // 获取流程记录并构建处理上下文
            FlowMonitor.Instance.TryGetRecord(recordId, out var record);
            var ctx = new FlowNodeHandleContext(recordId, node, def, state, _builder, req, record);

            // 按责任链顺序查找并执行处理器
            foreach (var h in _handlers)
            {
                if (h.CanHandle(node))
                {
                    try
                    {
                        // 发布节点进入 EventBus 事件
                        var eventBusPublisher = new FlowEventBusPublisher();
                        await eventBusPublisher.PublishNodeEnterAsync(recordId, FlowDefinition.Key, node, state.Variables);

                        var result = await h.HandleAsync(ctx);
                        // 触发节点处理完成事件
                        NodeHandled?.Invoke(this, new NodeHandledEventArgs(recordId, node, def, state, req, result));
                        // 发布节点离开 EventBus 事件
                        await eventBusPublisher.PublishNodeLeaveAsync(recordId, FlowDefinition.Key, node, req?.Action, req?.ActorUserId, null, state.Variables);
                        // 如果是审批操作，额外发布审批操作事件
                        if (!string.IsNullOrEmpty(req?.Action) && string.Equals(node.Type, ConstNodeType.UserNode, StringComparison.OrdinalIgnoreCase))
                        {
                            await eventBusPublisher.PublishApprovalActionAsync(recordId, FlowDefinition.Key, node, req.Action, req.ActorUserId, null, state.Variables);
                        }
                        return result;
                    }
                    catch (Exception ex)
                    {
                        // 处理失败：更新状态并记录错误
                        state.Status = ConstApprovalFlowStatus.Exception;
                        var errorMsg = $"节点处理失败，记录ID: {recordId}, 节点ID: {node.Id}, 节点类型: {node.Type}";
                        Logger.Error(ex, errorMsg);
                        RaiseOnError(recordId, node, def, state, req, ex, errorMsg);
                        FlowMonitor.Instance.UpdateRecordResult(recordId, SerializeUtil.Serialize(state), ConstApprovalFlowStatus.Exception);
                        return null;
                    }
                }
            }

            // 没有找到能处理该节点类型的处理器
            var errorMessage = $"没有找到能处理节点类型 '{node.Type}' 的处理器";
            state.Status = ConstApprovalFlowStatus.Exception;
            Logger.Error(errorMessage);
            var nodeEx = new InvalidOperationException(errorMessage);
            RaiseOnError(recordId, node, def, state, req, nodeEx, errorMessage);
            FlowMonitor.Instance.UpdateRecordResult(recordId, SerializeUtil.Serialize(state), ConstApprovalFlowStatus.Exception);
            return null;
        }
        catch (Exception ex)
        {
            // 处理过程中发生未预期的错误
            state.Status = ConstApprovalFlowStatus.Exception;
            var errorMsg = $"处理节点时发生未预期的错误，记录ID: {recordId}";
            Logger.Error(ex, errorMsg);
            RaiseOnError(recordId, node, def, state, req, ex, errorMsg);
            FlowMonitor.Instance.UpdateRecordResult(recordId, SerializeUtil.Serialize(state), ConstApprovalFlowStatus.Exception);
            return null;
        }
    }

    /// <summary>
    /// 触发错误事件。
    /// </summary>
    /// <param name="recordId">流程记录ID。</param>
    /// <param name="node">相关节点（可选）。</param>
    /// <param name="def">流程定义。</param>
    /// <param name="state">流程状态。</param>
    /// <param name="req">推进请求（可选）。</param>
    /// <param name="ex">异常对象。</param>
    /// <param name="message">错误消息（可选）。</param>
    private void RaiseOnError(long recordId, GraphNode? node, GraphFlowDefinition def, FlowRuntimeState? state, AdvanceRequest? req, Exception ex, string? message = null)
    {
        OnError?.Invoke(this, new FlowErrorEventArgs(recordId, node, def, state, req, ex, message));
    }

    /// <summary>
    /// 手动推进流程：根据用户操作或系统触发，将流程从当前节点推进到下一个节点。
    /// </summary>
    /// <param name="request">推进请求，包含记录ID和操作类型。</param>
    /// <returns>推进后的流程状态。</returns>
    public async Task<FlowRuntimeState> AdvanceAsync(AdvanceRequest request)
    {
        if (request == null)
        {
            var ex = new ArgumentNullException(nameof(request));
            Logger.Error(ex, "推进请求参数为空");
            return new FlowRuntimeState { Status = ConstApprovalFlowStatus.Processing };
        }

        // 使用命名锁确保并发安全
        using var locker = await LockerBuilder.Default.CreateAsync($"FlowExecutor_Advance_{request.RecordId}");

        // 获取流程记录
        if (!FlowMonitor.Instance.TryGetRecord(request.RecordId, out var record) || record == null)
        {
            var errorMsg = $"流程记录 {request.RecordId} 不存在";
            Logger.Error(errorMsg);
            var ex = new InvalidOperationException(errorMsg);
            var errorState = new FlowRuntimeState { Status = ConstApprovalFlowStatus.Processing };
            RaiseOnError(request.RecordId, null, FlowDefinition, errorState, request, ex, errorMsg);
            return errorState;
        }

        // 反序列化流程状态
        var state = SerializeUtil.Deserialize<FlowRuntimeState>(record.FlowResult ?? "{}");
        if (state == null)
        {
            var errorMsg = "无法反序列化流程状态";
            Logger.Error(errorMsg);
            var ex = new InvalidOperationException(errorMsg);
            var errorState = new FlowRuntimeState { Status = ConstApprovalFlowStatus.Processing };
            RaiseOnError(request.RecordId, null, FlowDefinition, errorState, request, ex, errorMsg);
            return errorState;
        }

        // 检查流程是否已结束
        if (string.Equals(state.Status, ConstApprovalFlowStatus.Completed, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(state.Status, ConstApprovalFlowStatus.Processing, StringComparison.OrdinalIgnoreCase))
        {
            return state;
        }

        // 序列化当前状态作为操作前快照（用于异常恢复）
        var preOperationState = SerializeUtil.Serialize(state);

        try
        {
            // 获取当前节点
            var currentNode = FlowDefinition.Nodes.FirstOrDefault(n => n.Id == state.CurrentNodeKey);
            if (currentNode == null)
            {
                var errorMsg = $"当前节点 {state.CurrentNodeKey} 不存在";
                Logger.Error(errorMsg);
                var ex = new InvalidOperationException(errorMsg);
                state.Status = ConstApprovalFlowStatus.Exception;
                RaiseOnError(request.RecordId, null, FlowDefinition, state, request, ex, errorMsg);
                FlowMonitor.Instance.UpdateRecordResult(request.RecordId, SerializeUtil.Serialize(state), ConstApprovalFlowStatus.Exception);
                return state;
            }

            // 处理当前节点
            await HandleNodeAsync(request.RecordId, currentNode, FlowDefinition, state, request);

            // 查找下一个节点
            var nextNode = FindNextNode(currentNode, request.Action, state, record);

            if (nextNode == null)
            {
                // 没有下一个节点，流程结束
                state.Status = ConstApprovalFlowStatus.Completed;
                state.CurrentNodeKey = null;
                FlowMonitor.Instance.UpdateRecordResult(request.RecordId, SerializeUtil.Serialize(state), ConstApprovalFlowStatus.Completed);
                Exited?.Invoke(this, new FlowExitedEventArgs(request.RecordId, FlowDefinition, state));
                // 发布 EventBus 流程完成事件
                var eventBusPublisher = new FlowEventBusPublisher();
                await eventBusPublisher.PublishCompletedAsync(request.RecordId, FlowDefinition.Key, state.Variables);
                return state;
            }

            // 更新当前节点
            state.CurrentNodeKey = nextNode.Id;

            // 处理特殊节点逻辑：任务网关节点
            if (string.Equals(nextNode.Type, ConstNodeType.TaskGatewayNode, StringComparison.OrdinalIgnoreCase))
            {
                // 检查上一个任务节点的状态
                var previousTaskNodeResult = await CheckPreviousTaskNodeStatus(request.RecordId, state);
                if (previousTaskNodeResult != null &&
                    (string.Equals(previousTaskNodeResult, ConstTaskNodeStatus.未开始, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(previousTaskNodeResult, ConstTaskNodeStatus.失败, StringComparison.OrdinalIgnoreCase)))
                {
                    // 任务未开始或失败，退出审批流
                    state.Status = ConstApprovalFlowStatus.Completed;
                    state.CurrentNodeKey = null;
                    FlowMonitor.Instance.UpdateRecordResult(request.RecordId, SerializeUtil.Serialize(state), ConstApprovalFlowStatus.Completed);
                    Exited?.Invoke(this, new FlowExitedEventArgs(request.RecordId, FlowDefinition, state));
                    return state;
                }
            }

            // 更新记录状态
            FlowMonitor.Instance.UpdateRecordResult(request.RecordId, SerializeUtil.Serialize(state), state.Status);

            // 自动推进（如果是自动节点）
            if (IsAutoAdvanceNode(nextNode))
            {
                return await AutoAdvanceAsync(request.RecordId, state);
            }

            return state;
        }
        catch (Exception ex)
        {
            // 发生异常时恢复到操作前状态
            try
            {
                var restoredState = SerializeUtil.Deserialize<FlowRuntimeState>(preOperationState ?? "{}");
                if (restoredState != null)
                {
                    restoredState.Status = ConstApprovalFlowStatus.Processing;
                    FlowMonitor.Instance.UpdateRecordResult(request.RecordId, SerializeUtil.Serialize(restoredState), ConstApprovalFlowStatus.Processing);
                }
            }
            catch { }

            var errorMsg = $"推进流程时发生错误，记录ID: {request.RecordId}";
            Logger.Error(ex, errorMsg);
            RaiseOnError(request.RecordId, null, FlowDefinition, state, request, ex, errorMsg);
            return state;
        }
    }

    /// <summary>
    /// 自动推进流程：无需用户操作，系统自动将流程从当前节点推进到下一个节点。
    /// </summary>
    /// <param name="recordId">流程记录ID。</param>
    /// <param name="state">当前流程状态（可选，为空则从记录中获取）。</param>
    /// <returns>推进后的流程状态。</returns>
    public async Task<FlowRuntimeState> AutoAdvanceAsync(long recordId, FlowRuntimeState? state = null)
    {
        try
        {
            // 使用命名锁确保并发安全
            using var locker = await LockerBuilder.Default.CreateAsync($"FlowExecutor_AutoAdvance_{recordId}");

            // 获取流程记录
            if (!FlowMonitor.Instance.TryGetRecord(recordId, out var record) || record == null)
            {
                var errorMsg = $"流程记录 {recordId} 不存在";
                Logger.Error(errorMsg);
                var ex = new InvalidOperationException(errorMsg);
                var errorState = new FlowRuntimeState { Status = ConstApprovalFlowStatus.Processing };
                RaiseOnError(recordId, null, FlowDefinition, errorState, null, ex, errorMsg);
                return errorState;
            }

            // 如果没有提供状态，则从记录中获取
            if (state == null)
            {
                state = SerializeUtil.Deserialize<FlowRuntimeState>(record.FlowResult ?? "{}");
                if (state == null)
                {
                    var errorMsg = "无法反序列化流程状态";
                    Logger.Error(errorMsg);
                    var ex = new InvalidOperationException(errorMsg);
                    var errorState = new FlowRuntimeState { Status = ConstApprovalFlowStatus.Processing };
                    RaiseOnError(recordId, null, FlowDefinition, errorState, null, ex, errorMsg);
                    return errorState;
                }
            }

            // 检查流程是否已结束或异常
            if (string.Equals(state.Status, ConstApprovalFlowStatus.Completed, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(state.Status, ConstApprovalFlowStatus.Exception, StringComparison.OrdinalIgnoreCase))
            {
                return state;
            }

            // 序列化当前状态作为操作前快照（用于异常恢复）
            var preOperationState = SerializeUtil.Serialize(state);

            try
            {
                // 获取当前节点
                var currentNode = FlowDefinition.Nodes.FirstOrDefault(n => n.Id == state.CurrentNodeKey);
                if (currentNode == null)
                {
                    var errorMsg = $"当前节点 {state.CurrentNodeKey} 不存在";
                    Logger.Error(errorMsg);
                    var ex = new InvalidOperationException(errorMsg);
                    state.Status = ConstApprovalFlowStatus.Processing;
                    RaiseOnError(recordId, null, FlowDefinition, state, null, ex, errorMsg);
                    FlowMonitor.Instance.UpdateRecordResult(recordId, SerializeUtil.Serialize(state), ConstApprovalFlowStatus.Processing);
                    return state;
                }

                // 处理当前节点
                await HandleNodeAsync(recordId, currentNode, FlowDefinition, state, null);

                // 查找下一个自动节点
                var nextNode = FindNextNode(currentNode, "auto", state, record);

                if (nextNode == null)
                {
                    // 没有下一个节点，流程结束
                    state.Status = ConstApprovalFlowStatus.Completed;
                    state.CurrentNodeKey = null;
                    FlowMonitor.Instance.UpdateRecordResult(recordId, SerializeUtil.Serialize(state), ConstApprovalFlowStatus.Completed);
                    Exited?.Invoke(this, new FlowExitedEventArgs(recordId, FlowDefinition, state));
                    return state;
                }

                // 更新当前节点
                state.CurrentNodeKey = nextNode.Id;

                // 处理特殊节点逻辑：任务网关节点
                if (string.Equals(nextNode.Type, ConstNodeType.TaskGatewayNode, StringComparison.OrdinalIgnoreCase))
                {
                // 检查上一个服务节点的状态
                    var previousTaskNodeResult = await CheckPreviousTaskNodeStatus(recordId, state);
                    if (previousTaskNodeResult != null &&
                        (string.Equals(previousTaskNodeResult, ConstTaskNodeStatus.未开始, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(previousTaskNodeResult, ConstTaskNodeStatus.失败, StringComparison.OrdinalIgnoreCase)))
                    {
                        // 任务未开始或失败，退出审批流
                        state.Status = ConstApprovalFlowStatus.Completed;
                        state.CurrentNodeKey = null;
                        FlowMonitor.Instance.UpdateRecordResult(recordId, SerializeUtil.Serialize(state), ConstApprovalFlowStatus.Completed);
                        Exited?.Invoke(this, new FlowExitedEventArgs(recordId, FlowDefinition, state));
                        return state;
                    }
                }

                // 更新记录状态
                FlowMonitor.Instance.UpdateRecordResult(recordId, SerializeUtil.Serialize(state), state.Status);

                // 继续自动推进（如果是自动节点）
                if (IsAutoAdvanceNode(nextNode))
                {
                    return await AutoAdvanceAsync(recordId, state);
                }

                return state;
            }
            catch (Exception ex)
            {
                // 发生异常时恢复到操作前状态并标记为异常
                try
                {
                    var restoredState = SerializeUtil.Deserialize<FlowRuntimeState>(preOperationState ?? "{}");
                    if (restoredState != null)
                    {
                        restoredState.Status = ConstApprovalFlowStatus.Exception;
                        FlowMonitor.Instance.UpdateRecordResult(recordId, SerializeUtil.Serialize(restoredState), ConstApprovalFlowStatus.Exception);
                    }
                }
                catch { }

                var errorMsg = $"自动推进流程时发生错误，记录ID: {recordId}";
                Logger.Error(ex, errorMsg);
                RaiseOnError(recordId, null, FlowDefinition, state, null, ex, errorMsg);
                return state;
            }
        }
        catch (Exception ex)
        {
            // 外层异常处理
            var errorMsg = $"自动推进流程时发生未预期的错误，记录ID: {recordId}";
            Logger.Error(ex, errorMsg);
            var errorState = new FlowRuntimeState { Status = ConstApprovalFlowStatus.Processing };
            RaiseOnError(recordId, null, FlowDefinition, errorState, null, ex, errorMsg);
            return errorState;
        }
    }

    /// <summary>
    /// 查找下一个节点：根据当前节点和操作类型查找匹配的边和目标节点。
    /// </summary>
    /// <param name="currentNode">当前节点。</param>
    /// <param name="action">操作类型。</param>
    /// <param name="state">流程运行时状态。</param>
    /// <param name="record">流程记录。</param>
    /// <returns>下一个节点，无匹配时返回null。</returns>
    private GraphNode? FindNextNode(GraphNode currentNode, string action, FlowRuntimeState state, FlowRecord record)
    {
        // 查找匹配的边：源节点为当前节点，边文本为操作类型（或无文本）
        var edge = FlowDefinition.Edges.FirstOrDefault(e =>
            e.SourceNodeId == currentNode.Id &&
            (string.IsNullOrEmpty(e.Text?.Value) || string.Equals(e.Text.Value, action, StringComparison.OrdinalIgnoreCase)));

        if (edge == null)
        {
            return null;
        }

        // 查找目标节点
        var targetNode = FlowDefinition.Nodes.FirstOrDefault(n => n.Id == edge.TargetNodeId);

        // 特殊处理网关节点：检查用户节点审批状态并重新查找边
        if (targetNode != null && string.Equals(targetNode.Type, ConstNodeType.GatewayNode, StringComparison.OrdinalIgnoreCase))
        {
            var userNodeStatus = CheckUserNodeApprovalStatus(currentNode, state, record);
            if (userNodeStatus != null)
            {
                // 根据用户节点审批状态重新查找网关边
                var gatewayEdge = FlowDefinition.Edges.FirstOrDefault(e =>
                    e.SourceNodeId == targetNode.Id &&
                    !string.IsNullOrEmpty(e.Text?.Value) &&
                    string.Equals(e.Text.Value, userNodeStatus, StringComparison.OrdinalIgnoreCase));

                if (gatewayEdge != null)
                {
                    return FlowDefinition.Nodes.FirstOrDefault(n => n.Id == gatewayEdge.TargetNodeId);
                }
            }
        }

        return targetNode;
    }

    /// <summary>
    /// 检查上一个服务节点的状态。
    /// </summary>
    /// <param name="recordId">流程记录ID。</param>
    /// <param name="state">流程运行时状态。</param>
    /// <returns>服务节点状态字符串。</returns>
    private Task<string?> CheckPreviousTaskNodeStatus(long recordId, FlowRuntimeState state)
    {
        // 从上下文中获取上一个服务节点状态
        var result = state.Context?["previousTaskStatus"] as string;
        return Task.FromResult(result);
    }

    /// <summary>
    /// 检查用户节点的审批状态。
    /// </summary>
    /// <param name="currentNode">当前节点。</param>
    /// <param name="state">流程运行时状态。</param>
    /// <param name="record">流程记录。</param>
    /// <returns>审批状态字符串。</returns>
    private string? CheckUserNodeApprovalStatus(GraphNode currentNode, FlowRuntimeState state, FlowRecord record)
    {
        // 从上下文中获取用户审批结果
        return state.Context?["userApprovalStatus"] as string;
    }

    /// <summary>
    /// 判断是否为自动推进节点。
    /// </summary>
    /// <param name="node">图节点。</param>
    /// <returns>是自动推进节点返回true，否则返回false。</returns>
    private bool IsAutoAdvanceNode(GraphNode node)
    {
        // 开始节点、服务节点、带自动推进属性的服务网关节点需要自动推进
        return string.Equals(node.Type, ConstNodeType.StartNode, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(node.Type, ConstNodeType.TaskNode, StringComparison.OrdinalIgnoreCase) ||
               (string.Equals(node.Type, ConstNodeType.TaskGatewayNode, StringComparison.OrdinalIgnoreCase) && node.Properties?.ContainsKey("autoAdvance") == true);
    }
}