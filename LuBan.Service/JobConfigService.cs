/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Service
*文件名： JobConfigService
*版本号： V1.0.0.0
*唯一标识：00000000-0000-0000-0000-000000000007
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/09/11 00:00:00
*描述：作业配置服务
*
*=================================================
*修改标记
*修改时间：2026/09/11 00:00:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：作业配置服务
*
*****************************************************************************/
namespace LuBan.Service;

/// <summary>
/// 作业配置服务
/// </summary>
public class JobConfigService : BaseService<JobConfigService>
{
    /// <summary>
    /// 初始化作业配置（存在则返回已有配置，不存在则插入默认配置）
    /// </summary>
    /// <param name="jobName">作业名称</param>
    /// <param name="defaultCron">默认 Cron 表达式</param>
    /// <returns>作业配置</returns>
    public DbJobInfo? InitJobConfig(string jobName, string defaultCron)
    {
        using var resp = new BaseRepository<DbJobInfo>();
        var existing = resp.GetFirst(q => q.Name == jobName);
        if (existing != null)
            return existing;

        var config = new DbJobInfo
        {
            Name = jobName,
            Cron = defaultCron,
            IsEnabled = true,
            CreateTime = DateTime.Now
        };
        resp.Insert(config);
        return config;
    }

    /// <summary>
    /// 获取作业配置
    /// </summary>
    /// <param name="jobName">作业名称</param>
    /// <returns>作业配置，不存在返回 null</returns>
    public DbJobInfo? GetJobConfig(string jobName)
    {
        using var resp = new BaseRepository<DbJobInfo>();
        return resp.GetFirst(q => q.Name == jobName);
    }

    /// <summary>
    /// 获取所有作业配置
    /// </summary>
    /// <returns>作业配置列表</returns>
    public List<DbJobInfo> GetAllJobConfigs()
    {
        using var resp = new BaseRepository<DbJobInfo>();
        return resp.AsQueryable().ToList();
    }

    /// <summary>
    /// 更新作业配置
    /// </summary>
    /// <param name="name">作业名称</param>
    /// <param name="cron">Cron 表达式</param>
    /// <param name="isEnabled">是否启用</param>
    /// <param name="remark">备注</param>
    public void UpdateJobConfig(string name, string? cron, bool? isEnabled, string? remark)
    {
        if (cron != null)
        {
            try
            {
                CronExpression.Parse(cron, CronFormat.IncludeSeconds);
            }
            catch (CronFormatException)
            {
                throw new FriendlyException($"Cron 表达式无效：{cron}", ErrorCategory.Business);
            }
        }

        using var resp = new BaseRepository<DbJobInfo>();
        var existing = resp.GetFirst(q => q.Name == name);
        if (existing == null)
            throw new FriendlyException($"作业 '{name}' 的配置不存在", ErrorCategory.Business);

        if (cron != null)
            existing.Cron = cron;
        if (isEnabled.HasValue)
            existing.IsEnabled = isEnabled.Value;
        if (remark != null)
            existing.Remark = remark;
        existing.UpdateTime = DateTime.Now;

        resp.Update(existing);
    }
}