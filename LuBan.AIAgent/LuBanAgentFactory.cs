using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LuBan.AIAgent.Plugins;
using LuBan.DI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

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
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>LuBanAgent 实例</returns>
    Task<LuBanAgent> CreateAsync(
        string? modelName = null,
        string? systemPrompt = null,
        IEnumerable<string>? toolGroups = null,
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
        CancellationToken cancellationToken = default)
    {
        var opts = _options.Value;
        var instructions = systemPrompt ?? opts.SystemPrompt ?? "你是一个智能助手。";

        var plugins = _pluginRegistry.GetPlugins(toolGroups);

        var tools = plugins
            .SelectMany(p => p.GetTools(_serviceProvider))
            .Cast<AITool>()
            .ToList();

        // 输出工具信息
        Console.WriteLine($"已加载 {plugins.Count} 个工具插件，共 {tools.Count} 个工具:");
        foreach (var tool in tools)
        {
            Console.WriteLine($"  - {tool.Name}: {tool.Description}");
        }

        var agent = new ChatClientAgent(
            _chatClient,
            instructions: instructions,
            name: "LuBanAgent",
            description: opts.Description ?? "LuBan AI Agent",
            tools: tools,
            services: _serviceProvider);

        return Task.FromResult(new LuBanAgent(agent));
    }
}