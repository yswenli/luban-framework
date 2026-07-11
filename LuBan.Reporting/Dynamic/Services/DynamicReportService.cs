/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Reporting.Dynamic.Services
*文件名： DynamicReportService
*版本号： V1.0.0.0
*唯一标识：
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/5
*描述：动态报表服务
*
*=================================================
*修改标记
*修改时间：2026/6/5
*修改人： yswenli
*版本号： V1.0.0.0
*描述：动态报表服务
*
*****************************************************************************/

namespace LuBan.Reporting.Dynamic.Services;

/// <summary>
/// 动态报表服务
/// </summary>
public class DynamicReportService
{
    private readonly DynamicReportRepository _repository;
    private readonly LuaScriptEngine _luaEngine;

    public DynamicReportService(DynamicReportRepository repository, LuaScriptEngine luaEngine)
    {
        _repository = repository;
        _luaEngine = luaEngine;
    }

    /// <summary>
    /// 预览报表（前 N 行）
    /// </summary>
    public async Task<DataTable> PreviewAsync(
        long reportConfigId,
        Dictionary<string, object>? sqlParams = null,
        int? previewRows = null)
    {
        // 1. 加载配置
        var config = await _repository.GetConfigAsync(reportConfigId)
            ?? throw new Exception($"报表配置不存在: {reportConfigId}");
        var columns = await _repository.GetColumnConfigsAsync(reportConfigId);

        // 2. SQL 模板直接作为纯 SQL 执行，参数通过参数化查询传递
        var sql = config.SqlTemplate ?? string.Empty;

        // 3. 执行 SQL（带行数限制）
        var rows = previewRows ?? config.PreviewRows;
        var dt = await _repository.ExecuteReportSqlAsync(sql, sqlParams, rows);

        // 4. 检查是否有重复列名
        CheckDuplicateColumnNames(dt);

        // 5. 应用列转换（无列配置时直接返回原始SQL结果）
        if (columns == null || columns.Count == 0)
            return dt;

        return ApplyColumnMapping(dt, columns);
    }

    /// <summary>
    /// 执行原生SQL（不经过列转换，用于解析列名）
    /// </summary>
    public async Task<DataTable> ExecuteRawSqlAsync(
        string sql,
        Dictionary<string, object>? sqlParams = null,
        int? maxRows = null)
    {
        var dt = await _repository.ExecuteReportSqlAsync(sql, sqlParams, maxRows);
        
        // 检查是否有重复列名
        CheckDuplicateColumnNames(dt);
        
        return dt;
    }
    
    /// <summary>
    /// 检查是否有重复列名
    /// </summary>
    private void CheckDuplicateColumnNames(DataTable dt)
    {
        var columnNames = dt.Columns.Cast<DataColumn>()
            .Select(c => c.ColumnName)
            .ToList();
            
        var duplicates = columnNames
            .GroupBy(name => name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
            
        if (duplicates.Any())
        {
            throw new Exception($"SQL查询结果存在重复列名 [{string.Join(", ", duplicates)}]，请使用别名区分。例如：SELECT a.id, b.id AS user_id FROM table1 a JOIN table2 b ON a.id = b.user_id");
        }
    }

    /// <summary>
    /// 导出报表到流
    /// </summary>
    public async Task<Stream> ExportAsync(
        long reportConfigId,
        Dictionary<string, object>? sqlParams,
        ExportFormat format)
    {
        // 1. 加载配置
        var config = await _repository.GetConfigAsync(reportConfigId)
            ?? throw new Exception($"报表配置不存在: {reportConfigId}");
        var columns = await _repository.GetColumnConfigsAsync(reportConfigId);

        // 2. SQL 模板直接作为纯 SQL 执行，参数通过参数化查询传递
        var sql = config.SqlTemplate ?? string.Empty;

        // 3. 执行 SQL（全量）
        var dt = await _repository.ExecuteReportSqlAsync(sql, sqlParams, null);
        
        // 4. 检查是否有重复列名
        CheckDuplicateColumnNames(dt);

        // 5. 应用列转换
        var mappedDt = ApplyColumnMapping(dt, columns);

        // 5. 导出
        return format switch
        {
            ExportFormat.CSV => CsvUtil.ExportFromDataTable(mappedDt),
            ExportFormat.Excel => ExcelUtil.ExportStreamFromDataTable(mappedDt,
                $"{config.Name}_{DateTime.Now:yyyyMMddHHmmss}.xlsx"),
            _ => throw new NotSupportedException($"不支持的导出格式: {format}")
        };
    }

    /// <summary>
    /// 导出报表到文件
    /// </summary>
    public async Task ExportToFileAsync(
        long reportConfigId,
        Dictionary<string, object>? sqlParams,
        ExportFormat format,
        string filePath)
    {
        using var stream = await ExportAsync(reportConfigId, sqlParams, format);
        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fs);
    }

    /// <summary>
    /// 应用列映射和转换
    /// </summary>
    private DataTable ApplyColumnMapping(DataTable sourceDt, List<DbReportColumnConfig> columns)
    {
        // 无可见列时直接返回原始数据
        var visibleColumns = columns.Where(c => c.IsVisible).OrderBy(c => c.SortNo).ToList();
        if (!visibleColumns.Any())
            return sourceDt;

        var resultDt = new DataTable();

        // 创建目标列
        foreach (var col in visibleColumns)
        {
            resultDt.Columns.Add(col.DisplayName, typeof(string));
        }

        // 填充数据
        foreach (DataRow sourceRow in sourceDt.Rows)
        {
            var newRow = resultDt.NewRow();
            int colIndex = 0;

            foreach (var col in visibleColumns)
            {
                var rawValue = sourceDt.Columns.Contains(col.ColumnName)
                    ? sourceRow[col.ColumnName]
                    : null;

                var displayValue = ConvertValue(col, rawValue);
                newRow[colIndex] = displayValue;
                colIndex++;
            }

            resultDt.Rows.Add(newRow);
        }

        return resultDt;
    }

    /// <summary>
    /// 值转换
    /// </summary>
    private string ConvertValue(DbReportColumnConfig col, object? value)
    {
        if (value == null || value == DBNull.Value) return "";

        try
        {
            return col.ConverterType switch
            {
                "ValueMap" => ApplyValueMap(col.ConverterConfig, value),
                "LuaScript" => _luaEngine.ConvertValue("", col.ConverterConfig ?? "", value),
                _ => value.ToString() ?? ""
            };
        }
        catch (Exception ex)
        {
            // Lua 脚本执行失败时返回空字符串，记录日志
            Console.WriteLine($"[ERROR] 值转换失败: {ex.Message}");
            return "";
        }
    }

    /// <summary>
    /// ValueMap 转换
    /// </summary>
    private string ApplyValueMap(string? converterConfig, object value)
    {
        if (converterConfig.IsNullOrEmpty()) return value.ToString() ?? "";

        try
        {
            var map = JsonSerializer.Deserialize<Dictionary<string, string>>(converterConfig!);
            return map?.TryGetValue(value.ToString() ?? "", out var result) == true
                ? result
                : value.ToString() ?? "";
        }
        catch
        {
            return value.ToString() ?? "";
        }
    }
}
