/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.Redis
*文件名： RedisToolPlugin
*版本号： V1.0.0.0
*唯一标识：47a5c6cb-a532-4fe8-8023-9ae0072fb910
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：Redis 工具插件
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Redis 工具插件
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Tools.Redis;

/// <summary>
/// Redis 工具插件
/// </summary>
public class RedisToolPlugin : ILuBanToolPlugin
{
    private readonly RedisToolOptions _options;
    private readonly ProcessRunner _processRunner;

    /// <summary>
    /// 创建 RedisToolPlugin 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="processRunner">进程执行器</param>
    public RedisToolPlugin(IOptions<LuBanAgentOptions> options, ProcessRunner processRunner)
    {
        _options = options.Value.Tools.Redis;
        _processRunner = processRunner;
    }

    /// <summary>
    /// 工具分组名称
    /// </summary>
    public string GroupName => "redis";

    /// <summary>
    /// 工具分组描述
    /// </summary>
    public string? Description => "Redis 操作工具，通过 redis-cli 执行 Redis 命令";

    /// <summary>
    /// 获取工具函数列表
    /// </summary>
    /// <param name="sp">服务提供者</param>
    /// <returns>工具函数列表</returns>
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp)
    {
        var confirmationService = sp.GetService(typeof(Services.IToolConfirmationService)) as Services.IToolConfirmationService
            ?? new Services.ToolConfirmationService(new Services.ToolConfirmationContext());
        var toolGroup = new RedisToolGroup(_options, _processRunner, confirmationService);
        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(toolGroup, nameof(RedisToolGroup.ExecAsync))
        };
    }

    /// <summary>
    /// 判断插件是否启用
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <returns>是否启用</returns>
    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.Redis.Enabled;
}

/// <summary>
/// Redis 工具分组
/// </summary>
public class RedisToolGroup
{
    private readonly RedisToolOptions _options;
    private readonly ProcessRunner _processRunner;
    private readonly Services.IToolConfirmationService _confirmationService;

    /// <summary>
    /// 创建 RedisToolGroup 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="processRunner">进程执行器</param>
    /// <param name="confirmationService">工具调用确认服务</param>
    public RedisToolGroup(RedisToolOptions options, ProcessRunner processRunner, Services.IToolConfirmationService confirmationService)
    {
        _options = options;
        _processRunner = processRunner;
        _confirmationService = confirmationService;
    }

    /// <summary>
    /// 执行 Redis 命令
    /// </summary>
    /// <param name="command">Redis 命令</param>
    /// <returns>执行结果</returns>
    [Description("执行 Redis 命令")]
    public async Task<ToolResult<string>> ExecAsync(string command)
    {
        var sanitizedCommand = SanitizeRedisCommand(command);

        // 拒绝可能重定向连接的 redis-cli flag（防止 REDISCLI_AUTH 凭据泄露到第三方主机）
        foreach (var token in sanitizedCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.StartsWith("--") || token is "-h" or "-p" or "-a" or "-u" or "-s")
            {
                return ToolResult.Fail<string>($"错误：命令中包含不允许的参数 '{token}'，redis-cli 连接参数由工具统一配置。");
            }
        }

        // 写操作与危险命令需要用户确认（GET/KEYS/INFO 等只读命令免确认）
        if (RequiresConfirmation(sanitizedCommand))
        {
            if (!_confirmationService.RequestConfirmation("ExecAsync",
                new Dictionary<string, object?> { ["command"] = command }))
            {
                return ToolResult.Cancelled<string>();
            }
        }
        var args = $"-h {_options.Host} -p {_options.Port} --no-auth-warning";

        var envBackup = Environment.GetEnvironmentVariable("REDISCLI_AUTH");
        try
        {
            if (!string.IsNullOrEmpty(_options.Password))
            {
                Environment.SetEnvironmentVariable("REDISCLI_AUTH", _options.Password);
            }

            var result = await _processRunner.RunAsync(
                "redis-cli",
                $"{args} {sanitizedCommand}",
                timeoutMs: 30000);

            return ToolResult.Ok<string>(new
            {
                exitCode = result.ExitCode,
                stdout = result.StandardOutput,
                stderr = result.StandardError,
                durationMs = result.DurationMs,
                timedOut = result.TimedOut
            }.ToJson());
        }
        catch (Exception ex)
        {
            Logger.Error("Redis 执行异常", ex, sanitizedCommand);
            return ToolResult.Fail<string>($"执行失败: {ex.Message}", new
            {
                exitCode = -1,
                stdout = "",
                stderr = $"执行失败: {ex.Message}",
                durationMs = 0,
                timedOut = false
            }.ToJson());
        }
        finally
        {
            Environment.SetEnvironmentVariable("REDISCLI_AUTH", envBackup);
        }
    }

    private static string SanitizeRedisCommand(string command)
    {
        return command
            .Replace("\n", " ")
            .Replace("\r", " ")
            .Replace("&", " ")
            .Replace("|", " ")
            .Replace(";", " ")
            .Replace("`", " ")
            .Replace("$(", " ");
    }

    /// <summary>
    /// 只读命令白名单，免确认
    /// </summary>
    private static readonly HashSet<string> ReadOnlyCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET", "MGET", "KEYS", "SCAN", "EXISTS", "TYPE", "TTL", "PTTL",
        "STRLEN", "LLEN", "SCARD", "HLEN", "HGET", "HGETALL", "HMGET",
        "LRANGE", "SMEMBERS", "SISMEMBER", "ZRANGE", "ZSCORE", "ZCARD",
        "INFO", "DBSIZE", "PING", "TIME", "RANDOMKEY", "DUMP"
    };

    /// <summary>
    /// 判断 Redis 命令是否需要用户确认（非只读命令均需确认）。
    /// </summary>
    private static bool RequiresConfirmation(string command)
    {
        var firstWord = command.TrimStart().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (string.IsNullOrEmpty(firstWord)) return true;
        return !ReadOnlyCommands.Contains(firstWord);
    }
}
