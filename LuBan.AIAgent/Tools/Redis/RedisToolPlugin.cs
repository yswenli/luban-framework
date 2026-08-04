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
        var toolGroup = new RedisToolGroup(_options, _processRunner);
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

    /// <summary>
    /// 创建 RedisToolGroup 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="processRunner">进程执行器</param>
    public RedisToolGroup(RedisToolOptions options, ProcessRunner processRunner)
    {
        _options = options;
        _processRunner = processRunner;
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

            return ToolResult.Ok<string>(JsonSerializer.Serialize(new
            {
                exitCode = result.ExitCode,
                stdout = result.StandardOutput,
                stderr = result.StandardError,
                durationMs = result.DurationMs,
                timedOut = result.TimedOut
            }));
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            Logger.Error("Redis 执行失败：redis-cli 不存在", ex, "redis-cli");
            return ToolResult.Fail<string>("可执行文件不存在: redis-cli。请确保已安装 Redis 并将 redis-cli 配置到 PATH 环境变量。", JsonSerializer.Serialize(new
            {
                exitCode = -1,
                stdout = "",
                stderr = "可执行文件不存在: redis-cli。请确保已安装 Redis 并将 redis-cli 配置到 PATH 环境变量。",
                durationMs = 0,
                timedOut = false
            }));
        }
        catch (Exception ex)
        {
            Logger.Error("Redis 执行异常", ex, sanitizedCommand);
            return ToolResult.Fail<string>($"执行失败: {ex.Message}", JsonSerializer.Serialize(new
            {
                exitCode = -1,
                stdout = "",
                stderr = $"执行失败: {ex.Message}",
                durationMs = 0,
                timedOut = false
            }));
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
}
