namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 审批流初始化事件参数：描述流程记录创建与初始状态。
/// </summary>
public class FlowInitializedEventArgs : EventArgs
{
    /// <summary>
    /// 流程记录主键ID。
    /// </summary>
    public long RecordId { get; }
    /// <summary>
    /// 绑定的流程定义（图式结构）。
    /// </summary>
    public GraphFlowDefinition Definition { get; }
    /// <summary>
    /// 初始运行时状态（包含当前节点与上下文）。
    /// </summary>
    public FlowRuntimeState State { get; }
    /// <summary>
    /// 表单数据载荷（触发或初始化提交的字段）。
    /// </summary>
    public Dictionary<string, object>? FormPayload { get; }

    /// <summary>
    /// 构造函数：初始化事件参数。
    /// </summary>
    /// <param name="recordId">流程记录主键ID。</param>
    /// <param name="definition">流程定义（图式结构）。</param>
    /// <param name="state">初始运行时状态。</param>
    /// <param name="formPayload">表单数据载荷（可为空）。</param>
    public FlowInitializedEventArgs(long recordId, GraphFlowDefinition definition, FlowRuntimeState state, Dictionary<string, object>? formPayload)
    {
        RecordId = recordId;
        Definition = definition;
        State = state;
        FormPayload = formPayload;
    }
}

/// <summary>
/// 节点处理完成事件参数：描述某节点被处理后的相关信息。
/// </summary>
public class NodeHandledEventArgs : EventArgs
{
    /// <summary>
    /// 流程记录主键ID。
    /// </summary>
    public long RecordId { get; }
    /// <summary>
    /// 被处理的图节点。
    /// </summary>
    public GraphNode Node { get; }
    /// <summary>
    /// 绑定的流程定义（图式结构）。
    /// </summary>
    public GraphFlowDefinition Definition { get; }
    /// <summary>
    /// 处理时的运行时状态。
    /// </summary>
    public FlowRuntimeState State { get; }
    /// <summary>
    /// 推进请求（含动作、操作者信息等）。
    /// </summary>
    public AdvanceRequest? Request { get; }
    /// <summary>
    /// 节点处理结果（可能为业务方法的返回值）。
    /// </summary>
    public object? Result { get; }

    /// <summary>
    /// 构造函数：初始化事件参数。
    /// </summary>
    /// <param name="recordId">流程记录主键ID。</param>
    /// <param name="node">被处理的图节点。</param>
    /// <param name="definition">流程定义。</param>
    /// <param name="state">运行时状态。</param>
    /// <param name="request">推进请求（可为空）。</param>
    /// <param name="result">节点处理结果（可为空）。</param>
    public NodeHandledEventArgs(long recordId, GraphNode node, GraphFlowDefinition definition, FlowRuntimeState state, AdvanceRequest? request, object? result)
    {
        RecordId = recordId;
        Node = node;
        Definition = definition;
        State = state;
        Request = request;
        Result = result;
    }
}

/// <summary>
/// 流程退出事件参数：描述流程达到结束或被拒绝等退出状态。
/// </summary>
public class FlowExitedEventArgs : EventArgs
{
    /// <summary>
    /// 流程记录主键ID。
    /// </summary>
    public long RecordId { get; }
    /// <summary>
    /// 绑定的流程定义（图式结构）。
    /// </summary>
    public GraphFlowDefinition Definition { get; }
    /// <summary>
    /// 最终运行时状态（包含历史与上下文）。
    /// </summary>
    public FlowRuntimeState FinalState { get; }
    /// <summary>
    /// 最终状态字符串（如 finished、rejected）。
    /// </summary>
    public string Status => FinalState.Status ?? string.Empty;

    /// <summary>
    /// 构造函数：初始化事件参数。
    /// </summary>
    /// <param name="recordId">流程记录主键ID。</param>
    /// <param name="definition">流程定义。</param>
    /// <param name="finalState">最终运行时状态。</param>
    public FlowExitedEventArgs(long recordId, GraphFlowDefinition definition, FlowRuntimeState finalState)
    {
        RecordId = recordId;
        Definition = definition;
        FinalState = finalState;
    }
}

/// <summary>
/// 流程错误事件参数：当执行流程或节点处理出现异常时触发。
/// </summary>
public class FlowErrorEventArgs : EventArgs
{
    /// <summary>
    /// 流程记录主键ID（若尚未创建，可能为 0）。
    /// </summary>
    public long RecordId { get; }
    /// <summary>
    /// 发生错误时关联的节点（可能为空）。
    /// </summary>
    public GraphNode? Node { get; }
    /// <summary>
    /// 当前流程定义。
    /// </summary>
    public GraphFlowDefinition Definition { get; }
    /// <summary>
    /// 发生错误时的运行时状态（可能为过程状态）。
    /// </summary>
    public FlowRuntimeState? State { get; }
    /// <summary>
    /// 推进行为的请求（可能为空）。
    /// </summary>
    public AdvanceRequest? Request { get; }
    /// <summary>
    /// 捕获的异常对象。
    /// </summary>
    public Exception Exception { get; }
    /// <summary>
    /// 错误消息（可用于友好展示）。
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// 构造函数：初始化错误事件参数。
    /// </summary>
    /// <param name="recordId">流程记录主键ID。</param>
    /// <param name="node">关联的流程节点（可为空）。</param>
    /// <param name="definition">流程定义。</param>
    /// <param name="state">运行时状态（可为空）。</param>
    /// <param name="request">推进请求（可为空）。</param>
    /// <param name="exception">捕获的异常对象。</param>
    /// <param name="message">友好错误消息。</param>
    public FlowErrorEventArgs(long recordId, GraphNode? node, GraphFlowDefinition definition, FlowRuntimeState? state, AdvanceRequest? request, Exception exception, string? message = null)
    {
        RecordId = recordId;
        Node = node;
        Definition = definition;
        State = state;
        Request = request;
        Exception = exception;
        Message = message ?? exception.Message;
    }
}