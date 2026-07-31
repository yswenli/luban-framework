/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent
*文件名： LuBanAgentExtensions
*版本号： V1.0.0.0
*唯一标识：5ecf6fa5-aa2a-4957-8be1-bddf447ca821
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:21:20
*描述：LuBan Agent 服务集合扩展
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:21:20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBan Agent 服务集合扩展
*
*****************************************************************************/
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
        services.AddSingleton<ILuBanToolPlugin, Tools.Retrieval.RetrievalToolPlugin>();
        services.AddSingleton<ILuBanToolPlugin, MCP.MCPToolPlugin>();

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

        // ===== Orchestration 子系统注册 =====
        // ContextStore 纯内存线程安全字典，可 Singleton
        services.AddSingleton<Orchestration.ContextStore>();

        // SubAgentFactory / DagScheduler / Orchestrator 依赖 Scoped 的 LuBanAgentFactory，必须 Scoped
        services.AddScoped<Orchestration.SubAgentFactory>();
        services.AddScoped<Orchestration.DagScheduler>();

        // 规划器：LlmTaskPlanner 依赖 IChatClient（通常 Scoped），TemplateTaskPlanner 无状态可 Singleton
        services.AddScoped<Orchestration.Planner.LlmTaskPlanner>();
        services.AddSingleton<Orchestration.Planner.TemplateTaskPlanner>();
        services.AddScoped<Orchestration.Planner.ITaskPlanner>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<LuBanAgentOptions>>().Value;
            return opts.Orchestration?.PlannerType switch
            {
                "llm"      => sp.GetRequiredService<Orchestration.Planner.LlmTaskPlanner>(),
                "template" => sp.GetRequiredService<Orchestration.Planner.TemplateTaskPlanner>(),
                _          => new Orchestration.Planner.CompositeTaskPlanner(
                                sp.GetRequiredService<Orchestration.Planner.TemplateTaskPlanner>(),
                                sp.GetRequiredService<Orchestration.Planner.LlmTaskPlanner>())
            };
        });

        services.AddScoped<Orchestration.IOrchestrator, Orchestration.Orchestrator>();

        // 暴露为工具（按配置开关）
        var orchestrationEnabled = configuration
            .GetSection("LuBanAgent:Orchestration:Enabled").Get<bool>();
        var exposeAsTool = configuration
            .GetSection("LuBanAgent:Orchestration:ExposeAsTool").Get<bool>();
        if (orchestrationEnabled && exposeAsTool)
        {
            services.AddSingleton<ILuBanToolPlugin, Tools.Orchestration.OrchestrationToolPlugin>();
        }

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
                Logger.Error("加载外部插件程序集失败", ex, assemblyName);
                System.Diagnostics.Debug.WriteLine($"加载外部插件程序集 '{assemblyName}' 失败: {ex.Message}");
            }
        }
    }
}