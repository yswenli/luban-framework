/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Retrieval
*文件名： VectorTopK
*版本号： V1.0.0.0
*唯一标识：有界 top-K 向量命中收集器
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*描述：逐条喂入相似度分数，始终只保留 K 条最优结果，避免全量物化与全量排序。
*
*****************************************************************************/
namespace LuBan.AIAgent.Retrieval;

/// <summary>
/// 有界 top-K 向量命中收集器：逐条喂入分数，只维护 K 条结果。
/// </summary>
/// <remarks>
/// 排序口径与原实现一致：分数降序；同分时切块 Id 升序（等价于
/// <c>OrderByDescending(Score).ThenBy(ChunkId)</c>，且同分保持喂入顺序的稳定性）。
/// </remarks>
public sealed class VectorTopK
{
    private readonly int _topK;
    private readonly List<VectorHit> _top;

    /// <summary>
    /// 创建收集器。
    /// </summary>
    /// <param name="topK">保留条数上限（小于 1 时按 1 处理）。</param>
    public VectorTopK(int topK)
    {
        _topK = Math.Max(1, topK);
        _top = new List<VectorHit>(_topK);
    }

    /// <summary>
    /// 尝试加入一条命中；不如当前末位时直接丢弃。
    /// </summary>
    /// <param name="chunkId">切块 Id。</param>
    /// <param name="score">余弦相似度。</param>
    public void Add(long chunkId, double score)
    {
        var hit = new VectorHit(chunkId, score);
        if (_top.Count >= _topK && !IsBetterThan(hit, _top[^1])) return;

        var index = _top.Count;
        for (var i = 0; i < _top.Count; i++)
        {
            if (IsBetterThan(hit, _top[i])) { index = i; break; }
        }
        _top.Insert(index, hit);
        if (_top.Count > _topK) _top.RemoveAt(_top.Count - 1);
    }

    /// <summary>
    /// 取出结果（已按分数降序、同分 Id 升序）。
    /// </summary>
    public IReadOnlyList<VectorHit> Results => _top;

    private static bool IsBetterThan(VectorHit left, VectorHit right)
        => left.Score > right.Score || (left.Score == right.Score && left.ChunkId < right.ChunkId);
}