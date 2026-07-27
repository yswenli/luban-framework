using System.Linq;
using System.Reflection;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Infrastructure;
using LuBan.AIAgent.MCP;
using LuBan.AIAgent.MCP.BuiltIn;
using LuBan.AIAgent.Plugins;
using LuBan.AIAgent.Rules;
using LuBan.AIAgent.Rules.BuiltIn;
using LuBan.AIAgent.Skills;
using LuBan.AIAgent.Skills.BuiltIn;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuBan.AIAgent;

/// <summary>
/// LuBan Agent 服务集合扩展
/// </summary>
public static class LuBanAgentExtensions
{
    /// <summary>
    /// 添加 LuBan Agent 服务（不带默认 ChatClient）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddLuBanAgent(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LuBanAgentOptions>(configuration.GetSection("LuBanAgent"));

        // 注册工具插件
        services.AddScoped<ILuBanToolPlugin, Tools.Browser.BrowserToolPlugin>();
        services.AddSingleton<ILuBanToolPlugin, Tools.FileSystem.FileSystemToolPlugin>();
        services.AddSingleton<ILuBanToolPlugin, Tools.Script.ScriptToolPlugin>();
        services.AddSingleton<ILuBanToolPlugin, Tools.Database.DatabaseToolPlugin>();
        services.AddSingleton<ILuBanToolPlugin, Tools.Redis.RedisToolPlugin>();
        services.AddSingleton<ILuBanToolPlugin, Tools.Web.WebToolPlugin>();

        LoadExternalPlugins(services, configuration);

        services.AddSingleton<ToolPluginRegistry>();

        // 注册 Rules
        services.AddSingleton<IRule, PathAccessRule>();
        services.AddSingleton<RuleEngine>();

        // 注册 MCP
        services.AddSingleton<IMCPClient, FileSystemMCPClient>();
        services.AddSingleton<MCPRegistry>();

        // 注册 Skills
        services.AddSingleton<ISkill, BrainstormingSkill>();
        services.AddSingleton<ISkill, CodeReviewSkill>();
        services.AddSingleton<ISkill, DocumentationSkill>();
        services.AddSingleton<SkillRegistry>();

        services.AddScoped<ILuBanAgentFactory, LuBanAgentFactory>();

        services.AddScoped<PlaywrightSession>();
        services.AddSingleton<ProcessRunner>();
        services.AddSingleton<PathGuard>();

        return services;
    }

    /// <summary>
    /// 添加 LuBan Agent 服务（带自定义 ChatClient）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <param name="chatClientFactory">ChatClient 工厂方法</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddLuBanAgent(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<IServiceProvider, IChatClient> chatClientFactory)
    {
        services.AddSingleton<IChatClient>(chatClientFactory);
        return services.AddLuBanAgent(configuration);
    }

    private static void LoadExternalPlugins(IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("LuBanAgent").Get<LuBanAgentOptions>();
        if (options?.ExternalPlugins == null) return;

        foreach (var assemblyName in options.ExternalPlugins)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(ILuBanToolPlugin).IsAssignableFrom(t) && !t.IsAbstract);

                foreach (var type in pluginTypes)
                {
                    services.AddSingleton(typeof(ILuBanToolPlugin), type);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载外部插件程序集 '{assemblyName}' 失败: {ex.Message}");
            }
        }
    }
}