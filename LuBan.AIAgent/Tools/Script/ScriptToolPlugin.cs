/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
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
        var confirmationService = sp.GetRequiredService<IToolConfirmationService>();
        var toolGroup = new ScriptToolGroup(_options, _processRunner, confirmationService);
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
    private readonly IToolConfirmationService _confirmationService;

    /// <summary>
    /// 创建 ScriptToolGroup 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="processRunner">进程执行器</param>
    /// <param name="confirmationService">工具调用确认服务</param>
    public ScriptToolGroup(ScriptToolOptions options, ProcessRunner processRunner, IToolConfirmationService confirmationService)
    {
        _options = options;
        _processRunner = processRunner;
        _confirmationService = confirmationService;
    }

    /// <summary>
    /// 执行 Shell 命令
    /// </summary>
    /// <param name="command">要执行的命令</param>
    /// <param name="workingDirectory">工作目录（可选）</param>
    /// <returns>执行结果</returns>
    [Description("执行 Shell 命令")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", 
        Justification = "JSON 序列化仅用于简单结果类型，已通过 JsonSerializerOptions 处理")]
    public async Task<ToolResult<string>> RunShellAsync(string command, string? workingDirectory = null)
    {
        // 确认执行
        if (!_confirmationService.RequestConfirmation("RunShellAsync",
            new Dictionary<string, object?> { ["command"] = command, ["workingDirectory"] = workingDirectory }))
        {
            return ToolResult.Cancelled<string>();
        }

        try
        {
            // Windows cmd 使用 /c 参数（执行后退出），Unix shell 使用 -c
            // 命令作为参数传递，而不是 stdin（cmd/sh 不支持 stdin 传递命令）
            var shellName = Path.GetFileNameWithoutExtension(_options.Shell);
            var shellArgs = shellName.Equals("cmd", StringComparison.OrdinalIgnoreCase)
                ? $"/c \"{command.Replace("\"", "\\\"\"")}"  // Windows: cmd /c "command"
                : $"-c \"{command.Replace("\"", "\\\"\"")}"; // Unix: sh -c "command"
            
            var result = await _processRunner.RunAsync(
                _options.Shell,
                shellArgs,
                workingDirectory,
                stdin: null,  // 不使用 stdin
                timeoutMs: _options.DefaultTimeout);

            // 检测可执行文件不存在错误，返回结构化错误供 AI 分析
            if (result.ExitCode == -1 && !string.IsNullOrEmpty(result.StandardError))
            {
                if (result.StandardError.Contains("可执行文件不存在") || result.StandardError.Contains("无法启动"))
                {
                    return ToolResult.Fail<string>(
                        $"Shell 工具不可用: {_options.Shell}。请检查环境配置或联系管理员。",
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
            Logger.Error("Shell 执行异常", ex, command);
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

    /// <summary>
    /// 执行 Lua 脚本
    /// </summary>
    /// <param name="script">Lua 脚本内容</param>
    /// <param name="workingDirectory">工作目录（可选）</param>
    /// <returns>执行结果</returns>
    [Description("执行 Lua 脚本")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", 
        Justification = "JSON 序列化仅用于简单结果类型，已通过 JsonSerializerOptions 处理")]
    public async Task<ToolResult<string>> RunLuaAsync(string script, string? workingDirectory = null)
    {
        // 确认执行
        if (!_confirmationService.RequestConfirmation("RunLuaAsync",
            new Dictionary<string, object?> { ["script"] = script, ["workingDirectory"] = workingDirectory }))
        {
            return ToolResult.Cancelled<string>();
        }

        try
        {
            // Lua 通过 stdin 执行脚本：lua - < script.lua
            var result = await _processRunner.RunAsync(
                _options.LuaPath,
                "-",  // "-" 表示从 stdin 读取脚本
                workingDirectory,
                stdin: script,
                timeoutMs: _options.DefaultTimeout);

            // 检测可执行文件不存在错误，返回结构化错误供 AI 分析
            if (result.ExitCode == -1 && !string.IsNullOrEmpty(result.StandardError))
            {
                if (result.StandardError.Contains("可执行文件不存在") || result.StandardError.Contains("无法启动"))
                {
                    return ToolResult.Fail<string>(
                        $"Lua 工具不可用: {_options.LuaPath}。请检查环境配置或联系管理员。",
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
            Logger.Error("Lua 执行异常", ex, script);
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
    if (!_confirmationService.RequestConfirmation("RunPythonAsync",
        new Dictionary<string, object?> { ["script"] = script, ["workingDirectory"] = workingDirectory }))
    {
        return ToolResult.Cancelled<string>();
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
