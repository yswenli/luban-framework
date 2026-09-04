/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Consts
*文件名： ConstNodeType.cs
*版本号： V1.0.0.0
*唯一标识：5a58548b-edb3-4d38-b079-0be6a1a4a1b3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ConstNodeType 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ConstNodeType 类
*
*****************************************************************************/

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
