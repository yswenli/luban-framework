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

    /// <summary>
    /// 从工作区 `.luban-agent/roles/*.json` 加载自定义角色。同名角色覆盖内置角色。单个文件失败不影响其他文件。
    /// </summary>
    /// <param name="workspaceRoot">工作区根路径。</param>
    /// <returns>成功加载的角色数量。</returns>
    [RequiresUnreferencedCode("角色 JSON 反序列化依赖反射")]
    public int LoadFromWorkspace(string workspaceRoot)
    {
        if (string.IsNullOrWhiteSpace(workspaceRoot))
            return 0;

        var dir = Path.Combine(workspaceRoot, ".luban-agent", "roles");
        if (!Directory.Exists(dir))
            return 0;

        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var count = 0;
        foreach (var file in Directory.EnumerateFiles(dir, "*.json"))
        {
            try
            {
                var role = JsonSerializer.Deserialize<SubAgentRole>(File.ReadAllText(file), opts);
                if (role == null || string.IsNullOrWhiteSpace(role.Name))
                {
                    Logger.Warn($"角色文件无效（缺少 name），已跳过: {file}");
                    continue;
                }
                if (_roles.ContainsKey(role.Name))
                    Logger.Warn($"自定义角色 '{role.Name}' 覆盖同名内置角色");
                Register(role);
                count++;
            }
            catch (Exception ex)
            {
                Logger.Warn($"加载角色文件失败: {file}", ex);
            }
        }

        if (count > 0)
            Logger.Info($"已从工作区加载 {count} 个自定义角色 ({dir})");
        return count;
    }
}
