namespace LuBan.ApprovalFlow.Core;

#nullable disable warnings

/// <summary>
/// 审批流程引擎：提供流程启动、审批处理、网关路由等核心功能。
/// 作为审批流程的主入口，协调各组件完成流程的完整生命周期管理。
/// </summary>
public class ApprovalFlowEngine
{
    private readonly FlowBuilder _builder;
    private readonly FlowEventListenerManager _listenerManager;
    private readonly AggregationEvaluator _aggregationEvaluator;
    private readonly RuleEngine _ruleEngine;
    private readonly HttpCallbackExecutor _httpExecutor;

    /// <summary>
    /// 初始化审批流程引擎实例。
    /// </summary>
    /// <param name="builder">流程构建器，用于创建流程执行器。</param>
    public ApprovalFlowEngine(FlowBuilder builder)
    {
        _builder = builder;
        _listenerManager = FlowEventListenerManager.Instance;
        _aggregationEvaluator = new AggregationEvaluator();
        _ruleEngine = new RuleEngine();
        _httpExecutor = new HttpCallbackExecutor();
    }

    /// <summary>
    /// 启动一个新的审批流程实例。
    /// </summary>
    /// <param name="request">启动请求，包含表单数据、变量和业务键等信息。</param>
    /// <param name="definition">流程定义，描述流程的节点和连线结构。</param>
    /// <param name="repository">审批记录存储库（可选），为空时使用内存存储。</param>
    /// <returns>启动响应，包含记录ID、状态和待办任务列表。</returns>
    public async Task<StartFlowResponse> StartFlowAsync(
        StartFlowRequest request,
        GraphFlowDefinition definition,
        IApprovalRepository? repository = null)
    {
        // 创建流程执行器
        var executor = _builder.Bind(definition);

        // 初始化流程运行时状态
        var state = new FlowRuntimeState
        {
            Status = ConstApprovalFlowStatus.Processing,
            CurrentNodeKey = FindStartNode(definition)?.Id,
            Variables = request.Variables ?? new Dictionary<string, object>(),
            Context = new Dictionary<string, object>(),
            History = new List<FlowStepResult>()
        };

        // 保存表单数据到上下文
        if (request.FormPayload != null)
        {
            state.Context!["payload"] = request.FormPayload;
        }

        // 序列化流程相关数据
        var formName = definition.Name;
        var formPayloadJson = request.FormPayload != null ? SerializeUtil.Serialize(request.FormPayload) : null;
        var flowJson = SerializeUtil.Serialize(definition);
        var flowResultJson = SerializeUtil.Serialize(state);

        // 创建流程记录
        var record = new FlowRecord
        {
            FormName = formName,
            FormStatus = ConstApprovalFlowStatus.NotStarted,
            FormJson = formPayloadJson,
            FlowJson = flowJson,
            FlowResult = flowResultJson
        };

        // 持久化流程记录
        long recordId = 0;
        if (repository != null)
        {
            recordId = await repository.CreateRecordAsync(record);
        }
        else
        {
            // 无存储库时使用内存监控器
            recordId = executor.Stats.TotalRecords + 1;
            FlowMonitor.Instance.AddRecord(definition.Key ?? string.Empty, record);
        }

        // 触发流程初始化事件
        var initArgs = new FlowInitializedEventArgs(recordId, definition, state, request.FormPayload as Dictionary<string, object>);
        await _listenerManager.TriggerEventAsync("OnInitialized", initArgs, definition.Key);

        // 执行流程初始化回调
        await ExecuteFlowEventCallbackAsync(definition, "onInitialized", state, recordId);

        // 处理开始节点进入事件
        var startNode = FindStartNode(definition);
        if (startNode != null)
        {
            var enterArgs = new NodeEnterEventArgs(
                recordId, startNode.Id, startNode.Text?.Value ?? "开始",
                startNode.Type, ConstNodeStatus.Processing, DateTime.Now, state.Context, request.BusinessKey);
            await _listenerManager.TriggerEventAsync("OnNodeEnter", enterArgs, definition.Key);
        }

        // 自动推进流程
        var newState = await executor.AutoAdvanceAsync(recordId, state);

        // 收集待办任务信息
        var pendingTasks = new List<PendingTaskInfo>();
        var currentNode = definition.Nodes.FirstOrDefault(n => n.Id == newState.CurrentNodeKey);
        if (currentNode != null && currentNode.Type == ConstNodeType.UserNode)
        {
            var assignees = GetAssignees(currentNode, newState);
            foreach (var assignee in assignees)
            {
                pendingTasks.Add(new PendingTaskInfo
                {
                    TaskId = 0,
                    RecordId = recordId,
                    FlowName = definition.Name,
                    NodeId = currentNode.Id,
                    NodeName = currentNode.Text?.Value ?? string.Empty,
                    NodeStatus = ConstNodeStatus.Pending,
                    BusinessKey = request.BusinessKey,
                    InitiatorName = request.InitiatorName,
                    CreatedAt = DateTime.Now,
                    FormData = request.FormPayload as Dictionary<string, object>
                });
            }
        }

        return new StartFlowResponse
        {
            RecordId = recordId,
            Status = newState.Status,
            CurrentNodeId = newState.CurrentNodeKey,
            CurrentNodeName = currentNode?.Text?.Value,
            PendingTasks = pendingTasks
        };
    }

    /// <summary>
    /// 执行审批操作。
    /// </summary>
    /// <param name="request">审批请求，包含记录ID、操作类型、审批人等信息。</param>
    /// <param name="definition">流程定义。</param>
    /// <param name="state">当前流程运行时状态。</param>
    /// <param name="repository">审批记录存储库（可选）。</param>
    /// <returns>审批响应，包含记录ID、状态和聚合结果。</returns>
    public async Task<ApprovalResponse> ApprovalAsync(
        ApprovalRequest request,
        GraphFlowDefinition definition,
        FlowRuntimeState state,
        IApprovalRepository? repository = null)
    {
        var executor = _builder.Bind(definition);

        // 获取当前节点
        var currentNode = definition.Nodes.FirstOrDefault(n => n.Id == state.CurrentNodeKey);
        if (currentNode == null)
        {
            return new ApprovalResponse
            {
                RecordId = request.RecordId,
                Status = state.Status
            };
        }

        // 判断是否为会签节点
        var isCountersign = IsCountersignNode(currentNode);
        var aggregationResult = AggregationResult.Pending;

        // 合并审批变量到上下文
        if (request.Variables != null && state.Context != null)
        {
            foreach (var kvp in request.Variables)
            {
                state.Context[kvp.Key] = kvp.Value;
            }
        }

        // 保存审批载荷到上下文
        if (request.Payload != null && state.Context != null)
        {
            state.Context["currentPayload"] = request.Payload;
        }

        // 记录审批步骤到历史
        var stepResult = new FlowStepResult
        {
            NodeId = currentNode.Id,
            NodeName = currentNode.Text?.Value ?? string.Empty,
            Outcome = request.Action,
            Time = DateTime.Now,
            ActorUserId = request.ActorUserId,
            Comment = request.Comment
        };
        state.History.Add(stepResult);

        // 触发审批操作事件
        var actionArgs = new ApprovalActionEventArgs(
            request.RecordId, currentNode.Id, currentNode.Text?.Value ?? string.Empty,
            request.ActorUserId, string.Empty, null, request.Action, request.Comment,
            request.Payload, DateTime.Now, state.Context, request.IsSystemAction);
        await _listenerManager.TriggerEventAsync("OnApprovalAction", actionArgs, definition.Key);

        // 执行审批操作HTTP回调（根据triggerActions过滤）
        await ExecuteNodeCallbackAsync(currentNode, "onApprovalAction", state, request.RecordId, request.Action);

        // 处理会签聚合逻辑
        if (isCountersign)
        {
            aggregationResult = await CheckAggregationAsync(currentNode, state, request.Action);
            if (aggregationResult == AggregationResult.Pending)
            {
                // 会签未完成，返回等待状态
                return new ApprovalResponse
                {
                    RecordId = request.RecordId,
                    Status = state.Status,
                    CurrentNodeId = state.CurrentNodeKey,
                    AggregationResult = aggregationResult.Value
                };
            }
        }

        // 触发节点离开事件
        var leaveArgs = new NodeLeaveEventArgs(
            request.RecordId, currentNode.Id, currentNode.Text?.Value ?? string.Empty,
            currentNode.Type, MapActionToStatus(request.Action), DateTime.Now,
            null, state.Context);
        await _listenerManager.TriggerEventAsync("OnNodeLeave", leaveArgs, definition.Key);

        // 执行节点后置回调（离开事件）
        await ExecuteNodeCallbackAsync(currentNode, "onNodeLeave", state, request.RecordId);

        // 构建推进请求
        var advanceRequest = new AdvanceRequest
        {
            RecordId = request.RecordId,
            Action = isCountersign ? MapAggregationToAction(aggregationResult) : request.Action,
            Comment = request.Comment,
            ActorUserId = request.ActorUserId,
            ActorRoles = request.ActorRoles
        };

        // 推进流程
        var newState = await executor.AdvanceAsync(advanceRequest);

        // 检查流程是否结束
        if (newState.CurrentNodeKey == null)
        {
            await CompleteFlowAsync(request.RecordId, definition, newState, request);
        }
        else
        {
            // 处理下一个节点
            var nextNode = definition.Nodes.FirstOrDefault(n => n.Id == newState.CurrentNodeKey);
            if (nextNode != null)
            {
                if (nextNode.Type == ConstNodeType.GatewayNode)
                {
                    // 处理网关节点
                    await ProcessGatewayAsync(nextNode, newState, definition, request.RecordId);
                }
                else if (nextNode.Type == ConstNodeType.HttpNode)
                {
                    // 处理HTTP回调节点
                    await ProcessHttpNodeAsync(nextNode, newState, definition, request.RecordId);
                }
                else
                {
                    // 处理普通节点
                    var enterArgs = new NodeEnterEventArgs(
                        request.RecordId, nextNode.Id, nextNode.Text?.Value ?? string.Empty,
                        nextNode.Type, ConstNodeStatus.Pending, DateTime.Now, state.Context);
                    await _listenerManager.TriggerEventAsync("OnNodeEnter", enterArgs, definition.Key);

                    // 执行节点前置回调（进入事件）
                    await ExecuteNodeCallbackAsync(nextNode, "onNodeEnter", newState, request.RecordId);
                }
            }
        }

        return new ApprovalResponse
        {
            RecordId = request.RecordId,
            Status = newState.Status,
            CurrentNodeId = newState.CurrentNodeKey,
            AggregationResult = aggregationResult.Value
        };
    }

    /// <summary>
    /// 查找流程定义中的开始节点。
    /// </summary>
    /// <param name="definition">流程定义。</param>
    /// <returns>开始节点，未找到则返回null。</returns>
    private GraphNode? FindStartNode(GraphFlowDefinition definition)
    {
        return definition.Nodes.FirstOrDefault(n => n.Type == ConstNodeType.StartNode);
    }

    /// <summary>
    /// 判断节点是否为会签节点。
    /// </summary>
    /// <param name="node">图节点。</param>
    /// <returns>是会签节点返回true，否则返回false。</returns>
    private bool IsCountersignNode(GraphNode node)
    {
        if (node.Properties == null) return false;
        return node.Properties.TryGetValue("multiApproval", out var multi) && multi is bool m && m;
    }

    /// <summary>
    /// 检查会签聚合结果。
    /// </summary>
    /// <param name="node">当前节点。</param>
    /// <param name="state">流程运行时状态。</param>
    /// <param name="action">当前操作类型。</param>
    /// <returns>聚合结果：已通过、已拒绝或等待中。</returns>
    private async Task<AggregationResult> CheckAggregationAsync(
        GraphNode node,
        FlowRuntimeState state,
        string action)
    {
        // 获取会签配置
        var props = node.Properties ?? new Dictionary<string, object>();
        var aggregationType = props.TryGetValue("aggregationType", out var at) ? at as string : ConstAggregationType.AllApprove;
        var approvePercentage = props.TryGetValue("approvePercentage", out var ap) ? ap as int? : 60;

        // 获取当前统计
        int approvedCount = 0;
        int rejectedCount = 0;
        int totalCount = 0;

        if (state.Context != null)
        {
            if (state.Context.TryGetValue("approvedCount", out var ac)) approvedCount = Convert.ToInt32(ac);
            if (state.Context.TryGetValue("rejectedCount", out var rc)) rejectedCount = Convert.ToInt32(rc);
            if (state.Context.TryGetValue("totalCount", out var tc)) totalCount = Convert.ToInt32(tc);
        }

        // 根据操作更新计数
        switch (action)
        {
            case ConstActionType.Approve:
                approvedCount++;
                break;
            case ConstActionType.Reject:
                rejectedCount++;
                break;
        }

        // 保存更新后的计数
        if (state.Context != null)
        {
            state.Context["approvedCount"] = approvedCount;
            state.Context["rejectedCount"] = rejectedCount;
        }

        await Task.CompletedTask;

        return _aggregationEvaluator.Evaluate(aggregationType ?? ConstAggregationType.AllApprove, approvedCount, rejectedCount, totalCount, approvePercentage);
    }

    /// <summary>
    /// 处理网关节点：根据规则条件决定流转路径。
    /// </summary>
    /// <param name="gatewayNode">网关节点。</param>
    /// <param name="state">流程运行时状态。</param>
    /// <param name="definition">流程定义。</param>
    /// <param name="recordId">流程记录ID。</param>
    private async Task ProcessGatewayAsync(
        GraphNode gatewayNode,
        FlowRuntimeState state,
        GraphFlowDefinition definition,
        long recordId)
    {
        // 获取网关规则配置
        var props = gatewayNode.Properties ?? new Dictionary<string, object>();
        var rules = props.TryGetValue("rules", out var r) ? r as List<GatewayRule> : null;
        var defaultEdgeId = props.TryGetValue("defaultEdgeId", out var de) ? de as string : null;

        // 评估规则
        var result = _ruleEngine.Evaluate(rules, state.Context, defaultEdgeId);

        // 保存网关结果到上下文
        if (state.Context != null)
        {
            state.Context["gatewayResult"] = result.EdgeText ?? "default";
        }

        // 触发节点进入事件
        var enterArgs = new NodeEnterEventArgs(
            recordId, gatewayNode.Id, gatewayNode.Text?.Value ?? string.Empty,
            gatewayNode.Type, ConstNodeStatus.Processing, DateTime.Now, state.Context);
        await _listenerManager.TriggerEventAsync("OnNodeEnter", enterArgs, definition.Key);

        // 触发节点离开事件
        var leaveArgs = new NodeLeaveEventArgs(
            recordId, gatewayNode.Id, gatewayNode.Text?.Value ?? string.Empty,
            gatewayNode.Type, ConstNodeStatus.Approved, DateTime.Now, null, state.Context);
        await _listenerManager.TriggerEventAsync("OnNodeLeave", leaveArgs, definition.Key);

        // 更新当前节点为目标节点
        if (result.EdgeId != null)
        {
            var targetNode = definition.Nodes.FirstOrDefault(n => n.Id == result.EdgeId);
            if (targetNode != null)
            {
                state.CurrentNodeKey = targetNode.Id;
            }
        }
    }

    /// <summary>
    /// 处理HTTP回调节点：执行HTTP请求并根据响应更新流程变量。
    /// </summary>
    /// <param name="httpNode">HTTP节点。</param>
    /// <param name="state">流程运行时状态。</param>
    /// <param name="definition">流程定义。</param>
    /// <param name="recordId">流程记录ID。</param>
    private async Task ProcessHttpNodeAsync(
        GraphNode httpNode,
        FlowRuntimeState state,
        GraphFlowDefinition definition,
        long recordId)
    {
        // 获取HTTP回调配置
        var props = httpNode.Properties ?? new Dictionary<string, object>();
        var callback = props.TryGetValue("callback", out var cb) ? cb as HttpCallbackConfig : null;

        if (callback != null)
        {
            // 构建执行上下文
            var context = new FlowExecutionContext
            {
                RecordId = recordId,
                NodeId = httpNode.Id,
                NodeName = httpNode.Text?.Value,
                NodeType = httpNode.Type,
                Variables = state.Context
            };

            // 执行HTTP请求
            var response = await _httpExecutor.ExecuteAsync(callback, context);

            // 映射响应结果到变量
            var mappings = props.TryGetValue("resultMapping", out var rm) ? rm as List<ResultMapping> : null;
            if (mappings != null && state.Context != null)
            {
                _httpExecutor.MapResponseToVariables(response, mappings, state.Context);
            }

            // 检查成功条件
            var successCondition = props.TryGetValue("successCondition", out var sc) ? sc as Dictionary<string, object> : null;
            var isSuccess = true;

            if (successCondition != null && response != null)
            {
                // 合并响应变量到上下文
                var mergedVars = new Dictionary<string, object>(state.Context ?? new Dictionary<string, object>());
                foreach (var kvp in response)
                {
                    mergedVars[$"response.{kvp.Key}"] = kvp.Value;
                }

                var conditions = successCondition.TryGetValue("conditions", out var c) ? c as List<RuleCondition> : null;
                var logic = successCondition.TryGetValue("logic", out var l) ? l as string : "and";

                if (conditions != null)
                {
                    isSuccess = _ruleEngine.EvaluateConditions(conditions, logic ?? "and", mergedVars);
                }
            }

            // 保存成功状态到上下文
            if (state.Context != null)
            {
                state.Context["httpSuccess"] = isSuccess;
            }
        }

        // 触发节点进入事件
        var enterArgs = new NodeEnterEventArgs(
            recordId, httpNode.Id, httpNode.Text?.Value ?? string.Empty,
            httpNode.Type, ConstNodeStatus.Processing, DateTime.Now, state.Context);
        await _listenerManager.TriggerEventAsync("OnNodeEnter", enterArgs, definition.Key);

        // 触发节点离开事件
        var leaveArgs = new NodeLeaveEventArgs(
            recordId, httpNode.Id, httpNode.Text?.Value ?? string.Empty,
            httpNode.Type, ConstNodeStatus.Approved, DateTime.Now, null, state.Context);
        await _listenerManager.TriggerEventAsync("OnNodeLeave", leaveArgs, definition.Key);
    }

    /// <summary>
    /// 完成流程：触发流程完成或拒绝事件。
    /// </summary>
    /// <param name="recordId">流程记录ID。</param>
    /// <param name="definition">流程定义。</param>
    /// <param name="state">流程运行时状态。</param>
    /// <param name="request">审批请求（可选）。</param>
    private async Task CompleteFlowAsync(
        long recordId,
        GraphFlowDefinition definition,
        FlowRuntimeState state,
        ApprovalRequest? request)
    {
        var finalStatus = state.Status;

        if (finalStatus == ConstApprovalFlowStatus.Completed)
        {
            // 触发流程完成事件
            var completedArgs = new FlowCompletedEventArgs(
                recordId, definition.Key, definition.Name,
                request?.ActorUserId ?? 0, string.Empty, null,
                state.Context, DateTime.Now, null, DateTime.Now);
            await _listenerManager.TriggerEventAsync("OnCompleted", completedArgs, definition.Key);

            // 执行流程完成回调
            await ExecuteFlowEventCallbackAsync(definition, "onCompleted", state, recordId);
        }
        else
        {
            // 触发流程拒绝事件
            var rejectedArgs = new FlowRejectedEventArgs(
                recordId, definition.Key, definition.Name,
                request?.Comment, request?.ActorUserId, string.Empty,
                DateTime.Now, null, state.Context);
            await _listenerManager.TriggerEventAsync("OnRejected", rejectedArgs, definition.Key);

            // 执行流程拒绝回调
            await ExecuteFlowEventCallbackAsync(definition, "onRejected", state, recordId);
        }
    }

    /// <summary>
    /// 执行流程事件回调。
    /// </summary>
    /// <param name="definition">流程定义。</param>
    /// <param name="eventKey">事件键名。</param>
    /// <param name="state">流程运行时状态。</param>
    /// <param name="recordId">流程记录ID。</param>
    private async Task ExecuteFlowEventCallbackAsync(
        GraphFlowDefinition definition,
        string eventKey,
        FlowRuntimeState state,
        long recordId)
    {
        var events = definition.Events ?? new Dictionary<string, object>();
        if (events.TryGetValue(eventKey, out var callbackObj) && callbackObj is HttpCallbackConfig callback)
        {
            var context = new FlowExecutionContext
            {
                RecordId = recordId,
                FlowKey = definition.Key,
                FlowName = definition.Name,
                Variables = state.Context
            };

            await _httpExecutor.ExecuteAsync(callback, context);
        }
    }

/// <summary>
/// 执行节点回调。
/// </summary>
/// <param name="node">图节点。</param>
/// <param name="eventKey">事件键名。</param>
/// <param name="state">流程运行时状态。</param>
/// <param name="recordId">流程记录ID。</param>
/// <param name="action">当前操作动作（用于onApprovalAction过滤）。</param>
private async Task ExecuteNodeCallbackAsync(
    GraphNode node,
    string eventKey,
    FlowRuntimeState state,
    long recordId,
    string? action = null)
{
    if (node.Properties == null) return;

    // 从 events 配置中读取事件回调列表（新格式：数组）
    if (node.Properties.TryGetValue("events", out var eventsObj) && eventsObj is Dictionary<string, object> events)
    {
        if (events.TryGetValue(eventKey, out var callbacksObj))
        {
            var callbacks = new List<HttpCallbackConfig>();

            // 兼容旧格式：单个对象
            if (callbacksObj is HttpCallbackConfig single)
            {
                callbacks.Add(single);
            }
            // 新格式：数组
            else if (callbacksObj is List<object> list)
            {
                foreach (var item in list)
                {
                    if (item is HttpCallbackConfig cfg)
                    {
                        callbacks.Add(cfg);
                    }
                }
            }

            var context = new FlowExecutionContext
            {
                RecordId = recordId,
                NodeId = node.Id,
                NodeName = node.Text?.Value,
                NodeType = node.Type,
                Variables = state.Context,
                Action = action
            };

            foreach (var callback in callbacks)
            {
                // 对于onApprovalAction事件，根据triggerActions过滤
                if (eventKey == "onApprovalAction" && !string.IsNullOrEmpty(action))
                {
                    // triggerActions为空或null时表示所有动作都触发
                    if (callback.TriggerActions != null && callback.TriggerActions.Count > 0)
                    {
                        // 只触发匹配当前action的回调
                        if (!callback.TriggerActions.Contains(action))
                        {
                            continue;
                        }
                    }
                }

                await _httpExecutor.ExecuteAsync(callback, context);
            }
        }
    }
}

    /// <summary>
    /// 获取节点的审批人列表。
    /// </summary>
    /// <param name="node">图节点。</param>
    /// <param name="state">流程运行时状态。</param>
    /// <returns>审批人信息列表。</returns>
    private List<AssigneeInfo> GetAssignees(GraphNode node, FlowRuntimeState state)
    {
        var result = new List<AssigneeInfo>();

        if (node.Properties == null) return result;

        // 优先使用审批人列表配置
        if (node.Properties.TryGetValue("assignees", out var assigneesObj) && assigneesObj is List<AssigneeInfo> assignees)
        {
            result.AddRange(assignees);
        }
        // 否则使用单用户配置
        else if (node.Properties.TryGetValue("userId", out var uid) && node.Properties.TryGetValue("userName", out var uname))
        {
            result.Add(new AssigneeInfo
            {
                UserId = Convert.ToInt64(uid),
                UserName = uname?.ToString() ?? string.Empty,
                Role = node.Properties.TryGetValue("role", out var role) ? role?.ToString() : null
            });
        }

        return result;
    }

    /// <summary>
    /// 将操作类型映射为节点状态。
    /// </summary>
    /// <param name="action">操作类型。</param>
    /// <returns>节点状态字符串。</returns>
    private string MapActionToStatus(string action)
    {
        return action switch
        {
            ConstActionType.Approve => ConstNodeStatus.Approved,
            ConstActionType.Reject => ConstNodeStatus.Rejected,
            ConstActionType.Return => ConstNodeStatus.Returned,
            ConstActionType.Cancel => ConstNodeStatus.Cancelled,
            _ => ConstNodeStatus.Processing
        };
    }

    /// <summary>
    /// 将聚合结果映射为操作类型。
    /// </summary>
    /// <param name="result">聚合结果。</param>
    /// <returns>操作类型字符串。</returns>
    private string MapAggregationToAction(AggregationResult result)
    {
        return result.Value switch
        {
            "approved" => ConstActionType.Approve,
            "rejected" => ConstActionType.Reject,
            _ => ConstActionType.Approve
        };
    }
}