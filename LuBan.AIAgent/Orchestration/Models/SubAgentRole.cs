/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Models
*文件名： SubAgentRole
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：SubAgent 角色定义
*
*****************************************************************************/
namespace LuBan.AIAgent.Orchestration.Models;

/// <summary>
/// SubAgent 角色定义
/// </summary>
public class SubAgentRole
{
    /// <summary>
    /// 角色名称（如 "analyst", "coder"）
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 系统提示词模板，支持 {prompt} 占位符
    /// </summary>
    public string SystemPromptTemplate { get; set; } = "";

    /// <summary>
    /// 默认工具组列表
    /// </summary>
    public List<string> DefaultToolGroups { get; set; } = new();
}
