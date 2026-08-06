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
        var toolGroup = new DatabaseToolGroup(_options);
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

    /// <summary>
    /// 创建 DatabaseToolGroup 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    public DatabaseToolGroup(DatabaseToolOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// 执行查询 SQL（SELECT）
    /// </summary>
    /// <param name="sql">SELECT SQL 语句</param>
    /// <returns>查询结果（JSON 格式）</returns>
    [Description("执行查询 SQL（SELECT），返回结果集")]
    public async Task<ToolResult<string>> ExecuteQueryAsync(string sql)
    {
        if (string.IsNullOrEmpty(_options.ConnectionString))
            return ToolResult.Fail<string>("错误：未配置数据库连接字符串");

        try
        {
            using var connection = CreateConnection();
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
    /// <returns>执行结果</returns>
    [Description("执行非查询 SQL（INSERT、UPDATE、DELETE、CREATE 等），返回受影响的行数")]
    public async Task<ToolResult<string>> ExecuteNonQueryAsync(string sql)
    {
        if (string.IsNullOrEmpty(_options.ConnectionString))
            return ToolResult.Fail<string>("错误：未配置数据库连接字符串");

        try
        {
            using var connection = CreateConnection();
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

    private DbConnection CreateConnection()
    {
        var connectionString = _options.ConnectionString!;
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
