namespace LuBan.ApprovalFlow.Abstractions;

/// <summary>
/// 审批流程数据仓储接口，提供流程定义、记录、任务等数据的持久化操作
/// </summary>
public interface IApprovalRepository
{
    /// <summary>
    /// 根据ID获取流程定义
    /// </summary>
    /// <param name="id">流程定义ID</param>
    /// <returns>流程定义对象，不存在则返回 null</returns>
    Task<GraphFlowDefinition?> GetFlowDefinitionByIdAsync(long id);

    /// <summary>
    /// 根据编码获取流程定义
    /// </summary>
    /// <param name="code">流程定义编码</param>
    /// <returns>流程定义对象，不存在则返回 null</returns>
    Task<GraphFlowDefinition?> GetFlowDefinitionByCodeAsync(string code);

    /// <summary>
    /// 获取指定编码的当前版本流程定义
    /// </summary>
    /// <param name="code">流程定义编码</param>
    /// <returns>当前版本的流程定义对象，不存在则返回 null</returns>
    Task<GraphFlowDefinition?> GetCurrentVersionAsync(string code);

    /// <summary>
    /// 获取指定编码的所有版本列表
    /// </summary>
    /// <param name="code">流程定义编码</param>
    /// <returns>版本信息列表</returns>
    Task<List<VersionInfo>> GetVersionListAsync(string code);

    /// <summary>
    /// 创建流程记录
    /// </summary>
    /// <param name="record">流程记录对象</param>
    /// <returns>新建记录的ID</returns>
    Task<long> CreateRecordAsync(FlowRecord record);

    /// <summary>
    /// 更新流程记录
    /// </summary>
    /// <param name="record">流程记录对象</param>
    Task UpdateRecordAsync(FlowRecord record);

    /// <summary>
    /// 根据ID获取流程记录
    /// </summary>
    /// <param name="id">流程记录ID</param>
    /// <returns>流程记录对象，不存在则返回 null</returns>
    Task<FlowRecord?> GetRecordByIdAsync(long id);

    /// <summary>
    /// 创建节点记录
    /// </summary>
    /// <param name="nodeRecord">节点记录信息</param>
    /// <returns>新建节点记录的ID</returns>
    Task<long> CreateNodeRecordAsync(NodeRecordInfo nodeRecord);

    /// <summary>
    /// 更新节点记录
    /// </summary>
    /// <param name="nodeRecord">节点记录信息</param>
    Task UpdateNodeRecordAsync(NodeRecordInfo nodeRecord);

    /// <summary>
    /// 获取指定流程记录和节点ID的节点记录
    /// </summary>
    /// <param name="recordId">流程记录ID</param>
    /// <param name="nodeId">节点ID</param>
    /// <returns>节点记录信息，不存在则返回 null</returns>
    Task<NodeRecordInfo?> GetNodeRecordAsync(long recordId, string nodeId);

    /// <summary>
    /// 获取指定流程记录的所有节点记录
    /// </summary>
    /// <param name="recordId">流程记录ID</param>
    /// <returns>节点记录列表</returns>
    Task<List<NodeRecordInfo>> GetAllNodeRecordsAsync(long recordId);

    /// <summary>
    /// 创建审批步骤
    /// </summary>
    /// <param name="step">步骤信息</param>
    /// <param name="nodeRecordId">节点记录ID</param>
    /// <returns>新建步骤的ID</returns>
    Task<long> CreateStepAsync(StepInfo step, long nodeRecordId);

    /// <summary>
    /// 获取指定节点的所有审批步骤
    /// </summary>
    /// <param name="recordId">流程记录ID</param>
    /// <param name="nodeId">节点ID</param>
    /// <returns>步骤信息列表</returns>
    Task<List<StepInfo>> GetNodeStepsAsync(long recordId, string nodeId);

    /// <summary>
    /// 创建待办任务
    /// </summary>
    /// <param name="task">待办任务信息</param>
    /// <returns>新建任务的ID</returns>
    Task<long> CreateTaskAsync(PendingTaskInfo task);

    /// <summary>
    /// 更新待办任务
    /// </summary>
    /// <param name="task">待办任务信息</param>
    Task UpdateTaskAsync(PendingTaskInfo task);

    /// <summary>
    /// 根据ID获取待办任务
    /// </summary>
    /// <param name="taskId">任务ID</param>
    /// <returns>待办任务信息，不存在则返回 null</returns>
    Task<PendingTaskInfo?> GetTaskByIdAsync(long taskId);

    /// <summary>
    /// 获取指定流程记录的所有待办任务
    /// </summary>
    /// <param name="recordId">流程记录ID</param>
    /// <returns>待办任务列表</returns>
    Task<List<PendingTaskInfo>> GetTasksByRecordAsync(long recordId);

    /// <summary>
    /// 获取指定用户的待处理任务列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="query">查询条件，可选</param>
    /// <returns>待处理任务列表</returns>
    Task<List<PendingTaskInfo>> GetPendingTasksByUserAsync(long userId, TaskQueryRequest? query = null);

    /// <summary>
    /// 获取指定用户的已处理任务列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="query">查询条件，可选</param>
    /// <returns>已处理任务列表</returns>
    Task<List<PendingTaskInfo>> GetCompletedTasksByUserAsync(long userId, TaskQueryRequest? query = null);

    /// <summary>
    /// 创建统计信息
    /// </summary>
    /// <param name="stats">统计信息对象</param>
    Task CreateStatisticsAsync(StatisticsInfo stats);

    /// <summary>
    /// 更新统计信息
    /// </summary>
    /// <param name="stats">统计信息对象</param>
    Task UpdateStatisticsAsync(StatisticsInfo stats);

    /// <summary>
    /// 获取指定用户的统计信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>统计信息对象，不存在则返回 null</returns>
    Task<StatisticsInfo?> GetStatisticsByUserAsync(long userId);

    /// <summary>
    /// 创建委托配置
    /// </summary>
    /// <param name="delegation">委托配置对象</param>
    /// <returns>新建委托配置的ID</returns>
    Task<long> CreateDelegationAsync(DelegateConfig delegation);

    /// <summary>
    /// 更新委托配置
    /// </summary>
    /// <param name="delegation">委托配置对象</param>
    Task UpdateDelegationAsync(DelegateConfig delegation);

    /// <summary>
    /// 获取指定用户的当前有效委托配置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>有效的委托配置，不存在则返回 null</returns>
    Task<DelegateConfig?> GetActiveDelegationAsync(long userId);

    /// <summary>
    /// 获取指定用户的所有委托配置列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>委托配置列表</returns>
    Task<List<DelegateConfig>> GetDelegationsByUserAsync(long userId);
}

/// <summary>
/// 节点记录信息，记录流程中单个节点的执行状态和结果
/// </summary>
public class NodeRecordInfo
{
    /// <summary>
    /// 节点记录ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 所属流程记录ID
    /// </summary>
    public long RecordId { get; set; }

    /// <summary>
    /// 节点ID
    /// </summary>
    public string NodeId { get; set; } = string.Empty;

    /// <summary>
    /// 节点名称
    /// </summary>
    public string NodeName { get; set; } = string.Empty;

    /// <summary>
    /// 节点类型
    /// </summary>
    public string NodeType { get; set; } = string.Empty;

    /// <summary>
    /// 节点状态
    /// </summary>
    public string NodeStatus { get; set; } = ConstNodeStatus.NotStarted;

    /// <summary>
    /// 节点X坐标，用于流程图显示
    /// </summary>
    public double? NodeX { get; set; }

    /// <summary>
    /// 节点Y坐标，用于流程图显示
    /// </summary>
    public double? NodeY { get; set; }

    /// <summary>
    /// 节点开始处理时间
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// 节点处理完成时间
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// 处理耗时（分钟）
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// 会签聚合类型
    /// </summary>
    public string? AggregationType { get; set; }

    /// <summary>
    /// 已通过数量
    /// </summary>
    public int ApprovedCount { get; set; }

    /// <summary>
    /// 已驳回数量
    /// </summary>
    public int RejectedCount { get; set; }

    /// <summary>
    /// 已退回数量
    /// </summary>
    public int ReturnedCount { get; set; }

    /// <summary>
    /// 已取消数量
    /// </summary>
    public int CancelledCount { get; set; }

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 会签聚合结果
    /// </summary>
    public string? AggregationResult { get; set; }

    /// <summary>
    /// 节点扩展属性
    /// </summary>
    public Dictionary<string, object>? NodeProperties { get; set; }

    /// <summary>
    /// 路由结果
    /// </summary>
    public string? RouteResult { get; set; }

    /// <summary>
    /// 条件匹配结果
    /// </summary>
    public string? ConditionMatched { get; set; }

    /// <summary>
    /// 响应数据
    /// </summary>
    public string? ResponseData { get; set; }

    /// <summary>
    /// 已通过的用户列表
    /// </summary>
    public List<AssigneeInfo>? ApprovedUsers { get; set; }

    /// <summary>
    /// 已驳回的用户列表
    /// </summary>
    public List<AssigneeInfo>? RejectedUsers { get; set; }

    /// <summary>
    /// 已退回的用户列表
    /// </summary>
    public List<AssigneeInfo>? ReturnedUsers { get; set; }

    /// <summary>
    /// 已取消的用户列表
    /// </summary>
    public List<AssigneeInfo>? CancelledUsers { get; set; }

    /// <summary>
    /// 待处理的用户列表
    /// </summary>
    public List<AssigneeInfo>? PendingUsers { get; set; }

    /// <summary>
    /// 处理中的用户列表
    /// </summary>
    public List<AssigneeInfo>? ProcessingUsers { get; set; }
}