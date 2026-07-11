using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using LuBan.AIAgent.Abstractions;
using Microsoft.Extensions.Logging;

namespace LuBan.AIAgent.Providers.Gemini;

/// <summary>
/// Gemini 聊天模型提供者，用于与 Gemini API 进行交互
/// </summary>
public class GeminiChatModelProvider : IChatModelProvider
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    public string ProviderName => "gemini";

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
    private readonly ILogger<GeminiChatModelProvider>? _logger;
    
    /// <summary>
    /// Gemini 配置选项
    /// </summary>
    private readonly GeminiOptions _options;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="httpClient">HTTP 客户端</param>
    /// <param name="options">Gemini 配置选项</param>
    /// <param name="logger">日志记录器</param>
    public GeminiChatModelProvider(
        HttpClient httpClient,
        GeminiOptions options,
        ILogger<GeminiChatModelProvider>? logger = null)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }
    
    /// <summary>
    /// 获取基础URL
    /// </summary>
    /// <returns>基础URL</returns>
    public string GetBaseUrl()
    {
        return _options.BaseUrl ?? "https://generativelanguage.googleapis.com/v1beta/";
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
        _logger?.LogInformation("Starting Gemini request {RequestId} to model {ModelId}", requestId, request.ModelId);

        try
        {
            var providerRequest = CreateProviderRequest(request);
            var json = JsonSerializer.Serialize(providerRequest, _jsonOptions);
            var modelId = request.ModelId.StartsWith("gemini:") ? request.ModelId.Substring("gemini:".Length) : request.ModelId;
            var response = await _httpClient.PostAsync($"models/{modelId}:generateContent?key={_httpClient.DefaultRequestHeaders.GetValues("x-goog-api-key").First()}", new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var providerResponse = JsonSerializer.Deserialize<GeminiGenerateContentResponse>(responseJson, _jsonOptions);
            _logger?.LogInformation("Gemini request {RequestId} completed successfully", requestId);
            return MapFromResponse(providerResponse);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Gemini request {RequestId} failed", requestId);
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
        _logger?.LogInformation("Starting streaming Gemini request {RequestId} to model {ModelId}", requestId, request.ModelId);

        HttpResponseMessage? response = null;
        Exception? initialException = null;

        try
        {
            var providerRequest = CreateProviderRequest(request);
            var json = JsonSerializer.Serialize(providerRequest, _jsonOptions);
            var modelId = request.ModelId.StartsWith("gemini:") ? request.ModelId.Substring("gemini:".Length) : request.ModelId;
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"models/{modelId}:streamGenerateContent?key={_httpClient.DefaultRequestHeaders.GetValues("x-goog-api-key").First()}&alt=sse")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Streaming Gemini request {RequestId} failed during initialization", requestId);
            initialException = ex;
        }

        if (initialException != null)
        {
            yield return new ErrorUpdate(initialException.Message);
            yield break;
        }

        if (response == null)
            yield break;

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            if (line.StartsWith("data: "))
                line = line["data: ".Length..];

            GeminiGenerateContentResponse? streamResponse;
            try
            {
                streamResponse = JsonSerializer.Deserialize<GeminiGenerateContentResponse>(line, _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger?.LogWarning(ex, "Failed to deserialize streaming line");
                continue;
            }

            if (streamResponse?.Candidates != null && streamResponse.Candidates.Count > 0)
            {
                var candidate = streamResponse.Candidates[0];
                if (candidate.Content?.Parts != null)
                {
                    foreach (var part in candidate.Content.Parts)
                    {
                        if (!string.IsNullOrEmpty(part.Text))
                            yield return new TextDeltaUpdate(part.Text);
                    }
                }

                if (!string.IsNullOrEmpty(candidate.FinishReason))
                    yield return new CompletedUpdate(candidate.FinishReason);
            }

            if (streamResponse?.UsageMetadata != null)
            {
                yield return new UsageUpdate(new UsageInfo
                {
                    PromptTokens = streamResponse.UsageMetadata.PromptTokenCount,
                    CompletionTokens = streamResponse.UsageMetadata.CandidatesTokenCount,
                    TotalTokens = streamResponse.UsageMetadata.TotalTokenCount
                });
            }
        }

        _logger?.LogInformation("Streaming Gemini request {RequestId} completed", requestId);
    }

    /// <summary>
    /// 创建提供者请求
    /// </summary>
    /// <param name="request">聊天请求</param>
    /// <returns>Gemini 生成内容请求</returns>
    private GeminiGenerateContentRequest CreateProviderRequest(ChatRequest request)
    {
        var providerRequest = new GeminiGenerateContentRequest
        {
            Contents = request.Messages.Select(MapToContent).ToList(),
            GenerationConfig = new GeminiGenerationConfig
            {
                Temperature = request.Options?.Temperature,
                TopP = request.Options?.TopP,
                MaxOutputTokens = request.Options?.MaxTokens,
                StopSequences = request.Options?.StopSequences
            }
        };

        if (request.Options?.Tools != null && request.Options.Tools.Count > 0)
        {
            providerRequest.Tools = new List<GeminiTool>
            {
                new GeminiTool
                {
                    FunctionDeclarations = request.Options.Tools.Select(t => new GeminiFunctionDeclaration
                    {
                        Name = t.Name,
                        Description = t.Description,
                        Parameters = t.ParametersSchema
                    }).ToList()
                }
            };
        }

        return providerRequest;
    }

    /// <summary>
    /// 将聊天消息映射为 Gemini 内容
    /// </summary>
    /// <param name="message">聊天消息</param>
    /// <returns>Gemini 内容</returns>
    private GeminiContent MapToContent(ChatMessage message)
    {
        var role = message.Role switch
        {
            ChatRole.User => "user",
            ChatRole.Assistant => "model",
            _ => "user"
        };

        var parts = new List<GeminiPart>();
        if (!string.IsNullOrEmpty(message.TextContent))
        {
            parts.Add(new GeminiPart { Text = message.TextContent });
        }
        else if (message.ContentParts != null && message.ContentParts.Count > 0)
        {
            foreach (var part in message.ContentParts)
            {
                switch (part)
                {
                    case TextPart textPart:
                        parts.Add(new GeminiPart { Text = textPart.Text });
                        break;
                    case ImageUrlPart imagePart:
                        parts.Add(new GeminiPart { Text = $"[image: {imagePart.Url}]" });
                        break;
                    case BinaryPart binaryPart:
                        parts.Add(new GeminiPart
                        {
                            InlineData = new GeminiInlineData
                            {
                                MimeType = binaryPart.MediaType,
                                Data = Convert.ToBase64String(binaryPart.Data)
                            }
                        });
                        break;
                }
            }
        }

        if (message.ToolCalls != null && message.ToolCalls.Count > 0)
        {
            foreach (var toolCall in message.ToolCalls)
            {
                parts.Add(new GeminiPart
                {
                    FunctionCall = new GeminiFunctionCall
                    {
                        Name = toolCall.Name,
                        Args = JsonSerializer.Deserialize<Dictionary<string, object>>(toolCall.Arguments)
                    }
                });
            }
        }

        return new GeminiContent
        {
            Role = role,
            Parts = parts
        };
    }

    /// <summary>
    /// 从 Gemini 响应映射为聊天响应
    /// </summary>
    /// <param name="response">Gemini 生成内容响应</param>
    /// <returns>聊天响应</returns>
    private ChatResponse MapFromResponse(GeminiGenerateContentResponse? response)
    {
        if (response?.Candidates == null || response.Candidates.Count == 0)
        {
            return new ChatResponse { IsSuccess = false, ErrorMessage = "Invalid response from Gemini" };
        }

        var candidate = response.Candidates[0];
        var outputText = string.Empty;
        IReadOnlyList<ToolCall>? toolCalls = null;
        var toolCallList = new List<ToolCall>();

        if (candidate.Content?.Parts != null)
        {
            foreach (var part in candidate.Content.Parts)
            {
                if (!string.IsNullOrEmpty(part.Text))
                {
                    outputText += part.Text;
                }
                else if (part.FunctionCall != null)
                {
                    toolCallList.Add(new ToolCall
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = part.FunctionCall.Name ?? string.Empty,
                        Arguments = JsonSerializer.Serialize(part.FunctionCall.Args)
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
            FinishReason = candidate.FinishReason,
            Usage = response.UsageMetadata == null ? null : new UsageInfo
            {
                PromptTokens = response.UsageMetadata.PromptTokenCount,
                CompletionTokens = response.UsageMetadata.CandidatesTokenCount,
                TotalTokens = response.UsageMetadata.TotalTokenCount
            }
        };
    }
}

/// <summary>
/// Gemini 生成内容请求
/// </summary>
public class GeminiGenerateContentRequest
{
    /// <summary>
    /// 内容列表
    /// </summary>
    public List<GeminiContent> Contents { get; set; } = [];
    
    /// <summary>
    /// 生成配置
    /// </summary>
    public GeminiGenerationConfig? GenerationConfig { get; set; }
    
    /// <summary>
    /// 工具列表
    /// </summary>
    public List<GeminiTool>? Tools { get; set; }
}

/// <summary>
/// Gemini 内容
/// </summary>
public class GeminiContent
{
    /// <summary>
    /// 角色
    /// </summary>
    public string Role { get; set; } = string.Empty;
    
    /// <summary>
    /// 部分列表
    /// </summary>
    public List<GeminiPart> Parts { get; set; } = [];
}

/// <summary>
/// Gemini 部分
/// </summary>
public class GeminiPart
{
    /// <summary>
    /// 文本
    /// </summary>
    public string? Text { get; set; }
    
    /// <summary>
    /// 函数调用
    /// </summary>
    public GeminiFunctionCall? FunctionCall { get; set; }
    
    /// <summary>
    /// 内联数据
    /// </summary>
    public GeminiInlineData? InlineData { get; set; }
}

/// <summary>
/// Gemini 内联数据
/// </summary>
public class GeminiInlineData
{
    /// <summary>
    /// MIME 类型
    /// </summary>
    public string MimeType { get; set; } = string.Empty;
    
    /// <summary>
    /// 数据
    /// </summary>
    public string Data { get; set; } = string.Empty;
}

/// <summary>
/// Gemini 函数调用
/// </summary>
public class GeminiFunctionCall
{
    /// <summary>
    /// 函数名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 函数参数
    /// </summary>
    public Dictionary<string, object>? Args { get; set; }
}

/// <summary>
/// Gemini 生成配置
/// </summary>
public class GeminiGenerationConfig
{
    /// <summary>
    /// 温度
    /// </summary>
    public double? Temperature { get; set; }
    
    /// <summary>
    /// Top P
    /// </summary>
    public double? TopP { get; set; }
    
    /// <summary>
    /// 最大输出令牌数
    /// </summary>
    public int? MaxOutputTokens { get; set; }
    
    /// <summary>
    /// 停止序列
    /// </summary>
    public IReadOnlyList<string>? StopSequences { get; set; }
}

/// <summary>
/// Gemini 工具
/// </summary>
public class GeminiTool
{
    /// <summary>
    /// 函数声明列表
    /// </summary>
    public List<GeminiFunctionDeclaration>? FunctionDeclarations { get; set; }
}

/// <summary>
/// Gemini 函数声明
/// </summary>
public class GeminiFunctionDeclaration
{
    /// <summary>
    /// 函数名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 函数描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 函数参数
    /// </summary>
    public object? Parameters { get; set; }
}

/// <summary>
/// Gemini 生成内容响应
/// </summary>
public class GeminiGenerateContentResponse
{
    /// <summary>
    /// 候选列表
    /// </summary>
    public List<GeminiCandidate>? Candidates { get; set; }
    
    /// <summary>
    /// 使用元数据
    /// </summary>
    public GeminiUsageMetadata? UsageMetadata { get; set; }
}

/// <summary>
/// Gemini 候选
/// </summary>
public class GeminiCandidate
{
    /// <summary>
    /// 内容
    /// </summary>
    public GeminiContent? Content { get; set; }
    
    /// <summary>
    /// 完成原因
    /// </summary>
    public string? FinishReason { get; set; }
}

/// <summary>
/// Gemini 使用元数据
/// </summary>
public class GeminiUsageMetadata
{
    /// <summary>
    /// 提示令牌数
    /// </summary>
    public int PromptTokenCount { get; set; }
    
    /// <summary>
    /// 候选令牌数
    /// </summary>
    public int CandidatesTokenCount { get; set; }
    
    /// <summary>
    /// 总令牌数
    /// </summary>
    public int TotalTokenCount { get; set; }
}
