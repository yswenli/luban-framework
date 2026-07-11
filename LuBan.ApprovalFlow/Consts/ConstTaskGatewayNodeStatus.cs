/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Consts
*文件名： ConstTaskGatewayStatus
*版本号： V1.0.0.0
*唯一标识：65c34a29-0eca-4c5f-b181-90c54fab8910
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/11/4 10:51:02
*描述：任务网关状态常量
*
*=================================================
*修改标记
*修改时间：2025/11/4 10:51:02
*修改人： yswenli
*版本号： V1.0.0.0
*描述：任务网关状态常量
*
*****************************************************************************/
namespace LuBan.ApprovalFlow.Consts;

/// <summary>
/// 任务网关状态常量
/// </summary>
public class ConstTaskGatewayNodeStatus
{
    /// <summary>
    /// 通过
    /// </summary>
    public const string 通过 = "通过";

    /// <summary>
    /// 拒绝
    /// </summary>
    public const string 拒绝 = "拒绝";

    /// <summary>
    /// 退回
    /// </summary>
    public const string 退回 = "退回";
}