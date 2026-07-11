/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Consts
*文件名： ConstTaskStatus
*版本号： V1.0.0.0
*唯一标识：9bc5159f-b347-4d09-ac3f-1a233cadeddd
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/11/4 10:54:38
*描述：服务节点状态常量
*
*=================================================
*修改标记
*修改时间：2025/11/4 10:54:38
*修改人： yswenli
*版本号： V1.0.0.0
*描述：服务节点状态常量
*
*****************************************************************************/
namespace LuBan.ApprovalFlow.Consts;

/// <summary>
/// 服务节点状态常量
/// </summary>
public class ConstTaskNodeStatus
{
    /// <summary>
    /// 未开始
    /// </summary>
    public const string 未开始 = "未开始";

    /// <summary>
    /// 进行中
    /// </summary>
    public const string 进行中 = "进行中";

    /// <summary>
    /// 已完成
    /// </summary>
    public const string 已完成 = "已完成";

    /// <summary>
    /// 取消
    /// </summary>
    public const string 取消 = "取消";

    /// <summary>
    /// 失败
    /// </summary>
    public const string 失败 = "失败";
}