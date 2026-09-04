/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： ResultMapping.cs
*版本号： V1.0.0.0
*唯一标识：62b21d00-00aa-4324-8cd8-784f1daed639
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ResultMapping 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ResultMapping 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// 结果映射，用于将业务返回值映射到流程变量。
/// </summary>
public class ResultMapping
{
    /// <summary>
    /// 源字段名称。
    /// </summary>
    public string Source { get; set; } = string.Empty;
    /// <summary>
    /// 目标字段名称。
    /// </summary>
    public string Target { get; set; } = string.Empty;
}