using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using LuBan.AIAgent.Abstractions;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent.Core;

/// <summary>
/// OpenAI兼容的聊天模型提供者基类
/// </summary>
public abstract class OpenAICompatibleProviderBase : IChatModelProvider
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    public abstract string ProviderName { get; }

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _requestJsonOptions;
    private readonly JsonSerializerOptions _responseJsonOptions;
    private readonly ILogger? _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="httpClient">HTTP客户端</param>
    /// <param name="logger">日志记录器</param>
    protected OpenAICompatibleProviderBase(HttpClient httpClient, ILogger? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        // 请求使用蛇形命名（SnakeCaseLower）
        _requestJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        // 响应使用驼峰命名（与Kimi API返回的JSON匹配）
        _responseJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// 获取基础URL
    /// </summary>
    /// <returns>基础URL</returns>
    protected virtual string GetBaseUrl()
        => "https://api.openai.com/v1/";

    /// <summary>
    /// 异步完成聊天请求
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>聊天响应</returns>
    public async Task<ChatResponse> CompleteAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString();
        _logger?.LogInformation("Starting request {RequestId} to model {ModelId} via {ProviderName}", requestId, request.ModelId, ProviderName);

        try
        {
            var providerRequest = CreateProviderRequest(request, stream: false);
            var json = JsonSerializer.Serialize(providerRequest, _requestJsonOptions);
            
            // 构建绝对URI
            var baseUrl = GetBaseUrl();
            var relativeUrl = BuildRelativeUrl(request.ModelId);
            var absoluteUri = new Uri(new Uri(baseUrl), relativeUrl);
            
            // 设置请求头
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, absoluteUri)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            
            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                // 尝试读取错误响应
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger?.LogError("Request {RequestId} failed with status code {StatusCode}: {ErrorContent}", requestId, response.StatusCode, errorContent);
                return new ChatResponse { IsSuccess = false, ErrorMessage = $"{response.StatusCode}: {errorContent}" };
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var providerResponse = JsonSerializer.Deserialize<OpenAICompatibleChatCompletionResponse>(responseJson, _responseJsonOptions);
            _logger?.LogInformation("Request {RequestId} completed successfully", requestId);
            return MapFromResponse(providerResponse, GetInvalidResponseMessage());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Request {RequestId} failed", requestId);
            return new ChatResponse { IsSuccess = false, ErrorMessage = ex.Message };
        }
    }

    /// <summary>
    /// 异步流式聊天请求
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>流式聊天更新的异步枚举</returns>
    public async IAsyncEnumerable<StreamingChatUpdate> StreamAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString();
        _logger?.LogInformation("Starting streaming request {RequestId} to model {ModelId} via {ProviderName}", requestId, request.ModelId, ProviderName);

        HttpResponseMessage? response = null;
        Exception? initialException = null;

        try
        {
            var providerRequest = CreateProviderRequest(request, stream: true);
            var json = JsonSerializer.Serialize(providerRequest, _requestJsonOptions);
            
            // 构建绝对URI
            var baseUrl = GetBaseUrl();
            var relativeUrl = BuildRelativeUrl(request.ModelId);
            var absoluteUri = new Uri(new Uri(baseUrl), relativeUrl);
            
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, absoluteUri)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Streaming request {RequestId} failed during initialization", requestId);
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
            if (line == "[DONE]")
                break;

            OpenAICompatibleChatCompletionStreamResponse? streamingResponse;
            try
            {
                streamingResponse = JsonSerializer.Deserialize<OpenAICompatibleChatCompletionStreamResponse>(line, _responseJsonOptions);
            }
            catch (JsonException ex)
            {
                _logger?.LogWarning(ex, "Failed to deserialize streaming line");
                continue;
            }

            if (streamingResponse?.Choices != null && streamingResponse.Choices.Count > 0)
            {
                var choice = streamingResponse.Choices[0];
                var delta = choice.Delta;

                if (!string.IsNullOrEmpty(delta?.Content))
                    yield return new TextDeltaUpdate(delta.Content);

                if (delta?.ToolCalls != null && delta.ToolCalls.Count > 0)
                {
                    foreach (var toolCall in delta.ToolCalls)
                    {
                        if (!toolCall.Index.HasValue)
                            continue;

                        var index = toolCall.Index.Value;
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
            }

            if (streamingResponse?.Usage != null)
            {
                yield return new UsageUpdate(new UsageInfo
                {
                    PromptTokens = streamingResponse.Usage.PromptTokens,
                    CompletionTokens = streamingResponse.Usage.CompletionTokens,
                    TotalTokens = streamingResponse.Usage.TotalTokens
                });
            }
        }

        _logger?.LogInformation("Streaming request {RequestId} completed", requestId);
    }

    /// <summary>
    /// 创建基础请求
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="stream">是否流式</param>
    /// <param name="includeModel">是否包含模型</param>
    /// <returns>OpenAI兼容的聊天完成请求</returns>
    protected OpenAICompatibleChatCompletionRequest CreateBaseRequest(ChatRequest request, bool stream, bool includeModel)
    {
        var providerRequest = new OpenAICompatibleChatCompletionRequest
        {
            Stream = stream,
            Model = includeModel ? request.ModelId : null,
            Messages = request.Messages.Select(MapToMessage).ToList(),
            Temperature = request.Options?.Temperature,
            TopP = request.Options?.TopP,
            MaxTokens = request.Options?.MaxTokens,
            Stop = request.Options?.StopSequences
        };

        if (request.Options?.Tools != null && request.Options.Tools.Count > 0)
        {
            providerRequest.Tools = request.Options.Tools.Select(t => new OpenAICompatibleToolDefinition
            {
                Type = "function",
                Function = new OpenAICompatibleFunctionDefinition
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
    /// 将聊天消息映射为OpenAI兼容消息
    /// </summary>
    /// <param name="message">聊天消息</param>
    /// <returns>OpenAI兼容消息</returns>
    protected virtual OpenAICompatibleMessage MapToMessage(ChatMessage message)
    {
        var role = message.Role switch
        {
            ChatRole.System => "system",
            ChatRole.User => "user",
            ChatRole.Assistant => "assistant",
            ChatRole.Tool => "tool",
            _ => "user"
        };

        var providerMessage = new OpenAICompatibleMessage
        {
            Role = role,
            Content = BuildTextContent(message),
            ToolCallId = message.ToolCallId
        };

        if (message.ToolCalls != null && message.ToolCalls.Count > 0)
        {
            providerMessage.ToolCalls = message.ToolCalls.Select(tc => new OpenAICompatibleToolCall
            {
                Id = tc.Id,
                Type = "function",
                Function = new OpenAICompatibleFunctionCall
                {
                    Name = tc.Name,
                    Arguments = tc.Arguments
                }
            }).ToList();
        }

        return providerMessage;
    }

    /// <summary>
    /// 构建文本内容
    /// </summary>
    /// <param name="message">聊天消息</param>
    /// <returns>文本内容</returns>
    protected static string? BuildTextContent(ChatMessage message)
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
    protected static string MapContentPartToText(ContentPart part)
        => part switch
        {
            TextPart text => text.Text,
            ImageUrlPart image => $"[image: {image.Url}]",
            BinaryPart binary => $"[binary: {binary.MediaType}, {binary.Data.Length} bytes]",
            _ => "[unsupported content part]"
        };

    /// <summary>
    /// 从响应映射为聊天响应
    /// </summary>
    /// <param name="response">OpenAI兼容响应</param>
    /// <param name="invalidResponseMessage">无效响应消息</param>
    /// <returns>聊天响应</returns>
    protected virtual ChatResponse MapFromResponse(OpenAICompatibleChatCompletionResponse? response, string invalidResponseMessage)
    {
        if (response?.Choices == null || response.Choices.Count == 0)
        {
            return new ChatResponse { IsSuccess = false, ErrorMessage = invalidResponseMessage };
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
                TotalTokens = response.Usage.TotalTokens
            }
        };
    }

    /// <summary>
    /// 创建提供者请求
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <param name="stream">是否流式</param>
    /// <returns>提供者请求对象</returns>
    protected abstract object CreateProviderRequest(ChatRequest request, bool stream);

    /// <summary>
    /// 构建相对URL
    /// </summary>
    /// <param name="modelOrDeployment">模型或部署</param>
    /// <returns>相对URL</returns>
    protected abstract string BuildRelativeUrl(string modelOrDeployment);

    /// <summary>
    /// 获取无效响应消息
    /// </summary>
    /// <returns>无效响应消息</returns>
    protected abstract string GetInvalidResponseMessage();
}

/// <summary>
/// OpenAI兼容的聊天完成请求
/// </summary>
public class OpenAICompatibleChatCompletionRequest
{
    /// <summary>
    /// 模型
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// 消息列表
    /// </summary>
    public List<OpenAICompatibleMessage> Messages { get; set; } = [];

    /// <summary>
    /// 温度
    /// </summary>
    public double? Temperature { get; set; }

    /// <summary>
    /// Top P
    /// </summary>
    public double? TopP { get; set; }

    /// <summary>
    /// 最大令牌数
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// 停止序列
    /// </summary>
    public IReadOnlyList<string>? Stop { get; set; }

    /// <summary>
    /// 工具定义列表
    /// </summary>
    public List<OpenAICompatibleToolDefinition>? Tools { get; set; }

    /// <summary>
    /// 是否流式
    /// </summary>
    public bool? Stream { get; set; }

    /// <summary>
    /// 思考配置（Kimi特有）
    /// </summary>
    public ThinkingConfig? Thinking { get; set; }
}

/// <summary>
/// 思考配置
/// </summary>
public class ThinkingConfig
{
    /// <summary>
    /// 思考类型
    /// </summary>
    public string Type { get; set; } = "enabled";
}

/// <summary>
/// OpenAI兼容的消息
/// </summary>
public class OpenAICompatibleMessage
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
    public List<OpenAICompatibleToolCall>? ToolCalls { get; set; }

    /// <summary>
    /// 推理内容（Kimi特有）
    /// </summary>
    public string? ReasoningContent { get; set; }
}

/// <summary>
/// OpenAI兼容的工具定义
/// </summary>
public class OpenAICompatibleToolDefinition
{
    /// <summary>
    /// 类型
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 函数定义
    /// </summary>
    public OpenAICompatibleFunctionDefinition Function { get; set; } = null!;
}

/// <summary>
/// OpenAI兼容的函数定义
/// </summary>
public class OpenAICompatibleFunctionDefinition
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
/// OpenAI兼容的工具调用
/// </summary>
public class OpenAICompatibleToolCall
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
    public OpenAICompatibleFunctionCall Function { get; set; } = null!;

    /// <summary>
    /// 索引
    /// </summary>
    public int? Index { get; set; }
}

/// <summary>
/// OpenAI兼容的函数调用
/// </summary>
public class OpenAICompatibleFunctionCall
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
/// OpenAI兼容的聊天完成响应
/// </summary>
public class OpenAICompatibleChatCompletionResponse
{
    /// <summary>
    /// 选择列表
    /// </summary>
    public List<OpenAICompatibleChoice>? Choices { get; set; }

    /// <summary>
    /// 使用信息
    /// </summary>
    public OpenAICompatibleUsage? Usage { get; set; }
}

/// <summary>
/// OpenAI兼容的选择
/// </summary>
public class OpenAICompatibleChoice
{
    /// <summary>
    /// 消息
    /// </summary>
    public OpenAICompatibleMessage? Message { get; set; }

    /// <summary>
    /// 完成原因
    /// </summary>
    public string? FinishReason { get; set; }
}

/// <summary>
/// OpenAI兼容的使用信息
/// </summary>
public class OpenAICompatibleUsage
{
    /// <summary>
    /// 提示令牌数
    /// </summary>
    public int? PromptTokens { get; set; }

    /// <summary>
    /// 完成令牌数
    /// </summary>
    public int? CompletionTokens { get; set; }

    /// <summary>
    /// 总令牌数
    /// </summary>
    public int? TotalTokens { get; set; }
}

/// <summary>
/// OpenAI兼容的聊天完成流式响应
/// </summary>
public class OpenAICompatibleChatCompletionStreamResponse
{
    /// <summary>
    /// 选择列表
    /// </summary>
    public List<OpenAICompatibleStreamChoice>? Choices { get; set; }

    /// <summary>
    /// 使用信息
    /// </summary>
    public OpenAICompatibleUsage? Usage { get; set; }
}

/// <summary>
/// OpenAI兼容的流式选择
/// </summary>
public class OpenAICompatibleStreamChoice
{
    /// <summary>
    /// 增量
    /// </summary>
    public OpenAICompatibleDelta? Delta { get; set; }

    /// <summary>
    /// 完成原因
    /// </summary>
    public string? FinishReason { get; set; }
}

/// <summary>
/// OpenAI兼容的增量
/// </summary>
public class OpenAICompatibleDelta
{
    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 工具调用列表
    /// </summary>
    public List<OpenAICompatibleToolCall>? ToolCalls { get; set; }
}

/// <summary>
/// 工具调用累加器
/// </summary>
internal class ToolCallAccumulator
{
    /// <summary>
    /// ID
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 参数
    /// </summary>
    public string Arguments { get; set; } = string.Empty;
}
