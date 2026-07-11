namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 本地技能加载器接口，用于从目录加载技能清单
/// </summary>
public interface ILocalSkillLoader
{
    /// <summary>
    /// 从目录异步加载技能清单
    /// </summary>
    /// <param name="rootDirectory">根目录路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>技能清单列表</returns>
    Task<IReadOnlyList<SkillManifest>> LoadFromDirectoryAsync(
        string rootDirectory,
        CancellationToken cancellationToken = default);
}
