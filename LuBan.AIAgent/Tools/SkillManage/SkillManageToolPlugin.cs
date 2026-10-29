/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.SkillManage
*文件名： SkillManageToolPlugin
*版本号： V1.0.0.0
*唯一标识：1025c72c-f960-4529-870c-9fa1f807bf29
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/14
*描述：Skill 管理工具组插件（GroupName="skillmanage"）
*
*****************************************************************************/

namespace LuBan.AIAgent.Tools.SkillManage;

/// <summary>
/// Skill 管理工具组插件。提供创建/删除 Skill 的工具。
/// </summary>
public class SkillManageToolPlugin : ILuBanToolPlugin
{
    private readonly SkillManageToolOptions _options;
    private readonly IWorkspaceContext? _workspaceContext;
    private readonly string? _workspaceRootFallback;

    /// <summary>
    /// 从全局选项读取 Skill 管理配置，并保留可选的工作区上下文。
    /// </summary>
    /// <param name="options">宿主全局选项，取其中的 Skill 管理配置节。</param>
    /// <param name="workspaceContext">工作区上下文，用于提供默认根路径。</param>
    public SkillManageToolPlugin(IOptions<LuBanAgentOptions> options, IWorkspaceContext? workspaceContext = null)
    {
        _options = options.Value.Tools.SkillManage;
        _workspaceContext = workspaceContext;
        // 工作区上下文缺失时回退全局配置（与 FileSystemToolPlugin 同口径）
        _workspaceRootFallback = options.Value.WorkspaceRoot;
    }

    /// <summary>
    /// 工具组名称，固定为 <c>skillmanage</c>。
    /// </summary>
    public string GroupName => "skillmanage";
    /// <summary>
    /// 工具组描述，说明用于创建与删除工作区级 Skill。
    /// </summary>
    public string? Description => "Skill 管理工具，创建/删除工作区级 Skill";

    /// <summary>
    /// 构建写入与删除两个 Skill 管理工具函数。
    /// </summary>
    /// <param name="sp">用于解析 Skill 注册表与重载触发器的服务提供者。</param>
    /// <param name="toolsOptions">工具组级选项，当前未使用。</param>
    /// <returns>该工具组暴露的 AIFunction 列表。</returns>
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp, ToolGroupOptions? toolsOptions = null)
    {
        var registry = sp.GetRequiredService<SkillRegistry>();
        var reloadTrigger = sp.GetService<IAgentReloadTrigger>();
        // 写/删必须经确认服务（Default/AcceptEdits 弹确认、Plan 记计划、Bypass 放行）
        var confirmationService = sp.GetRequiredService<IToolConfirmationService>();

        // 工作区根：会话上下文优先，缺失回退全局配置；作为工具省略 workspaceRoot 时的兜底
        Func<string?> workspaceRootProvider = () => _workspaceContext?.RootPath ?? _workspaceRootFallback;

        var toolGroup = new SkillManageToolGroup(registry, _options, reloadTrigger, confirmationService, workspaceRootProvider);

        // 写/删工具不做"缺参即兜底到工作区根"（缺参会由必填参数守卫直接提示），
        // 但相对路径锚定仍生效；workspaceRoot 的兜底由工具内 ManageToolSupport 完成
        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(toolGroup, nameof(SkillManageToolGroup.WriteSkillAsync), workspaceRootProvider, fallbackToWorkspaceRootWhenMissing: false),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(SkillManageToolGroup.DeleteSkillAsync), workspaceRootProvider, fallbackToWorkspaceRootWhenMissing: false),
        };
    }

    /// <summary>
    /// 按配置判断 Skill 管理工具组是否启用。
    /// </summary>
    /// <param name="options">宿主全局选项。</param>
    /// <returns>启用返回 true。</returns>
    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.SkillManage.Enabled;
}