/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Tools.SkillManage
*文件名： SkillManageToolPlugin
*描述：Skill 管理工具组插件（GroupName="skillmanage"）
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Skills;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tools.SkillManage;

/// <summary>
/// Skill 管理工具组插件。提供创建/删除 Skill 的工具。
/// </summary>
public class SkillManageToolPlugin : ILuBanToolPlugin
{
    private readonly SkillManageToolOptions _options;
    private readonly IWorkspaceContext? _workspaceContext;

    public SkillManageToolPlugin(IOptions<LuBanAgentOptions> options, IWorkspaceContext? workspaceContext = null)
    {
        _options = options.Value.Tools.SkillManage;
        _workspaceContext = workspaceContext;
    }

    public string GroupName => "skillmanage";
    public string? Description => "Skill 管理工具，创建/删除工作区级 Skill";

    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp, ToolGroupOptions? toolsOptions = null)
    {
        var registry = sp.GetRequiredService<SkillRegistry>();
        var reloadTrigger = sp.GetService<IAgentReloadTrigger>();

        var toolGroup = new SkillManageToolGroup(registry, _options, reloadTrigger);

        Func<string?> workspaceRootProvider = () => _workspaceContext?.RootPath;

        // 写/删工具不启用工作区根兜底（需显式传 path），但相对路径锚定仍生效
        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(toolGroup, nameof(SkillManageToolGroup.WriteSkillAsync), workspaceRootProvider, fallbackToWorkspaceRootWhenMissing: false),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(SkillManageToolGroup.DeleteSkillAsync), workspaceRootProvider, fallbackToWorkspaceRootWhenMissing: false),
        };
    }

    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.SkillManage.Enabled;
}