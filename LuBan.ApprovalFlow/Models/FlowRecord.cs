/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： FlowRecord
*版本号： V1.0.0.0
*唯一标识：17d2d288-04dc-4fc8-a018-dce3088f9332
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30 16:58:44
*描述：
*
*=================================================
*修改标记
*修改时间：2025/10/30 16:58:44
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 审批流流程记录：保存表单及流程相关 JSON 信息。
/// </summary>
public sealed class FlowRecord
{
    /// <summary>
    /// 表单名称。
    /// </summary>
    public string? FormName { get; set; }
    /// <summary>
    /// 表单状态。
    /// </summary>
    public string? FormStatus { get; set; }
    /// <summary>
    /// 表单数据的 JSON 字符串（触发或初始载荷）。
    /// </summary>
    public string? FormJson { get; set; }
    /// <summary>
    /// 流程结构的 JSON 字符串（图式定义）。
    /// </summary>
    public string? FlowJson { get; set; }
    /// <summary>
    /// 流程执行结果的 JSON 字符串。
    /// </summary>
    public string? FlowResult { get; set; }
}
