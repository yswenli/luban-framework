/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Reporting.Dynamic.Services
*文件名： ReportConfigService
*版本号： V1.0.0.0
*唯一标识：
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/5
*描述：报表配置管理服务
*
*=================================================
*修改标记
*修改时间：2026/6/5
*修改人： yswenli
*版本号： V1.0.0.0
*描述：报表配置管理服务
*
*****************************************************************************/

namespace LuBan.Reporting.Dynamic.Services;

/// <summary>
/// 报表配置管理服务
/// </summary>
public class ReportConfigService
{
    private readonly DynamicReportRepository _repository;

    public ReportConfigService(DynamicReportRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 保存报表配置（新增/编辑）
    /// </summary>
    public async Task<long> SaveConfigAsync(DbReportConfig config)
    {
        if (config.Id > 0)
        {
            await _repository.UpdateConfigAsync(config);
            return config.Id;
        }
        else
        {
            return await _repository.InsertConfigAsync(config);
        }
    }

    /// <summary>
    /// 获取报表配置
    /// </summary>
    public async Task<DbReportConfig?> GetConfigAsync(long id)
    {
        return await _repository.GetConfigAsync(id);
    }

    /// <summary>
    /// 获取报表配置列表
    /// </summary>
    public async Task<List<DbReportConfig>> GetListAsync(string? keyword = null)
    {
        return await _repository.GetConfigListAsync(keyword);
    }

    /// <summary>
    /// 删除报表配置
    /// </summary>
    public async Task<bool> DeleteConfigAsync(long id)
    {
        // 删除配置及其关联的列配置
        await _repository.DeleteColumnConfigsByReportIdAsync(id);
        return await _repository.DeleteConfigAsync(id);
    }

    /// <summary>
    /// 批量保存列配置
    /// </summary>
    public async Task SaveColumnConfigsAsync(long reportConfigId, List<DbReportColumnConfig> columns)
    {
        // 先删除原有列配置
        await _repository.DeleteColumnConfigsByReportIdAsync(reportConfigId);

        // 设置关联ID并插入新列配置
        foreach (var col in columns)
        {
            col.ReportConfigId = reportConfigId;
        }
        await _repository.BatchInsertColumnConfigsAsync(columns);
    }

    /// <summary>
    /// 获取列配置
    /// </summary>
    public async Task<List<DbReportColumnConfig>> GetColumnConfigsAsync(long reportConfigId)
    {
        return await _repository.GetColumnConfigsAsync(reportConfigId);
    }

    /// <summary>
    /// 删除列配置
    /// </summary>
    public async Task<bool> DeleteColumnConfigsAsync(long reportConfigId)
    {
        return await _repository.DeleteColumnConfigsByReportIdAsync(reportConfigId);
    }
}
