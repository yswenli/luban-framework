namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 流程执行上下文，包含流程执行过程中的完整状态信息。
/// </summary>
public class FlowExecutionContext
{
    /// <summary>
    /// 流程记录ID。
    /// </summary>
    public long RecordId { get; set; }
    /// <summary>
    /// 流程编码。
    /// </summary>
    public string? FlowKey { get; set; }
    /// <summary>
    /// 流程名称。
    /// </summary>
    public string? FlowName { get; set; }
    /// <summary>
    /// 业务主键。
    /// </summary>
    public string? BusinessKey { get; set; }
    /// <summary>
    /// 发起人用户ID。
    /// </summary>
    public long InitiatorUserId { get; set; }
    /// <summary>
    /// 发起人名称。
    /// </summary>
    public string? InitiatorName { get; set; }
    /// <summary>
    /// 当前节点ID。
    /// </summary>
    public string? NodeId { get; set; }
    /// <summary>
    /// 当前节点名称。
    /// </summary>
    public string? NodeName { get; set; }
    /// <summary>
    /// 节点状态。
    /// </summary>
    public string? NodeStatus { get; set; }
    /// <summary>
    /// 节点类型。
    /// </summary>
    public string? NodeType { get; set; }
    /// <summary>
    /// 操作人用户ID。
    /// </summary>
    public long? ActorUserId { get; set; }
    /// <summary>
    /// 操作人名称。
    /// </summary>
    public string? ActorName { get; set; }
    /// <summary>
    /// 操作人角色。
    /// </summary>
    public string? ActorRole { get; set; }
    /// <summary>
    /// 操作动作。
    /// </summary>
    public string? Action { get; set; }
    /// <summary>
    /// 审批意见。
    /// </summary>
    public string? Comment { get; set; }
    /// <summary>
    /// 流程变量字典。
    /// </summary>
    public Dictionary<string, object>? Variables { get; set; }
    /// <summary>
    /// 全局变量字典。
    /// </summary>
    public Dictionary<string, object>? GlobalVariables { get; set; }
    /// <summary>
    /// 响应数据。
    /// </summary>
    public Dictionary<string, object>? Response { get; set; }
    /// <summary>
    /// 操作时间。
    /// </summary>
    public DateTime? ActionTime { get; set; }
    /// <summary>
    /// 流程开始时间。
    /// </summary>
    public DateTime? StartedAt { get; set; }
    /// <summary>
    /// 流程完成时间。
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    /// <summary>
    /// 节点处理时长（秒）。
    /// </summary>
    public int? NodeDuration { get; set; }
    /// <summary>
    /// 错误信息。
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 根据键名获取上下文值。
    /// </summary>
    /// <param name="key">键名（不区分大小写）。</param>
    /// <returns>对应的值，未找到返回null。</returns>
    public object? GetValue(string key)
    {
        return key.ToLower() switch
        {
            "recordid" => RecordId,
            "flowkey" => FlowKey,
            "flowname" => FlowName,
            "businesskey" => BusinessKey,
            "initiatoruserid" => InitiatorUserId,
            "initiatorname" => InitiatorName,
            "nodeid" => NodeId,
            "nodename" => NodeName,
            "nodestatus" => NodeStatus,
            "nodetype" => NodeType,
            "actoruserid" => ActorUserId,
            "actorname" => ActorName,
            "actorrole" => ActorRole,
            "action" => Action,
            "comment" => Comment,
            "actiontime" => ActionTime?.ToString("yyyy-MM-dd HH:mm:ss"),
            "startedat" => StartedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
            "completedat" => CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
            "nodeduration" => NodeDuration,
            "errormessage" => ErrorMessage,
            "variables" => Variables,
            "payload" => Variables?.TryGetValue("payload", out var p) == true ? p : null,
            _ => Variables?.TryGetValue(key, out var v) == true ? v : null
        };
    }

    /// <summary>
    /// 获取嵌套属性值，支持多级路径访问。
    /// </summary>
    /// <param name="root">根对象名称。</param>
    /// <param name="path">属性路径数组。</param>
    /// <returns>嵌套属性值，未找到返回null。</returns>
    public object? GetNestedValue(string root, string[] path)
    {
        var rootValue = root.ToLower() switch
        {
            "variables" => Variables,
            "globalvariables" => GlobalVariables,
            "response" => Response,
            _ => GetValue(root)
        };

        if (rootValue == null) return null;

        object? current = rootValue;
        foreach (var p in path)
        {
            if (current is Dictionary<string, object> dict)
            {
                if (!dict.TryGetValue(p, out current))
                    return null;
            }
            else
            {
                var prop = current?.GetType().GetProperty(p);
                if (prop == null) return null;
                current = prop.GetValue(current);
            }
        }

        return current;
    }
}