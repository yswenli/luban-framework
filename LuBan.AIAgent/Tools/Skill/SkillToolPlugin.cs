/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.Skill
*文件名： SkillToolPlugin
*版本号： V1.0.0.0
*唯一标识：62ee423f-c73c-4af6-aae5-42712c3662a8
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/14
*描述：Skill 消费工具组插件（GroupName="skill"）
*
*****************************************************************************/

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
    public IReadOnlyList<AIFunction> GetTools(DiContainer sp, ToolGroupOptions? toolsOptions = null)
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