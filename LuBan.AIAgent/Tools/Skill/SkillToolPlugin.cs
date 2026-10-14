/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Tools.Skill
*文件名： SkillToolPlugin
*描述：Skill 消费工具组插件（GroupName="skill"）
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Skills;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tools.Skill;

/// <summary>
/// Skill 消费工具组插件。提供列出/使用 Skill 的工具。
/// </summary>
public class SkillToolPlugin : ILuBanToolPlugin
{
    private readonly SkillToolOptions _options;
    private readonly IWorkspaceContext? _workspaceContext;

    /// <summary>
    /// 创建 SkillToolPlugin 实例。
    /// </summary>
    public SkillToolPlugin(IOptions<LuBanAgentOptions> options, IWorkspaceContext? workspaceContext = null)
    {
        _options = options.Value.Tools.Skill;
        _workspaceContext = workspaceContext;
    }

    /// <inheritdoc />
    public string GroupName => "skill";

    /// <inheritdoc />
    public string? Description => "Skill 消费工具，列出/使用当前可用的 Skill";

    /// <inheritdoc />
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp, ToolGroupOptions? toolsOptions = null)
    {
        var registry = sp.GetRequiredService<SkillRegistry>();
        var reloadTrigger = sp.GetService<IAgentReloadTrigger>();

        var toolGroup = new SkillToolGroup(registry, _options, reloadTrigger);

        // 工作区根提供者：用于只读工具免确认
        Func<string?> workspaceRootProvider = () => _workspaceContext?.RootPath;

        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(toolGroup, nameof(SkillToolGroup.ListSkillsAsync), workspaceRootProvider),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(SkillToolGroup.UseSkillAsync), workspaceRootProvider),
        };
    }

    /// <inheritdoc />
    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.Skill.Enabled;
}