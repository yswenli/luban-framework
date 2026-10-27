/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent
*文件名： LuBanAgentOpenAIExtensions
*版本号： V1.0.0.0
*唯一标识：OpenAI 兼容 Provider 注册扩展
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：按配置注册 OpenAI 兼容 Provider 的 IChatClient
*
*****************************************************************************/
using System.ClientModel;
using LuBan.AIAgent.Configuration.Providers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenAI;

namespace LuBan.AIAgent;

/// <summary>OpenAI 兼容 Provider 注册扩展。</summary>
public static class LuBanAgentOpenAIExtensions
{
    /// <summary>
    /// 按 <c>LuBanAgent:Models</c> 配置注册 <see cref="IChatClient"/>；配置缺失时只记 warning，不注册。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="configuration">配置。</param>
    /// <returns>服务集合。</returns>
    /// <remarks>
    /// <para>
    /// 配置形态：
    /// <code>
    /// "LuBanAgent": {
    ///   "Models": {
    ///     "Provider": "deepseek",
    ///     "Model": "deepseek-v4-pro",
    ///     "ApiKey": "sk-xxx",
    ///     "BaseUrl": "https://api.deepseek.com/v1/"
    ///   }
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// 未配置完整时，宿主可自行注册 <see cref="IChatClient"/>，或读取 <see cref="ChatClientFactoryWarning"/> 提示用户。
    /// </para>
    /// </remarks>
    public static IServiceCollection AddLuBanAgentOpenAI(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection("LuBanAgent:Models");
        var provider = section["Provider"];
        var model = section["Model"];
        var apiKey = section["ApiKey"];
        var baseUrl = section["BaseUrl"] ?? ProviderDefaults.GetDefaultBaseUrl(provider ?? "");

        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(apiKey))
        {
            // 配置不全：保持宿主自行注册 IChatClient 的能力，这里不注册也不抛
            services.AddSingleton<ChatClientFactoryWarning>(new ChatClientFactoryWarning(
                "LuBanAgent:Models 未配置完整（Provider/Model/ApiKey），未注册默认 IChatClient。"));
            return services;
        }

        services.TryAddSingleton<IChatClient>(_ =>
        {
            var client = new OpenAIClient(
                new ApiKeyCredential(apiKey!),
                new OpenAIClientOptions { Endpoint = baseUrl is { Length: > 0 } ? new Uri(baseUrl) : null });

            return client.GetChatClient(model!).AsIChatClient();
        });

        return services;
    }
}

/// <summary>启动期提示载体（宿主可读取并打印）。</summary>
/// <param name="Message">提示文案。</param>
public sealed record ChatClientFactoryWarning(string Message);