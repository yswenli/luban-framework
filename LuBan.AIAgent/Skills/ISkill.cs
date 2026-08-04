/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills
*文件名： ISkill
*版本号： V1.0.0.0
*唯一标识：89584034-bacc-4ab5-a223-8640ad5a769e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：Skill 接口定义
*
*=================================================
*修改标记
*修改时间：2026/7/27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Skill 接口定义
*
*****************************************************************************/

namespace LuBan.AIAgent.Skills;

/// <summary>
/// Skill 接口 - 封装可复用的 AI 能力模式
/// </summary>
public interface ISkill
{
    /// <summary>
    /// Skill 唯一标识
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Skill 名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Skill 描述
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Skill 分类（如：creative, analysis, development, productivity）
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Skill 使用示例
    /// </summary>
    IEnumerable<string> Examples { get; }

    /// <summary>
    /// Skill 自动激活触发关键词（可选）。当用户输入包含这些关键词时，
    /// 系统可自动激活该 Skill，无需手动 /skill -switch。
    /// </summary>
    IEnumerable<string> TriggerKeywords { get; }

    /// <summary>
    /// Skill 的提示词模板内容（SKILL.md 正文或内置 Skill 的系统指令）。
    /// 用于在对话中激活时注入到 SystemPrompt。null 表示该 Skill 不支持直接注入。
    /// </summary>
    string? PromptTemplate => null;

    /// <summary>
    /// 执行 Skill
    /// </summary>
    /// <param name="context">Skill 执行上下文</param>
    /// <param name="input">用户输入</param>
    /// <returns>执行结果</returns>
    Task<SkillResult> ExecuteAsync(SkillContext context, string input);
}

/// <summary>
/// Skill 执行上下文
/// </summary>
public class SkillContext
{
    /// <summary>
    /// Agent 实例
    /// </summary>
    public LuBanAgent? Agent { get; set; }

    /// <summary>
    /// 服务提供者
    /// </summary>
    public IServiceProvider? ServiceProvider { get; set; }

    /// <summary>
    /// 用户配置
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// 取消令牌
    /// </summary>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// 日志输出回调
    /// </summary>
    public Action<string>? Log { get; set; }

    /// <summary>
    /// 状态更新回调
    /// </summary>
    public Action<string>? UpdateStatus { get; set; }
}

/// <summary>
/// Skill 执行结果
/// </summary>
public class SkillResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 结果文本
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// 结果数据
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();

    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static SkillResult Ok(string text) => new() { Success = true, Text = text };

    /// <summary>
    /// 创建失败结果
    /// </summary>
    public static SkillResult Fail(string error) => new() { Success = false, Error = error };
}
