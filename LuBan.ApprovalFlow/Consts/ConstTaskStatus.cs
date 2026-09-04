/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Consts
*文件名： ConstTaskStatus.cs
*版本号： V1.0.0.0
*唯一标识：fab2c2b2-1d4c-4782-9841-9a70351a52c3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ConstTaskStatus 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ConstTaskStatus 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Consts;

/// <summary>
/// 任务状态常量，用于定义审批任务的状态
/// </summary>
public class ConstTaskStatus
{
    /// <summary>
    /// 待处理
    /// </summary>
    public const string Pending = "待处理";

    /// <summary>
    /// 处理中
    /// </summary>
    public const string Processing = "处理中";

    /// <summary>
    /// 已完成
    /// </summary>
    public const string Completed = "已完成";

    /// <summary>
    /// 已转办
    /// </summary>
    public const string Transferred = "已转办";

    /// <summary>
    /// 已委托
    /// </summary>
    public const string Delegated = "已委托";

    /// <summary>
    /// 已取消
    /// </summary>
    public const string Cancelled = "已取消";
}