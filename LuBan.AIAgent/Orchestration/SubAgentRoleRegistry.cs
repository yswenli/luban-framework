/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration
*文件名： SubAgentRoleRegistry
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：SubAgent 角色注册表
*
*****************************************************************************/
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.AIAgent.Orchestration;

/// <summary>
/// SubAgent 角色注册表
/// </summary>
public class SubAgentRoleRegistry
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, SubAgentRole> _roles = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 创建 SubAgentRoleRegistry 实例，注册内置角色
    /// </summary>
    public SubAgentRoleRegistry()
    {
        RegisterBuiltInRoles();
    }

    private void RegisterBuiltInRoles()
    {
        Register(new SubAgentRole
        {
            Name = "analyst",
            SystemPromptTemplate = "You are a problem analysis expert. Analyze the task systematically and provide structured insights. Task: {prompt}",
            DefaultToolGroups = new List<string> { "filesystem" }
        });

        Register(new SubAgentRole
        {
            Name = "researcher",
            SystemPromptTemplate = "You are a research specialist. Gather information from multiple sources and verify findings. Task: {prompt}",
            DefaultToolGroups = new List<string> { "web", "filesystem" }
        });

        Register(new SubAgentRole
        {
            Name = "coder",
            SystemPromptTemplate = "You are a code implementation expert. Write clean, runnable code with proper error handling. Task: {prompt}",
            DefaultToolGroups = new List<string> { "filesystem", "script", "database" }
        });

        Register(new SubAgentRole
        {
            Name = "writer",
            SystemPromptTemplate = "You are a writing specialist. Create clear, well-structured content. Task: {prompt}",
            DefaultToolGroups = new List<string> { "filesystem" }
        });
    }

    /// <summary>
    /// 注册角色
    /// </summary>
    /// <param name="role">角色定义</param>
    public void Register(SubAgentRole role)
    {
        _roles[role.Name] = role;
    }

    /// <summary>
    /// 获取角色
    /// </summary>
    /// <param name="name">角色名称</param>
    /// <returns>角色定义，未找到返回 null</returns>
    public SubAgentRole? GetRole(string name)
    {
        return _roles.TryGetValue(name, out var role) ? role : null;
    }

    /// <summary>
    /// 获取所有角色
    /// </summary>
    /// <returns>角色列表</returns>
    public IReadOnlyList<SubAgentRole> GetAllRoles()
    {
        return _roles.Values.ToList();
    }
}
