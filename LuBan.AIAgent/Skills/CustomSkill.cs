/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills
*文件名： CustomSkill
*版本号： V1.0.0.0
*唯一标识：917b7227-2b43-4ee8-b8af-b2b9f705db7e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/28
*描述：自定义 Skill 适配器，将 CustomSkillConfig 包装为 ISkill（提示词模板型）
*
*=================================================
*修改标记
*修改时间：2026/7/28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：自定义 Skill 适配器，将 CustomSkillConfig 包装为 ISkill（提示词模板型）
*
*****************************************************************************/

using LuBan.AIAgent.Configuration;

namespace LuBan.AIAgent.Skills;

/// <summary>
/// 自定义 Skill 适配器，将 CustomSkillConfig 包装为 ISkill（提示词模板型）
/// </summary>
public class CustomSkill : ISkill
{
    private readonly CustomSkillConfig _config;

    /// <summary>
    /// 创建自定义 Skill 实例
    /// </summary>
    /// <param name="config">自定义 Skill 配置</param>
    public CustomSkill(CustomSkillConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Skill 唯一标识
    /// </summary>
    public string Id => _config.Id;

    /// <summary>
    /// Skill 名称
    /// </summary>
    public string Name => _config.Name;

    /// <summary>
    /// Skill 描述
    /// </summary>
    public string Description => _config.Description;

    /// <summary>
    /// Skill 分类
    /// </summary>
    public string Category => _config.Category;

    /// <summary>
    /// Skill 使用示例
    /// </summary>
    public IEnumerable<string> Examples => _config.Examples;

    /// <summary>
    /// 执行 Skill：按提示词模板渲染输入并调用 Agent
    /// </summary>
    /// <param name="context">Skill 执行上下文</param>
    /// <param name="input">用户输入</param>
    /// <returns>执行结果</returns>
    public async Task<SkillResult> ExecuteAsync(SkillContext context, string input)
    {
        if (context.Agent == null)
            return SkillResult.Fail("Agent 不可用");

        var prompt = _config.PromptTemplate.Contains("{input}")
            ? _config.PromptTemplate.Replace("{input}", input)
            : $"{_config.PromptTemplate}\n\n{input}";

        try
        {
            var response = await context.Agent.RunAsync(prompt, context.CancellationToken);
            return SkillResult.Ok(response.Text ?? string.Empty);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error("CustomSkill 执行失败", ex, _config.Name, _config.Id);
            return SkillResult.Fail($"执行失败: {ex.Message}");
        }
    }
}
