namespace LuBan.ApprovalFlow.Consts;

/// <summary>
/// 节点类型常量，用于定义流程节点的类型
/// </summary>
public class ConstNodeType
{
    /// <summary>
    /// 开始节点，流程入口
    /// </summary>
    public const string StartNode = "start-node";

    /// <summary>
    /// 服务节点，用于自动执行任务
    /// </summary>
    public const string TaskNode = "service-node";

    /// <summary>
    /// 服务网关节点，用于服务分支聚合
    /// </summary>
    public const string TaskGatewayNode = "service-gateway-node";

    /// <summary>
    /// 用户节点，指定具体处理人
    /// </summary>
    public const string UserNode = "user-node";

    /// <summary>
    /// 审批网关节点，用于条件分支
    /// </summary>
    public const string GatewayNode = "gateway-node";

    /// <summary>
    /// 结束节点，流程出口
    /// </summary>
    public const string EndNode = "end-node";

    /// <summary>
    /// HTTP节点，用于调用外部接口
    /// </summary>
    public const string HttpNode = "http-node";
}
