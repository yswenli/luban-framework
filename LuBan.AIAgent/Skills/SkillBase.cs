/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills
*文件名： SkillBase
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：Skill 基类，定义所有 Skill 的公共接口与默认执行逻辑
*
*****************************************************************************/
namespace LuBan.AIAgent.Skills;

/// <summary>
/// Skill 基类，所有 Skill 都是纯提示词模板
/// </summary>
public abstract class SkillBase : ISkill
{
    /// <summary>
    /// Skill 唯一标识
    /// </summary>
    public abstract string Id { get; }

    /// <summary>
    /// Skill 名称
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Skill 描述
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Skill 分类
    /// </summary>
    public virtual string Category => "general";

    /// <summary>
    /// Skill 使用示例
    /// </summary>
    public virtual IEnumerable<string> Examples => Array.Empty<string>();

    /// <summary>
    /// Skill 自动激活触发关键词。内置 Skill 可覆盖此属性以支持自动激活。
    /// </summary>
    public virtual IEnumerable<string> TriggerKeywords => Array.Empty<string>();

    /// <summary>
    /// Skill 的提示词模板内容。所有 Skill 必须提供。
    /// </summary>
    public abstract string PromptTemplate { get; }

    /// <summary>
    /// 执行 Skill：渲染提示词模板并调用 Agent 获取响应
    /// </summary>
    /// <param name="context">Skill 执行上下文，包含 Agent、取消令牌等信息</param>
    /// <param name="input">用户输入内容</param>
    /// <returns>Skill 执行结果，成功时包含 Agent 响应文本，失败时包含错误信息</returns>
    public virtual async Task<SkillResult> ExecuteAsync(SkillContext context, string input)
    {
        if (context.Agent == null)
            return SkillResult.Fail("Agent 不可用");

        try
        {
            var prompt = PromptTemplate.Contains("{input}")
                ? PromptTemplate.Replace("{input}", input)
                : $"{PromptTemplate}\n\n{input}";

            var response = await context.Agent.RunAsync(prompt, context.CancellationToken);
            return SkillResult.Ok(response.Text ?? string.Empty);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error("Skill 执行失败", ex, Name, Id);
            return SkillResult.Fail($"执行失败: {ex.Message}");
        }
    }
}