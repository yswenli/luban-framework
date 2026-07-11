using WebApplication1.Models;

namespace WebApplication1.Services.JobServices;

/// <summary>
/// 在线用户过期记录清理作业
/// </summary>
[JobInfo("在线用户清理", "清理数据库中已过期的在线用户记录")]
public sealed class OnlineUserCleanupJob : BaseJobService
{
    /// <summary>
    /// 在线用户过期记录清理作业
    /// </summary>
    public OnlineUserCleanupJob() : base(5 * 60 * 1000)
    {
    }

    /// <summary>
    /// 清理过期记录
    /// </summary>
    public override async Task RunAsync()
    {
        if (!HostingOptions.Default.AppOptions.OnlineUser.Enabled)
        {
            return;
        }

        if (HostingOptions.Default.EnableRedisCache)
        {
            return;
        }

        var expireSeconds = HostingOptions.Default.AppOptions.JwtAuthConfig?.AccessExpiration ?? 7200;
        var expireTime = DateTime.UtcNow.AddSeconds(-expireSeconds);

        var configs = LuBanOrm.DbConnectionOptions?.ConnectionConfigs;
        if (configs == null)
        {
            return;
        }

        foreach (var config in configs)
        {
            if (config.ConfigId?.ToString() == LuBanOrmConst.LogConfigId)
            {
                continue;
            }

            if (!long.TryParse(config.ConfigId?.ToString(), out var tenantId))
            {
                continue;
            }

            try
            {
                var rep = new BaseRepository<DbOnlineUser>(tenantId);
                await rep.AsUpdateable()
                    .SetColumns(q => new DbOnlineUser { IsDelete = true })
                    .Where(q => q.IsDelete == false && q.LastActiveTime < expireTime)
                    .ExecuteCommandAsync();
            }
            catch (Exception ex)
            {
                //因为这里配置的数据连接可能是无效的，所以这里捕获异常，记录日志即可
                Logger.Info($"清理过期在线用户失败：{config.ConfigId}", ex);
            }
        }
    }
}
