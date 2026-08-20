/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent
*文件名： LuBanAgentFactory
*版本号： V1.0.0.0
*唯一标识：5ecf6fa5-aa2a-4957-8be1-bddf447ca821
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:21:20
*描述：LuBan Agent 工厂实现
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：提取共享逻辑，新增 CreateSubAgentAsync
*
*****************************************************************************/
namespace LuBan.AIAgent;

/// <summary>
/// LuBan Agent 工厂实现
/// </summary>
public class LuBanAgentFactory : ILuBanAgentFactory, IScoped
{
    private readonly IChatClient _chatClient;
    private readonly IProviderRouter? _providerRouter;
    private readonly ToolPluginRegistry _pluginRegistry;
    private readonly IOptions<LuBanAgentOptions> _options;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 创建 LuBanAgentFactory 实例
    /// </summary>
    /// <param name="chatClient">聊天客户端（默认模型）</param>
    /// <param name="pluginRegistry">工具插件注册表</param>
    /// <param name="options">配置选项</param>
    /// <param name="serviceProvider">服务提供者</param>
    /// <param name="providerRouter">模型提供者路由（可选，注册后支持按 modelName 路由）</param>
    public LuBanAgentFactory(
        IChatClient chatClient,
        ToolPluginRegistry pluginRegistry,
        IOptions<LuBanAgentOptions> options,
        IServiceProvider serviceProvider,
        IProviderRouter? providerRouter = null)
    {
        _chatClient = chatClient;
        _pluginRegistry = pluginRegistry;
        _options = options;
        _serviceProvider = serviceProvider;
        _providerRouter = providerRouter;
    }

    /// <summary>
    /// 创建 Agent 实例
    /// </summary>
    public Task<LuBanAgent> CreateAsync(
        string? modelName = null,
        string? systemPrompt = null,
        IEnumerable<string>? toolGroups = null,
        bool useSessionHistory = false,
        CancellationToken cancellationToken = default)
    {
        var opts = _options.Value;
        var instructions = systemPrompt ?? opts.SystemPrompt ?? "你是一个智能助手。";

        var tools = BuildTools(toolGroups);

        var functionClient = BuildFunctionClient(tools, opts, modelName);
        var historyProvider = BuildHistoryProvider(useSessionHistory, opts);

        var agent = new ChatClientAgent(
            functionClient,
            new ChatClientAgentOptions
            {
                Name = "LuBanAgent",
                Description = opts.Description ?? "LuBan AI Agent",
                ChatOptions = new ChatOptions
                {
                    Instructions = instructions,
                    Tools = tools
                },
                ChatHistoryProvider = historyProvider,
                ThrowOnChatHistoryProviderConflict = false,
                WarnOnChatHistoryProviderConflict = false
            },
            null,
            _serviceProvider);

        return Task.FromResult(new LuBanAgent(agent));
    }

    /// <summary>
    /// 创建 SubAgent 实例。静默创建，不打印工具列表，不启用 SessionHistory。
    /// </summary>
    /// <param name="modelName">模型名称（格式 "provider:model"，经 IProviderRouter 路由；null 表示默认模型）。</param>
    /// <param name="toolGroups">工具组列表，null 表示全部启用。</param>
    /// <param name="systemPrompt">系统提示词。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>LuBanAgent 实例。</returns>
    public Task<LuBanAgent> CreateSubAgentAsync(
        string? modelName,
        IEnumerable<string>? toolGroups,
        string systemPrompt,
        CancellationToken cancellationToken = default)
    {
        var opts = _options.Value;
        var tools = BuildTools(toolGroups);

        var functionClient = BuildFunctionClient(tools, opts, modelName);

        var agent = new ChatClientAgent(
            functionClient,
            new ChatClientAgentOptions
            {
                Name = "SubAgent",
                Description = "LuBan SubAgent for Orchestration",
                ChatOptions = new ChatOptions
                {
                    Instructions = systemPrompt,
                    Tools = tools
                }
            },
            null,
            _serviceProvider);

        return Task.FromResult(new LuBanAgent(agent));
    }

    /// <summary>
    /// 构建工具列表，应用规则装饰。
    /// </summary>
    /// <param name="toolGroups">工具组筛选，null 表示全部。</param>
    /// <returns>装饰后的工具列表。</returns>
    private List<AITool> BuildTools(IEnumerable<string>? toolGroups)
    {
        var plugins = _pluginRegistry.GetPlugins(toolGroups);

        var tools = plugins
            .SelectMany(p => p.GetTools(_serviceProvider))
            .Cast<AITool>()
            .ToList();

        var ruleEngine = _serviceProvider.GetService<RuleEngine>();
        if (ruleEngine != null)
        {
            tools = tools
                .Select(t => t is AIFunction f ? new RuleCheckedAIFunction(f, ruleEngine) : t)
                .ToList();
        }
        return tools;
    }

    /// <summary>
    /// 构建 FunctionInvokingChatClient。
    /// </summary>
    /// <param name="tools">工具列表。</param>
    /// <param name="opts">配置选项。</param>
    /// <param name="modelName">模型名称（格式 "provider:model"），null 表示默认模型。</param>
    /// <returns>FunctionInvokingChatClient 实例。</returns>
    private FunctionInvokingChatClient BuildFunctionClient(List<AITool> tools, LuBanAgentOptions opts, string? modelName = null)
    {
        var sanitizedClient = new SanitizingChatClient(ResolveChatClient(modelName));
        var loggerFactory = _serviceProvider.GetService<ILoggerFactory>();
        return new FunctionInvokingChatClient(sanitizedClient, loggerFactory, _serviceProvider)
        {
            MaximumIterationsPerRequest = Math.Max(1, opts.MaxToolLoopIterations)
        };
    }

    /// <summary>
    /// 按模型名称解析聊天客户端。未指定模型或未注册路由时使用注入的默认客户端；路由失败回退默认客户端。
    /// </summary>
    /// <param name="modelName">模型名称（格式 "provider:model"）。</param>
    /// <returns>聊天客户端实例。</returns>
    private IChatClient ResolveChatClient(string? modelName)
    {
        if (string.IsNullOrEmpty(modelName) || _providerRouter == null)
            return _chatClient;
        try
        {
            return _providerRouter.CreateChatClient(modelName);
        }
        catch (Exception ex)
        {
            Logger.Warn($"模型 '{modelName}' 路由失败（{ex.Message}），回退默认模型");
            return _chatClient;
        }
    }

    /// <summary>
    /// 构建 SessionChatHistoryProvider（仅当 useSessionHistory 为 true 时）。
    /// </summary>
    /// <param name="useSessionHistory">是否启用会话历史。</param>
    /// <param name="opts">配置选项。</param>
    /// <returns>ChatHistoryProvider 实例或 null。</returns>
    private ChatHistoryProvider? BuildHistoryProvider(bool useSessionHistory, LuBanAgentOptions opts)
    {
        if (!useSessionHistory
            || _serviceProvider.GetService<ISessionManager>() is not { } sessionManager)
            return null;

        var ruleEngine = _serviceProvider.GetService<RuleEngine>();
        return new SessionChatHistoryProvider(
            sessionManager,
            _chatClient,
            opts.Session.CompactTargetMessages,
            opts.Session.CompactThreshold,
            ruleEngine);
    }

}
