namespace LuBan.AIAgent.Tools.Script;

/// <summary>
/// 脚本工具插件
/// </summary>
public class ScriptToolPlugin : ILuBanToolPlugin
{
    private readonly ScriptToolOptions _options;
    private readonly ProcessRunner _processRunner;

    /// <summary>
    /// 创建 ScriptToolPlugin 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="processRunner">进程执行器</param>
    public ScriptToolPlugin(IOptions<LuBanAgentOptions> options, ProcessRunner processRunner)
    {
        _options = options.Value.Tools.Script;
        _processRunner = processRunner;
    }

    /// <summary>
    /// 工具分组名称
    /// </summary>
    public string GroupName => "script";

    /// <summary>
    /// 工具分组描述
    /// </summary>
    public string? Description => "脚本执行工具，支持 Shell、Lua、Python 等脚本执行";

    /// <summary>
    /// 获取工具函数列表
    /// </summary>
    /// <param name="sp">服务提供者</param>
    /// <returns>工具函数列表</returns>
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp)
    {
        var toolGroup = new ScriptToolGroup(_options, _processRunner);
        var tools = new List<AIFunction>();

        foreach (var method in typeof(ScriptToolGroup).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
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
    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.Script.Enabled;
}

/// <summary>
/// 脚本工具分组
/// </summary>
public class ScriptToolGroup
{
    private readonly ScriptToolOptions _options;
    private readonly ProcessRunner _processRunner;

    /// <summary>
    /// 创建 ScriptToolGroup 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="processRunner">进程执行器</param>
    public ScriptToolGroup(ScriptToolOptions options, ProcessRunner processRunner)
    {
        _options = options;
        _processRunner = processRunner;
    }

    /// <summary>
    /// 执行 Shell 命令
    /// </summary>
    /// <param name="command">要执行的命令</param>
    /// <param name="workingDirectory">工作目录（可选）</param>
    /// <returns>执行结果</returns>
    [Description("执行 Shell 命令")]
    public async Task<string> RunShellAsync(string command, string? workingDirectory = null)
    {
        // 确认执行
        if (!ToolConfirmationService.RequestConfirmation("RunShellAsync", 
            new Dictionary<string, object?> { ["command"] = command, ["workingDirectory"] = workingDirectory }))
        {
            return "操作已被用户取消";
        }

        var result = await _processRunner.RunAsync(
            _options.Shell,
            "-c",
            workingDirectory,
            stdin: command,
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

    /// <summary>
    /// 执行 Lua 脚本
    /// </summary>
    /// <param name="script">Lua 脚本内容</param>
    /// <param name="workingDirectory">工作目录（可选）</param>
    /// <returns>执行结果</returns>
    [Description("执行 Lua 脚本")]
    public async Task<string> RunLuaAsync(string script, string? workingDirectory = null)
    {
        // 确认执行
        if (!ToolConfirmationService.RequestConfirmation("RunLuaAsync", 
            new Dictionary<string, object?> { ["script"] = script, ["workingDirectory"] = workingDirectory }))
        {
            return "操作已被用户取消";
        }

        var result = await _processRunner.RunAsync(
            _options.LuaPath,
            "-e",
            workingDirectory,
            stdin: script,
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

    /// <summary>
    /// 执行 Python 脚本
    /// </summary>
    /// <param name="script">Python 脚本内容</param>
    /// <param name="workingDirectory">工作目录（可选）</param>
    /// <returns>执行结果</returns>
    [Description("执行 Python 脚本")]
    public async Task<string> RunPythonAsync(string script, string? workingDirectory = null)
    {
        // 确认执行
        if (!ToolConfirmationService.RequestConfirmation("RunPythonAsync", 
            new Dictionary<string, object?> { ["script"] = script, ["workingDirectory"] = workingDirectory }))
        {
            return "操作已被用户取消";
        }

        var result = await _processRunner.RunAsync(
            _options.PythonPath,
            "-c",
            workingDirectory,
            stdin: script,
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
}
