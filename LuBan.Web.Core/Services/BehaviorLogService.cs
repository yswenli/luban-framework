/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.Web.Core.Services
*文件名： BehaviorLogService
*描述：用户行为日志服务
*
*****************************************************************************/

using LuBan.Common.LogCom;

namespace LuBan.Web.Core.Services;

/// <summary>
/// 用户行为日志服务
/// 采集侧统一走 <see cref="RecordAsync"/>，管理端查询走 <see cref="PageAsync"/> 等方法，
/// 实体与采集入口均留在框架，业务侧的差异只体现在 <see cref="DbBehaviorLog.Module"/> 与 <see cref="DbBehaviorLog.BehaviorType"/>。
/// </summary>
public class BehaviorLogService : BaseService<BehaviorLogService>
{
    /// <summary>
    /// IP列宽
    /// </summary>
    private const int IpMaxLength = 50;

    /// <summary>
    /// 写入计数器，用于按间隔触发容量裁剪
    /// </summary>
    private static long _writeCounter;

    /// <summary>
    /// 行为日志配置
    /// </summary>
    public static BehaviorLogOptions Options => HostingOptions.Default?.AppOptions?.BehaviorLog ?? new BehaviorLogOptions();

    /// <summary>
    /// 是否启用行为日志采集
    /// </summary>
    public static bool Enabled => Options.Enable;

    /// <summary>
    /// 整理IP，按列宽截断，避免超长导致写入失败
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static string? NormalizeIp(string? ip)
    {
        if (ip.IsNullOrWhiteSpace())
        {
            return null;
        }
        ip = ip.Trim();
        return ip.Length <= IpMaxLength ? ip : ip[..IpMaxLength];
    }

    /// <summary>
    /// 写入一条行为日志
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    public async Task<bool> RecordAsync(DbBehaviorLog log)
    {
        if (!Enabled || log == null)
        {
            return false;
        }

        try
        {
            using var repository = new BaseRepository<DbBehaviorLog>();
            if (log.TenantId == null || log.TenantId == 0)
            {
                log.TenantId = LuBanOrmConst.DefaultTenantId;
            }
            if (log.CreateTime == default)
            {
                log.CreateTime = DateTime.Now;
            }
            log.Ip = NormalizeIp(log.Ip);
            await repository.InsertReturnEntityAsync(log);
            await TryTrimAsync(repository);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Warn("写入用户行为日志失败", ex);
            return false;
        }
    }

    /// <summary>
    /// 写入一批行为日志
    /// </summary>
    /// <param name="logs"></param>
    /// <returns></returns>
    public async Task<bool> RecordAsync(IEnumerable<DbBehaviorLog> logs)
    {
        if (!Enabled || logs == null)
        {
            return false;
        }

        var list = logs.Where(u => u != null).ToList();
        if (list.Count == 0)
        {
            return false;
        }

        try
        {
            using var repository = new BaseRepository<DbBehaviorLog>();
            foreach (var log in list)
            {
                if (log.TenantId == null || log.TenantId == 0)
                {
                    log.TenantId = LuBanOrmConst.DefaultTenantId;
                }
                if (log.CreateTime == default)
                {
                    log.CreateTime = DateTime.Now;
                }
                log.Ip = NormalizeIp(log.Ip);
            }
            await repository.InsertRangeAsync(list);
            await TryTrimAsync(repository);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Warn("批量写入用户行为日志失败", ex);
            return false;
        }
    }

    /// <summary>
    /// 分页查询行为日志
    /// </summary>
    /// <param name="where">过滤条件</param>
    /// <param name="page">页码，从 1 开始</param>
    /// <param name="pageSize">页容量</param>
    /// <returns></returns>
    public async Task<PagedList<DbBehaviorLog>> PageAsync(Expression<Func<DbBehaviorLog, bool>>? where, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;

        using var repository = new BaseRepository<DbBehaviorLog>();
        RefAsync<int> total = 0;
        var items = await repository.AsQueryable()
            .WhereIF(where != null, where!)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .OrderBy(u => u.Id, OrderByType.Desc)
            .ToPageListAsync(page, pageSize, total);

        return items.ToPagedList(total, page, pageSize);
    }

    /// <summary>
    /// 获取行为日志详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<DbBehaviorLog?> GetAsync(long id)
    {
        if (id <= 0)
        {
            return null;
        }
        using var repository = new BaseRepository<DbBehaviorLog>();
        return await repository.FirstAsync(u => u.Id == id);
    }

    /// <summary>
    /// 统计行为日志条数
    /// </summary>
    /// <param name="where"></param>
    /// <returns></returns>
    public async Task<int> CountAsync(Expression<Func<DbBehaviorLog, bool>>? where = null)
    {
        using var repository = new BaseRepository<DbBehaviorLog>();
        return await repository.AsQueryable().WhereIF(where != null, where!).CountAsync();
    }

    /// <summary>
    /// 清空行为日志
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ClearAsync()
    {
        using var repository = new BaseRepository<DbBehaviorLog>();
        return await repository.DeleteAsync(u => u.Id > 0);
    }

    /// <summary>
    /// 按保留上限裁剪历史日志，写入每满 TrimInterval 条检查一次
    /// </summary>
    /// <param name="repository"></param>
    /// <returns></returns>
    private static async Task TryTrimAsync(BaseRepository<DbBehaviorLog> repository)
    {
        var maxRows = Options.MaxRows;
        if (maxRows <= 0)
        {
            return;
        }

        var interval = Options.TrimInterval < 1 ? 1 : Options.TrimInterval;
        if (Interlocked.Increment(ref _writeCounter) % interval != 0)
        {
            return;
        }

        try
        {
            var total = await repository.AsQueryable().CountAsync();
            if (total <= maxRows)
            {
                return;
            }

            var boundary = await repository.AsQueryable()
                .OrderBy(u => u.Id, OrderByType.Desc)
                .Skip(maxRows)
                .Take(1)
                .FirstAsync();
            if (boundary != null)
            {
                await repository.DeleteAsync(u => u.Id <= boundary.Id);
            }
        }
        catch (Exception ex)
        {
            Logger.Warn("裁剪用户行为日志失败", ex);
        }
    }
}