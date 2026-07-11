namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 技能接口，定义了技能的基本属性和执行方法
/// </summary>
public interface ISkill
{
    /// <summary>
    /// 技能名称
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// 技能描述
    /// </summary>
    string? Description { get; }
    
    /// <summary>
    /// 技能清单，包含技能的详细信息
    /// </summary>
    SkillManifest? Manifest { get; }
    
    /// <summary>
    /// 异步执行技能
    /// </summary>
    /// <param name="context">技能执行上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>代理执行结果</returns>
    Task<AgentResult> ExecuteAsync(SkillExecutionContext context, CancellationToken cancellationToken = default);
}
