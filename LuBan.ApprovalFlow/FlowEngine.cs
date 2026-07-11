/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.ApprovalFlow
*文件名： FlowEngine
*版本号： V1.0.0.0
*唯一标识：8e4a26f9-6f12-4a2b-8f1a-e8b6ed0b2e10
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/11/10 10:00:00
*描述：审批流引擎：管理线程池并定期检查执行器状态。
*
*=================================================
*修改标记
*修改时间：2025/11/10 10:00:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：新增流程引擎类。
*
*****************************************************************************/

namespace LuBan.ApprovalFlow;

/// <summary>
/// 审批流引擎：管理线程池并定期检查执行器状态。
/// </summary>
public sealed class FlowEngine : BaseSingleInstance<FlowEngine>, IDisposable
{
    /// <summary>
    /// 流程引擎线程池
    /// </summary>
    private SimpleThreadPool? _threadPool;
    /// <summary>
    /// 状态检查任务
    /// </summary>
    private Task? _checkTask;
    /// <summary>
    /// 引擎运行状态标志
    /// </summary>
    private volatile bool _isRunning = false;
    /// <summary>
    /// 引擎是否已停止
    /// </summary>
    private volatile bool _isDispose = false;

    /// <summary>
    /// 监控中的执行器集合（按流程键分组）
    /// </summary>
    private readonly ConcurrentDictionary<string, List<FlowExecutor>> _monitoredExecutors = new();
    /// <summary>
    /// 执行器状态检查间隔
    /// </summary>
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    /// <summary>
    /// 引擎是否正在运行
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// 审批流引擎：管理线程池并定期检查执行器状态。
    /// </summary>
    public FlowEngine()
    {
        _threadPool = new SimpleThreadPool("FlowEnginePool", ApprovalFlowOptions.Default.ThreadPoolSize);
        _threadPool.OnRunning += ThreadPool_OnRunning;
        _checkTask = Task.Run(CheckLoopAsync);
    }

    private void ThreadPool_OnRunning(object? sender, TaskInfoArgs taskInfoArgs)
    {
        ConsoleUtil.WriteTable(taskInfoArgs);
    }

    /// <summary>
    /// 启动流程引擎
    /// </summary>
    /// <remarks>
    /// 初始化线程池，加载执行器，并启动定期检查线程
    /// </remarks>
    public void Start()
    {
        if (_isRunning)
        {
            return;
        }
        _isRunning = true;
    }

    /// <summary>
    /// 停止流程引擎
    /// </summary>
    /// <remarks>
    /// 停止检查线程，释放线程池资源，清空监控列表
    /// </remarks>
    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        _isRunning = false;
    }

    /// <summary>
    /// 定期检查执行器状态的循环方法
    /// </summary>
    /// <remarks>
    /// 以指定的时间间隔刷新执行器列表并检查每个流程记录的状态
    /// </remarks>
    private async Task CheckLoopAsync()
    {
        while (!_isDispose)
        {
            if (!_isRunning)
            {
                await Task.Delay(_checkInterval);
                continue;
            }
            try
            {
                await Task.Delay(_checkInterval);

                if (_isDispose) break;

                // 检查每个执行器的所有记录
                foreach (var key in _monitoredExecutors.Keys)
                {
                    if (!_isRunning) break;

                    var executors = _monitoredExecutors[key];
                    if (executors.Count == 0) continue;

                    // 获取该流程类型的所有记录ID
                    var monitor = FlowMonitor.Instance;
                    if (monitor != null && monitor.KeyToRecordIds.TryGetValue(key, out var recordIds))
                    {
                        foreach (var recordId in recordIds.ToList())
                        {
                            if (!_isRunning) break;

                            // 使用线程池执行检查
                            _threadPool?.Enqueue(() =>
                            {
                                CheckAndAdvanceFlow(recordId, executors.FirstOrDefault());
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"FlowEngine check loop error: {ex}");
            }
        }
    }

    /// <summary>
    /// 检查并推进流程实例
    /// </summary>
    /// <param name="recordId">流程记录ID</param>
    /// <param name="executor">流程执行器</param>
    /// <remarks>
    /// 检查流程状态，如果流程未结束且有当前节点，则尝试自动推进流程
    /// </remarks>
    private void CheckAndAdvanceFlow(long recordId, FlowExecutor? executor)
    {
        if (executor == null || !_isRunning) return;

        try
        {
            // 获取记录
            var monitor = FlowMonitor.Instance;
            if (monitor == null || !monitor.TryGetRecord(recordId, out var record) || record == null)
            {
                return;
            }

            // 检查是否已结束或异常
            var formStatus = record.FormStatus;
            if (!string.IsNullOrEmpty(formStatus) &&
                (string.Equals(formStatus, ConstApprovalFlowStatus.Completed, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(formStatus, ConstApprovalFlowStatus.Exception, StringComparison.OrdinalIgnoreCase)))
            {
                return; // 已完成或异常的流程不再处理
            }

            // 反序列化状态
            FlowRuntimeState? state = null;
            if (!string.IsNullOrEmpty(record.FlowResult))
            {
                try
                {
                    state = SerializeUtil.Deserialize<FlowRuntimeState>(record.FlowResult);
                }
                catch (JsonException ex)
                {
                    Logger.Error($"Failed to deserialize flow result for record {recordId}: {ex}");
                }
            }

            if (state != null && !string.IsNullOrEmpty(state.CurrentNodeKey))
            {
                executor.AutoAdvanceAsync(recordId, state).GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"FlowEngine advance flow error for record {recordId}: {ex}");
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Stop();
        _isDispose = true;
        if (_checkTask != null)
        {
            try
            {
                _checkTask.Wait(3000);
            }
            catch (Exception)
            {
                // 忽略等待异常
            }
        }
        if (_threadPool != null)
        {
            _threadPool.Dispose();
            _threadPool = null;
        }
        _monitoredExecutors.Clear();
    }
}