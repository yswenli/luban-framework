/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
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
using Microsoft.Extensions.DependencyInjection.Extensions;

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
    [RequiresUnreferencedCode("AddLuBanAgent loads external plugins via Assembly.Load and GetTypes, which is incompatible with Native AOT trimming.")]
    public static IServiceCollection AddLuBanAgent(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LuBanAgentOptions>(configuration.GetSection("LuBanAgent"));

        // 注册工具插件（scoped：每会话一份，避免跨会话共享可变确认/根解析状态）
        services.AddScoped<ILuBanToolPlugin, Tools.Browser.BrowserToolPlugin>();
        services.AddScoped<ILuBanToolPlugin, Tools.FileSystem.FileSystemToolPlugin>();
        services.AddScoped<ILuBanToolPlugin, Tools.Script.ScriptToolPlugin>();
        services.AddScoped<ILuBanToolPlugin, Tools.Web.WebToolPlugin>();
        services.AddScoped<ILuBanToolPlugin, Tools.Git.GitToolPlugin>();
        services.AddScoped<ILuBanToolPlugin, Tools.Retrieval.RetrievalToolPlugin>();
        services.AddScoped<ILuBanToolPlugin, Tools.LocalMemory.LocalMemoryToolPlugin>();
        services.AddScoped<ILuBanToolPlugin, MCP.MCPToolPlugin>();

        services.AddScoped<ILuBanToolPlugin, Tools.Context.CompactContextToolPlugin>();
        services.AddScoped<ILuBanToolPlugin, Tools.Wiki.WikiToolPlugin>();
        services.AddScoped<ILuBanToolPlugin, Tools.Log.LogToolPlugin>();

        LoadExternalPlugins(services, configuration);

        services.AddScoped<ToolPluginRegistry>();

        // 注册 Rules（scoped：每会话一份，避免跨会话/跨工作区共享可变注册表）
        services.AddScoped<IRule, PathAccessRule>();
        services.AddScoped<IRule, Rules.BuiltIn.BaseBehaviorRule>();
        services.AddScoped<IRule, Rules.BuiltIn.MemoryRecallRule>();
        services.AddScoped<RuleEngine>();

        // 注册 MCP（scoped：每会话一份，避免跨会话/跨工作区共享可变注册表）
        services.AddScoped<IMCPClient, FileSystemMCPClient>();
        services.AddScoped<MCPRegistry>();

        // 注册 Skills（scoped：每会话一份，避免跨会话/跨工作区共享可变注册表）
        services.AddScoped<ISkill, BrainstormingSkill>();
        services.AddScoped<ISkill, CodeReviewSkill>();
        services.AddScoped<ISkill, DocumentationSkill>();
        services.AddScoped<ISkill, CodeRefactorSkill>();
        services.AddScoped<ISkill, TestGenerationSkill>();
        services.AddScoped<ISkill, CodeExplainSkill>();
        services.AddScoped<ISkill, DebugAssistantSkill>();
        services.AddScoped<ISkill, GitCommitSkill>();
        services.AddScoped<ISkill, FindSkillsSkill>();
        services.AddScoped<ISkill, AgentsMdGeneratorSkill>();
        services.AddScoped<SkillRegistry>();

        services.AddScoped<ILuBanAgentFactory, LuBanAgentFactory>();

        services.AddScoped<PlaywrightSession>();
        services.AddSingleton<ProcessRunner>();
        services.AddSingleton<LuaScriptRunner>();
        services.AddSingleton<ShellEnvironmentDetector>();
        services.AddScoped<PathGuard>();

        // 大日志分析：绑定配置 + 注册流式聚合器（工具组插件在 Task 10 追加注册）
        services.Configure<LogToolOptions>(configuration.GetSection("LuBanAgent:Tools:Log"));
        services.AddSingleton<ILogAggregator, LogAggregator>();

        services.AddSingleton<LuBan.AIAgent.Attachments.IAttachmentProcessor, LuBan.AIAgent.Attachments.DefaultAttachmentProcessor>();

        // 注册工具确认服务（scoped：每会话一份，宿主层每轮对话在会话作用域内设置/重置）
        services.AddScoped<ToolConfirmationContext>();
        services.AddScoped<IToolConfirmationService, ToolConfirmationService>();

        // 注册本地长期记忆（可选依赖 IEmbeddingGenerator）
        // 注意：ILocalMemoryStore 由宿主注册，框架不再默认注册 SqliteLocalMemoryStore
        services.Configure<LocalMemoryOptions>(configuration.GetSection("LuBanAgent:Tools:LocalMemory"));
        services.AddSingleton<ILocalMemoryService, LocalMemoryService>();

        // ===== Orchestration 子系统注册 =====
        // ContextStore 纯内存线程安全字典，可 Singleton
        services.AddSingleton<Orchestration.ContextStore>();

        // 已规划图谱暂存（plan_task 登记 → run_orchestration 取用）
        services.AddSingleton<Orchestration.GraphPlanStore>();

        // 编排进度出口：宿主可在调用 AddLuBanAgent 前覆盖注册 UI 实现，否则保持 no-op
        services.TryAddSingleton<Orchestration.IOrchestrationProgressSink, Orchestration.NullOrchestrationProgressSink>();

        // SubAgent 角色注册表
        services.AddSingleton<Orchestration.SubAgentRoleRegistry>();

        // context-build 注入构造器：常规对话（SessionChatHistoryProvider）与编排分支（Orchestrator）共用
        // scoped：依赖 RuleEngine（scoped），与消费者同为会话作用域，避免 captive dependency
        services.AddScoped<Rules.ContextInjectBuilder>();

        // SubAgentFactory / DagScheduler / Orchestrator 依赖 Scoped 的 LuBanAgentFactory，必须 Scoped
        services.AddScoped<Orchestration.SubAgentFactory>();
        // 子代理执行器接缝：默认实现为 SubAgentFactory（直接创建子代理）；启用 A2A 后由入口替换
        services.AddScoped<Orchestration.ISubAgentExecutor>(sp => sp.GetRequiredService<Orchestration.SubAgentFactory>());
        services.AddScoped<Orchestration.DagScheduler>();

        // 规划器：LlmTaskPlanner 依赖 IChatClient（通常 Scoped）+ 可选 IProviderRouter（PlannerModel 路由），必须 Scoped
        services.AddScoped<Orchestration.Planner.LlmTaskPlanner>();
        services.AddScoped<Orchestration.Planner.ITaskPlanner>(sp => sp.GetRequiredService<Orchestration.Planner.LlmTaskPlanner>());

        services.AddScoped<Orchestration.IOrchestrator, Orchestration.Orchestrator>();

        // 编排工具暴露（按配置 Enabled 开关；由 LLM 经 plan_task/run_orchestration 自行判定）
        var orchestrationEnabled = configuration
            .GetSection("LuBanAgent:Orchestration:Enabled").Get<bool>();
        if (orchestrationEnabled)
        {
            services.AddScoped<ILuBanToolPlugin, Tools.Orchestration.OrchestrationToolPlugin>();
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
    [RequiresUnreferencedCode("AddLuBanAgent loads external plugins via Assembly.Load and GetTypes, which is incompatible with Native AOT trimming.")]
    public static IServiceCollection AddLuBanAgent(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<IServiceProvider, IChatClient> chatClientFactory)
    {
        services.AddSingleton<IChatClient>(chatClientFactory);
        return services.AddLuBanAgent(configuration);
    }

    [RequiresUnreferencedCode("Calls Microsoft.Extensions.Configuration.ConfigurationBinder.Get<T>()")]
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
                    .Where(t => typeof(ILuBanToolPlugin).IsAssignableFrom(t) && !t.IsAbstract)
                    .ToList();

                // 诊断：当外部插件程序集中未找到任何 ILuBanToolPlugin 实现时，
                // 提示消费方可能需要在 PublishTrimmed=true 场景下配置 TrimmerRootAssembly
                if (pluginTypes.Count == 0)
                {
                    Logger.Warn($"外部插件程序集 '{assemblyName}' 中未找到任何 ILuBanToolPlugin 实现。"
                        + "若已启用 PublishTrimmed，请确保为该程序集配置 TrimmerRootAssembly 或 "
                        + "使用 [DynamicDependency] 注解以防止类型被裁剪。");
                }

                foreach (var type in pluginTypes)
                {
                    services.AddScoped(typeof(ILuBanToolPlugin), type);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("加载外部插件程序集失败", ex, assemblyName);
                ConsoleUtil.WriteLine($"加载外部插件程序集 '{assemblyName}' 失败: {ex.Message}", color: ConsoleColor.Red);
            }
        }
    }
}