/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Consts
*文件名： ConstActionType.cs
*版本号： V1.0.0.0
*唯一标识：3d9fe682-3531-402d-8b53-93f1fb956221
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ConstActionType 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ConstActionType 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Consts;

/// <summary>
/// 操作类型常量，用于定义审批任务可执行的操作
/// </summary>
public class ConstActionType
{
    /// <summary>
    /// 同意审批
    /// </summary>
    public const string Approve = "approve";

    /// <summary>
    /// 拒绝审批
    /// </summary>
    public const string Reject = "reject";

    /// <summary>
    /// 退回到指定节点
    /// </summary>
    public const string Return = "return";

    /// <summary>
    /// 取消审批流程
    /// </summary>
    public const string Cancel = "cancel";

    /// <summary>
    /// 撤回已提交的审批
    /// </summary>
    public const string Withdraw = "withdraw";

    /// <summary>
    /// 转办给其他人处理
    /// </summary>
    public const string Transfer = "transfer";

    /// <summary>
    /// 委托给代理人处理
    /// </summary>
    public const string Delegate = "delegate";
}