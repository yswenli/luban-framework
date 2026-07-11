namespace LuBan.ApprovalFlow.Core.Handlers;

#nullable disable warnings

/// <summary>
/// HTTP节点处理器：负责执行HTTP回调请求，支持响应映射和成功条件判断。
/// 用于在审批流程中调用外部HTTP接口，并根据返回结果进行后续处理。
/// </summary>
public class HttpNodeHandler : BaseNodeHandler
{
    /// <summary>
    /// HTTP回调执行器，负责发送HTTP请求
    /// </summary>
    private readonly HttpCallbackExecutor _httpExecutor = new();

    /// <summary>
    /// 规则引擎，负责条件表达式计算
    /// </summary>
    private readonly RuleEngine _ruleEngine = new();

    /// <summary>
    /// 判断是否能处理指定节点
    /// </summary>
    /// <param name="node">图节点对象</param>
    /// <returns>如果节点类型为HTTP节点返回 true，否则返回 false</returns>
    public override bool CanHandle(GraphNode node)
        => string.Equals(node?.Type, ConstNodeType.HttpNode, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 处理HTTP节点：执行HTTP回调请求，映射响应结果到变量，并判断是否成功
    /// </summary>
    /// <param name="ctx">节点处理上下文，包含节点信息、流程状态、请求参数等</param>
    /// <returns>HTTP响应结果字典</returns>
    public override async Task<object?> HandleAsync(FlowNodeHandleContext ctx)
    {
        var node = ctx.Node;
        if (node == null || node.Properties == null) return null;

        var props = node.Properties;

        // 获取HTTP回调配置
        var callback = props.TryGetValue("callback", out var cb) ? cb as HttpCallbackConfig : null;
        if (callback == null) return null;

        // 构建执行上下文并发送HTTP请求
        var context = BuildExecutionContext(ctx);
        var response = await _httpExecutor.ExecuteAsync(callback, context);

        // 将HTTP响应映射到流程变量
        var mappings = props.TryGetValue("resultMapping", out var rm) ? rm as List<ResultMapping> : null;
        if (mappings != null && ctx.State.Context != null)
        {
            _httpExecutor.MapResponseToVariables(response, mappings, ctx.State.Context);
        }

        // 获取成功条件配置
        var successCondition = props.TryGetValue("successCondition", out var sc) ? sc as Dictionary<string, object> : null;
        var isSuccess = true;

        // 根据条件判断HTTP请求是否成功
        if (successCondition != null)
        {
            var conditions = successCondition.TryGetValue("conditions", out var c) ? c as List<RuleCondition> : null;
            var logic = successCondition.TryGetValue("logic", out var l) ? l as string : "and";

            if (conditions != null)
            {
                // 合并上下文变量和响应变量
                var mergedVars = new Dictionary<string, object>(context.Variables ?? new Dictionary<string, object>());
                if (response != null)
                {
                    foreach (var kvp in response)
                    {
                        mergedVars[$"response.{kvp.Key}"] = kvp.Value;
                    }
                }

                // 使用规则引擎评估条件
                isSuccess = _ruleEngine.EvaluateConditions(conditions, logic ?? "and", mergedVars);
            }
        }

        // 将执行结果保存到流程上下文
        if (ctx.State.Context != null)
        {
            ctx.State.Context["httpSuccess"] = isSuccess;
            ctx.State.Context["httpResponse"] = response;
        }

        return response;
    }

    /// <summary>
    /// 构建流程执行上下文，用于HTTP请求调用
    /// </summary>
    /// <param name="ctx">节点处理上下文</param>
    /// <returns>流程执行上下文对象</returns>
    private FlowExecutionContext BuildExecutionContext(FlowNodeHandleContext ctx)
    {
        return new FlowExecutionContext
        {
            RecordId = ctx.RecordId,
            NodeId = ctx.Node?.Id,
            NodeName = ctx.Node?.Text?.Value,
            NodeType = ctx.Node?.Type,
            Variables = ctx.State.Context,
            Action = ctx.Request?.Action,
            ActorUserId = ctx.Request?.ActorUserId,
            Comment = ctx.Request?.Comment,
            ActionTime = DateTime.Now
        };
    }
}