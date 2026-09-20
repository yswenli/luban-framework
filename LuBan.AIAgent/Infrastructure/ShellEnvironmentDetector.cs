/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Infrastructure
*文件名： ShellEnvironmentDetector
*版本号： V1.0.0.0
*唯一标识：c4a7b1e2-8f3d-4c5a-b6e9-2d0f7a1c8b34
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：Shell 运行环境探测与命令构造
*
*=================================================
*修改标记
*修改时间：2026/9/20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Shell 运行环境探测与命令构造
*
*****************************************************************************/
using System.Collections.Concurrent;

namespace LuBan.AIAgent.Infrastructure;

/// <summary>
/// Shell 类型
/// </summary>
public enum ShellKind
{
    /// <summary>未知/不可用</summary>
    Unknown = 0,

    /// <summary>Windows 命令提示符 cmd.exe</summary>
    Cmd,

    /// <summary>PowerShell（pwsh 7+ 或 Windows PowerShell 5.1）</summary>
    PowerShell,

    /// <summary>类 Unix Shell（bash/sh/zsh/dash，Windows 上为 WSL bash）</summary>
    Bash
}

/// <summary>
/// Shell 运行环境
/// </summary>
/// <param name="Kind">Shell 类型</param>
/// <param name="Executable">可执行文件路径</param>
/// <param name="Name">显示名（pwsh/powershell/cmd/bash/sh）</param>
/// <param name="Platform">平台名（Windows/Linux/macOS）</param>
/// <param name="IsAvailable">是否可用</param>
/// <param name="Warning">探测过程中的告警（如配置的 shell 未找到已回退）</param>
public sealed record ShellEnvironment(
    ShellKind Kind,
    string Executable,
    string Name,
    string Platform,
    bool IsAvailable,
    string? Warning);

/// <summary>
/// 构造好的可执行命令
/// </summary>
/// <param name="Executable">可执行文件路径</param>
/// <param name="Arguments">原始参数字符串（ArgumentList 为 null 时使用）</param>
/// <param name="ArgumentList">参数列表（非 null 时优先使用，由框架负责安全引用）</param>
/// <param name="Display">命令展示串，用于回传给 AI</param>
public sealed record ShellCommand(
    string Executable,
    string? Arguments,
    IReadOnlyList<string>? ArgumentList,
    string Display);

/// <summary>
/// Shell 运行环境探测器。
/// <para>未显式配置 shell 时按优先级自动探测：Windows 为 pwsh &gt; powershell &gt; cmd，
/// 类 Unix 为 bash &gt; sh；显式配置时优先使用配置项（需能解析到可执行文件，否则回退自动探测）。</para>
/// <para>探测结果按配置的 shell 名缓存，避免每次调用重复扫描 PATH。</para>
/// </summary>
public sealed class ShellEnvironmentDetector
{
    private static readonly string[] WindowsPriority = ["pwsh", "powershell", "cmd"];
    private static readonly string[] UnixPriority = ["bash", "sh"];

    private readonly ConcurrentDictionary<string, ShellEnvironment> _cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 解析 Shell 运行环境
    /// </summary>
    /// <param name="configuredShell">宿主配置的 shell（可为空，表示自动探测）</param>
    /// <returns>Shell 运行环境</returns>
    public ShellEnvironment Resolve(string? configuredShell)
    {
        var key = configuredShell ?? string.Empty;
        return _cache.GetOrAdd(key, _ => Detect(configuredShell));
    }

    /// <summary>
    /// 按环境构造命令（含 UTF-8 输出前缀与参数风格适配）
    /// </summary>
    /// <param name="environment">Shell 运行环境</param>
    /// <param name="command">用户命令</param>
    /// <returns>可执行命令</returns>
    public static ShellCommand BuildCommand(ShellEnvironment environment, string command)
    {
        switch (environment.Kind)
        {
            case ShellKind.PowerShell:
                {
                    // -NoProfile/-NonInteractive 避免加载 profile 与交互阻塞；
                    // -Command 通过 ArgumentList 传入，由框架负责引用，内部引号不会被破坏。
                    // 统一显式设置控制台输出编码为 UTF-8，避免 Windows PowerShell 5.1 中文乱码。
                    var script = "[Console]::OutputEncoding=[Text.Encoding]::UTF8; " + command;
                    return new ShellCommand(
                        environment.Executable,
                        null,
                        ["-NoProfile", "-NonInteractive", "-Command", script],
                        $"{environment.Name} -NoProfile -Command");
                }

            case ShellKind.Cmd:
                {
                    // cmd 使用 /d（跳过 AutoRun）/s（按首尾引号剥离）/c（执行后退出）。
                    // cmd 不遵循 MSVCRT 反斜杠转义规则，故直接传入原始参数字符串；
                    // /s 会剥离首尾引号，命令内部引号保持原样。
                    // chcp 65001 保证 UTF-8 输出，避免中文乱码。
                    var script = "chcp 65001>nul & " + command;
                    return new ShellCommand(
                        environment.Executable,
                        "/d /s /c \"" + script + "\"",
                        null,
                        "cmd /d /s /c");
                }

            default:
                {
                    // bash/sh 通过 -c 传入单个参数；类 Unix 上由 .NET 直接构造 argv，无 shell 层引用问题。
                    return new ShellCommand(
                        environment.Executable,
                        null,
                        ["-c", command],
                        $"{environment.Name} -c");
                }
        }
    }

    private static ShellEnvironment Detect(string? configuredShell)
    {
        var platform = GetPlatformName();
        string? warning = null;

        if (!string.IsNullOrWhiteSpace(configuredShell))
        {
            var resolved = ResolveExecutable(configuredShell);
            if (resolved != null)
                return Build(KindOf(configuredShell), resolved, platform, warning);

            warning = $"配置的 Shell '{configuredShell}' 未找到，已回退到自动探测。";
        }

        foreach (var name in OperatingSystem.IsWindows() ? WindowsPriority : UnixPriority)
        {
            var resolved = ResolveExecutable(name);
            if (resolved != null)
                return Build(KindOf(name), resolved, platform, warning);
        }

        return new ShellEnvironment(
            ShellKind.Unknown,
            configuredShell ?? string.Empty,
            string.IsNullOrWhiteSpace(configuredShell) ? "(none)" : configuredShell,
            platform,
            false,
            warning ?? "未找到任何可用的 Shell 可执行文件（已尝试 pwsh/powershell/cmd 或 bash/sh）。");
    }

    private static ShellEnvironment Build(ShellKind kind, string executable, string platform, string? warning)
    {
        var name = Path.GetFileNameWithoutExtension(executable);
        if (string.IsNullOrWhiteSpace(name))
            name = executable;
        return new ShellEnvironment(kind, executable, name.ToLowerInvariant(), platform, true, warning);
    }

    private static ShellKind KindOf(string name)
    {
        var normalized = Path.GetFileNameWithoutExtension(name).ToLowerInvariant();
        return normalized switch
        {
            "cmd" => ShellKind.Cmd,
            "powershell" or "pwsh" => ShellKind.PowerShell,
            "bash" or "sh" or "zsh" or "dash" or "wsl" => ShellKind.Bash,
            _ => OperatingSystem.IsWindows() ? ShellKind.Cmd : ShellKind.Bash
        };
    }

    /// <summary>
    /// 解析可执行文件：支持绝对路径与 PATH 搜索
    /// </summary>
    /// <param name="name">可执行文件名或路径</param>
    /// <returns>解析到的绝对路径，找不到返回 null</returns>
    public static string? ResolveExecutable(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        if (Path.IsPathRooted(name))
            return File.Exists(name) ? name : null;

        var extensions = OperatingSystem.IsWindows()
            ? new[] { ".exe", ".cmd", ".bat", ".com", ".ps1", "" }
            : new[] { "" };

        var pathValue = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var directory in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = directory.Trim().Trim('"');
            if (trimmed.Length == 0)
                continue;

            foreach (var extension in extensions)
            {
                try
                {
                    var candidate = Path.Combine(trimmed, name + extension);
                    if (File.Exists(candidate))
                        return candidate;
                }
                catch
                {
                    // 非法路径字符等，跳过
                }
            }
        }

        return null;
    }

    private static string GetPlatformName()
    {
        if (OperatingSystem.IsWindows()) return "Windows";
        if (OperatingSystem.IsMacOS()) return "macOS";
        if (OperatingSystem.IsLinux()) return "Linux";
        return "Unknown";
    }
}