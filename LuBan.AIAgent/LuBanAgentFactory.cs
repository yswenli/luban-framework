/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent
*文件名： ILuBanAgentFactory
*版本号： V1.0.0.0
*唯一标识：5ecf6fa5-aa2a-4957-8be1-bddf447ca821
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:21:20
*描述：LuBan Agent 工厂接口
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:21:20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBan Agent 工厂接口
*
*****************************************************************************/
using LuBan.AIAgent.Core;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent;

/// <summary>
/// LuBan Agent 工厂接口
/// </summary>
public interface ILuBanAgentFactory
{
    /// <summary>
    /// 创建 Agent 实例
    /// </summary>
    /// <param name="modelName">模型名称，格式 "provider:model"</param>
    /// <param name="systemPrompt">自定义系统提示词</param>
    /// <param name="toolGroups">指定启用的工具组，null 表示全部启用</param>
    /// <param name="useSessionHistory">是否启用 Session 历史</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>LuBanAgent 实例</returns>
    Task<LuBanAgent> CreateAsync(
        string? modelName = null,
        string? systemPrompt = null,
        IEnumerable<string>? toolGroups = null,
        bool useSessionHistory = false,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// LuBan Agent 工厂实现
/// </summary>
public class LuBanAgentFactory : ILuBanAgentFactory, IScoped
{
    private readonly IChatClient _chatClient;
    private readonly ToolPluginRegistry _pluginRegistry;
    private readonly IOptions<Configuration.LuBanAgentOptions> _options;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 创建 LuBanAgentFactory 实例
    /// </summary>
    /// <param name="chatClient">聊天客户端</param>
    /// <param name="pluginRegistry">工具插件注册表</param>
    /// <param name="options">配置选项</param>
    /// <param name="serviceProvider">服务提供者</param>
    public LuBanAgentFactory(
        IChatClient chatClient,
        ToolPluginRegistry pluginRegistry,
        IOptions<Configuration.LuBanAgentOptions> options,
        IServiceProvider serviceProvider)
    {
        _chatClient = chatClient;
        _pluginRegistry = pluginRegistry;
        _options = options;
        _serviceProvider = serviceProvider;
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

        var plugins = _pluginRegistry.GetPlugins(toolGroups);

        var tools = plugins
            .SelectMany(p => p.GetTools(_serviceProvider))
            .Cast<AITool>()
            .ToList();

        var ruleEngine = _serviceProvider.GetService<Rules.RuleEngine>();
        if (ruleEngine != null)
        {
            tools = tools
                .Select(t => t is AIFunction f ? new Rules.RuleCheckedAIFunction(f, ruleEngine) : t)
                .ToList();
        }

        Console.WriteLine($"已加载 {plugins.Count} 个工具插件，共 {tools.Count} 个工具:");
        foreach (var tool in tools)
        {
            Console.WriteLine($"  - {tool.Name}: {tool.Description}");
        }

        var sanitizedClient = new SanitizingChatClient(_chatClient);

        var loggerFactory = _serviceProvider.GetService<ILoggerFactory>();
        var functionClient = new FunctionInvokingChatClient(sanitizedClient, loggerFactory, _serviceProvider)
        {
            MaximumIterationsPerRequest = Math.Max(1, opts.MaxToolLoopIterations)
        };

        ChatHistoryProvider? historyProvider = null;
        if (useSessionHistory
            && _serviceProvider.GetService<Sessions.ISessionManager>() is { } sessionManager)
        {
            historyProvider = new Sessions.SessionChatHistoryProvider(
                sessionManager,
                _chatClient,
                opts.Session.CompactTargetMessages,
                opts.Session.CompactThreshold);
        }

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
}