using LuBan.AIAgent.Abstractions;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 聊天会话构建器，用于构建聊天会话实例
/// </summary>
/// <param name="chatClient">聊天客户端</param>
/// <param name="modelId">模型ID</param>
public class ChatSessionBuilder(IChatClient chatClient, string modelId)
{
    private IToolRegistry? _toolRegistry;
    private int _maxToolLoopIterations = 5;
    private IServiceProvider? _serviceProvider;
    private ILogger<ChatSession>? _logger;
    private IReadOnlyList<ChatMessage> _history = [];
    private IToolExecutionGate? _toolExecutionGate;
    private string? _sessionId;
    private string? _conversationId;
    private IReadOnlyList<IChatTurnMiddleware>? _chatTurnMiddlewares;
    private IReadOnlyList<IStreamingChatTurnMiddleware>? _streamingChatTurnMiddlewares;
    private IReadOnlyList<IToolExecutionMiddleware>? _toolExecutionMiddlewares;

    /// <summary>
    /// 设置工具注册表
    /// </summary>
    /// <param name="toolRegistry">工具注册表</param>
    /// <returns>聊天会话构建器</returns>
    public ChatSessionBuilder WithToolRegistry(IToolRegistry? toolRegistry)
    {
        _toolRegistry = toolRegistry;
        return this;
    }

    /// <summary>
    /// 设置最大工具循环迭代次数
    /// </summary>
    /// <param name="maxToolLoopIterations">最大工具循环迭代次数</param>
    /// <returns>聊天会话构建器</returns>
    public ChatSessionBuilder WithMaxToolLoopIterations(int maxToolLoopIterations)
    {
        _maxToolLoopIterations = maxToolLoopIterations;
        return this;
    }

    /// <summary>
    /// 设置服务提供者
    /// </summary>
    /// <param name="serviceProvider">服务提供者</param>
    /// <returns>聊天会话构建器</returns>
    public ChatSessionBuilder WithServiceProvider(IServiceProvider? serviceProvider)
    {
        _serviceProvider = serviceProvider;
        return this;
    }

    /// <summary>
    /// 使用服务提供者
    /// </summary>
    /// <param name="serviceProvider">服务提供者</param>
    /// <returns>聊天会话构建器</returns>
    public ChatSessionBuilder UseServiceProvider(IServiceProvider? serviceProvider)
    {
        _serviceProvider = serviceProvider;
        return this;
    }

    /// <summary>
    /// 设置日志记录器
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <returns>聊天会话构建器</returns>
    public ChatSessionBuilder WithLogger(ILogger<ChatSession>? logger)
    {
        _logger = logger;
        return this;
    }

    /// <summary>
    /// 设置工具执行门控
    /// </summary>
    /// <param name="toolExecutionGate">工具执行门控</param>
    /// <returns>聊天会话构建器</returns>
    public ChatSessionBuilder WithToolExecutionGate(IToolExecutionGate? toolExecutionGate)
    {
        _toolExecutionGate = toolExecutionGate;
        return this;
    }

    /// <summary>
    /// 设置会话ID
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <returns>聊天会话构建器</returns>
    public ChatSessionBuilder WithSessionId(string? sessionId)
    {
        _sessionId = sessionId;
        return this;
    }

    /// <summary>
    /// 设置对话ID
    /// </summary>
    /// <param name="conversationId">对话ID</param>
    /// <returns>聊天会话构建器</returns>
    public ChatSessionBuilder WithConversationId(string? conversationId)
    {
        _conversationId = conversationId;
        return this;
    }

    /// <summary>
    /// 设置聊天回合中间件
    /// </summary>
    /// <param name="chatTurnMiddlewares">聊天回合中间件集合</param>
    /// <returns>聊天会话构建器</returns>
    public ChatSessionBuilder WithChatTurnMiddleware(IEnumerable<IChatTurnMiddleware> chatTurnMiddlewares)
    {
        _chatTurnMiddlewares = chatTurnMiddlewares.ToList();
        return this;
    }

    /// <summary>
    /// 设置流式聊天回合中间件
    /// </summary>
    /// <param name="streamingChatTurnMiddlewares">流式聊天回合中间件集合</param>
    /// <returns>聊天会话构建器</returns>
    public ChatSessionBuilder WithStreamingChatTurnMiddleware(IEnumerable<IStreamingChatTurnMiddleware> streamingChatTurnMiddlewares)
    {
        _streamingChatTurnMiddlewares = streamingChatTurnMiddlewares.ToList();
        return this;
    }

    /// <summary>
    /// 设置工具执行中间件
    /// </summary>
    /// <param name="toolExecutionMiddlewares">工具执行中间件集合</param>
    /// <returns>聊天会话构建器</returns>
    public ChatSessionBuilder WithToolExecutionMiddleware(IEnumerable<IToolExecutionMiddleware> toolExecutionMiddlewares)
    {
        _toolExecutionMiddlewares = toolExecutionMiddlewares.ToList();
        return this;
    }

    /// <summary>
    /// 设置聊天历史记录
    /// </summary>
    /// <param name="history">聊天历史记录</param>
    /// <returns>聊天会话构建器</returns>
    public ChatSessionBuilder WithHistory(IEnumerable<ChatMessage> history)
    {
        _history = history.ToList();
        return this;
    }

    /// <summary>
    /// 构建聊天会话实例
    /// </summary>
    /// <returns>聊天会话实例</returns>
    public ChatSession Build()
    {
        var session = new ChatSession(
            chatClient,
            modelId,
            _toolRegistry,
            _maxToolLoopIterations,
            _toolExecutionGate,
            _sessionId,
            _conversationId,
            _serviceProvider,
            _logger,
            _chatTurnMiddlewares,
            _streamingChatTurnMiddlewares,
            _toolExecutionMiddlewares);

        foreach (var message in _history)
        {
            session.AddMessage(message);
        }

        return session;
    }
}
