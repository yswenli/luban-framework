/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.Database
*文件名： DatabaseToolPlugin
*版本号： V1.0.0.0
*唯一标识：3c318a26-580c-4ecf-a218-8515f054a06c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：数据库工具插件
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：数据库工具插件
*
*****************************************************************************/
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using MySqlConnector;
using Npgsql;
using System.Data.SQLite;
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Tools.Database;

/// <summary>
/// 数据库工具插件
/// </summary>
public class DatabaseToolPlugin : ILuBanToolPlugin
{
    private readonly DatabaseToolOptions _options;

    /// <summary>
    /// 创建 DatabaseToolPlugin 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    public DatabaseToolPlugin(IOptions<LuBanAgentOptions> options)
    {
        _options = options.Value.Tools.Database;
    }

    /// <summary>
    /// 工具分组名称
    /// </summary>
    public string GroupName => "database";

    /// <summary>
    /// 工具分组描述
    /// </summary>
    public string? Description => "数据库操作工具，支持 MySQL、PostgreSQL、SQL Server、SQLite";

    /// <summary>
    /// 获取工具函数列表
    /// </summary>
    /// <param name="sp">服务提供者</param>
    /// <returns>工具函数列表</returns>
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp)
    {
        var confirmationService = sp.GetService(typeof(Services.IToolConfirmationService)) as Services.IToolConfirmationService
            ?? new Services.ToolConfirmationService(new Services.ToolConfirmationContext());
        var toolGroup = new DatabaseToolGroup(_options, confirmationService);
        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(toolGroup, nameof(DatabaseToolGroup.ExecuteQueryAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(DatabaseToolGroup.ExecuteNonQueryAsync))
        };
    }

    /// <summary>
    /// 判断插件是否启用
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <returns>是否启用</returns>
    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.Database.Enabled;
}

/// <summary>
/// 数据库工具分组
/// </summary>
public class DatabaseToolGroup
{
    private readonly DatabaseToolOptions _options;
    private readonly Services.IToolConfirmationService _confirmationService;

    /// <summary>
    /// 创建 DatabaseToolGroup 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="confirmationService">工具调用确认服务</param>
    public DatabaseToolGroup(DatabaseToolOptions options, Services.IToolConfirmationService confirmationService)
    {
        _options = options;
        _confirmationService = confirmationService;
    }

    /// <summary>
    /// 验证连接字符串基本格式。
    /// </summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns>合法返回 null，否则返回错误消息</returns>
    private static string? ValidateConnectionString(string connectionString)
    {
        // 长度限制：防止异常输入
        if (connectionString.Length > 2048)
            return "错误：连接字符串过长（最大 2048 字符）";

        // 必须包含键值对
        if (!connectionString.Contains('='))
            return "错误：连接字符串格式无效，缺少键值对（如 Server=host;Database=db;）";

        // 必须能识别数据库类型
        DatabaseType dbType;
        try
        {
            dbType = DetectDatabaseType(connectionString);
        }
        catch
        {
            return "错误：无法识别的数据库连接字符串，请检查格式";
        }

        var lower = connectionString.ToLowerInvariant();

        // SQLite 只需文件路径
        if (dbType == DatabaseType.SQLite)
            return null;

        // 网络型数据库必须包含服务器与数据库名
        var hasServer = lower.Contains("server=") || lower.Contains("host=") || lower.Contains("data source=");
        if (!hasServer)
            return "错误：连接字符串缺少服务器地址（Server= 或 Host=）";

        var hasDatabase = lower.Contains("database=") || lower.Contains("initial catalog=");
        if (!hasDatabase)
            return "错误：连接字符串缺少数据库名（Database= 或 Initial Catalog=）";

        return null;
    }

    /// <summary>
    /// 只读查询允许的语句起始关键字。
    /// 注：不允许 WITH（PostgreSQL 数据修改 CTE 可写）与 EXPLAIN（EXPLAIN ANALYZE 会真实执行写操作）。
    /// </summary>
    private static readonly string[] ReadOnlyPrefixes = { "select", "show", "describe", "desc" };

    /// <summary>
    /// 校验 SQL 是否为只读查询语句。
    /// </summary>
    /// <param name="sql">SQL 语句</param>
    /// <returns>合法返回 null，否则返回错误消息</returns>
    private static string? ValidateReadOnlySql(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return "错误：SQL 语句不能为空";

        // 去除前导注释与空白后取第一个关键字
        var trimmed = sql.TrimStart();
        while (trimmed.StartsWith("--") || trimmed.StartsWith("/*"))
        {
            if (trimmed.StartsWith("--"))
            {
                var newline = trimmed.IndexOf('\n');
                if (newline < 0) return "错误：SQL 语句不能为空";
                trimmed = trimmed.Substring(newline + 1).TrimStart();
            }
            else
            {
                var end = trimmed.IndexOf("*/", StringComparison.Ordinal);
                if (end < 0) return "错误：SQL 注释未闭合";
                trimmed = trimmed.Substring(end + 2).TrimStart();
            }
        }

        var firstWordEnd = trimmed.IndexOfAny(new[] { ' ', '\t', '\r', '\n', '(' });
        var firstWord = (firstWordEnd > 0 ? trimmed.Substring(0, firstWordEnd) : trimmed).ToLowerInvariant();

        if (!ReadOnlyPrefixes.Contains(firstWord))
        {
            return $"错误：ExecuteQuery 仅允许只读查询（SELECT/SHOW/DESCRIBE），检测到 '{firstWord.ToUpperInvariant()}'。如需执行写操作，请使用 ExecuteNonQuery（需用户确认）。";
        }

        // 阻止查询中夹带的写操作（分号后的第二条语句）
        foreach (var segment in trimmed.Split(';'))
        {
            var segWord = segment.TrimStart();
            if (segWord.Length == 0) continue;
            var segEnd = segWord.IndexOfAny(new[] { ' ', '\t', '\r', '\n', '(' });
            var segFirst = (segEnd > 0 ? segWord.Substring(0, segEnd) : segWord).ToLowerInvariant();
            if (!ReadOnlyPrefixes.Contains(segFirst))
            {
                return $"错误：检测到多语句中包含非只读操作 '{segFirst.ToUpperInvariant()}'，已拒绝执行。";
            }
        }

        return null;
    }

/// <summary>
/// 执行查询 SQL（SELECT）
/// </summary>
/// <param name="sql">SELECT SQL 语句</param>
/// <param name="connectionString">数据库连接字符串</param>
/// <returns>查询结果（JSON 格式）</returns>
[Description("执行查询 SQL（SELECT），返回结果集。需提供数据库连接字符串参数。")]
    public async Task<ToolResult<string>> ExecuteQueryAsync(string sql, string? connectionString = null)
    {
        var connStr = connectionString ?? _options.ConnectionString;
        if (string.IsNullOrEmpty(connStr))
            return ToolResult.Fail<string>("错误：未配置数据库连接字符串，请通过参数提供连接字符串");

        // 连接字符串格式验证
        var connError = ValidateConnectionString(connStr);
        if (connError != null)
            return ToolResult.Fail<string>(connError);

        // SQL 注入保护：仅允许只读查询语句
        var validationError = ValidateReadOnlySql(sql);
        if (validationError != null)
            return ToolResult.Fail<string>(validationError);

        try
        {
            using var connection = CreateConnection(connStr);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = _options.DefaultTimeout / 1000;

            using var reader = await command.ExecuteReaderAsync();
            var results = new List<Dictionary<string, object?>>();
            var columns = new List<string>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                columns.Add(reader.GetName(i));
            }

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[columns[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                results.Add(row);
            }

            return ToolResult.Ok<string>(new
            {
                success = true,
                columns,
                rows = results,
                rowCount = results.Count
            }.ToJson(hasIndentation: true));
        }
        catch (Exception ex)
        {
            Logger.Error("数据库查询执行失败", ex, sql);
            return ToolResult.Fail<string>($"数据库查询执行失败: {ex.Message}");
        }
    }

/// <summary>
/// 执行非查询 SQL（INSERT、UPDATE、DELETE、CREATE 等）
/// </summary>
/// <param name="sql">SQL 语句</param>
/// <param name="connectionString">数据库连接字符串</param>
/// <returns>执行结果（受影响的行数）</returns>
[Description("执行非查询 SQL（INSERT、UPDATE、DELETE、CREATE 等），返回受影响的行数。需提供数据库连接字符串参数。注意：此操作会修改数据。")]
    public async Task<ToolResult<string>> ExecuteNonQueryAsync(string sql, string? connectionString = null)
    {
        var connStr = connectionString ?? _options.ConnectionString;
        if (string.IsNullOrEmpty(connStr))
            return ToolResult.Fail<string>("错误：未配置数据库连接字符串，请通过参数提供连接字符串");

        // 连接字符串格式验证
        var connError = ValidateConnectionString(connStr);
        if (connError != null)
            return ToolResult.Fail<string>(connError);

        // 写操作需要用户确认
        if (!_confirmationService.RequestConfirmation("ExecuteNonQueryAsync",
            new Dictionary<string, object?> { ["sql"] = sql }))
        {
            return ToolResult.Cancelled<string>();
        }

        try
        {
            using var connection = CreateConnection(connStr);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = _options.DefaultTimeout / 1000;

            var affectedRows = await command.ExecuteNonQueryAsync();

            return ToolResult.Ok<string>(new
            {
                success = true,
                affectedRows,
                message = $"成功执行，受影响行数：{affectedRows}"
            }.ToJson());
        }
        catch (Exception ex)
        {
            Logger.Error("数据库非查询执行失败", ex, sql);
            return ToolResult.Fail<string>($"数据库非查询执行失败: {ex.Message}");
        }
    }

    private static DbConnection CreateConnection(string connectionString)
    {
        var dbType = DetectDatabaseType(connectionString);

        return dbType switch
        {
            DatabaseType.MySql => new MySqlConnection(connectionString),
            DatabaseType.PostgreSql => new NpgsqlConnection(connectionString),
            DatabaseType.SqlServer => new SqlConnection(connectionString),
            DatabaseType.SQLite => new SQLiteConnection(connectionString),
            _ => throw new NotSupportedException($"不支持的数据库类型：{dbType}")
        };
    }

    private static DatabaseType DetectDatabaseType(string connectionString)
    {
        var lower = connectionString.ToLower();

        if (lower.Contains("server=") || lower.Contains("data source=") && lower.Contains("initial catalog="))
        {
            if (lower.Contains("mysql") || lower.Contains("port=3306"))
                return DatabaseType.MySql;
            if (lower.Contains("postgresql") || lower.Contains("port=5432"))
                return DatabaseType.PostgreSql;
            return DatabaseType.SqlServer;
        }

        if (lower.Contains("host=") && (lower.Contains("port=5432") || lower.Contains("postgresql")))
            return DatabaseType.PostgreSql;

        if (lower.Contains("host=") && (lower.Contains("port=3306") || lower.Contains("mysql")))
            return DatabaseType.MySql;

        if (lower.Contains(".db") || lower.Contains(".sqlite") || lower.Contains("data source=") && !lower.Contains("server="))
            return DatabaseType.SQLite;

        if (lower.Contains("mysql"))
            return DatabaseType.MySql;

        if (lower.Contains("postgresql") || lower.Contains("postgres"))
            return DatabaseType.PostgreSql;

        return DatabaseType.SqlServer;
    }
}

/// <summary>
/// 数据库类型
/// </summary>
public enum DatabaseType
{
    SqlServer,
    MySql,
    PostgreSql,
    SQLite
}
