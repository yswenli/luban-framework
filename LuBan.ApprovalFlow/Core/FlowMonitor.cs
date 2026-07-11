/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow.Core
*文件名： FlowMonitor
*版本号： V1.0.0.0
*唯一标识：f0a0d7f1-9c2e-4f1f-9f1a-e8b6ed0b2e10
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/30 18:20:00
*描述：审批流运行监控：集中管理所有 FlowExecutor 的流程记录与统计。
*
*=================================================
*修改标记
*修改时间：2025/10/30 18:20:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：新增运行监控类。
*
*****************************************************************************/
namespace LuBan.ApprovalFlow.Core;

/// <summary>
/// 审批流运行监控：集中管理所有 FlowExecutor 的流程记录与统计。
/// 采用单例模式，使用并发字典确保线程安全。
/// </summary>
public sealed class FlowMonitor : BaseSingleInstance<FlowMonitor>
{
    /// <summary>
    /// 流程记录字典：以记录ID为键。
    /// </summary>
    private readonly ConcurrentDictionary<long, FlowRecord> _records = new();

    /// <summary>
    /// 记录ID到流程键的索引映射。
    /// </summary>
    private readonly ConcurrentDictionary<long, string> _recordKeyIndex = new();

    /// <summary>
    /// 流程键到记录ID集合的映射。
    /// </summary>
    private readonly ConcurrentDictionary<string, HashSet<long>> _keyToRecordIds = new();

    /// <summary>
    /// 流程统计信息字典：以流程键为键。
    /// </summary>
    private readonly ConcurrentDictionary<string, (int total, int finished, DateTime? lastUpdated, long? lastId)> _stats = new();

    /// <summary>
    /// 流程键到执行器集合的映射。
    /// </summary>
    private readonly ConcurrentDictionary<string, HashSet<FlowExecutor>> _executorsByKey = new();

    /// <summary>
    /// 获取所有监控中的执行器（按流程键分组）。
    /// </summary>
    public ConcurrentDictionary<string, HashSet<FlowExecutor>> MonitoredExecutors => _executorsByKey;

    /// <summary>
    /// 获取流程键到记录ID的映射。
    /// </summary>
    public ConcurrentDictionary<string, HashSet<long>> KeyToRecordIds => _keyToRecordIds;

    /// <summary>
    /// 记录ID生成种子。
    /// </summary>
    private long _idSeed = DateTime.UtcNow.Ticks;

    /// <summary>
    /// 审批流运行监控构造函数。
    /// </summary>
    public FlowMonitor() { }

    /// <summary>
    /// 注册执行器实例用于监控（按流程 Key 分类）。
    /// </summary>
    /// <param name="executor">要注册的执行器实例。</param>
    public void RegisterExecutor(FlowExecutor executor)
    {
        var key = executor.FlowDefinition.Key ?? string.Empty;
        var set = _executorsByKey.GetOrAdd(key, _ => new HashSet<FlowExecutor>());
        lock (set) { set.Add(executor); }
    }

    /// <summary>
    /// 注销执行器实例。
    /// </summary>
    /// <param name="executor">要注销的执行器实例。</param>
    public void UnregisterExecutor(FlowExecutor executor)
    {
        var key = executor.FlowDefinition.Key ?? string.Empty;
        if (_executorsByKey.TryGetValue(key, out var set))
        {
            lock (set) { set.Remove(executor); }
        }
    }

    /// <summary>
    /// 添加流程记录并索引到对应流程 Key。
    /// </summary>
    /// <param name="flowKey">流程键。</param>
    /// <param name="record">流程记录。</param>
    /// <returns>新记录的ID。</returns>
    public long AddRecord(string flowKey, FlowRecord record)
    {
        // 生成唯一ID
        var id = Interlocked.Increment(ref _idSeed);
        _records[id] = record;
        _recordKeyIndex[id] = flowKey;

        // 添加到流程键索引
        var ids = _keyToRecordIds.GetOrAdd(flowKey, _ => new HashSet<long>());
        lock (ids) { ids.Add(id); }

        // 更新统计信息
        _stats.AddOrUpdate(flowKey, _ => (1, 0, DateTime.Now, id),
            (_, t) => (t.total + 1, t.finished, DateTime.Now, id));
        return id;
    }

    /// <summary>
    /// 获取指定 ID 的记录。
    /// </summary>
    /// <param name="id">记录ID。</param>
    /// <param name="record">输出参数：流程记录。</param>
    /// <returns>是否成功获取。</returns>
    public bool TryGetRecord(long id, out FlowRecord? record)
    {
        var ok = _records.TryGetValue(id, out var r);
        record = r;
        return ok;
    }

    /// <summary>
    /// 更新记录的流程结果 JSON，并在完成时更新统计。
    /// </summary>
    /// <param name="id">记录ID。</param>
    /// <param name="flowResultJson">流程结果JSON字符串。</param>
    /// <param name="finalStatus">最终状态（可选）。</param>
    public void UpdateRecordResult(long id, string flowResultJson, string? finalStatus = null)
    {
        if (_records.TryGetValue(id, out var record))
        {
            // 更新流程结果
            record.FlowResult = flowResultJson;

            if (_recordKeyIndex.TryGetValue(id, out var key))
            {
                // 计算完成数增量
                var finishedInc = (!string.IsNullOrWhiteSpace(finalStatus) &&
                                   (string.Equals(finalStatus, "finished", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(finalStatus, "rejected", StringComparison.OrdinalIgnoreCase))) ? 1 : 0;

                // 更新统计信息
                _stats.AddOrUpdate(key,
                    _ => (finishedInc, finishedInc, DateTime.Now, id),
                    (_, t) => (t.total, t.finished + finishedInc, DateTime.Now, id));
            }
        }
    }

    /// <summary>
    /// 获取指定流程 Key 的统计信息。
    /// </summary>
    /// <param name="flowKey">流程键。</param>
    /// <returns>流程执行统计信息。</returns>
    public FlowExecutionStats GetStats(string flowKey)
    {
        _stats.TryGetValue(flowKey, out var t);
        var total = t.total;
        var finished = t.finished;
        return new FlowExecutionStats
        {
            Key = flowKey,
            TotalRecords = total,
            FinishedRecords = finished,
            LastUpdated = t.lastUpdated,
            LastRecordId = t.lastId
        };
    }
}