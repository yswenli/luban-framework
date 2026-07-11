/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Reporting.Dynamic.Services
*文件名： LuaScriptEngine
*版本号： V1.0.0.0
*唯一标识：
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/5
*描述：Lua 脚本引擎封装
*
*=================================================
*修改标记
*修改时间：2026/6/5
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Lua 脚本引擎封装
*
*****************************************************************************/

namespace LuBan.Reporting.Dynamic.Services;

/// <summary>
/// Lua 脚本引擎封装
/// </summary>
public class LuaScriptEngine : IDisposable
{
    private readonly Script _engine;
    private readonly HttpClient _httpClient;

    public LuaScriptEngine()
    {
        _engine = new Script(CoreModules.Preset_SoftSandbox);
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        Initialize();
    }

    /// <summary>
    /// 注入框架函数
    /// </summary>
    private void Initialize()
    {
        // HTTP 调用
        _engine.Globals["http_get"] = (Func<string, object>)HttpGet;
        _engine.Globals["http_post"] = (Func<string, string, object>)HttpPost;
        _engine.Globals["http_put"] = (Func<string, string, object>)HttpPut;
        _engine.Globals["http_delete"] = (Func<string, object>)HttpDelete;
        _engine.Globals["http_patch"] = (Func<string, string, object>)HttpPatch;

        // JSON 处理
        _engine.Globals["json_parse"] = (Func<string, object>)JsonParse;
        _engine.Globals["json_stringify"] = (Func<object, string>)JsonStringify;

        // 字符串处理
        _engine.Globals["string_format"] = (Func<string, DynValue[], string>)StringFormat;
        _engine.Globals["string_split"] = (Func<string, string, List<string>>)StringSplit;
        _engine.Globals["string_match"] = (Func<string, string, string?>)StringMatch;

        // 日期处理
        _engine.Globals["date_format"] = (Func<object, string, string>)DateFormat;
        _engine.Globals["date_now"] = (Func<string>)DateNow;

        // 加密
        _engine.Globals["md5"] = (Func<string, string>)Md5;
        _engine.Globals["sha256"] = (Func<string, string>)Sha256;
        _engine.Globals["base64_encode"] = (Func<string, string>)Base64Encode;
        _engine.Globals["base64_decode"] = (Func<string, string>)Base64Decode;

        // 正则
        _engine.Globals["regex_match"] = (Func<string, string, string?>)RegexMatch;
        _engine.Globals["regex_replace"] = (Func<string, string, string, string>)RegexReplace;

        // 日志
        _engine.Globals["log_info"] = (Action<string>)Logger.Info;
        _engine.Globals["log_error"] = (Action<string>)Logger.Error;
    }

    /// <summary>
    /// 注册自定义函数
    /// </summary>
    public void RegisterFunction(string name, Delegate func)
    {
        _engine.Globals[name] = func;
    }

    /// <summary>
    /// 生成 SQL：执行 SqlTemplate 脚本
    /// </summary>
    public string GenerateSql(string sqlTemplateScript, Dictionary<string, object>? sqlParams)
    {
        _engine.Globals["params"] = sqlParams ?? new Dictionary<string, object>();
        var result = _engine.DoString(sqlTemplateScript);
        return result.String;
    }

    /// <summary>
    /// 列值转换：执行列转换逻辑
    /// </summary>
    public string ConvertValue(string converterScript, string expression, object value)
    {
        // 1. 先执行转换脚本（定义函数）
        if (!converterScript.IsNullOrEmpty())
            _engine.DoString(converterScript);

        // 2. 设置 value 并执行表达式
        _engine.Globals["value"] = value?.ToString() ?? "";
        var result = _engine.DoString(expression);
        return result?.String ?? "";
    }

    /// <summary>
    /// 执行任意 Lua 脚本（调试用）
    /// </summary>
    public string Execute(string script)
    {
        var result = _engine.DoString(script);
        return result?.String ?? "";
    }

    #region 内置函数实现

    private object HttpGet(string url)
    {
        try
        {
            var response = _httpClient.GetAsync(url).Result;
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonParse(content);
        }
        catch (Exception ex)
        {
            Logger.Error($"HTTP GET failed: {ex.Message}");
            return "";
        }
    }

    private object HttpPost(string url, string body)
    {
        try
        {
            var httpContent = new StringContent(body, Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync(url, httpContent).Result;
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonParse(content);
        }
        catch (Exception ex)
        {
            Logger.Error($"HTTP POST failed: {ex.Message}");
            return "";
        }
    }

    private object HttpPut(string url, string body)
    {
        try
        {
            var httpContent = new StringContent(body, Encoding.UTF8, "application/json");
            var response = _httpClient.PutAsync(url, httpContent).Result;
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonParse(content);
        }
        catch (Exception ex)
        {
            Logger.Error($"HTTP PUT failed: {ex.Message}");
            return "";
        }
    }

    private object HttpDelete(string url)
    {
        try
        {
            var response = _httpClient.DeleteAsync(url).Result;
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonParse(content);
        }
        catch (Exception ex)
        {
            Logger.Error($"HTTP DELETE failed: {ex.Message}");
            return "";
        }
    }

    private object HttpPatch(string url, string body)
    {
        try
        {
            var httpContent = new StringContent(body, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            {
                Content = httpContent
            };
            var response = _httpClient.SendAsync(request).Result;
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonParse(content);
        }
        catch (Exception ex)
        {
            Logger.Error($"HTTP PATCH failed: {ex.Message}");
            return "";
        }
    }

    private object JsonParse(string str)
    {
        try
        {
            // 返回字符串，让 Lua 脚本处理
            return str;
        }
        catch
        {
            return str;
        }
    }

    private string JsonStringify(object obj)
    {
        try
        {
            return JsonSerializer.Serialize(obj);
        }
        catch
        {
            return obj?.ToString() ?? "";
        }
    }

    private string StringFormat(string fmt, DynValue[] args)
    {
        var convertedArgs = args.Select(a => a.ToObject()).ToArray();
        return string.Format(fmt, convertedArgs);
    }

    private List<string> StringSplit(string str, string sep)
    {
        return str.Split(sep).ToList();
    }

    private string? StringMatch(string str, string pattern)
    {
        var match = Regex.Match(str, pattern);
        return match.Success ? match.Value : null;
    }

    private string DateFormat(object date, string fmt)
    {
        if (date is DateTime dt)
            return dt.ToString(fmt);
        if (DateTime.TryParse(date.ToString(), out var parsed))
            return parsed.ToString(fmt);
        return date.ToString() ?? "";
    }

    private string DateNow()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private string Md5(string str)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(str));
        return Convert.ToHexString(hash).ToLower();
    }

    private string Sha256(string str)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(str));
        return Convert.ToHexString(hash).ToLower();
    }

    private string Base64Encode(string str)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
    }

    private string Base64Decode(string str)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(str));
    }

    private string? RegexMatch(string str, string pattern)
    {
        var match = Regex.Match(str, pattern);
        return match.Success ? match.Value : null;
    }

    private string RegexReplace(string str, string pattern, string replacement)
    {
        return Regex.Replace(str, pattern, replacement);
    }


    #endregion

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
