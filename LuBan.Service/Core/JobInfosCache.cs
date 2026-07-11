/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Service
*文件名： JobInfosCache
*版本号： V1.0.0.0
*唯一标识：1839f63b-a6df-4f62-93c2-f6238aab0004
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/9/11 14:48:51
*描述：任务信息缓存
*
*=================================================
*修改标记
*修改时间：2023/9/11 14:48:51
*修改人： yswenli
*版本号： V1.0.0.0
*描述：任务信息缓存
*
*****************************************************************************/

namespace LuBan.Service.Core;

/// <summary>
/// 任务信息缓存
/// </summary>
public class JobInfosCache : BaseSingleInstance<JobInfosCache>
{
    MemoryCache<JobInfo> _jobInfos;

    /// <summary>
    /// 任务信息缓存
    /// </summary>
    [Obsolete("请使用Instance属性初始化")]
    public JobInfosCache()
    {
        _jobInfos = MemoryCache<JobInfo>.Instance;
    }

    /// <summary>
    /// 任务信息缓存列表
    /// </summary>
    public List<JobInfo> List
    {
        get
        {
            return _jobInfos.ToList();
        }
    }

    /// <summary>
    /// 获取某个任务
    /// </summary>
    /// <param name="taskName"></param>
    /// <returns></returns>
    public JobInfo? this[string taskName]
    {
        get
        {
            if (_jobInfos.ContainsKey(taskName))
            {
                return _jobInfos[taskName];
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 添加记录
    /// </summary>
    /// <param name="taskName"></param>
    /// <param name="type"></param>
    public void Add(string taskName, string type)
    {
        using var locker = LockerBuilder.Default.Create("TaskInfosCache_" + taskName);
        if (this[taskName] == null)
        {
            _jobInfos.Set(taskName, new JobInfo
            {
                Name = taskName,
                Type = type,
                Status = "已初始化"
            });
        }
    }

    /// <summary>
    /// 启动
    /// </summary>
    /// <param name="taskName"></param>
    public void Start(string taskName)
    {
        using var locker = LockerBuilder.Default.Create("TaskInfosCache_" + taskName);
        var old = this[taskName];
        if (old != null)
        {
            if (old.Status != "运行中")
            {
                old.Status = "运行中";
                old.LastStarted = DateTime.Now;
            }
        }
    }

    /// <summary>
    /// 停止
    /// </summary>
    /// <param name="taskName"></param>
    public void Stop(string taskName)
    {
        using var locker = LockerBuilder.Default.Create("TaskInfosCache_" + taskName);
        var old = this[taskName];
        if (old != null)
        {
            if (old.Status == "运行中")
            {
                old.Status = "已停止";
                old.LastStoped = DateTime.Now;
            }
        }
    }

    /// <summary>
    /// 全部暂停、全部恢复
    /// </summary>
    /// <param name="isPause"></param>
    public void PauseAll(bool isPause)
    {
        var list = List;
        if (list != null && list.Count > 0)
        {
            foreach (var item in list)
            {
                Pause(item.Name, isPause);
            }
        }
    }
    /// <summary>
    /// 暂停
    /// </summary>
    /// <param name="taskName"></param>
    /// <param name="isPause"></param>
    public void Pause(string taskName, bool isPause)
    {
        using var locker = LockerBuilder.Default.Create("TaskInfosCache_" + taskName);
        var old = this[taskName];
        if (old != null)
        {
            if (isPause)
            {
                old.Status = "暂停中";
                old.LastStoped = DateTime.Now;
            }
            else
            {
                old.Status = "运行中";
                old.LastStarted = DateTime.Now;
            }
        }
    }

    /// <summary>
    /// 记录运行次数
    /// </summary>
    /// <param name="taskName"></param>
    public void RecordRun(string taskName)
    {
        using var locker = LockerBuilder.Default.Create("TaskInfosCache_" + taskName);
        var old = this[taskName];
        if (old != null)
        {
            old.RunTimes += 1;
        }
    }


    /// <summary>
    /// 记录异常
    /// </summary>
    /// <param name="taskName"></param>
    /// <param name="err"></param>
    public void RecordError(string taskName, string err)
    {
        using var locker = LockerBuilder.Default.Create("TaskInfosCache_" + taskName);
        var old = this[taskName];
        if (old != null)
        {
            old.ErrorTimes += 1;
            old.LastError = err;
        }
    }
    /// <summary>
    /// 检查暂停情况
    /// </summary>
    /// <param name="taskName"></param>
    public void CheckPause(string taskName)
    {
        using var locker = LockerBuilder.Default.Create("TaskInfosCache_" + taskName);
        var old = this[taskName];
        if (old != null && old.Status == "暂停中")
        {
            old.Status = "已暂停";
        }
    }


}
