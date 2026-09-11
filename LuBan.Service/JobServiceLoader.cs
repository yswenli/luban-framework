/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Service
*文件名： JobServiceLoader
*版本号： V1.0.0.0
*唯一标识：73adde66-8f55-4105-92d6-fc049e7dd26b
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/11/24 14:03:42
*描述：框架级自动后台服务动态管理类
*
*=================================================
*修改标记
*修改时间：2022/11/24 14:03:42
*修改人： yswen
*版本号： V1.0.0.0
*描述：框架级自动后台服务动态管理类
*
*****************************************************************************/
namespace LuBan.Service;

/// <summary>
/// 框架级自动后台服务动态管理类，
/// 自动注入自定义后台业务服务
/// </summary>
public static class JobServiceLoader
{
    /// <summary>
    /// 任务工厂集合，key为任务类型，value为创建任务实例的工厂函数
    /// </summary>
    static readonly Dictionary<Type, Func<IJob>> _jobFactories = [];

    /// <summary>
    /// 运行中的任务集合，key为任务类型，value为任务实例
    /// </summary>
    static readonly Dictionary<Type, IJob> _runningJobs = [];

    /// <summary>
    /// 任务名称过滤器列表
    /// </summary>
    static List<string>? _jobNamesFilter;

    /// <summary>
    /// 自动注入自定义后台业务服务
    /// </summary>
    /// <param name="jobNames">指定要加载的任务名称列表，如果为null或空则加载所有任务</param>
    public static void Init(List<string>? jobNames)
    {
        _jobNamesFilter = jobNames;

        // 先获取所有实现了IJob接口的类型
        var jobTypes = DynamicUtil.DynamicLoadTypes<IJob>();
        if (jobTypes == null || !jobTypes.Any())
        {
            return;
        }

        // 创建类型到实例工厂的映射，实现延迟实例化
        foreach (var type in jobTypes)
        {
            if (type == null || type.IsAbstract || type.IsInterface) continue;

            if (_jobNamesFilter == null || _jobNamesFilter.Count < 1 || _jobNamesFilter.Contains(JobInfoAttribute.GetJobName(type), StringComparison.InvariantCultureIgnoreCase))
            {
                _jobFactories.TryAdd(type, () => (IJob)Activator.CreateInstance(type)!);
                JobInfosCache.Instance.Add(JobInfoAttribute.GetJobName(type), "后台任务");
            }
        }
    }

    /// <summary>
    /// 启动全部框架中的后台任务
    /// </summary>
    /// <param name="methodName">要执行的方法名</param>
    /// <param name="args">方法参数</param>
    public static void Start(string methodName, params object[] args)
    {
        if (_jobFactories == null || _jobFactories.Count == 0) return;

        foreach (var factoryItem in _jobFactories)
        {
            if (!_runningJobs.ContainsKey(factoryItem.Key))
            {
                var jobInstance = factoryItem.Value();
                _runningJobs.TryAdd(factoryItem.Key, jobInstance);
            }
        }

        var enabledStates = new Dictionary<Type, bool>();

        foreach (var kv in _runningJobs)
        {
            var instance = kv.Value;
            if (instance is BaseBackgroundService bgService)
            {
                var jobName = JobInfoAttribute.GetJobName(instance.GetType());
                var defaultCron = bgService.Cron;

                if (!string.IsNullOrEmpty(defaultCron))
                {
                    JobConfigService.Instance.InitJobConfig(jobName, defaultCron);
                    var dbConfig = JobConfigService.Instance.GetJobConfig(jobName);
                    if (dbConfig != null)
                    {
                        bgService.SetCronWithoutRestart(dbConfig.Cron);
                        enabledStates[kv.Key] = dbConfig.IsEnabled;
                        continue;
                    }
                }
            }
            enabledStates[kv.Key] = true;
        }

        if (_runningJobs.Count > 0)
        {
            var toStart = new Dictionary<Type, IJob>();
            foreach (var kv in _runningJobs)
            {
                if (enabledStates.TryGetValue(kv.Key, out var enabled) && enabled)
                {
                    toStart[kv.Key] = kv.Value;
                }
            }
            _ = toStart.DynamicExecute(methodName, args);

            foreach (var kv in toStart)
            {
                JobInfosCache.Instance.Start(JobInfoAttribute.GetJobName(kv.Value.GetType()));
            }
        }
    }

    /// <summary>
    /// 启动全部框架中的后台任务
    /// </summary>
    public static void Start()
    {
        Start("Start");
    }

    /// <summary>
    /// 停止全部框架中的后台任务（支持并发执行和超时）
    /// </summary>
    /// <param name="methodName">方法名</param>
    /// <param name="timeoutMilliseconds">超时时间（毫秒），默认5000毫秒</param>
    /// <param name="args">参数</param>
    public static void Stop(string methodName = "Stop", int timeoutMilliseconds = 5000, params object[] args)
    {
        if (_runningJobs == null || _runningJobs.Count == 0)
            return;

        using var cts = new CancellationTokenSource(timeoutMilliseconds);

        try
        {
            // 并发执行所有任务的Stop方法
            Parallel.ForEach(_runningJobs, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = cts.Token },
                (item) =>
                {
                    try
                    {
                        var jobName = JobInfoAttribute.GetJobName(item.Value.GetType());

                        // 优先调用带有CancellationToken的Stop方法
                        if (methodName == "Stop")
                        {
                            // 尝试调用带有CancellationToken的Stop方法
                            try
                            {
                                item.Value.Stop(cts.Token);
                            }
                            catch (NotImplementedException)
                            {
                                // 如果没有实现带有CancellationToken的Stop方法，调用原有的Stop方法
                                item.Value.Stop();
                            }
                        }
                        else
                        {
                            // 动态执行指定方法
                            item.Key.DynamicExecute(item.Value, methodName, args);
                        }

                        // 更新任务状态
                        try
                        {
                            JobInfosCache.Instance.Stop(jobName);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, $"更新任务 {jobName} 状态失败");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // 超时异常，记录日志
                        Logger.Warn($"任务 {JobInfoAttribute.GetJobName(item.Key)} 的 {methodName} 方法执行超时");
                    }
                    catch (Exception ex)
                    {
                        // 其他异常，记录日志
                        Logger.Error(ex, $"任务 {JobInfoAttribute.GetJobName(item.Key)} 的 {methodName} 方法执行失败");
                    }
                });

            // 清空运行中的任务集合，释放资源
            _runningJobs.Clear();
        }
        catch (OperationCanceledException)
        {
            // 捕获Parallel.ForEach的取消异常，记录日志但不向上抛出
            Logger.Warn($"停止任务操作超时");
            // 即使超时也尝试清空运行中的任务集合
            _runningJobs.Clear();
        }
        catch (Exception ex)
        {
            // 其他异常，记录日志但不向上抛出
            Logger.Error(ex, $"停止任务操作失败");
            // 即使失败也尝试清空运行中的任务集合
            _runningJobs.Clear();
        }
    }

    /// <summary>
    /// 停止全部框架中的后台任务
    /// </summary>
    public static void Stop()
    {
        Stop("Stop");
    }

    /// <summary>
    /// 启动指定的后台任务
    /// </summary>
    /// <param name="jobName">任务名称</param>
    public static void StartJob(string jobName)
    {
        var runningJob = _runningJobs.FirstOrDefault(u => JobInfoAttribute.GetJobName(u.Key).Equals(jobName, StringComparison.InvariantCultureIgnoreCase)).Value;
        if (runningJob != null)
        {
            runningJob.DynamicExecute("Start");
            JobInfosCache.Instance.Start(jobName);
            return;
        }

        var factoryItem = _jobFactories.FirstOrDefault(u => JobInfoAttribute.GetJobName(u.Key).Equals(jobName, StringComparison.InvariantCultureIgnoreCase));
        if (factoryItem.Value != null)
        {
            var jobInstance = factoryItem.Value();
            _runningJobs.TryAdd(factoryItem.Key, jobInstance);

            if (jobInstance is BaseBackgroundService bgService && !string.IsNullOrEmpty(bgService.Cron))
            {
                var dbConfig = JobConfigService.Instance.GetJobConfig(jobName);
                if (dbConfig != null)
                {
                    bgService.SetCronWithoutRestart(dbConfig.Cron);
                }
            }

            jobInstance.DynamicExecute("Start");
            JobInfosCache.Instance.Start(jobName);
        }
    }

    /// <summary>
    /// 停止指定的后台任务（支持超时）
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="timeoutMilliseconds">超时时间（毫秒），默认5000毫秒</param>
    public static void StopJob(string jobName, int timeoutMilliseconds = 5000)
    {
        // 从运行中的任务集合中获取任务
        var runningItem = _runningJobs.FirstOrDefault(u => JobInfoAttribute.GetJobName(u.Key).Equals(jobName, StringComparison.InvariantCultureIgnoreCase));
        if (runningItem.Value != null)
        {
            using var cts = new CancellationTokenSource(timeoutMilliseconds);

            try
            {
                // 优先调用带有CancellationToken的Stop方法
                try
                {
                    runningItem.Value.Stop(cts.Token);
                }
                catch (NotImplementedException)
                {
                    // 如果没有实现带有CancellationToken的Stop方法，调用原有的Stop方法
                    runningItem.Value.DynamicExecute("Stop");
                }

                // 更新任务状态
                JobInfosCache.Instance.Stop(jobName);

                // 从运行中的任务集合中移除
                _runningJobs.Remove(runningItem.Key);
            }
            catch (OperationCanceledException)
            {
                Logger.Warn($"任务 {jobName} 的Stop方法执行超时");
                // 即使超时也尝试从运行中的任务集合中移除
                _runningJobs.Remove(runningItem.Key);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"停止任务 {jobName} 失败");
                // 即使失败也尝试从运行中的任务集合中移除
                _runningJobs.Remove(runningItem.Key);
            }
        }
    }

    /// <summary>
    /// 获取指定任务当前的 Cron 表达式
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <returns>Cron 表达式，不存在或非 BaseBackgroundService 则返回 null</returns>
    public static string? GetJobCron(string jobName)
    {
        var job = GetJobInstance(jobName);
        return (job as BaseBackgroundService)?.Cron;
    }

    /// <summary>
    /// 获取指定任务下一次执行时间（本地时区）
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <returns>下一次执行时间，不存在或非 BaseBackgroundService 则返回 null</returns>
    public static DateTime? GetJobNextOccurrence(string jobName)
    {
        var job = GetJobInstance(jobName);
        return (job as BaseBackgroundService)?.GetNextOccurrence();
    }

    /// <summary>
    /// 动态更新运行中任务的 Cron 表达式
    /// </summary>
    /// <param name="jobName">任务名称</param>
    /// <param name="cron">6 段秒级 cron 表达式</param>
    public static void UpdateJobCron(string jobName, string cron)
    {
        var runningItem = _runningJobs.FirstOrDefault(u => JobInfoAttribute.GetJobName(u.Key).Equals(jobName, StringComparison.InvariantCultureIgnoreCase));
        if (runningItem.Value == null)
            throw new FriendlyException($"Job '{jobName}' 未运行，无法更新 Cron", ErrorCategory.Business);
        if (runningItem.Value is not BaseBackgroundService bgService)
            throw new FriendlyException($"Job '{jobName}' 不支持 Cron 调度", ErrorCategory.Business);
        bgService.SetCron(cron);
    }

    public static IJob? GetJobInstance(string jobName)
    {
        // 先从运行中的任务中查找
        var runningItem = _runningJobs.FirstOrDefault(u => JobInfoAttribute.GetJobName(u.Key).Equals(jobName, StringComparison.InvariantCultureIgnoreCase));
        if (runningItem.Value != null)
            return runningItem.Value;

        // 不在运行中，从工厂创建新实例
        var factoryItem = _jobFactories.FirstOrDefault(u => JobInfoAttribute.GetJobName(u.Key).Equals(jobName, StringComparison.InvariantCultureIgnoreCase));
        if (factoryItem.Value != null)
            return factoryItem.Value();

        return null;
    }

}
