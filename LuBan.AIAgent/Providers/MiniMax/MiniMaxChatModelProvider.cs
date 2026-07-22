using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using LuBan.AIAgent.Abstractions;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent.Providers.MiniMax;

/// <summary>
/// MiniMax 聊天模型提供者，用于与 MiniMax API 进行交互
/// </summary>
public class MiniMaxChatModelProvider : IChatModelProvider
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    public string ProviderName => "minimax";

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
    private readonly ILogger<MiniMaxChatModelProvider>? _logger;

    /// <summary>
    /// MiniMax 配置选项
    /// </summary>
    private readonly MiniMaxOptions _options;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="httpClient">HTTP 客户端</param>
    /// <param name="options">MiniMax 配置选项</param>
    /// <param name="logger">日志记录器</param>
    public MiniMaxChatModelProvider(
        HttpClient httpClient,
        Microsoft.Extensions.Options.IOptions<MiniMaxOptions> options,
        ILogger<MiniMaxChatModelProvider>? logger = null)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
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
    public string GetBaseUrl()
    {
        return _options.BaseUrl ?? "https://api.minimax.chat/v/";
    }

    /// <summary>
    /// 异步完成聊天请求
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天响应</returns>
    public async Task<ChatResponse> CompleteAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString();
        _logger?.LogInformation("Starting MiniMax request {RequestId} to model {ModelId}", requestId, request.ModelId);

        try
        {
            var providerRequest = CreateProviderRequest(request, stream: false);
            var json = JsonSerializer.Serialize(providerRequest, _jsonOptions);

            var url = BuildRequestUrl();
            var response = await _httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var providerResponse = JsonSerializer.Deserialize<MiniMaxChatCompletionResponse>(responseJson, _jsonOptions);
            _logger?.LogInformation("MiniMax request {RequestId} completed successfully", requestId);
            return MapFromResponse(providerResponse);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "MiniMax request {RequestId} failed", requestId);
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
        _logger?.LogInformation("Starting streaming MiniMax request {RequestId} to model {ModelId}", requestId, request.ModelId);

        HttpResponseMessage? response = null;
        Exception? initialException = null;

        try
        {
            var providerRequest = CreateProviderRequest(request, stream: true);
            var json = JsonSerializer.Serialize(providerRequest, _jsonOptions);
            var url = BuildRequestUrl();

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Streaming MiniMax request {RequestId} failed during initialization", requestId);
            initialException = ex;
        }

        if (initialException != null)
        {
            yield return new ErrorUpdate(initialException.Message);
            yield break;
        }

        if (response == null)
            yield break;

        var toolCallAccumulators = new Dictionary<int, ToolCallAccumulator>();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            if (line.StartsWith("data: "))
                line = line["data: ".Length..];

            MiniMaxStreamEvent? streamEvent;
            try
            {
                streamEvent = JsonSerializer.Deserialize<MiniMaxStreamEvent>(line, _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger?.LogWarning(ex, "Failed to deserialize streaming line");
                continue;
            }

            if (streamEvent?.Choices == null || streamEvent.Choices.Count == 0)
                continue;

            var choice = streamEvent.Choices[0];
            var delta = choice.Delta;

            if (!string.IsNullOrEmpty(delta?.Content))
                yield return new TextDeltaUpdate(delta.Content);

            if (delta?.ToolCalls != null && delta.ToolCalls.Count > 0)
            {
                foreach (var toolCall in delta.ToolCalls)
                {
                    var index = toolCall.Index ?? 0;
                    if (!toolCallAccumulators.TryGetValue(index, out var accumulator))
                    {
                        accumulator = new ToolCallAccumulator();
                        toolCallAccumulators[index] = accumulator;
                    }

                    if (!string.IsNullOrEmpty(toolCall.Id))
                        accumulator.Id = toolCall.Id;
                    if (!string.IsNullOrEmpty(toolCall.Function?.Name))
                        accumulator.Name = toolCall.Function.Name;
                    if (!string.IsNullOrEmpty(toolCall.Function?.Arguments))
                        accumulator.Arguments += toolCall.Function.Arguments;

                    yield return new ToolCallDeltaUpdate(accumulator.Id ?? string.Empty, toolCall.Function?.Name, toolCall.Function?.Arguments);
                }
            }

            if (!string.IsNullOrEmpty(choice.FinishReason))
                yield return new CompletedUpdate(choice.FinishReason);

            if (streamEvent.Usage != null)
            {
                yield return new UsageUpdate(new UsageInfo
                {
                    PromptTokens = streamEvent.Usage.PromptTokens,
                    CompletionTokens = streamEvent.Usage.CompletionTokens,
                    TotalTokens = streamEvent.Usage.PromptTokens + streamEvent.Usage.CompletionTokens
                });
            }
        }

        _logger?.LogInformation("Streaming MiniMax request {RequestId} completed", requestId);
    }

    /// <summary>
    /// 构建请求 URL
    /// </summary>
    /// <returns>请求 URL</returns>
    private string BuildRequestUrl()
    {
        if (!string.IsNullOrEmpty(_options.GroupId))
        {
            return $"text/chatcompletion_v2?GroupId={_options.GroupId}";
        }
        return "text/chatcompletion_v2";
    }

    /// <summary>
    /// 创建提供者请求
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="stream">是否流式响应</param>
    /// <returns>MiniMax 聊天完成请求</returns>
    private MiniMaxChatCompletionRequest CreateProviderRequest(ChatRequest request, bool stream)
    {
        var messages = request.Messages.Where(m => m.Role != ChatRole.System).Select(MapToMessage).ToList();

        var providerRequest = new MiniMaxChatCompletionRequest
        {
            Model = request.ModelId.StartsWith("minimax:") ? request.ModelId.Substring("minimax:".Length) : request.ModelId,
            Stream = stream,
            Messages = messages,
            MaxTokens = request.Options?.MaxTokens ?? 1024,
            Temperature = request.Options?.Temperature,
            TopP = request.Options?.TopP,
            Stop = request.Options?.StopSequences
        };

        var systemMessage = request.Messages.FirstOrDefault(m => m.Role == ChatRole.System);
        if (systemMessage != null && !string.IsNullOrEmpty(systemMessage.TextContent))
        {
            providerRequest.SystemInstruction = new MiniMaxMessage
            {
                Role = "system",
                Content = systemMessage.TextContent
            };
        }

        if (request.Options?.Tools != null && request.Options.Tools.Count > 0)
        {
            providerRequest.Tools = request.Options.Tools.Select(t => new MiniMaxToolDefinition
            {
                Type = "function",
                Function = new MiniMaxFunctionDefinition
                {
                    Name = t.Name,
                    Description = t.Description,
                    Parameters = t.ParametersSchema
                }
            }).ToList();
        }

        return providerRequest;
    }

    /// <summary>
    /// 将聊天消息映射为 MiniMax 消息
    /// </summary>
    /// <param name="message">聊天消息</param>
    /// <returns>MiniMax 消息</returns>
    private MiniMaxMessage MapToMessage(ChatMessage message)
    {
        var role = message.Role switch
        {
            ChatRole.System => "system",
            ChatRole.User => "user",
            ChatRole.Assistant => "assistant",
            ChatRole.Tool => "tool",
            _ => "user"
        };

        var content = BuildTextContent(message);

        var miniMaxMessage = new MiniMaxMessage
        {
            Role = role,
            Content = content,
            ToolCallId = message.ToolCallId
        };

        if (message.ToolCalls != null && message.ToolCalls.Count > 0)
        {
            miniMaxMessage.ToolCalls = message.ToolCalls.Select(tc => new MiniMaxToolCall
            {
                Id = tc.Id,
                Type = "function",
                Function = new MiniMaxFunctionCall
                {
                    Name = tc.Name,
                    Arguments = tc.Arguments
                }
            }).ToList();
        }

        return miniMaxMessage;
    }

    /// <summary>
    /// 构建文本内容
    /// </summary>
    /// <param name="message">聊天消息</param>
    /// <returns>文本内容</returns>
    private static string? BuildTextContent(ChatMessage message)
    {
        if (!string.IsNullOrEmpty(message.TextContent))
        {
            return message.TextContent;
        }

        if (message.ContentParts == null || message.ContentParts.Count == 0)
        {
            return null;
        }

        return string.Join("\n", message.ContentParts.Select(MapContentPartToText));
    }

    /// <summary>
    /// 将内容部分映射为文本
    /// </summary>
    /// <param name="part">内容部分</param>
    /// <returns>文本表示</returns>
    private static string MapContentPartToText(ContentPart part)
        => part switch
        {
            TextPart text => text.Text,
            ImageUrlPart image => $"[image: {image.Url}]",
            BinaryPart binary => $"[binary: {binary.MediaType}, {binary.Data.Length} bytes]",
            _ => "[unsupported content part]"
        };

    /// <summary>
    /// 从 MiniMax 响应映射为聊天响应
    /// </summary>
    /// <param name="response">MiniMax 聊天完成响应</param>
    /// <returns>聊天响应</returns>
    private ChatResponse MapFromResponse(MiniMaxChatCompletionResponse? response)
    {
        if (response == null || response.Choices == null || response.Choices.Count == 0)
        {
            return new ChatResponse { IsSuccess = false, ErrorMessage = "MiniMax返回了无效响应" };
        }

        var choice = response.Choices[0];
        var message = choice.Message;
        IReadOnlyList<ToolCall>? toolCalls = null;

        if (message?.ToolCalls != null && message.ToolCalls.Count > 0)
        {
            toolCalls = message.ToolCalls.Select(tc => new ToolCall
            {
                Id = tc.Id,
                Name = tc.Function?.Name ?? string.Empty,
                Arguments = tc.Function?.Arguments ?? string.Empty
            }).ToList();
        }

        return new ChatResponse
        {
            IsSuccess = true,
            Message = new ChatMessage
            {
                Role = ChatRole.Assistant,
                TextContent = message?.Content,
                ToolCalls = toolCalls
            },
            FinishReason = choice.FinishReason,
            Usage = response.Usage == null ? null : new UsageInfo
            {
                PromptTokens = response.Usage.PromptTokens,
                CompletionTokens = response.Usage.CompletionTokens,
                TotalTokens = response.Usage.PromptTokens + response.Usage.CompletionTokens
            }
        };
    }

    /// <summary>
    /// 异步获取该提供者支持的模型列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>模型信息列表</returns>
    /// <exception cref="NotSupportedException">MiniMax 不支持获取模型列表</exception>
    public Task<IReadOnlyList<ModelInfo>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("MiniMax 不支持获取模型列表");
    }
}

/// <summary>
/// MiniMax 聊天完成请求
/// </summary>
public class MiniMaxChatCompletionRequest
{
    /// <summary>
    /// 模型名称
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// 消息列表
    /// </summary>
    public List<MiniMaxMessage> Messages { get; set; } = [];

    /// <summary>
    /// 系统指令
    /// </summary>
    public MiniMaxMessage? SystemInstruction { get; set; }

    /// <summary>
    /// 是否流式响应
    /// </summary>
    public bool? Stream { get; set; }

    /// <summary>
    /// 最大令牌数
    /// </summary>
    public int? MaxTokens { get; set; }

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
    public IReadOnlyList<string>? Stop { get; set; }

    /// <summary>
    /// 工具定义列表
    /// </summary>
    public List<MiniMaxToolDefinition>? Tools { get; set; }
}

/// <summary>
/// MiniMax 消息
/// </summary>
public class MiniMaxMessage
{
    /// <summary>
    /// 角色
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 工具调用ID
    /// </summary>
    public string? ToolCallId { get; set; }

    /// <summary>
    /// 工具调用列表
    /// </summary>
    public List<MiniMaxToolCall>? ToolCalls { get; set; }
}

/// <summary>
/// MiniMax 工具定义
/// </summary>
public class MiniMaxToolDefinition
{
    /// <summary>
    /// 类型
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 函数定义
    /// </summary>
    public MiniMaxFunctionDefinition Function { get; set; } = null!;
}

/// <summary>
/// MiniMax 函数定义
/// </summary>
public class MiniMaxFunctionDefinition
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 参数模式
    /// </summary>
    public object? Parameters { get; set; }
}

/// <summary>
/// MiniMax 工具调用
/// </summary>
public class MiniMaxToolCall
{
    /// <summary>
    /// ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 类型
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 函数调用
    /// </summary>
    public MiniMaxFunctionCall Function { get; set; } = null!;

    /// <summary>
    /// 索引
    /// </summary>
    public int? Index { get; set; }
}

/// <summary>
/// MiniMax 函数调用
/// </summary>
public class MiniMaxFunctionCall
{
    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 参数
    /// </summary>
    public string? Arguments { get; set; }
}

/// <summary>
/// MiniMax 聊天完成响应
/// </summary>
public class MiniMaxChatCompletionResponse
{
    /// <summary>
    /// 选择列表
    /// </summary>
    public List<MiniMaxChoice>? Choices { get; set; }

    /// <summary>
    /// 使用信息
    /// </summary>
    public MiniMaxUsage? Usage { get; set; }
}

/// <summary>
/// MiniMax 选择
/// </summary>
public class MiniMaxChoice
{
    /// <summary>
    /// 消息
    /// </summary>
    public MiniMaxMessage? Message { get; set; }

    /// <summary>
    /// 完成原因
    /// </summary>
    public string? FinishReason { get; set; }
}

/// <summary>
/// MiniMax 使用信息
/// </summary>
public class MiniMaxUsage
{
    /// <summary>
    /// 提示令牌数
    /// </summary>
    public int PromptTokens { get; set; }

    /// <summary>
    /// 完成令牌数
    /// </summary>
    public int CompletionTokens { get; set; }

    /// <summary>
    /// 总令牌数
    /// </summary>
    public int TotalTokens { get; set; }
}

/// <summary>
/// MiniMax 流式事件
/// </summary>
public class MiniMaxStreamEvent
{
    /// <summary>
    /// 选择列表
    /// </summary>
    public List<MiniMaxStreamChoice>? Choices { get; set; }

    /// <summary>
    /// 使用信息
    /// </summary>
    public MiniMaxUsage? Usage { get; set; }
}

/// <summary>
/// MiniMax 流式选择
/// </summary>
public class MiniMaxStreamChoice
{
    /// <summary>
    /// 增量
    /// </summary>
    public MiniMaxDelta? Delta { get; set; }

    /// <summary>
    /// 完成原因
    /// </summary>
    public string? FinishReason { get; set; }
}

/// <summary>
/// MiniMax 增量
/// </summary>
public class MiniMaxDelta
{
    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 工具调用列表
    /// </summary>
    public List<MiniMaxToolCall>? ToolCalls { get; set; }
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
