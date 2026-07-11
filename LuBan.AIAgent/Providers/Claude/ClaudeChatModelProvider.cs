using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using LuBan.AIAgent.Abstractions;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent.Providers.Claude;

/// <summary>
/// Claude 聊天模型提供者，用于与 Claude API 进行交互
/// </summary>
public class ClaudeChatModelProvider : IChatModelProvider
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    public string ProviderName => "claude";

    /// <summary>
    /// HTTP 客户端
    /// </summary>
    private readonly HttpClient _httpClient;
    
    /// <summary>
    /// JSON 序列化选项
    /// </summary>
    private readonly JsonSerializerOptions _jsonOptions;
    
    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<ClaudeChatModelProvider>? _logger;

    /// <summary>
    /// Claude 配置选项
    /// </summary>
    private readonly ClaudeOptions _options;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="httpClient">HTTP 客户端</param>
    /// <param name="options">Claude 配置选项</param>
    /// <param name="logger">日志记录器</param>
    public ClaudeChatModelProvider(
        HttpClient httpClient,
        Microsoft.Extensions.Options.IOptions<ClaudeOptions> options,
        ILogger<ClaudeChatModelProvider>? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// 获取基础URL
    /// </summary>
    /// <returns>基础URL</returns>
    protected string GetBaseUrl()
        => _options.BaseUrl ?? "https://api.anthropic.com/v1/";

    /// <summary>
    /// 异步完成聊天请求
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天响应</returns>
    public async Task<ChatResponse> CompleteAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString();
        _logger?.LogInformation("Starting Claude request {RequestId} to model {ModelId}", requestId, request.ModelId);

        try
        {
            var providerRequest = CreateProviderRequest(request, stream: false);
            var json = JsonSerializer.Serialize(providerRequest, _jsonOptions);
            var response = await _httpClient.PostAsync("messages", new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var providerResponse = JsonSerializer.Deserialize<ClaudeMessagesResponse>(responseJson, _jsonOptions);
            _logger?.LogInformation("Claude request {RequestId} completed successfully", requestId);
            return MapFromResponse(providerResponse);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Claude request {RequestId} failed", requestId);
            return new ChatResponse { IsSuccess = false, ErrorMessage = ex.Message };
        }
    }

    /// <summary>
    /// 异步流式完成聊天请求
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>流式聊天更新</returns>
    public async IAsyncEnumerable<StreamingChatUpdate> StreamAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString();
        _logger?.LogInformation("Starting streaming Claude request {RequestId} to model {ModelId}", requestId, request.ModelId);

        HttpResponseMessage? response = null;
        Exception? initialException = null;

        try
        {
            var providerRequest = CreateProviderRequest(request, stream: true);
            var json = JsonSerializer.Serialize(providerRequest, _jsonOptions);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "messages")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Streaming Claude request {RequestId} failed during initialization", requestId);
            initialException = ex;
        }

        if (initialException != null)
        {
            yield return new ErrorUpdate(initialException.Message);
            yield break;
        }

        if (response == null)
            yield break;

        var toolCallAccumulators = new Dictionary<string, ToolCallAccumulator>();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            if (line.StartsWith("data: "))
                line = line["data: ".Length..];

            ClaudeMessagesStreamEvent? streamEvent;
            try
            {
                streamEvent = JsonSerializer.Deserialize<ClaudeMessagesStreamEvent>(line, _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger?.LogWarning(ex, "Failed to deserialize streaming line");
                continue;
            }

            if (streamEvent?.Type == "content_block_delta")
            {
                if (!string.IsNullOrEmpty(streamEvent.Delta?.Text))
                    yield return new TextDeltaUpdate(streamEvent.Delta.Text);
                else if (streamEvent.Delta?.PartialJson != null)
                {
                    if (streamEvent.Index.HasValue && toolCallAccumulators.TryGetValue(streamEvent.Index.Value.ToString(), out var accumulator))
                    {
                        accumulator.Arguments += streamEvent.Delta.PartialJson;
                        yield return new ToolCallDeltaUpdate(accumulator.Id ?? string.Empty, accumulator.Name, streamEvent.Delta.PartialJson);
                    }
                }
            }
            else if (streamEvent?.Type == "content_block_start")
            {
                if (streamEvent.ContentBlock?.Type == "tool_use")
                {
                    var index = streamEvent.Index ?? 0;
                    var accumulator = new ToolCallAccumulator
                    {
                        Id = streamEvent.ContentBlock.Id,
                        Name = streamEvent.ContentBlock.Name
                    };
                    toolCallAccumulators[index.ToString()] = accumulator;
                }
            }
            else if (streamEvent?.Type == "message_stop")
            {
                yield return new CompletedUpdate("stop");
            }
            else if (streamEvent?.Type == "message_delta" && streamEvent.Usage != null)
            {
                yield return new UsageUpdate(new UsageInfo
                {
                    PromptTokens = streamEvent.Usage.InputTokens,
                    CompletionTokens = streamEvent.Usage.OutputTokens,
                    TotalTokens = streamEvent.Usage.InputTokens + streamEvent.Usage.OutputTokens
                });
            }
        }

        _logger?.LogInformation("Streaming Claude request {RequestId} completed", requestId);
    }

    /// <summary>
    /// 创建提供者请求
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="stream">是否流式响应</param>
    /// <returns>Claude 消息请求</returns>
    private ClaudeMessagesRequest CreateProviderRequest(ChatRequest request, bool stream)
    {
        var systemMessage = request.Messages.FirstOrDefault(m => m.Role == ChatRole.System);
        var messages = request.Messages.Where(m => m.Role != ChatRole.System).Select(MapToMessage).ToList();

        var providerRequest = new ClaudeMessagesRequest
        {
            Model = request.ModelId.StartsWith("claude:") ? request.ModelId.Substring("claude:".Length) : request.ModelId,
            Stream = stream,
            Messages = messages,
            MaxTokens = request.Options?.MaxTokens ?? 1024,
            Temperature = request.Options?.Temperature,
            TopP = request.Options?.TopP,
            StopSequences = request.Options?.StopSequences
        };

        if (systemMessage != null && !string.IsNullOrEmpty(systemMessage.TextContent))
        {
            providerRequest.System = systemMessage.TextContent;
        }

        if (request.Options?.Tools != null && request.Options.Tools.Count > 0)
        {
            providerRequest.Tools = request.Options.Tools.Select(t => new ClaudeToolDefinition
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = t.ParametersSchema
            }).ToList();
        }

        return providerRequest;
    }

    /// <summary>
    /// 将聊天消息映射为 Claude 消息
    /// </summary>
    /// <param name="message">聊天消息</param>
    /// <returns>Claude 消息</returns>
    private ClaudeMessage MapToMessage(ChatMessage message)
    {
        var role = message.Role switch
        {
            ChatRole.User => "user",
            ChatRole.Assistant => "assistant",
            ChatRole.Tool => "user",
            _ => "user"
        };

        var content = new List<ClaudeContentBlock>();

        if (!string.IsNullOrEmpty(message.TextContent))
        {
            if (message.Role == ChatRole.Tool)
            {
                content.Add(new ClaudeContentBlock
                {
                    Type = "tool_result",
                    ToolUseId = message.ToolCallId,
                    Content = message.TextContent
                });
            }
            else
            {
                content.Add(new ClaudeContentBlock
                {
                    Type = "text",
                    Text = message.TextContent
                });
            }
        }
        else if (message.ContentParts != null && message.ContentParts.Count > 0)
        {
            foreach (var part in message.ContentParts)
            {
                if (part is TextPart textPart)
                {
                    content.Add(new ClaudeContentBlock
                    {
                        Type = "text",
                        Text = textPart.Text
                    });
                }
                else
                {
                    content.Add(new ClaudeContentBlock
                    {
                        Type = "text",
                        Text = MapUnsupportedContentPartToText(part)
                    });
                }
            }
        }

        if (message.ToolCalls != null && message.ToolCalls.Count > 0)
        {
            foreach (var toolCall in message.ToolCalls)
            {
                content.Add(new ClaudeContentBlock
                {
                    Type = "tool_use",
                    Id = toolCall.Id,
                    Name = toolCall.Name,
                    Input = JsonSerializer.Deserialize<Dictionary<string, object>>(toolCall.Arguments)
                });
            }
        }

        return new ClaudeMessage
        {
            Role = role,
            Content = content
        };
    }

    /// <summary>
    /// 从 Claude 响应映射为聊天响应
    /// </summary>
    /// <param name="response">Claude 消息响应</param>
    /// <returns>聊天响应</returns>
    private ChatResponse MapFromResponse(ClaudeMessagesResponse? response)
    {
        if (response == null)
        {
            return new ChatResponse { IsSuccess = false, ErrorMessage = "Invalid response from Claude" };
        }

        var outputText = string.Empty;
        IReadOnlyList<ToolCall>? toolCalls = null;
        var toolCallList = new List<ToolCall>();

        if (response.Content != null)
        {
            foreach (var block in response.Content)
            {
                if (block.Type == "text" && !string.IsNullOrEmpty(block.Text))
                {
                    outputText += block.Text;
                }
                else if (block.Type == "tool_use")
                {
                    toolCallList.Add(new ToolCall
                    {
                        Id = block.Id ?? string.Empty,
                        Name = block.Name ?? string.Empty,
                        Arguments = JsonSerializer.Serialize(block.Input)
                    });
                }
            }
        }

        if (toolCallList.Count > 0)
        {
            toolCalls = toolCallList;
        }

        return new ChatResponse
        {
            IsSuccess = true,
            Message = new ChatMessage
            {
                Role = ChatRole.Assistant,
                TextContent = outputText,
                ToolCalls = toolCalls
            },
            FinishReason = response.StopReason,
            Usage = response.Usage == null ? null : new UsageInfo
            {
                PromptTokens = response.Usage.InputTokens,
                CompletionTokens = response.Usage.OutputTokens,
                TotalTokens = response.Usage.InputTokens + response.Usage.OutputTokens
            }
        };
    }

    /// <summary>
    /// 将不支持的内容部分映射为文本
    /// </summary>
    /// <param name="part">内容部分</param>
    /// <returns>文本表示</returns>
    private static string MapUnsupportedContentPartToText(ContentPart part)
        => part switch
        {
            ImageUrlPart image => $"[image: {image.Url}]",
            BinaryPart binary => $"[binary: {binary.MediaType}, {binary.Data.Length} bytes]",
            _ => "[unsupported content part]"
        };
}

/// <summary>
/// Claude 消息请求
/// </summary>
public class ClaudeMessagesRequest
{
    /// <summary>
    /// 模型名称
    /// </summary>
    public string Model { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否流式响应
    /// </summary>
    public bool? Stream { get; set; }
    
    /// <summary>
    /// 消息列表
    /// </summary>
    public List<ClaudeMessage> Messages { get; set; } = [];
    
    /// <summary>
    /// 系统消息
    /// </summary>
    public string? System { get; set; }
    
    /// <summary>
    /// 最大令牌数
    /// </summary>
    public int MaxTokens { get; set; }
    
    /// <summary>
    /// 温度
    /// </summary>
    public double? Temperature { get; set; }
    
    /// <summary>
    /// Top P
    /// </summary>
    public double? TopP { get; set; }
    
    /// <summary>
    /// 停止序列
    /// </summary>
    public IReadOnlyList<string>? StopSequences { get; set; }
    
    /// <summary>
    /// 工具定义列表
    /// </summary>
    public List<ClaudeToolDefinition>? Tools { get; set; }
}

/// <summary>
/// Claude 消息
/// </summary>
public class ClaudeMessage
{
    /// <summary>
    /// 角色
    /// </summary>
    public string Role { get; set; } = string.Empty;
    
    /// <summary>
    /// 内容块列表
    /// </summary>
    public List<ClaudeContentBlock> Content { get; set; } = [];
}

/// <summary>
/// Claude 内容块
/// </summary>
public class ClaudeContentBlock
{
    /// <summary>
    /// 类型
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// 文本内容
    /// </summary>
    public string? Text { get; set; }
    
    /// <summary>
    /// ID
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// 输入
    /// </summary>
    public Dictionary<string, object>? Input { get; set; }
    
    /// <summary>
    /// 工具使用 ID
    /// </summary>
    public string? ToolUseId { get; set; }
    
    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }
}

/// <summary>
/// Claude 工具定义
/// </summary>
public class ClaudeToolDefinition
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 工具描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 输入模式
    /// </summary>
    public object? InputSchema { get; set; }
}

/// <summary>
/// Claude 消息响应
/// </summary>
public class ClaudeMessagesResponse
{
    /// <summary>
    /// 内容块列表
    /// </summary>
    public List<ClaudeContentBlock>? Content { get; set; }
    
    /// <summary>
    /// 停止原因
    /// </summary>
    public string? StopReason { get; set; }
    
    /// <summary>
    /// 使用情况
    /// </summary>
    public ClaudeUsage? Usage { get; set; }
}

/// <summary>
/// Claude 使用情况
/// </summary>
public class ClaudeUsage
{
    /// <summary>
    /// 输入令牌数
    /// </summary>
    public int InputTokens { get; set; }
    
    /// <summary>
    /// 输出令牌数
    /// </summary>
    public int OutputTokens { get; set; }
}

/// <summary>
/// Claude 消息流事件
/// </summary>
public class ClaudeMessagesStreamEvent
{
    /// <summary>
    /// 事件类型
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// 索引
    /// </summary>
    public int? Index { get; set; }
    
    /// <summary>
    /// 内容块
    /// </summary>
    public ClaudeContentBlock? ContentBlock { get; set; }
    
    /// <summary>
    /// 增量
    /// </summary>
    public ClaudeContentBlockDelta? Delta { get; set; }
    
    /// <summary>
    /// 使用情况
    /// </summary>
    public ClaudeUsage? Usage { get; set; }
}

/// <summary>
/// Claude 内容块增量
/// </summary>
public class ClaudeContentBlockDelta
{
    /// <summary>
    /// 文本增量
    /// </summary>
    public string? Text { get; set; }
    
    /// <summary>
    /// 部分 JSON
    /// </summary>
    public string? PartialJson { get; set; }
}

/// <summary>
/// 工具调用累加器
/// </summary>
internal class ToolCallAccumulator
{
    /// <summary>
    /// 工具调用 ID
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// 工具名称
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// 工具参数
    /// </summary>
    public string Arguments { get; set; } = string.Empty;
}
