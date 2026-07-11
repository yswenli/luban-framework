using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using LuBan.AIAgent.Abstractions;
using System.Runtime.CompilerServices;
using System.Text;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 聊天会话类，负责管理聊天历史、执行聊天请求和工具调用
/// </summary>
public class ChatSession : IChatSession
{
    private readonly IChatClient _chatClient;
    private readonly string _modelId;
    private readonly List<ChatMessage> _history = new();
    private readonly IToolRegistry? _toolRegistry;
    private readonly int _maxToolLoopIterations;
    private readonly IToolExecutionGate _toolExecutionGate;
    private readonly string? _sessionId;
    private readonly string? _conversationId;
    private readonly IServiceProvider? _serviceProvider;
    private readonly ILogger<ChatSession>? _logger;
    private readonly ToolExecutor _toolExecutor;
    private readonly IReadOnlyList<IChatTurnMiddleware> _chatTurnMiddlewares;
    private readonly IReadOnlyList<IStreamingChatTurnMiddleware> _streamingChatTurnMiddlewares;

    /// <summary>
    /// 聊天历史记录
    /// </summary>
    public IReadOnlyList<ChatMessage> History => _history.AsReadOnly();

    /// <summary>
    /// 初始化ChatSession实例
    /// </summary>
    /// <param name="chatClient">聊天客户端</param>
    /// <param name="modelId">模型ID</param>
    /// <param name="toolRegistry">工具注册表</param>
    /// <param name="maxToolLoopIterations">最大工具循环迭代次数</param>
    /// <param name="toolExecutionGate">工具执行门</param>
    /// <param name="sessionId">会话ID</param>
    /// <param name="conversationId">对话ID</param>
    /// <param name="serviceProvider">服务提供者</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="chatTurnMiddlewares">聊天回合中间件</param>
    /// <param name="streamingChatTurnMiddlewares">流式聊天回合中间件</param>
    /// <param name="toolExecutionMiddlewares">工具执行中间件</param>
    public ChatSession(
        IChatClient chatClient,
        string modelId,
        IToolRegistry? toolRegistry = null,
        int maxToolLoopIterations = 5,
        IToolExecutionGate? toolExecutionGate = null,
        string? sessionId = null,
        string? conversationId = null,
        IServiceProvider? serviceProvider = null,
        ILogger<ChatSession>? logger = null,
        IEnumerable<IChatTurnMiddleware>? chatTurnMiddlewares = null,
        IEnumerable<IStreamingChatTurnMiddleware>? streamingChatTurnMiddlewares = null,
        IEnumerable<IToolExecutionMiddleware>? toolExecutionMiddlewares = null)
    {
        _chatClient = chatClient;
        _modelId = modelId;
        _toolRegistry = toolRegistry;
        _maxToolLoopIterations = maxToolLoopIterations;
        _toolExecutionGate = toolExecutionGate ?? new AutoApproveToolExecutionGate();
        _sessionId = sessionId;
        _conversationId = conversationId;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _chatTurnMiddlewares = chatTurnMiddlewares?.ToList()
            ?? serviceProvider?.GetServices<IChatTurnMiddleware>().ToList()
            ?? [];
        _streamingChatTurnMiddlewares = streamingChatTurnMiddlewares?.ToList()
            ?? serviceProvider?.GetServices<IStreamingChatTurnMiddleware>().ToList()
            ?? [];
        var resolvedToolMiddlewares = toolExecutionMiddlewares?.ToList()
            ?? serviceProvider?.GetServices<IToolExecutionMiddleware>().ToList()
            ?? [];
        _toolExecutor = new ToolExecutor(
            _toolExecutionGate,
            serviceProvider?.GetService(typeof(ILogger<ToolExecutor>)) as ILogger<ToolExecutor>,
            resolvedToolMiddlewares);
    }

    /// <summary>
    /// 添加消息到聊天历史
    /// </summary>
    /// <param name="message">聊天消息</param>
    public void AddMessage(ChatMessage message)
    {
        _history.Add(message);
    }

    /// <summary>
    /// 清除聊天历史
    /// </summary>
    public void ClearHistory()
    {
        _history.Clear();
    }

    /// <summary>
    /// 异步发送消息并返回响应
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="options">聊天选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天响应</returns>
    public async Task<ChatResponse> SendAsync(string message, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var result = await SendTurnAsync(message, options, cancellationToken);
        return result.Response;
    }

    /// <summary>
    /// 异步流式发送消息并返回实时更新
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="options">聊天选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合流更新的异步枚举</returns>
    public async IAsyncEnumerable<ChatTurnStreamUpdate> StreamTurnAsync(
        string message,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        AddMessage(ChatMessage.User(message));

        var context = new StreamingChatTurnExecutionContext
        {
            Kind = ChatTurnExecutionKind.SessionTurn,
            ModelId = _modelId,
            Input = message,
            Messages = _history.AsReadOnly(),
            Options = options,
            SessionId = _sessionId,
            ConversationId = _conversationId,
            ServiceProvider = _serviceProvider
        };

        await foreach (var update in ExecuteStreamingWithMiddlewareAsync(context, cancellationToken).WithCancellation(cancellationToken))
        {
            yield return update;
        }
    }

    /// <summary>
    /// 异步继续对话
    /// </summary>
    /// <param name="options">聊天选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合结果</returns>
    public Task<ChatTurnResult> ContinueAsync(ChatOptions? options = null, CancellationToken cancellationToken = default)
        => ExecuteTurnWithMiddlewareAsync(new ChatTurnExecutionContext
        {
            Kind = ChatTurnExecutionKind.SessionContinuation,
            ModelId = _modelId,
            Messages = _history.AsReadOnly(),
            Options = options,
            SessionId = _sessionId,
            ConversationId = _conversationId,
            ServiceProvider = _serviceProvider
        }, cancellationToken);

    /// <summary>
    /// 异步流式继续对话
    /// </summary>
    /// <param name="options">聊天选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合流更新的异步枚举</returns>
    public async IAsyncEnumerable<ChatTurnStreamUpdate> ContinueStreamAsync(
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var context = new StreamingChatTurnExecutionContext
        {
            Kind = ChatTurnExecutionKind.SessionContinuation,
            ModelId = _modelId,
            Messages = _history.AsReadOnly(),
            Options = options,
            SessionId = _sessionId,
            ConversationId = _conversationId,
            ServiceProvider = _serviceProvider
        };

        await foreach (var update in ExecuteStreamingWithMiddlewareAsync(context, cancellationToken).WithCancellation(cancellationToken))
        {
            yield return update;
        }
    }

    /// <summary>
    /// 异步发送聊天回合
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="options">聊天选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合结果</returns>
    public async Task<ChatTurnResult> SendTurnAsync(string message, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        AddMessage(ChatMessage.User(message));
        return await ExecuteTurnWithMiddlewareAsync(new ChatTurnExecutionContext
        {
            Kind = ChatTurnExecutionKind.SessionTurn,
            ModelId = _modelId,
            Input = message,
            Messages = _history.AsReadOnly(),
            Options = options,
            SessionId = _sessionId,
            ConversationId = _conversationId,
            ServiceProvider = _serviceProvider
        }, cancellationToken);
    }

    /// <summary>
    /// 执行带有中间件的聊天回合
    /// </summary>
    /// <param name="context">聊天回合执行上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合结果</returns>
    private Task<ChatTurnResult> ExecuteTurnWithMiddlewareAsync(
        ChatTurnExecutionContext context,
        CancellationToken cancellationToken)
    {
        return MiddlewarePipeline.ExecuteAsync(
            _chatTurnMiddlewares,
            context,
            static (middleware, executionContext, next, ct) => middleware.InvokeAsync(executionContext, next, ct),
            () => ExecuteTurnLoopAsync(context.Options, cancellationToken),
            cancellationToken);
    }

    /// <summary>
    /// 执行带有中间件的流式聊天回合
    /// </summary>
    /// <param name="context">流式聊天回合执行上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合流更新的异步枚举</returns>
    private IAsyncEnumerable<ChatTurnStreamUpdate> ExecuteStreamingWithMiddlewareAsync(
        StreamingChatTurnExecutionContext context,
        CancellationToken cancellationToken)
    {
        return MiddlewarePipeline.ExecuteStreaming(
            _streamingChatTurnMiddlewares,
            context,
            static (middleware, executionContext, next, ct) => middleware.InvokeAsync(executionContext, next, ct),
            () => ExecuteTurnLoopStreamingAsync(context.Options, cancellationToken),
            cancellationToken);
    }

    /// <summary>
    /// 执行聊天回合循环
    /// </summary>
    /// <param name="options">聊天选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合结果</returns>
    private async Task<ChatTurnResult> ExecuteTurnLoopAsync(ChatOptions? options, CancellationToken cancellationToken)
    {
        var finalOptions = BuildEffectiveOptions(options);

        ChatResponse? lastResponse = null;
        List<ToolResult>? lastToolResults = null;
        var iteration = 0;

        while (iteration < _maxToolLoopIterations)
        {
            _logger?.LogInformation("ChatSession iteration {Iteration}/{MaxIterations}", iteration + 1, _maxToolLoopIterations);
            
            var request = new ChatRequest
            {
                ModelId = _modelId,
                Messages = _history.AsReadOnly(),
                Options = finalOptions
            };

            lastResponse = await _chatClient.CompleteAsync(request, cancellationToken);

            if (!lastResponse.IsSuccess)
            {
                _logger?.LogError("ChatSession request failed: {ErrorMessage}", lastResponse.ErrorMessage);
                return new ChatTurnResult { Response = lastResponse };
            }

            if (lastResponse.Message != null)
            {
                _history.Add(lastResponse.Message);
            }

            var toolCalls = lastResponse.Message?.ToolCalls;
            if (toolCalls == null || toolCalls.Count == 0 || _toolRegistry == null)
            {
                _logger?.LogInformation("ChatSession completed without tool calls");
                break;
            }

            _logger?.LogInformation("ChatSession executing {ToolCount} tool calls", toolCalls.Count);
            var toolResults = new List<ToolResult>();
            foreach (var toolCall in toolCalls)
            {
                _logger?.LogInformation("Executing tool: {ToolName}", toolCall.Name);
                if (_toolRegistry.TryGetTool(toolCall.Name, out var tool) && tool != null)
                {
                    var context = new ToolExecutionContext
                    {
                        ToolCall = toolCall,
                        ChatHistory = _history.AsReadOnly(),
                        ServiceProvider = _serviceProvider,
                        SessionId = _sessionId,
                        ConversationId = _conversationId
                    };
                    var execution = await _toolExecutor.ExecuteAsync(tool, context, cancellationToken);
                    toolResults.Add(execution.Result);

                    if (execution.PendingApprovalRequest != null)
                    {
                        lastToolResults = toolResults;
                        return new ChatTurnResult
                        {
                            Response = lastResponse,
                            ToolResults = lastToolResults,
                            PendingApprovalRequest = execution.PendingApprovalRequest
                        };
                    }
                }
                else
                {
                    _logger?.LogWarning("Tool '{ToolName}' not found", toolCall.Name);
                    toolResults.Add(new ToolResult
                    {
                        ToolCallId = toolCall.Id,
                        Content = $"Tool '{toolCall.Name}' not found",
                        IsSuccess = false,
                        Status = ToolExecutionStatus.Failed
                    });
                }
            }

            lastToolResults = toolResults;

            foreach (var result in toolResults)
            {
                _history.Add(new ChatMessage
                {
                    Role = ChatRole.Tool,
                    ToolCallId = result.ToolCallId,
                    TextContent = result.Content
                });
            }

            iteration++;
        }

        return new ChatTurnResult
        {
            Response = lastResponse ?? new ChatResponse
            {
                IsSuccess = false,
                ErrorMessage = "No response received"
            },
            ToolResults = lastToolResults
        };
    }

    /// <summary>
    /// 构建有效选项
    /// </summary>
    /// <param name="options">聊天选项</param>
    /// <returns>有效聊天选项</returns>
    private ChatOptions BuildEffectiveOptions(ChatOptions? options)
    {
        var finalOptions = options ?? new ChatOptions();
        if (_toolRegistry != null)
        {
            var toolDefs = _toolRegistry.GetToolDefinitions();
            if (toolDefs.Count > 0)
            {
                finalOptions = finalOptions with
                {
                    Tools = toolDefs
                };
            }
        }

        return finalOptions;
    }

    /// <summary>
    /// 执行流式聊天回合循环
    /// </summary>
    /// <param name="options">聊天选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合流更新的异步枚举</returns>
    private async IAsyncEnumerable<ChatTurnStreamUpdate> ExecuteTurnLoopStreamingAsync(
        ChatOptions? options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var finalOptions = BuildEffectiveOptions(options);
        List<ToolResult>? lastToolResults = null;
        List<string>? lastToolNames = null;
        var iteration = 0;

        while (iteration < _maxToolLoopIterations)
        {
            _logger?.LogInformation("ChatSession streaming iteration {Iteration}/{MaxIterations}", iteration + 1, _maxToolLoopIterations);

            ChatResponse? response = null;
            await using var streamEnumerator = StreamModelPassAsync(finalOptions, cancellationToken).GetAsyncEnumerator(cancellationToken);
            while (true)
            {
                bool hasNext;
                string? moveNextError = null;
                try
                {
                    hasNext = await streamEnumerator.MoveNextAsync();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "ChatSession streaming request failed");
                    hasNext = false;
                    moveNextError = ex.Message;
                }

                if (moveNextError != null)
                {
                    yield return new ChatTurnError(moveNextError);
                    yield break;
                }

                if (!hasNext)
                {
                    break;
                }

                var update = streamEnumerator.Current;
                if (update is ChatTurnModelResponse modelResponse)
                {
                    response = modelResponse.Response;
                    continue;
                }

                yield return update;
            }

            if (response == null)
            {
                yield return new ChatTurnError("Model request produced no response.");
                yield break;
            }

            if (!response.IsSuccess)
            {
                _logger?.LogError("ChatSession streaming request failed: {ErrorMessage}", response.ErrorMessage);
                yield return new ChatTurnError(response.ErrorMessage ?? "Model request failed.");
                yield break;
            }

            if (response.Message != null)
            {
                _history.Add(response.Message);
            }

            var toolCalls = response.Message?.ToolCalls;
            if (toolCalls == null || toolCalls.Count == 0 || _toolRegistry == null)
            {
                _logger?.LogInformation("ChatSession streaming completed without tool calls");
                yield return new ChatTurnCompleted(response, lastToolResults, lastToolNames);
                yield break;
            }

            _logger?.LogInformation("ChatSession streaming executing {ToolCount} tool calls", toolCalls.Count);
            var toolResults = new List<ToolResult>();
            var toolNames = new List<string>();
            foreach (var toolCall in toolCalls)
            {
                _logger?.LogInformation("Executing tool: {ToolName}", toolCall.Name);
                toolNames.Add(toolCall.Name);
                if (_toolRegistry.TryGetTool(toolCall.Name, out var tool) && tool != null)
                {
                    var context = new ToolExecutionContext
                    {
                        ToolCall = toolCall,
                        ChatHistory = _history.AsReadOnly(),
                        ServiceProvider = _serviceProvider,
                        SessionId = _sessionId,
                        ConversationId = _conversationId
                    };
                    var execution = await _toolExecutor.ExecuteAsync(tool, context, cancellationToken);
                    toolResults.Add(execution.Result);

                    if (execution.PendingApprovalRequest != null)
                    {
                        lastToolResults = toolResults;
                        lastToolNames = toolNames;
                        yield return new ChatTurnPendingApproval(response, execution.PendingApprovalRequest, lastToolResults, lastToolNames);
                        yield break;
                    }
                }
                else
                {
                    _logger?.LogWarning("Tool '{ToolName}' not found", toolCall.Name);
                    toolResults.Add(new ToolResult
                    {
                        ToolCallId = toolCall.Id,
                        Content = $"Tool '{toolCall.Name}' not found",
                        IsSuccess = false,
                        Status = ToolExecutionStatus.Failed
                    });
                }
            }

            lastToolResults = toolResults;
            lastToolNames = toolNames;

            foreach (var result in toolResults)
            {
                _history.Add(new ChatMessage
                {
                    Role = ChatRole.Tool,
                    ToolCallId = result.ToolCallId,
                    TextContent = result.Content
                });
            }

            iteration++;
        }

        yield return new ChatTurnCompleted(new ChatResponse
        {
            IsSuccess = false,
            ErrorMessage = "Maximum tool loop iterations reached"
        }, lastToolResults, lastToolNames);
    }

    /// <summary>
    /// 流式模型传递
    /// </summary>
    /// <param name="options">聊天选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天回合流更新的异步枚举</returns>
    private async IAsyncEnumerable<ChatTurnStreamUpdate> StreamModelPassAsync(
        ChatOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var request = new ChatRequest
        {
            ModelId = _modelId,
            Messages = _history.AsReadOnly(),
            Options = options
        };

        var assistantBuilder = new StringBuilder();
        string? finishReason = null;
        UsageInfo? usage = null;
        var toolCallAccumulators = new Dictionary<string, ToolCallAccumulator>(StringComparer.Ordinal);
        var toolCallOrder = new List<string>();

        await foreach (var update in _chatClient.StreamAsync(request, cancellationToken).WithCancellation(cancellationToken))
        {
            switch (update)
            {
                case TextDeltaUpdate textDelta:
                    assistantBuilder.Append(textDelta.Delta);
                    yield return new ChatTurnTextDelta(textDelta.Delta);
                    break;
                case ToolCallDeltaUpdate toolCallDelta:
                    var toolCallId = string.IsNullOrWhiteSpace(toolCallDelta.ToolCallId)
                        ? $"tool-call-{toolCallOrder.Count + 1}"
                        : toolCallDelta.ToolCallId;
                    if (!toolCallAccumulators.TryGetValue(toolCallId, out var accumulator))
                    {
                        accumulator = new ToolCallAccumulator(toolCallId);
                        toolCallAccumulators[toolCallId] = accumulator;
                        toolCallOrder.Add(toolCallId);
                    }

                    if (!string.IsNullOrWhiteSpace(toolCallDelta.NameDelta))
                    {
                        accumulator.Name.Append(toolCallDelta.NameDelta);
                    }

                    if (!string.IsNullOrWhiteSpace(toolCallDelta.ArgumentsDelta))
                    {
                        accumulator.Arguments.Append(toolCallDelta.ArgumentsDelta);
                    }
                    break;
                case UsageUpdate usageUpdate:
                    usage = usageUpdate.Usage;
                    yield return new ChatTurnUsage(usageUpdate.Usage);
                    break;
                case CompletedUpdate completedUpdate:
                    finishReason = completedUpdate.FinishReason;
                    break;
                case ErrorUpdate errorUpdate:
                    throw new InvalidOperationException(errorUpdate.ErrorMessage);
            }
        }

        var toolCalls = toolCallOrder.Count == 0
            ? null
            : toolCallOrder.Select(id => toolCallAccumulators[id].Build()).ToList();

        yield return new ChatTurnModelResponse(new ChatResponse
        {
            IsSuccess = true,
            Message = new ChatMessage
            {
                Role = ChatRole.Assistant,
                TextContent = assistantBuilder.ToString(),
                ToolCalls = toolCalls
            },
            Usage = usage,
            FinishReason = finishReason
        });
    }

    /// <summary>
    /// 工具调用累加器
    /// </summary>
    private sealed class ToolCallAccumulator(string id)
    {
        /// <summary>
        /// 工具调用ID
        /// </summary>
        public string Id { get; } = id;
        /// <summary>
        /// 工具名称
        /// </summary>
        public StringBuilder Name { get; } = new();
        /// <summary>
        /// 工具参数
        /// </summary>
        public StringBuilder Arguments { get; } = new();

        /// <summary>
        /// 构建工具调用
        /// </summary>
        /// <returns>工具调用对象</returns>
        public ToolCall Build()
            => new()
            {
                Id = Id,
                Name = Name.ToString(),
                Arguments = Arguments.ToString()
            };
    }
}
