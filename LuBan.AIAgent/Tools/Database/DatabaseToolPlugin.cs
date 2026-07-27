namespace LuBan.AIAgent.Tools.Database;

/// <summary>
/// 数据库工具插件
/// </summary>
public class DatabaseToolPlugin : ILuBanToolPlugin
{
    private readonly DatabaseToolOptions _options;
    private readonly ProcessRunner _processRunner;

    /// <summary>
    /// 创建 DatabaseToolPlugin 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="processRunner">进程执行器</param>
    public DatabaseToolPlugin(IOptions<LuBanAgentOptions> options, ProcessRunner processRunner)
    {
        _options = options.Value.Tools.Database;
        _processRunner = processRunner;
    }

    /// <summary>
    /// 工具分组名称
    /// </summary>
    public string GroupName => "database";

    /// <summary>
    /// 工具分组描述
    /// </summary>
    public string? Description => "数据库操作工具，通过 sqlcmd 执行 SQL 语句";

    /// <summary>
    /// 获取工具函数列表
    /// </summary>
    /// <param name="sp">服务提供者</param>
    /// <returns>工具函数列表</returns>
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp)
    {
        var toolGroup = new DatabaseToolGroup(_options, _processRunner);
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
    private readonly ProcessRunner _processRunner;

    /// <summary>
    /// 创建 DatabaseToolGroup 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="processRunner">进程执行器</param>
    public DatabaseToolGroup(DatabaseToolOptions options, ProcessRunner processRunner)
    {
        _options = options;
        _processRunner = processRunner;
    }

    /// <summary>
    /// 执行 SQL 语句
    /// </summary>
    /// <param name="sql">SQL 语句</param>
    /// <returns>执行结果</returns>
    [Description("执行 SQL 语句")]
    public async Task<string> RunSqlAsync(string sql)
    {
        if (string.IsNullOrEmpty(_options.ConnectionString))
            return "错误：未配置数据库连接字符串";

        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, sql);
            var args = BuildSqlCmdArgs(tempFile);
            var result = await _processRunner.RunAsync(
                _options.Engine,
                args,
                timeoutMs: _options.DefaultTimeout);

            return JsonSerializer.Serialize(new
            {
                exitCode = result.ExitCode,
                stdout = result.StandardOutput,
                stderr = result.StandardError,
                durationMs = result.DurationMs,
                timedOut = result.TimedOut
            });
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    private string BuildSqlCmdArgs(string sqlFilePath)
    {
        var args = new StringBuilder();
        if (_options.Engine == "sqlcmd")
        {
            args.Append($"-i \"{sqlFilePath}\" ");
            if (!string.IsNullOrEmpty(_options.ConnectionString))
            {
                args.Append($"-S \"{_options.ConnectionString}\"");
            }
        }
        return args.ToString();
    }
}
