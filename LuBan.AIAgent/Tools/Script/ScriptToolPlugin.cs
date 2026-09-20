/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.Script
*文件名： ScriptToolPlugin
*版本号： V1.0.0.0
*唯一标识：10c2d395-bdc9-4643-a22c-8ef337c4cbb9
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：脚本工具插件
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：脚本工具插件
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Tools.Script;

/// <summary>
/// 脚本工具插件
/// </summary>
public class ScriptToolPlugin : ILuBanToolPlugin
{
    private readonly ScriptToolOptions _options;
    private readonly ProcessRunner _processRunner;
    private readonly LuaScriptRunner _luaRunner;
    private readonly ShellEnvironmentDetector _shellDetector;

    /// <summary>
    /// 创建 ScriptToolPlugin 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="processRunner">进程执行器</param>
    /// <param name="luaRunner">内嵌 Lua 沙箱执行器</param>
    /// <param name="shellDetector">Shell 环境探测器</param>
    public ScriptToolPlugin(
        IOptions<LuBanAgentOptions> options,
        ProcessRunner processRunner,
        LuaScriptRunner luaRunner,
        ShellEnvironmentDetector shellDetector)
    {
        _options = options.Value.Tools.Script;
        _processRunner = processRunner;
        _luaRunner = luaRunner;
        _shellDetector = shellDetector;
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
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp, ToolGroupOptions? toolsOptions = null)
    {
        var opts = toolsOptions?.Script ?? _options;
        var confirmationService = sp.GetRequiredService<IToolConfirmationService>();
        var toolGroup = new ScriptToolGroup(opts, _processRunner, _luaRunner, _shellDetector, confirmationService);
        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(toolGroup, nameof(ScriptToolGroup.RunShellAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(ScriptToolGroup.RunLuaAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(ScriptToolGroup.RunPythonAsync))
        };
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
    private readonly LuaScriptRunner _luaRunner;
    private readonly ShellEnvironmentDetector _shellDetector;
    private readonly IToolConfirmationService _confirmationService;

    /// <summary>
    /// 创建 ScriptToolGroup 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="processRunner">进程执行器</param>
    /// <param name="luaRunner">内嵌 Lua 沙箱执行器</param>
    /// <param name="shellDetector">Shell 环境探测器</param>
    /// <param name="confirmationService">工具调用确认服务</param>
    public ScriptToolGroup(
        ScriptToolOptions options,
        ProcessRunner processRunner,
        LuaScriptRunner luaRunner,
        ShellEnvironmentDetector shellDetector,
        IToolConfirmationService confirmationService)
    {
        _options = options;
        _processRunner = processRunner;
        _luaRunner = luaRunner;
        _shellDetector = shellDetector;
        _confirmationService = confirmationService;
    }

    /// <summary>
    /// 执行 Shell 命令
    /// </summary>
    /// <param name="command">要执行的命令</param>
    /// <param name="workingDirectory">工作目录（可选）</param>
    /// <returns>执行结果</returns>
    [Description("执行 Shell 命令。执行环境自动探测（Windows 优先 pwsh > powershell > cmd，类 Unix 优先 bash），返回值中的 shell/shellPath/platform 字段会告知实际使用的解释器；命令请按该解释器语法编写。")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", 
        Justification = "JSON 序列化仅用于简单结果类型，已通过 JsonSerializerOptions 处理")]
    public async Task<ToolResult<string>> RunShellAsync(string command, string? workingDirectory = null)
    {
        // 确认执行
        var outcome = await _confirmationService.EvaluateAsync(nameof(RunShellAsync), null,
            new Dictionary<string, object?> { ["command"] = command, ["workingDirectory"] = workingDirectory });
        if (outcome == EnumConfirmationOutcome.Planned)
        {
            return ToolResult.Plan<string>();
        }
        if (outcome != EnumConfirmationOutcome.Allowed)
        {
            return ToolResult.Denied<string>();
        }

        try
        {
            // 先探测运行环境，再按其类型构造参数风格与引用方式
            var environment = _shellDetector.Resolve(_options.Shell);
            if (!environment.IsAvailable)
            {
                Logger.Error($"Shell 环境不可用: {environment.Warning}");
                return ToolResult.Fail<string>(
                    $"Shell 工具不可用：{environment.Warning}",
                    new
                    {
                        exitCode = -1,
                        stdout = "",
                        stderr = environment.Warning,
                        durationMs = 0,
                        timedOut = false,
                        shell = environment.Name,
                        platform = environment.Platform
                    }.ToJson());
            }

            var shellCommand = ShellEnvironmentDetector.BuildCommand(environment, command);
            var result = await _processRunner.RunAsync(
                shellCommand.Executable,
                shellCommand.Arguments ?? string.Empty,
                workingDirectory,
                stdin: null,
                timeoutMs: _options.DefaultTimeout,
                argumentList: shellCommand.ArgumentList,
                outputEncoding: Encoding.UTF8);

            return ToolResult.Ok<string>(new
            {
                exitCode = result.ExitCode,
                stdout = result.StandardOutput,
                stderr = result.StandardError,
                durationMs = result.DurationMs,
                timedOut = result.TimedOut,
                shell = environment.Name,
                shellPath = environment.Executable,
                platform = environment.Platform,
                commandLine = shellCommand.Display,
                warning = environment.Warning
            }.ToJson());
        }
        catch (Exception ex)
        {
            Logger.Error("Shell 执行异常", ex, command);
            var environment = _shellDetector.Resolve(_options.Shell);
            return ToolResult.Fail<string>($"执行失败: {ex.Message}", new
            {
                exitCode = -1,
                stdout = "",
                stderr = $"执行失败: {ex.Message}",
                durationMs = 0,
                timedOut = false,
                shell = environment.Name,
                platform = environment.Platform
            }.ToJson());
        }
    }

    /// <summary>
    /// 执行 Lua 脚本（内嵌 MoonSharp 沙箱，无外部解释器依赖）
    /// </summary>
    /// <param name="script">Lua 脚本内容</param>
    /// <param name="workingDirectory">保留参数仅为签名兼容；内嵌沙箱无文件系统访问能力，该参数不生效。</param>
    /// <returns>执行结果</returns>
    [Description("在嵌入式 Lua 沙箱中执行脚本。沙箱无文件系统与系统命令能力，结果通过 print 输出（也可用 return 返回末尾表达式）。")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", 
        Justification = "JSON 序列化仅用于简单结果类型，已通过 JsonSerializerOptions 处理")]
    public async Task<ToolResult<string>> RunLuaAsync(string script, string? workingDirectory = null)
    {
        // 确认执行
        var outcome = await _confirmationService.EvaluateAsync(nameof(RunLuaAsync), null,
            new Dictionary<string, object?> { ["script"] = script, ["workingDirectory"] = workingDirectory });
        if (outcome == EnumConfirmationOutcome.Planned)
        {
            return ToolResult.Plan<string>();
        }
        if (outcome != EnumConfirmationOutcome.Allowed)
        {
            return ToolResult.Denied<string>();
        }

        try
        {
            // 内嵌沙箱执行：无需外部 lua 解释器；print 与末尾表达式值写入 stdout
            var result = _luaRunner.Execute(script, _options.DefaultTimeout);

            return ToolResult.Ok<string>(new
            {
                exitCode = result.ExitCode,
                stdout = result.StandardOutput,
                stderr = result.StandardError,
                durationMs = result.DurationMs,
                timedOut = result.TimedOut,
                runtime = "moonSharp-soft-sandbox"
            }.ToJson());
        }
        catch (Exception ex)
        {
            Logger.Error("Lua 执行异常", ex, script);
            return ToolResult.Fail<string>($"执行失败: {ex.Message}", new
            {
                exitCode = -1,
                stdout = "",
                stderr = $"执行失败: {ex.Message}",
                durationMs = 0,
                timedOut = false,
                runtime = "moonSharp-soft-sandbox"
            }.ToJson());
        }
    }

/// <summary>
/// 执行 Python 脚本
/// </summary>
/// <param name="script">Python 脚本内容</param>
/// <param name="workingDirectory">工作目录（可选）</param>
/// <returns>执行结果</returns>
[Description("执行 Python 脚本")]
[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", 
    Justification = "JSON 序列化仅用于简单结果类型，已通过 JsonSerializerOptions 处理")]
public async Task<ToolResult<string>> RunPythonAsync(string script, string? workingDirectory = null)
{
    // 确认执行
    var outcome = await _confirmationService.EvaluateAsync(nameof(RunPythonAsync), null,
        new Dictionary<string, object?> { ["script"] = script, ["workingDirectory"] = workingDirectory });
    if (outcome == EnumConfirmationOutcome.Planned)
    {
        return ToolResult.Plan<string>();
    }
    if (outcome != EnumConfirmationOutcome.Allowed)
    {
        return ToolResult.Denied<string>();
    }

    try
    {
        // 诊断日志：Python 环境
        Logger.Info($"Python 执行: executable={_options.PythonPath}, workingDir={workingDirectory ?? "未指定"}");
        Logger.Debug($"脚本内容（前200字符）:\n{(script.Length > 200 ? script.Substring(0, 200) + "..." : script)}");

        // Python 通过 stdin 执行脚本：python - < script.py
        // 使用 "-" 表示从 stdin 读取，而不是空 arguments（会启动交互模式）
        var result = await _processRunner.RunAsync(
            _options.PythonPath,
            "-",  // "-" 表示从 stdin 读取脚本
            workingDirectory,
            stdin: script,
            timeoutMs: _options.DefaultTimeout);

        // 诊断日志：执行结果
        Logger.Info($"Python 结果: exitCode={result.ExitCode}, stdout.length={result.StandardOutput.Length}, stderr.length={result.StandardError.Length}, duration={result.DurationMs}ms");
        if (!string.IsNullOrEmpty(result.StandardError))
        {
            Logger.Warn($"Python stderr: {result.StandardError}");
        }

        // 检测可执行文件不存在错误，返回结构化错误供 AI 分析
        if (result.ExitCode == -1 && !string.IsNullOrEmpty(result.StandardError))
        {
            if (result.StandardError.Contains("可执行文件不存在") || result.StandardError.Contains("无法启动"))
            {
                return ToolResult.Fail<string>(
                    $"Python 工具不可用: {_options.PythonPath}。请检查环境配置或联系管理员。",
                    result.StandardError);
            }
        }

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
        Logger.Error("Python 执行异常", ex, script);
        return ToolResult.Fail<string>($"执行失败: {ex.Message}", new
        {
            exitCode = -1,
            stdout = "",
            stderr = $"执行失败: {ex.Message}",
            durationMs = 0,
            timedOut = false
        }.ToJson());
    }
}
}
