/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Skills
*文件名： ISkill
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/27
*描述：Skill 接口定义
*
*****************************************************************************/
using System.Collections.Generic;
using System.Threading.Tasks;

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