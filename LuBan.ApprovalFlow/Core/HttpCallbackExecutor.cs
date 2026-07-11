namespace LuBan.ApprovalFlow.Core;

/// <summary>
/// HTTP回调执行器：执行HTTP请求并处理响应映射。
/// 支持多种认证方式、请求重试、响应变量映射等功能。
/// </summary>
public class HttpCallbackExecutor
{
    /// <summary>
    /// 执行HTTP回调请求。
    /// </summary>
    /// <param name="config">HTTP回调配置。</param>
    /// <param name="context">流程执行上下文。</param>
    /// <returns>响应结果字典。</returns>
    public async Task<Dictionary<string, object>> ExecuteAsync(
        HttpCallbackConfig? config,
        FlowExecutionContext context)
    {
        if (config == null || string.IsNullOrEmpty(config.Url))
            return new Dictionary<string, object>();

        // 解析占位符：URL、请求头、查询参数、认证信息
        var resolvedUrl = PlaceholderResolver.Resolve(config.Url, context);
        var resolvedHeaders = ResolveHeaders(config.Headers, context);
        var resolvedQueryParams = PlaceholderResolver.ResolveDictionary(config.QueryParams, context);
        var resolvedAuth = ResolveAuth(config.Auth, context);

        // 构建最终URL和请求体
        var finalUrl = BuildUrl(resolvedUrl, resolvedQueryParams, resolvedAuth);
        var requestBody = BuildRequestBody(config, context);

        // 发送请求（支持重试）
        var response = await SendWithRetryAsync(
            finalUrl,
            config.Method,
            requestBody,
            resolvedHeaders,
            resolvedAuth,
            config.Timeout,
            config.RetryCount,
            config.RetryInterval);

        // 异步模式：不等待响应
        if (config.Async)
        {
            _ = Task.Run(async () => await Task.CompletedTask);
            return new Dictionary<string, object>();
        }

        return response;
    }

    /// <summary>
    /// 构建完整的请求URL（包含查询参数和认证信息）。
    /// </summary>
    /// <param name="baseUrl">基础URL。</param>
    /// <param name="queryParams">查询参数字典。</param>
    /// <param name="authHeaders">认证头信息。</param>
    /// <returns>完整的请求URL。</returns>
    private string BuildUrl(
        string baseUrl,
        Dictionary<string, object>? queryParams,
        Dictionary<string, string>? authHeaders)
    {
        var url = baseUrl;

        // 添加认证查询参数（如API Key在URL中）
        if (authHeaders != null && authHeaders.TryGetValue("query_auth", out var queryAuth))
        {
            if (!url.Contains("?"))
                url += "?";
            else
                url += "&";
            url += queryAuth;
        }

        // 添加查询参数
        if (queryParams != null && queryParams.Count > 0)
        {
            var queryParts = queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value?.ToString() ?? "")}");
            var queryString = string.Join("&", queryParts);

            if (!url.Contains("?"))
                url += "?" + queryString;
            else
                url += "&" + queryString;
        }

        return url;
    }

    /// <summary>
    /// 构建请求体。
    /// </summary>
    /// <param name="config">HTTP回调配置。</param>
    /// <param name="context">流程执行上下文。</param>
    /// <returns>请求体对象。</returns>
    private object? BuildRequestBody(
        HttpCallbackConfig config,
        FlowExecutionContext context)
    {
        // 根据 BodyType 决定使用哪种请求体格式
        switch (config.BodyType?.ToLower())
        {
            case "json":
                if (config.BodyJson != null)
                {
                    return PlaceholderResolver.ResolveValue(config.BodyJson, context);
                }
                break;

            case "formdata":
                if (config.FormData != null && config.FormData.Count > 0)
                {
                    return PlaceholderResolver.ResolveDictionary(config.FormData, context);
                }
                break;

            case "raw":
                if (!string.IsNullOrEmpty(config.BodyRaw))
                {
                    return PlaceholderResolver.Resolve(config.BodyRaw, context);
                }
                break;

            default:
                // 默认行为：优先使用 JSON，其次 FormData
                if (config.BodyJson != null)
                {
                    return PlaceholderResolver.ResolveValue(config.BodyJson, context);
                }
                if (config.FormData != null && config.FormData.Count > 0)
                {
                    return PlaceholderResolver.ResolveDictionary(config.FormData, context);
                }
                break;
        }

        return null;
    }

    /// <summary>
    /// 解析请求头中的占位符。
    /// </summary>
    /// <param name="headers">原始请求头字典。</param>
    /// <param name="context">流程执行上下文。</param>
    /// <returns>解析后的请求头字典。</returns>
    private Dictionary<string, string> ResolveHeaders(
        Dictionary<string, string>? headers,
        FlowExecutionContext context)
    {
        if (headers == null) return new Dictionary<string, string>();

        var result = new Dictionary<string, string>();
        foreach (var kvp in headers)
        {
            var resolvedValue = PlaceholderResolver.Resolve(kvp.Value, context);
            result[kvp.Key] = resolvedValue;
        }
        return result;
    }

    /// <summary>
    /// 解析认证配置并生成认证头。
    /// </summary>
    /// <param name="auth">认证配置。</param>
    /// <param name="context">流程执行上下文。</param>
    /// <returns>认证头字典。</returns>
    private Dictionary<string, string>? ResolveAuth(
        AuthConfig? auth,
        FlowExecutionContext context)
    {
        if (auth == null || auth.Type == ConstAuthType.None)
            return null;

        var result = new Dictionary<string, string>();

        switch (auth.Type)
        {
            case ConstAuthType.Basic:
                // Basic认证：Base64编码用户名密码
                var username = PlaceholderResolver.Resolve(auth.BasicUsername ?? "", context);
                var password = PlaceholderResolver.Resolve(auth.BasicPassword ?? "", context);
                var basicToken = Base64Encode($"{username}:{password}");
                result["Authorization"] = $"Basic {basicToken}";
                break;

            case ConstAuthType.Bearer:
                // Bearer认证：Token方式
                var token = PlaceholderResolver.Resolve(auth.BearerToken ?? "", context);
                result["Authorization"] = $"Bearer {token}";
                break;

            case ConstAuthType.ApiKey:
                // API Key认证：支持Header或Query方式
                var apiKeyValue = PlaceholderResolver.Resolve(auth.ApiKeyValue ?? "", context);
                if (auth.ApiKeyLocation == "header")
                {
                    result[auth.ApiKeyName ?? "X-API-Key"] = apiKeyValue;
                }
                else
                {
                    result["query_auth"] = $"{auth.ApiKeyName ?? "api_key"}={Uri.EscapeDataString(apiKeyValue)}";
                }
                break;
        }

        // 添加自定义请求头
        if (auth.CustomHeaders != null)
        {
            foreach (var kvp in auth.CustomHeaders)
            {
                var resolvedValue = PlaceholderResolver.Resolve(kvp.Value, context);
                result[kvp.Key] = resolvedValue;
            }
        }

        return result;
    }

    /// <summary>
    /// 发送HTTP请求，支持重试机制。
    /// </summary>
    /// <param name="url">请求URL。</param>
    /// <param name="method">HTTP方法。</param>
    /// <param name="body">请求体。</param>
    /// <param name="headers">请求头。</param>
    /// <param name="authHeaders">认证头。</param>
    /// <param name="timeout">超时时间（毫秒）。</param>
    /// <param name="retryCount">重试次数。</param>
    /// <param name="retryInterval">重试间隔（毫秒）。</param>
    /// <returns>响应结果字典。</returns>
    private async Task<Dictionary<string, object>> SendWithRetryAsync(
        string url,
        string method,
        object? body,
        Dictionary<string, string>? headers,
        Dictionary<string, string>? authHeaders,
        int timeout,
        int retryCount,
        int retryInterval)
    {
        for (int attempt = 0; attempt <= retryCount; attempt++)
        {
            try
            {
                // 复用 HttpClientProxy，以完整 URL 作为 baseUrl
                var client = HttpClientProxy.Create(url, timeout / 1000);

                // 构建请求消息
                var request = new System.Net.Http.HttpRequestMessage(
                    new System.Net.Http.HttpMethod(method.ToUpper()), "");

                // 合并请求头
                var mergedHeaders = new Dictionary<string, string>();
                if (headers != null)
                {
                    foreach (var kvp in headers)
                    {
                        mergedHeaders[kvp.Key] = kvp.Value;
                    }
                }
                if (authHeaders != null)
                {
                    foreach (var kvp in authHeaders)
                    {
                        if (kvp.Key != "query_auth")
                        {
                            mergedHeaders[kvp.Key] = kvp.Value;
                        }
                    }
                }

                // 添加请求体（GET请求除外）
                if (body != null && method.ToUpper() != "GET")
                {
                    // 根据请求体类型设置 Content
                    if (body is string rawContent)
                    {
                        // Raw 类型：直接发送字符串内容
                        request.Content = new System.Net.Http.StringContent(
                            rawContent,
                            System.Text.Encoding.UTF8,
                            "text/plain");
                    }
                    else
                    {
                        // JSON/FormData 类型：序列化为 JSON
                        var jsonContent = SerializeUtil.Serialize(body);
                        request.Content = new System.Net.Http.StringContent(
                            jsonContent,
                            System.Text.Encoding.UTF8,
                            "application/json");
                    }
                }

                // 发送请求
                var response = await client.SendAsync(request, timeout / 1000);
                var content = await response.Content.ReadAsStringAsync();

                // 解析响应
                try
                {
                    return SerializeUtil.Deserialize<Dictionary<string, object>>(content) ??
                        new Dictionary<string, object> { { "rawResponse", content } };
                }
                catch
                {
                    // JSON解析失败时返回原始响应
                    return new Dictionary<string, object>
                    {
                        { "rawResponse", content },
                        { "statusCode", (int)response.StatusCode }
                    };
                }
            }
            catch (Exception ex)
            {
                // 达到最大重试次数，返回错误信息
                if (attempt == retryCount)
                {
                    Logger.Error($"HTTP请求失败: {url}, 错误: {ex.Message}");
                    return new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "success", false }
                    };
                }

                // 记录重试日志并等待
                Logger.Warn($"HTTP请求失败，第{attempt + 1}次重试: {ex.Message}");
                await Task.Delay(retryInterval);
            }
        }

        return new Dictionary<string, object>();
    }

    /// <summary>
    /// 将HTTP响应映射到流程变量。
    /// </summary>
    /// <param name="response">HTTP响应字典。</param>
    /// <param name="mappings">变量映射配置列表。</param>
    /// <param name="variables">目标变量字典。</param>
    public void MapResponseToVariables(
        Dictionary<string, object>? response,
        List<ResultMapping>? mappings,
        Dictionary<string, object> variables)
    {
        if (response == null || mappings == null || variables == null)
            return;

        foreach (var mapping in mappings)
        {
            // 提取源路径和目标键名
            var sourcePath = mapping.Source.Replace("response.", "").Replace("Response.", "");
            var targetKey = mapping.Target.Replace("Variables.", "").Replace("variables.", "");

            // 从响应中获取嵌套值
            var value = GetNestedValueFromResponse(response, sourcePath);
            if (value != null)
            {
                variables[targetKey] = value;
            }
        }
    }

    /// <summary>
    /// 从响应字典中获取嵌套值。
    /// </summary>
    /// <param name="response">响应字典。</param>
    /// <param name="path">属性路径（用点号分隔）。</param>
    /// <returns>嵌套属性值。</returns>
    private object? GetNestedValueFromResponse(Dictionary<string, object> response, string path)
    {
        var parts = path.Split('.');
        object? current = response;

        foreach (var part in parts)
        {
            if (current == null) return null;

            if (current is Dictionary<string, object> dict)
            {
                // 字典类型：通过键获取
                if (!dict.TryGetValue(part, out current))
                    return null;
            }
            else
            {
                // 对象类型：通过反射获取属性
                var prop = current.GetType().GetProperty(part);
                if (prop == null) return null;
                current = prop.GetValue(current);
            }
        }

        return current;
    }

    /// <summary>
    /// Base64编码字符串。
    /// </summary>
    /// <param name="plainText">原始字符串。</param>
    /// <returns>Base64编码后的字符串。</returns>
    private static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }
}