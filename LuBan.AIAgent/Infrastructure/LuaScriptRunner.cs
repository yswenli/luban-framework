/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Infrastructure
*文件名： LuaScriptRunner
*版本号： V1.0.0.0
*唯一标识：8d1c2f46-2f4b-4a0e-9c3d-71b6a5e9d0c1
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：内嵌 MoonSharp Lua 沙箱执行器
*
*=================================================
*修改标记
*修改时间：2026/9/20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：内嵌 MoonSharp Lua 沙箱执行器（参考 LuBan.Reporting 的 LuaScriptEngine）
*
*****************************************************************************/
using System.Text.RegularExpressions;

using MoonSharp.Interpreter;

namespace LuBan.AIAgent.Infrastructure;

/// <summary>
/// Lua 脚本执行结果
/// </summary>
/// <param name="ExitCode">退出码，0 成功、1 脚本错误、-1 超时</param>
/// <param name="StandardOutput">标准输出（print 与末尾表达式值）</param>
/// <param name="StandardError">标准错误（脚本语法/运行时错误信息）</param>
/// <param name="DurationMs">执行时长（毫秒）</param>
/// <param name="TimedOut">是否超时</param>
public sealed record LuaExecutionResult(
    int ExitCode,
    string StandardOutput,
    string StandardError,
    int DurationMs,
    bool TimedOut);

/// <summary>
/// 内嵌 MoonSharp Lua 沙箱执行器，无需外部 lua 解释器。
/// <para>使用 <see cref="CoreModules.Preset_SoftSandbox"/> 沙箱：包含 Basic/Math/String/Table/
/// Coroutine/Bit32/OS_Time/Json/Dynamic 等能力，<b>不含</b> io（文件系统）、OS_System
/// （os.execute/os.exit/os.getenv 等）与 LoadMethods（require/dofile/loadfile），
/// 相比外部进程更安全。另注册与 LuBan.Reporting 一致的辅助函数（http/json 之外的加解密、
/// 正则、日期、字符串、日志等）。</para>
/// <para>超时限制：MoonSharp 2.0 未公开指令级中断钩子，故超时通过后台线程等待实现。
/// 一旦超时，脚本线程无法强制终止，会作为后台线程继续运行（沙箱无 IO，影响限于 CPU 占用）；
/// 此时返回空 stdout 以避免与仍在运行的线程产生数据竞争。</para>
/// </summary>
public sealed class LuaScriptRunner : IDisposable
{
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

    /// <summary>
    /// 执行 Lua 脚本
    /// </summary>
    /// <param name="script">脚本内容</param>
    /// <param name="timeoutMs">超时时间（毫秒），&lt;=0 表示不限制</param>
    /// <returns>执行结果</returns>
    public LuaExecutionResult Execute(string script, int timeoutMs)
    {
        var output = new OutputBuffer();
        var startedAt = DateTimeOffset.UtcNow;
        LuaExecutionResult? result = null;
        string? error = null;

        using var completed = new ManualResetEventSlim(false);

        var worker = new Thread(() =>
        {
            try
            {
                result = ExecuteCore(script, output);
            }
            catch (InterpreterException ex)
            {
                error = string.IsNullOrWhiteSpace(ex.DecoratedMessage) ? ex.Message : ex.DecoratedMessage;
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                // 超时返回后 completed 可能已被 Dispose，忽略之
                try { completed.Set(); } catch (ObjectDisposedException) { }
            }
        })
        {
            IsBackground = true,
            Name = "luban-lua-sandbox"
        };

        worker.Start();

        var waitMs = timeoutMs > 0 ? timeoutMs : Timeout.Infinite;
        if (!completed.Wait(waitMs))
        {
            return new LuaExecutionResult(
                -1,
                string.Empty,
                $"Lua 脚本执行超时（>{timeoutMs}ms），已放弃等待；沙箱脚本线程可能仍在后台运行。",
                ElapsedMs(startedAt),
                true);
        }

        if (result != null)
            return result with { DurationMs = ElapsedMs(startedAt) };

        return new LuaExecutionResult(1, output.Snapshot(), error ?? "Lua 脚本执行失败", ElapsedMs(startedAt), false);
    }

    private LuaExecutionResult ExecuteCore(string script, OutputBuffer output)
    {
        var engine = new Script(CoreModules.Preset_SoftSandbox);
        engine.Options.UseLuaErrorLocations = true;
        engine.Options.DebugPrint = output.AppendLine;

        // 捕获 print 输出（Preset_SoftSandbox 的 print 默认写 Console，此处重定向到结果集）
        // 必须用 DynValue.NewCallback 以可变参数形式接收；Action<DynValue[]> 会被当作单个
        // 必需参数处理，导致 "cannot convert a string to DynValue[]"。
        engine.Globals["print"] = DynValue.NewCallback((context, args) =>
        {
            var parts = new List<string>(args.Count);
            for (var i = 0; i < args.Count; i++)
                parts.Add(args[i].ToPrintString());

            output.AppendLine(string.Join("\t", parts));
            return DynValue.Nil;
        });

        RegisterBuiltIns(engine);

        var value = engine.DoString(script);

        // 末尾表达式有返回值时一并输出，便于 AI 直接观察结果
        if (value != null && !value.IsNil() && !value.IsVoid())
        {
            var text = value.ToPrintString();
            if (!string.IsNullOrEmpty(text))
                output.AppendLine(text);
        }

        return new LuaExecutionResult(0, output.Snapshot(), string.Empty, 0, false);
    }

    #region 内置辅助函数（与 LuBan.Reporting.LuaScriptEngine 保持一致）

    private void RegisterBuiltIns(Script engine)
    {
        // HTTP
        engine.Globals["http_get"] = (Func<string, object>)HttpGet;
        engine.Globals["http_post"] = (Func<string, string, object>)HttpPost;
        engine.Globals["http_put"] = (Func<string, string, object>)HttpPut;
        engine.Globals["http_delete"] = (Func<string, object>)HttpDelete;
        engine.Globals["http_patch"] = (Func<string, string, object>)HttpPatch;

        // 字符串
        engine.Globals["string_format"] = (Func<string, DynValue[], string>)StringFormat;
        engine.Globals["string_split"] = (Func<string, string, List<string>>)StringSplit;
        engine.Globals["string_match"] = (Func<string, string, string?>)StringMatch;

        // 日期
        engine.Globals["date_format"] = (Func<object, string, string>)DateFormat;
        engine.Globals["date_now"] = (Func<string>)DateNow;

        // 加密
        engine.Globals["md5"] = (Func<string, string>)Md5;
        engine.Globals["sha256"] = (Func<string, string>)Sha256;
        engine.Globals["base64_encode"] = (Func<string, string>)Base64Encode;
        engine.Globals["base64_decode"] = (Func<string, string>)Base64Decode;

        // 正则
        engine.Globals["regex_match"] = (Func<string, string, string?>)RegexMatch;
        engine.Globals["regex_replace"] = (Func<string, string, string, string>)RegexReplace;

        // 日志
        engine.Globals["log_info"] = (Action<string>)Logger.Info;
        engine.Globals["log_error"] = (Action<string>)Logger.Error;
    }

    private object HttpGet(string url)
    {
        try
        {
            var response = _httpClient.GetAsync(url).Result;
            return response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            Logger.Error($"Lua HTTP GET 失败: {ex.Message}");
            // 抛出异常而非返回空串：让脚本能区分“请求失败”与“响应为空”
            throw new InvalidOperationException($"HTTP GET failed for url '{url}': {ex.Message}", ex);
        }
    }

    private object HttpPost(string url, string body) => SendBody(HttpMethod.Post, url, body);

    private object HttpPut(string url, string body) => SendBody(HttpMethod.Put, url, body);

    private object HttpPatch(string url, string body) => SendBody(new HttpMethod("PATCH"), url, body);

    private object HttpDelete(string url)
    {
        try
        {
            var response = _httpClient.DeleteAsync(url).Result;
            return response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            Logger.Error($"Lua HTTP DELETE 失败: {ex.Message}");
            throw new InvalidOperationException($"HTTP DELETE failed for url '{url}': {ex.Message}", ex);
        }
    }

    private object SendBody(HttpMethod method, string url, string body)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            var response = _httpClient.SendAsync(request).Result;
            return response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            Logger.Error($"Lua HTTP {method.Method} 失败: {ex.Message}");
            throw new InvalidOperationException($"HTTP {method.Method} failed for url '{url}': {ex.Message}", ex);
        }
    }

    private static string StringFormat(string fmt, DynValue[] args)
    {
        var converted = args.Select(a => a.ToObject()).ToArray();
        return string.Format(fmt, converted);
    }

    private static List<string> StringSplit(string str, string sep) => str.Split(sep).ToList();

    private static string? StringMatch(string str, string pattern)
    {
        var match = Regex.Match(str, pattern, RegexOptions.None, TimeSpan.FromSeconds(5));
        return match.Success ? match.Value : null;
    }

    private static string DateFormat(object date, string fmt)
    {
        if (date is DateTime dt)
            return dt.ToString(fmt);
        if (DateTime.TryParse(date.ToString(), out var parsed))
            return parsed.ToString(fmt);
        return date.ToString() ?? "";
    }

    private static string DateNow() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    private static string Md5(string str) => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(str))).ToLowerInvariant();

    private static string Sha256(string str) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(str))).ToLowerInvariant();

    private static string Base64Encode(string str) => Convert.ToBase64String(Encoding.UTF8.GetBytes(str));

    private static string Base64Decode(string str) => Encoding.UTF8.GetString(Convert.FromBase64String(str));

    private static string? RegexMatch(string str, string pattern)
    {
        var match = Regex.Match(str, pattern, RegexOptions.None, TimeSpan.FromSeconds(5));
        return match.Success ? match.Value : null;
    }

    private static string RegexReplace(string str, string pattern, string replacement)
        => Regex.Replace(str, pattern, replacement, RegexOptions.None, TimeSpan.FromSeconds(5));

    #endregion

    private static int ElapsedMs(DateTimeOffset startedAt)
        => (int)(DateTimeOffset.UtcNow - startedAt).TotalMilliseconds;

    /// <summary>
    /// 线程安全的输出缓冲（脚本线程与调用线程可能并发访问）
    /// </summary>
    private sealed class OutputBuffer
    {
        private readonly StringBuilder _builder = new();

        public void AppendLine(string value)
        {
            lock (_builder)
            {
                _builder.AppendLine(value);
            }
        }

        public string Snapshot()
        {
            lock (_builder)
            {
                return _builder.ToString();
            }
        }
    }

    /// <summary>
    /// 释放 HTTP 客户端
    /// </summary>
    public void Dispose() => _httpClient.Dispose();
}