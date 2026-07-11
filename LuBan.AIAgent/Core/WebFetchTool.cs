using System.Text.Json;
using LuBan.AIAgent.Abstractions;

namespace LuBan.AIAgent.Core;

/// <summary>
/// Web获取工具，用于从URL获取网页内容
/// </summary>
/// <param name="httpClient">HTTP客户端</param>
public sealed class WebFetchTool(HttpClient httpClient) : ITool
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name => "web_fetch";

    /// <summary>
    /// 工具描述
    /// </summary>
    public string Description => "Fetch webpage content from a URL using HTTP GET.";

    /// <summary>
    /// 参数模式
    /// </summary>
    public object ParametersSchema => new
    {
        type = "object",
        properties = new
        {
            url = new { type = "string", description = "Absolute HTTP or HTTPS URL to fetch." },
            maxCharacters = new { type = "integer", description = "Optional maximum response characters to return.", minimum = 256, maximum = 50000 }
        },
        required = new[] { "url" }
    };

    /// <summary>
    /// 异步执行工具
    /// </summary>
    /// <param name="context">工具执行上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工具执行结果</returns>
    public async Task<ToolResult> ExecuteAsync(ToolExecutionContext context, CancellationToken cancellationToken = default)
    {
        var request = JsonSerializer.Deserialize<WebFetchRequest>(context.ToolCall.Arguments, JsonOptions())
            ?? throw new InvalidOperationException("Invalid web_fetch arguments.");

        if (string.IsNullOrWhiteSpace(request.Url) || !Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException("web_fetch requires a valid absolute URL.");
        }

        if (uri.Scheme is not ("http" or "https"))
        {
            throw new InvalidOperationException("web_fetch only supports http and https URLs.");
        }

        using var response = await httpClient.GetAsync(uri, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new ToolResult
            {
                ToolCallId = context.ToolCall.Id,
                IsSuccess = false,
                Status = ToolExecutionStatus.Failed,
                Content = JsonSerializer.Serialize(new
                {
                    url = uri.ToString(),
                    statusCode = (int)response.StatusCode,
                    reasonPhrase = response.ReasonPhrase,
                    content = Truncate(content, NormalizeLimit(request.MaxCharacters))
                })
            };
        }

        return new ToolResult
        {
            ToolCallId = context.ToolCall.Id,
            IsSuccess = true,
            Status = ToolExecutionStatus.Completed,
            Content = JsonSerializer.Serialize(new
            {
                url = uri.ToString(),
                statusCode = (int)response.StatusCode,
                contentType = response.Content.Headers.ContentType?.ToString(),
                content = Truncate(content, NormalizeLimit(request.MaxCharacters))
            })
        };
    }

    /// <summary>
    /// 规范化限制
    /// </summary>
    /// <param name="requested">请求的限制</param>
    /// <returns>规范化后的限制</returns>
    private static int NormalizeLimit(int? requested)
        => requested is >= 256 and <= 50000 ? requested.Value : 12000;

    /// <summary>
    /// 截断内容
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="limit">限制</param>
    /// <returns>截断后的内容</returns>
    private static string Truncate(string content, int limit)
    {
        if (content.Length <= limit)
        {
            return content;
        }

        return $"{content[..limit]}\n\n[Output truncated to {limit} characters]";
    }

    /// <summary>
    /// 获取JSON序列化选项
    /// </summary>
    /// <returns>JSON序列化选项</returns>
    private static JsonSerializerOptions JsonOptions() => new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Web获取请求
    /// </summary>
    /// <param name="Url">URL</param>
    /// <param name="MaxCharacters">最大字符数</param>
    private sealed record WebFetchRequest(string Url, int? MaxCharacters);
}
