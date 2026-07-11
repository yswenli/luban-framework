/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Qingflow.Models
*文件名： LatestApprovalProcess
*版本号： V1.0.0.0
*唯一标识：968bf8b8-fca5-473f-8079-3f01f6af2f68
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/1/8 9:24:41
*描述：最近审核流程
*
*=================================================
*修改标记
*修改时间：2025/1/8 9:24:41
*修改人： yswenli
*版本号： V1.0.0.0
*描述：最近审核流程
*
*****************************************************************************/
namespace LuBan.Qingflow.Models;



/// <summary>
/// 最近审核流程
/// </summary>
public class LatestAuditFlow
{
    /// <summary>
    /// 流程状态
    /// </summary>
    public EnumFlowType ApplyStatus { get; set; }
    /// <summary>
    /// 流程节点id
    /// </summary>
    public int? AuditNodeId { get; set; }
    /// <summary>
    /// 流程节点名称
    /// </summary>
    public string AuditNodeName { get; set; } = "审核中";
    /// <summary>
    /// 审核时间
    /// </summary>
    public DateTime? AuditTime { get; set; }
}
