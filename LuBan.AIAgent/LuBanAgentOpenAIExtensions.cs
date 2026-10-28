/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent
*文件名： LuBanAgentOpenAIExtensions
*版本号： V1.0.0.0
*唯一标识：1da05be0-324c-4cc7-84c4-428de09159c0
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：按配置注册 OpenAI 兼容 Provider 的 IChatClient
*
*****************************************************************************/

namespace LuBan.AIAgent;

/// <summary>OpenAI 兼容 Provider 注册扩展。</summary>
public static class LuBanAgentOpenAIExtensions
{
    /// <summary>
    /// 按 <c>LuBanAgent:Models</c> 配置注册 <see cref="IChatClient"/> 与 <see cref="IProviderRouter"/>；配置缺失时只记 warning，不注册。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="configuration">配置。</param>
    /// <returns>服务集合。</returns>
    /// <remarks>
    /// <para>
    /// 配置形态（数组，每项一个 provider:model 组合）：
    /// <code>
    /// "LuBanAgent": {
    ///   "DefaultModel": "deepseek:deepseek-v4-pro",
    ///   "Models": [
    ///     { "Provider": "deepseek", "Model": "deepseek-v4-pro", "ApiKey": "sk-xxx" },
    ///     { "Provider": "kimi", "Model": "kimi-k3", "ApiKey": "sk-yyy", "BaseUrl": "https://api.moonshot.cn/v1/" }
    ///   ]
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// 同一 Provider 可配置多个模型，但 <c>ApiKey</c> / <c>BaseUrl</c> 必须一致；不一致时跳过该 Provider 并写入
    /// <see cref="ChatClientFactoryWarning"/>。未配置完整时，宿主可自行注册 <see cref="IChatClient"/>。
    /// </para>
    /// </remarks>
    public static IServiceCollection AddLuBanAgentOpenAI(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var models = ReadModels(configuration);
        var warnings = new List<string>();

        var valid = new List<ModelEndpointOptions>();
        foreach (var item in models)
        {
            if (string.IsNullOrWhiteSpace(item.Provider)
                || string.IsNullOrWhiteSpace(item.Model)
                || string.IsNullOrWhiteSpace(item.ApiKey))
            {
                warnings.Add("LuBanAgent:Models 中有项缺少 Provider/Model/ApiKey，已忽略该项。");
                continue;
            }
            valid.Add(item);
        }

        if (valid.Count == 0)
        {
            // 配置不全：保持宿主自行注册 IChatClient 的能力，这里不注册也不抛
            services.TryAddSingleton(new ChatClientFactoryWarning(
                "LuBanAgent:Models 未配置（或全部项不完整），未注册默认 IChatClient。"));
            return services;
        }

        // Provider → OpenAI 客户端；同一 Provider 的多条模型共用一份凭据与端点
        var clients = new Dictionary<string, (OpenAIClient Client, OpenAIClientOptions? Options)>(StringComparer.OrdinalIgnoreCase);
        foreach (var group in valid.GroupBy(m => m.Provider!, StringComparer.OrdinalIgnoreCase))
        {
            var first = group.First();

            if (group.Skip(1).Any(m => !SameCredentials(first, m)))
            {
                warnings.Add($"Provider '{first.Provider}' 在同一 Models 数组中出现多条但 ApiKey/BaseUrl 不一致，已跳过该 Provider。");
                continue;
            }

            var baseUrl = ResolveBaseUrl(first);
            Uri? endpoint = null;
            if (!string.IsNullOrWhiteSpace(baseUrl)
                && (!Uri.TryCreate(baseUrl, UriKind.Absolute, out endpoint)
                    || (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps)))
            {
                warnings.Add($"Provider '{first.Provider}' 的 BaseUrl '{baseUrl}' 不是合法的 http(s) 地址，已跳过该 Provider。");
                continue;
            }

            var clientOptions = new OpenAIClientOptions { Endpoint = endpoint };
            clients[first.Provider!] = (new OpenAIClient(new ApiKeyCredential(first.ApiKey!), clientOptions), clientOptions);
        }

        if (clients.Count == 0)
        {
            services.TryAddSingleton(new ChatClientFactoryWarning(
                string.Join(" ", warnings) + " 未注册默认 IChatClient。"));
            return services;
        }

        var defaultEntry = ResolveDefaultEntry(valid, clients, configuration["LuBanAgent:DefaultModel"], warnings);

        // 路由：按 provider:model 分发，供 LuBanAgentFactory 在显式选择模型时使用
        services.TryAddSingleton<IProviderRouter>(new LuBanChatClient(clients, defaultEntry.Provider!));

        // 默认客户端：给不带 ModelId 的调用（会话摘要、上下文压缩等）使用，复用已建好的 Provider 客户端
        var defaultOpenAIClient = clients[defaultEntry.Provider!].Client;
        services.TryAddSingleton<IChatClient>(_ =>
            defaultOpenAIClient.GetChatClient(defaultEntry.Model!).AsIChatClient());

        if (warnings.Count > 0)
            services.TryAddSingleton(new ChatClientFactoryWarning(string.Join(" ", warnings)));

        return services;
    }

    /// <summary>
    /// 读取 <c>LuBanAgent:Models</c> 数组。逐项手工读取，避免配置绑定器对空数组的兼容分支。
    /// </summary>
    private static List<ModelEndpointOptions> ReadModels(IConfiguration configuration)
    {
        var models = new List<ModelEndpointOptions>();
        foreach (var child in configuration.GetSection("LuBanAgent:Models").GetChildren())
        {
            // 数组节点是 "0"、"1"…；旧版扁平对象写法（Provider/Model/ApiKey 同级）不再支持
            if (!int.TryParse(child.Key, out _)) continue;

            models.Add(new ModelEndpointOptions
            {
                Provider = child["Provider"]?.Trim(),
                Model = child["Model"]?.Trim(),
                ApiKey = child["ApiKey"]?.Trim(),
                BaseUrl = child["BaseUrl"]?.Trim()
            });
        }
        return models;
    }

    /// <summary>取默认模型项；未配置或未命中数组时回退到第一个可用项。</summary>
    private static ModelEndpointOptions ResolveDefaultEntry(
        List<ModelEndpointOptions> valid,
        Dictionary<string, (OpenAIClient Client, OpenAIClientOptions? Options)> clients,
        string? defaultModel,
        List<string> warnings)
    {
        ModelEndpointOptions? selected = null;
        defaultModel = defaultModel?.Trim();

        if (!string.IsNullOrWhiteSpace(defaultModel))
        {
            var idx = defaultModel.IndexOf(':');
            if (idx > 0 && idx < defaultModel.Length - 1)
            {
                var provider = defaultModel[..idx];
                var model = defaultModel[(idx + 1)..];
                selected = valid.FirstOrDefault(m =>
                    string.Equals(m.Provider, provider, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(m.Model, model, StringComparison.Ordinal));
            }

            if (selected is null)
                warnings.Add($"DefaultModel '{defaultModel}' 未在 Models 数组中找到对应项，已回退到第一个可用模型。");
        }

        selected ??= valid.FirstOrDefault(m => clients.ContainsKey(m.Provider!));
        return selected ?? valid.First(m => clients.ContainsKey(m.Provider!));
    }

    /// <summary>有效 BaseUrl：显式配置优先，缺省取 Provider 内置地址（可能为 null，落到 OpenAI 默认端点）。</summary>
    private static string? ResolveBaseUrl(ModelEndpointOptions item)
        => string.IsNullOrWhiteSpace(item.BaseUrl)
            ? ProviderDefaults.GetDefaultBaseUrl(item.Provider ?? "")
            : item.BaseUrl.Trim();

    /// <summary>同一 Provider 的多条模型是否使用同一份凭据与端点。</summary>
    private static bool SameCredentials(ModelEndpointOptions a, ModelEndpointOptions b)
        => string.Equals(a.ApiKey, b.ApiKey, StringComparison.Ordinal)
           && string.Equals(NormalizeUrl(ResolveBaseUrl(a)), NormalizeUrl(ResolveBaseUrl(b)), StringComparison.OrdinalIgnoreCase);

    private static string NormalizeUrl(string? url) => url?.Trim().TrimEnd('/') ?? string.Empty;
}

/// <summary>启动期提示载体（宿主可读取并打印）。</summary>
/// <param name="Message">提示文案。</param>
public sealed record ChatClientFactoryWarning(string Message);