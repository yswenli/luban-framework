/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Configuration
*文件名： ProviderModels
*版本号： V1.0.0.0
*唯一标识：80bfaede-6ed2-49c2-9338-6c0e6c8d741d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：模型提供方数据模型
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：模型提供方数据模型
*
*****************************************************************************/
namespace LuBan.AIAgent.Configuration;

/// <summary>
/// 预定义的 Provider 模型配置
/// </summary>
public static class ProviderModels
{
    /// <summary>
    /// 运行时从 /v1/models 拉取的模型缓存
    /// </summary>
    private static readonly ConcurrentDictionary<string, List<string>> _fetchedModels = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 默认 API 基础地址（OpenAI 兼容端点）
    /// </summary>
    private static readonly Dictionary<string, string> _defaultBaseUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        ["openai"] = "https://api.openai.com/v1/",
        ["deepseek"] = "https://api.deepseek.com/v1/",
        ["kimi"] = "https://api.moonshot.cn/v1/",
        ["glm"] = "https://open.bigmodel.cn/api/paas/v4/",
        ["qwen"] = "https://dashscope.aliyuncs.com/compatible-mode/v1/",
        ["doubao"] = "https://ark.cn-beijing.volces.com/api/v3/",
        ["ollama"] = "http://localhost:11434/v1/"
    };

    /// <summary>
    /// 获取 Provider 支持的模型列表（预定义 + 运行时拉取）
    /// </summary>
    /// <param name="providerName">Provider 名称</param>
    /// <returns>模型列表</returns>
    public static List<string> GetModels(string providerName)
    {
        var key = providerName.ToLowerInvariant();
        var models = _models.TryGetValue(key, out var predefined)
            ? predefined.ToList()
            : new List<string>();

        if (_fetchedModels.TryGetValue(key, out var fetched))
        {
            foreach (var model in fetched)
            {
                if (!models.Contains(model, StringComparer.OrdinalIgnoreCase))
                    models.Add(model);
            }
        }

        return models;
    }

    /// <summary>
    /// 从 Provider 的 /v1/models 端点刷新模型列表。
    /// 刷新失败时保留原有缓存；首次失败则使用预定义列表。
    /// </summary>
    /// <param name="providerName">Provider 名称</param>
    /// <param name="apiKey">API 密钥</param>
    /// <param name="baseUrl">自定义基础 URL（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>刷新后的模型列表</returns>
    public static async Task<List<string>> RefreshModelsAsync(string providerName, string apiKey, string? baseUrl = null, CancellationToken cancellationToken = default)
    {
        var key = providerName.ToLowerInvariant();

        if (!string.IsNullOrEmpty(baseUrl))
        {
            baseUrl = NormalizeBaseUrl(baseUrl);
        }
        else if (_defaultBaseUrls.TryGetValue(key, out var defaultUrl))
        {
            baseUrl = defaultUrl;
        }
        else
        {
            // 无默认地址，无法刷新，返回预定义列表
            return GetModels(providerName);
        }

        try
        {
            var models = await FetchModelsFromEndpointAsync(providerName, apiKey, baseUrl, cancellationToken).ConfigureAwait(false);
            _fetchedModels[key] = models;
            return GetModels(providerName);
        }
        catch (Exception ex)
        {
            Logger.Warn($"从 /v1/models 刷新 {providerName} 模型列表失败: {ex.Message}");
            return GetModels(providerName);
        }
    }

    private static async Task<List<string>> FetchModelsFromEndpointAsync(string providerName, string apiKey, string baseUrl, CancellationToken cancellationToken)
    {
        var proxy = HttpClientProxy.Create(new Uri(baseUrl), timeout: 30, useLog: false);

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(apiKey))
        {
            headers["Authorization"] = $"Bearer {apiKey}";
        }

        var json = await proxy.GetAsync("models", headers, 30).ConfigureAwait(false);

        var models = new List<string>();
        if (string.IsNullOrWhiteSpace(json))
            return models;

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in data.EnumerateArray())
                {
                    if (item.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String)
                    {
                        var modelId = id.GetString();
                        if (!string.IsNullOrWhiteSpace(modelId) && !models.Contains(modelId, StringComparer.OrdinalIgnoreCase))
                            models.Add(modelId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"解析 {providerName} /v1/models 响应失败: {ex.Message}");
        }

        return models;
    }

    private static string NormalizeBaseUrl(string url)
    {
        if (!url.EndsWith('/'))
            url += "/";
        return url;
    }

    /// <summary>
    /// 获取 Provider 显示名称
    /// </summary>
    /// <param name="providerName">Provider 名称</param>
    /// <returns>显示名称</returns>
    public static string GetDisplayName(string providerName)
    {
        return _displayNames.TryGetValue(providerName.ToLowerInvariant(), out var name)
            ? name
            : providerName;
    }

    private static readonly Dictionary<string, List<string>> _models = new()
    {
        ["openai"] = new List<string>
        {
            "gpt-4.1",
            "gpt-4.1-mini",
            "gpt-4.1-nano",
            "gpt-4o",
            "gpt-4o-mini",
            "gpt-4-turbo",
            "gpt-4",
            "gpt-3.5-turbo",
            "o1",
            "o1-mini",
            "o3-mini"
        },
        ["azure"] = new List<string>
        {
            "gpt-4o",
            "gpt-4o-mini",
            "gpt-4-turbo",
            "gpt-4",
            "gpt-35-turbo"
        },
        ["deepseek"] = new List<string>
        {
            "deepseek-chat",
            "deepseek-coder",
            "deepseek-reasoner"
        },
        ["kimi"] = new List<string>
        {
            "k3",
            "k3-256k",
            "kimi-for-coding",
            "kimi-for-coding-highspeed"
        },
        ["glm"] = new List<string>
        {
            "glm-4-plus",
            "glm-4-0520",
            "glm-4-air",
            "glm-4-airx",
            "glm-4-flash",
            "glm-3-turbo"
        },
        ["qwen"] = new List<string>
        {
            "qwen-turbo",
            "qwen-plus",
            "qwen-max",
            "qwen-max-longcontext"
        },
        ["doubao"] = new List<string>
        {
            "doubao-pro-4k",
            "doubao-pro-32k",
            "doubao-pro-128k",
            "doubao-lite-4k"
        },
        ["claude"] = new List<string>
        {
            "claude-3-5-sonnet-20241022",
            "claude-3-5-haiku-20241022",
            "claude-3-opus-20240229",
            "claude-3-sonnet-20240229",
            "claude-3-haiku-20240307"
        },
        ["gemini"] = new List<string>
        {
            "gemini-2.0-flash",
            "gemini-1.5-pro",
            "gemini-1.5-flash",
            "gemini-1.5-flash-8b"
        },
        ["ollama"] = new List<string>
        {
            "llama3.1",
            "llama3.2",
            "qwen2.5",
            "deepseek-coder-v2",
            "codellama"
        }
    };

    private static readonly Dictionary<string, string> _displayNames = new()
    {
        ["openai"] = "OpenAI",
        ["azure"] = "Azure OpenAI",
        ["deepseek"] = "DeepSeek",
        ["kimi"] = "Kimi",
        ["glm"] = "智谱 GLM",
        ["qwen"] = "通义千问",
        ["doubao"] = "豆包",
        ["claude"] = "Claude",
        ["gemini"] = "Google Gemini",
        ["ollama"] = "Ollama (本地)"
    };
}