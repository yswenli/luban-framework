/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Skills
*文件名： SkillBase
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：Skill 基类，提供通用功能
*
*****************************************************************************/

namespace LuBan.AIAgent.Skills;

/// <summary>
/// Skill 基类，提供通用功能
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
    /// 执行 Skill
    /// </summary>
    public abstract Task<SkillResult> ExecuteAsync(SkillContext context, string input);

    /// <summary>
    /// 记录日志
    /// </summary>
    protected void Log(SkillContext context, string message)
    {
        context.Log?.Invoke(message);
    }

    /// <summary>
    /// 更新状态
    /// </summary>
    protected void UpdateStatus(SkillContext context, string status)
    {
        context.UpdateStatus?.Invoke(status);
    }

    /// <summary>
    /// 调用 Agent 执行对话
    /// </summary>
    protected async Task<string?> CallAgentAsync(SkillContext context, string prompt)
    {
        if (context.Agent == null)
            throw new InvalidOperationException("Agent 未设置");

        var response = await context.Agent.RunAsync(prompt, context.CancellationToken);
        return response.Text;
    }
}
