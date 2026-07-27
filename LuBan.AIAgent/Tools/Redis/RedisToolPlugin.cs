using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Infrastructure;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

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
        var tools = new List<AIFunction>();

        foreach (var method in typeof(RedisToolGroup).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
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
    public async Task<string> ExecAsync(string command)
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