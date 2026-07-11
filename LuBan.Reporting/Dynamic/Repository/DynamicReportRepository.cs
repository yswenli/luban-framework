/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Reporting.Dynamic.Repository
*文件名： DynamicReportRepository
*版本号： V1.0.0.0
*唯一标识：
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/5
*描述：动态报表数据访问层
*
*=================================================
*修改标记
*修改时间：2026/6/5
*修改人： yswenli
*版本号： V1.0.0.0
*描述：动态报表数据访问层
*
*****************************************************************************/

namespace LuBan.Reporting.Dynamic.Repository;

/// <summary>
/// 动态报表数据访问层
/// </summary>
public class DynamicReportRepository : BaseRepository<DbReportConfig>
{
    public DynamicReportRepository(long tenantId = LuBanOrmConst.DefaultTenantId)
        : base(tenantId)
    {
    }

    /// <summary>
    /// 获取报表配置
    /// </summary>
    public async Task<DbReportConfig?> GetConfigAsync(long id)
    {
        return await GetFirstAsync(r => r.Id == id);
    }

    /// <summary>
    /// 获取报表配置列表
    /// </summary>
    public async Task<List<DbReportConfig>> GetConfigListAsync(string? keyword = null)
    {
        var query = AsQueryable();
        if (!keyword.IsNullOrEmpty())
        {
            query = query.Where(r => r.Name.Contains(keyword!) || r.Description!.Contains(keyword!));
        }
        return await query.OrderByDescending(r => r.Id).ToListAsync();
    }

    /// <summary>
    /// 插入报表配置
    /// </summary>
    public async Task<long> InsertConfigAsync(DbReportConfig config)
    {
        return await InsertReturnEntityAsync(config).ContinueWith(t => t.Result.Id);
    }

    /// <summary>
    /// 更新报表配置
    /// </summary>
    public async Task<bool> UpdateConfigAsync(DbReportConfig config)
    {
        return await UpdateAsync(config);
    }

    /// <summary>
    /// 删除报表配置
    /// </summary>
    public async Task<bool> DeleteConfigAsync(long id)
    {
        return await LogicDeleteByIdAsync(id);
    }

    /// <summary>
    /// 执行报表 SQL（带参数、带行数限制）
    /// </summary>
    public async Task<DataTable> ExecuteReportSqlAsync(
        string sql,
        Dictionary<string, object>? parameters,
        int? maxRows)
    {
        var client = AsSugarClient();

        // 清理 SQL 末尾的分号，防止嵌套查询时语法错误
        sql = sql.TrimEnd(';').TrimEnd();

        // 参数化查询，防止 SQL 注入
        var sugarParams = parameters?
            .Select(p => new SugarParameter($"@{p.Key}", p.Value))
            .ToArray();

        // 预览时拼接行数限制
        if (maxRows.HasValue)
        {
            sql = ApplyRowLimit(sql, maxRows.Value, client.CurrentConnectionConfig.DbType);
        }

        return await client.Ado.GetDataTableAsync(sql, sugarParams);
    }

    /// <summary>
    /// 根据数据库类型拼接行数限制
    /// </summary>
    private string ApplyRowLimit(string sql, int maxRows, SqlSugar.DbType dbType)
    {
        return dbType switch
        {
            // SQL Server: 使用TOP避免子查询列名冲突
            SqlSugar.DbType.SqlServer => $"SELECT TOP {maxRows} * FROM ({sql}) AS _t",
            // MySQL: 使用LIMIT子查询，但需添加AS避免列名冲突警告
            SqlSugar.DbType.MySql => $"SELECT * FROM ({sql}) AS _t LIMIT {maxRows}",
            // PostgreSQL: 使用LIMIT子查询
            SqlSugar.DbType.PostgreSQL => $"SELECT * FROM ({sql}) AS _t LIMIT {maxRows}",
            // Oracle: 使用ROWNUM限制
            SqlSugar.DbType.Oracle => $"SELECT * FROM ({sql}) _t WHERE ROWNUM <= {maxRows}",
            _ => sql
        };
    }

    /// <summary>
    /// 获取列配置
    /// </summary>
    public async Task<List<DbReportColumnConfig>> GetColumnConfigsAsync(long reportConfigId)
    {
        var client = AsSugarClient();
        return await client.Queryable<DbReportColumnConfig>()
            .Where(c => c.ReportConfigId == reportConfigId)
            .OrderBy(c => c.SortNo)
            .ToListAsync();
    }

    /// <summary>
    /// 插入列配置
    /// </summary>
    public async Task<long> InsertColumnConfigAsync(DbReportColumnConfig column)
    {
        var client = AsSugarClient();
        return await client.Insertable(column).ExecuteReturnSnowflakeIdAsync();
    }

    /// <summary>
    /// 删除列配置
    /// </summary>
    public async Task<bool> DeleteColumnConfigsByReportIdAsync(long reportConfigId)
    {
        var client = AsSugarClient();
        return await client.Deleteable<DbReportColumnConfig>()
            .Where(c => c.ReportConfigId == reportConfigId)
            .ExecuteCommandHasChangeAsync();
    }

    /// <summary>
    /// 批量插入列配置
    /// </summary>
    public async Task<int> BatchInsertColumnConfigsAsync(List<DbReportColumnConfig> columns)
    {
        var client = AsSugarClient();
        return await client.Insertable(columns).ExecuteCommandAsync();
    }
}
