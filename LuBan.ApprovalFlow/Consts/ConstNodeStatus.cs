/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Consts
*文件名： ConstNodeStatus.cs
*版本号： V1.0.0.0
*唯一标识：4e08b07c-f7f3-457a-898d-640936eaf983
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ConstNodeStatus 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ConstNodeStatus 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Consts;

/// <summary>
/// 节点状态常量，用于定义流程节点的状态
/// </summary>
public class ConstNodeStatus
{
    /// <summary>
    /// 未开始
    /// </summary>
    public const string NotStarted = "未开始";

    /// <summary>
    /// 待处理
    /// </summary>
    public const string Pending = "待处理";

    /// <summary>
    /// 处理中
    /// </summary>
    public const string Processing = "处理中";

    /// <summary>
    /// 已审批通过
    /// </summary>
    public const string Approved = "已审批";

    /// <summary>
    /// 已拒绝
    /// </summary>
    public const string Rejected = "已拒绝";

    /// <summary>
    /// 已退回
    /// </summary>
    public const string Returned = "已退回";

    /// <summary>
    /// 已取消
    /// </summary>
    public const string Cancelled = "已取消";

    /// <summary>
    /// 已跳过
    /// </summary>
    public const string Skipped = "已跳过";

    /// <summary>
    /// 已撤回
    /// </summary>
    public const string Withdrawn = "已撤回";
}