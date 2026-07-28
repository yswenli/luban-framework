using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using MySqlConnector;
using Npgsql;
using System.Data.SQLite;

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
        var tools = new List<AIFunction>();

        foreach (var method in typeof(DatabaseToolGroup).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            var func = AIFunctionFactory.Create(method, toolGroup);
            tools.Add(func);
        }

        return tools;
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
    public async Task<string> ExecuteQueryAsync(string sql)
    {
        if (string.IsNullOrEmpty(_options.ConnectionString))
            return "错误：未配置数据库连接字符串";

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

            return JsonSerializer.Serialize(new
            {
                success = true,
                columns,
                rows = results,
                rowCount = results.Count
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// 执行非查询 SQL（INSERT、UPDATE、DELETE、CREATE 等）
    /// </summary>
    /// <param name="sql">SQL 语句</param>
    /// <returns>执行结果</returns>
    [Description("执行非查询 SQL（INSERT、UPDATE、DELETE、CREATE 等），返回受影响的行数")]
    public async Task<string> ExecuteNonQueryAsync(string sql)
    {
        if (string.IsNullOrEmpty(_options.ConnectionString))
            return "错误：未配置数据库连接字符串";

        try
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = _options.DefaultTimeout / 1000;

            var affectedRows = await command.ExecuteNonQueryAsync();

            return JsonSerializer.Serialize(new
            {
                success = true,
                affectedRows,
                message = $"成功执行，受影响行数：{affectedRows}"
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message
            });
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
