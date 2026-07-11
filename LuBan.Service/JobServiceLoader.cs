/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
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

            if (_jobNamesFilter == null || _jobNamesFilter.Count < 1 || _jobNamesFilter.Contains(type.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                _jobFactories.TryAdd(type, () => (IJob)Activator.CreateInstance(type)!);
                JobInfosCache.Instance.Add(type.Name, "后台任务");
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
        if (_jobFactories != null && _jobFactories.Count > 0)
        {
            foreach (var factoryItem in _jobFactories)
            {
                // 延迟实例化，只在启动时创建实例
                if (!_runningJobs.ContainsKey(factoryItem.Key))
                {
                    var jobInstance = factoryItem.Value();
                    _runningJobs.TryAdd(factoryItem.Key, jobInstance);
                }
            }

            // 启动所有运行中的任务
            if (_runningJobs.Count > 0)
            {
                _ = _runningJobs.DynamicExecute(methodName, args);

                foreach (var item in _runningJobs)
                {
                    JobInfosCache.Instance.Start(item.Value.GetType().Name);
                }
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
                        var jobName = item.Value.GetType().Name;

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
                        Logger.Warn($"任务 {item.Key.Name} 的 {methodName} 方法执行超时");
                    }
                    catch (Exception ex)
                    {
                        // 其他异常，记录日志
                        Logger.Error(ex, $"任务 {item.Key.Name} 的 {methodName} 方法执行失败");
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
        // 先检查是否已经在运行中
        var runningJob = _runningJobs.FirstOrDefault(u => u.Key.Name.Equals(jobName, StringComparison.InvariantCultureIgnoreCase)).Value;
        if (runningJob != null)
        {
            runningJob.DynamicExecute("Start");
            JobInfosCache.Instance.Start(jobName);
            return;
        }

        // 如果不在运行中，检查工厂集合
        var factoryItem = _jobFactories.FirstOrDefault(u => u.Key.Name.Equals(jobName, StringComparison.InvariantCultureIgnoreCase));
        if (factoryItem.Value != null)
        {
            // 延迟实例化
            var jobInstance = factoryItem.Value();
            _runningJobs.TryAdd(factoryItem.Key, jobInstance);
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
        var runningItem = _runningJobs.FirstOrDefault(u => u.Key.Name.Equals(jobName, StringComparison.InvariantCultureIgnoreCase));
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

}
