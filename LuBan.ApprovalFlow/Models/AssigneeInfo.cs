/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.ApprovalFlow.Models
*文件名： AssigneeInfo.cs
*版本号： V1.0.0.0
*唯一标识：476838b3-91f8-463c-954a-f6348f4a67f8
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：AssigneeInfo 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：AssigneeInfo 类
*
*****************************************************************************/

namespace LuBan.ApprovalFlow.Models;

/// <summary>
/// AssigneeInfo 模型类
/// </summary>

public class AssigneeInfo
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Role { get; set; }
}