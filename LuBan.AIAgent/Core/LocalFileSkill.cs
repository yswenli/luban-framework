using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 本地文件技能，基于文件系统中的技能清单执行技能
/// </summary>
public sealed class LocalFileSkill : ISkill
{
    private readonly SkillManifest _manifest;
    private readonly ISkillExecutor _executor;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="manifest">技能清单</param>
    /// <param name="executor">技能执行器</param>
    public LocalFileSkill(SkillManifest manifest, ISkillExecutor executor)
    {
        _manifest = manifest;
        _executor = executor;
    }

    /// <summary>
    /// 技能名称
    /// </summary>
    public string Name => _manifest.Name;

    /// <summary>
    /// 技能描述
    /// </summary>
    public string? Description => _manifest.Description;

    /// <summary>
    /// 技能清单
    /// </summary>
    public SkillManifest? Manifest => _manifest;

    /// <summary>
    /// 异步执行技能
    /// </summary>
    /// <param name="context">技能执行上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>代理执行结果</returns>
    public Task<AgentResult> ExecuteAsync(SkillExecutionContext context, CancellationToken cancellationToken = default)
        => _executor.ExecuteAsync(_manifest, context, cancellationToken);
}
