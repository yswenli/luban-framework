/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*命名空间：LuBan.AIAgent.Orchestration
*文件名： GraphPlanStore
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/23
*描述：已规划任务图谱的进程内暂存，供 plan_task 登记、run_orchestration 取用
*
*****************************************************************************/
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.AIAgent.Orchestration;

/// <summary>
/// 已规划任务图谱的进程内暂存。容量上限 32（超出按最旧淘汰）、TTL 30 分钟（懒清理），
/// 不持久化（进程内会话级）；跨进程 A2A 的 Task 生命周期由 Phase 2 的 TaskStore 承担。
/// </summary>
public class GraphPlanStore
{
    /// <summary>容量上限，超出按最旧淘汰。</summary>
    public const int MaxEntries = 32;

    /// <summary>条目存活时长。</summary>
    public static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

    private readonly ConcurrentDictionary<string, Entry> _entries = new();
    private readonly object _gate = new();

    /// <summary>
    /// 登记任务图谱并返回图谱标识（沿用 <see cref="TaskGraph.GraphId"/>）。
    /// </summary>
    /// <param name="graph">任务图谱。</param>
    /// <returns>图谱标识。</returns>
    public string Save(TaskGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        lock (_gate)
        {
            PurgeExpiredLocked();

            // 先淘汰最旧条目，为新条目腾出容量（TTL 相同，过期时刻即插入先后顺序）
            while (_entries.Count >= MaxEntries)
            {
                var oldest = _entries.Values.OrderBy(e => e.ExpiresAtUtc).FirstOrDefault();
                if (oldest is null) break;
                _entries.TryRemove(oldest.Graph.GraphId, out _);
            }

            _entries[graph.GraphId] = new Entry(graph, DateTime.UtcNow.Add(Ttl));
        }

        return graph.GraphId;
    }

    /// <summary>
    /// 只读获取图谱（不消费）。过期条目返回 null 并顺手清理。
    /// </summary>
    /// <param name="graphId">图谱标识。</param>
    /// <returns>任务图谱；不存在或已过期时返回 null。</returns>
    public TaskGraph? Get(string graphId)
    {
        if (string.IsNullOrEmpty(graphId) || !_entries.TryGetValue(graphId, out var entry))
            return null;

        if (entry.ExpiresAtUtc <= DateTime.UtcNow)
        {
            _entries.TryRemove(graphId, out _);
            return null;
        }

        return entry.Graph;
    }

    /// <summary>
    /// 显式移除图谱（编排成功后调用；失败时保留以便重试）。
    /// </summary>
    /// <param name="graphId">图谱标识。</param>
    public void Remove(string graphId)
    {
        if (!string.IsNullOrEmpty(graphId))
            _entries.TryRemove(graphId, out _);
    }

    /// <summary>当前条目数（供测试与诊断）。</summary>
    public int Count => _entries.Count;

    private void PurgeExpiredLocked()
    {
        var now = DateTime.UtcNow;
        foreach (var kv in _entries)
        {
            if (kv.Value.ExpiresAtUtc <= now)
                _entries.TryRemove(kv.Key, out _);
        }
    }

    private sealed record Entry(TaskGraph Graph, DateTime ExpiresAtUtc);
}