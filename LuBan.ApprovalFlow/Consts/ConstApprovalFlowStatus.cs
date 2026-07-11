/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Consts;
*文件名： ConstApprovalFlowStatus
*版本号： V1.0.0.0
*唯一标识：bf10d07a-ac26-4d6a-8417-9f34a7df4877
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/11/4 10:29:22
*描述：审批流表单状态
*
*=================================================
*修改标记
*修改时间：2025/11/4 10:29:22
*修改人： yswenli
*版本号： V1.0.0.0
*描述：审批流表单状态
*
*****************************************************************************/
namespace LuBan.ApprovalFlow.Consts;

/// <summary>
/// 审批流表单状态
/// </summary>
public class ConstApprovalFlowStatus
{
    /// <summary>
    /// 未开始
    /// </summary>
    public const string NotStarted = "未开始";

    /// <summary>
    /// 处理中
    /// </summary>
    public const string Processing = "处理中";

    /// <summary>
    /// 已完成
    /// </summary>
    public const string Completed = "已完成";

    /// <summary>
    /// 异常
    /// </summary>
    public const string Exception = "异常";
}
