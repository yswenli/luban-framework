/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.MCP
*文件名： HttpMCPClient
*版本号： V1.0.0.0
*唯一标识：a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/4
*描述：基于 HTTP 的 MCP 客户端实现（Streamable HTTP / 旧 HTTP+SSE）
*
*=================================================
*修改标记
*修改时间：2026/10/19
*修改人： yswenli
*版本号： V1.0.1.0
*描述：新增 Url 字段、请求头（含 $credential 引用）注入与 Streamable HTTP 传输
*
*****************************************************************************/
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LuBan.AIAgent.Configuration;

namespace LuBan.AIAgent.MCP;

/// <summary>
/// 基于 HTTP 的 MCP 客户端实现。
/// </summary>
/// <remarks>
/// <para>
/// 传输分派由 <see cref="McpServerConfig.Transport"/> 决定：
/// <list type="bullet">
/// <item><description><c>http</c>：Streamable HTTP —— 直接 POST 到 <see cref="McpServerConfig.Url"/>，
/// 响应可为单个 JSON 或 <c>text/event-stream</c>，并按服务端返回维持 <c>Mcp-Session-Id</c>。</description></item>
/// <item><description><c>sse</c>：旧「HTTP + SSE」语义 —— 端点取 <c>Url</c>（回退历史写法 <c>Args[0]</c>/<c>Command</c>）
/// 并追加 <c>/message</c>。</description></item>
/// </list>
/// </para>
/// <para>
/// 请求头与 URL 值支持凭据引用 <c>$credential:&lt;键&gt;</c>，连接时经
/// <see cref="IMcpConfigValueResolver"/> 解析，避免令牌明文落盘。
/// </para>
/// </remarks>
public class HttpMCPClient : IMCPClient, IDisposable
{
    private readonly McpServerConfig _config;
    private readonly IMcpConfigValueResolver _resolver;
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _rpcLock = new(1, 1);
    private int _nextId;
    private bool _connected;
    private bool _disposed;
    private string? _sessionId;

    private static readonly TimeSpan RpcTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 创建 HttpMCPClient 实例（不做凭据引用解析）。
    /// </summary>
    /// <param name="config">MCP 服务器配置。</param>
    public HttpMCPClient(McpServerConfig config)
        : this(config, NullMcpConfigValueResolver.Instance)
    {
    }

    /// <summary>
    /// 创建 HttpMCPClient 实例。
    /// </summary>
    /// <param name="config">MCP 服务器配置。</param>
    /// <param name="resolver">配置值解析器（凭据引用）。</param>
    public HttpMCPClient(McpServerConfig config, IMcpConfigValueResolver? resolver)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _resolver = resolver ?? NullMcpConfigValueResolver.Instance;
        _httpClient = new HttpClient { Timeout = RpcTimeout };
    }

    /// <inheritdoc />
    public string Name => _config.Name;

    /// <inheritdoc />
    public string Description => _config.Description;

    /// <inheritdoc />
    public bool IsConnected => _connected && !_disposed;

    /// <summary>
    /// 是否为 Streamable HTTP（<c>transport: "http"</c>）。
    /// </summary>
    internal bool StreamableHttp
        => string.Equals(_config.Transport?.Trim(), "http", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 端点基址：<see cref="McpServerConfig.Url"/> 优先，其次回退历史写法 <c>Args[0]</c> / <c>Command</c>。
    /// </summary>
    private string? BaseUrl
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_config.Url)) return _config.Url.Trim();

            var legacy = _config.Args.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a));
            if (legacy is not null) return legacy.Trim();

            return string.IsNullOrWhiteSpace(_config.Command) ? null : _config.Command.Trim();
        }
    }

    /// <summary>
    /// 实际请求端点：Streamable HTTP 直连；旧 SSE 追加 <c>/message</c>。
    /// </summary>
    internal string? Endpoint
    {
        get
        {
            var baseUrl = BaseUrl;
            if (baseUrl is null) return null;

            return StreamableHttp ? baseUrl : baseUrl.TrimEnd('/') + "/message";
        }
    }

    /// <inheritdoc />
    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) return false;
        if (_connected) return true;

        try
        {
            if (!await ApplyHeadersAsync(cancellationToken).ConfigureAwait(false))
                return false;

            var initResult = await SendRequestAsync("initialize", new JsonObject
            {
                ["protocolVersion"] = "2024-11-05",
                ["capabilities"] = new JsonObject(),
                ["clientInfo"] = new JsonObject
                {
                    ["name"] = "luban-ai-agent",
                    ["version"] = "1.0"
                }
            }, cancellationToken).ConfigureAwait(false);

            if (initResult == null) return false;

            await SendNotificationAsync("notifications/initialized", cancellationToken).ConfigureAwait(false);
            _connected = true;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 解析并写入静态请求头（值可含 <c>$credential:</c> 引用）。
    /// </summary>
    /// <returns>全部解析成功返回 <c>true</c>；任一引用缺失返回 <c>false</c>。</returns>
    private async Task<bool> ApplyHeadersAsync(CancellationToken cancellationToken)
    {
        foreach (var (name, rawValue) in _config.Headers)
        {
            if (string.IsNullOrWhiteSpace(name)) continue;

            // 这两个头由每个请求自行声明（需随传输方式变化），配置里忽略
            if (name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("Accept", StringComparison.OrdinalIgnoreCase))
                continue;

            var value = await _resolver.ResolveAsync(rawValue, cancellationToken).ConfigureAwait(false);
            if (value is null)
            {
                Logger.Warn($"MCP 请求头 {name} 的值无法解析（凭据缺失？），连接取消。");
                return false;
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Remove(name);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(name, value);
            }
            catch (Exception ex)
            {
                Logger.Error($"MCP 请求头设置失败：{name}", ex);
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public Task DisconnectAsync()
    {
        _connected = false;
        _sessionId = null;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MCPTool>> ListToolsAsync(CancellationToken cancellationToken = default)
    {
        var result = await SendRequestAsync("tools/list", null, cancellationToken).ConfigureAwait(false);
        if (result?["tools"] is not JsonArray tools) return Enumerable.Empty<MCPTool>();

        return tools
            .Where(t => t != null)
            .Select(t => new MCPTool
            {
                Name = t!["name"]?.GetValue<string>() ?? "",
                Description = t!["description"]?.GetValue<string>() ?? "",
                InputSchema = t!["inputSchema"]?.ToJsonString()
            })
            .Where(t => t.Name.Length > 0)
            .ToList();
    }

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", 
        Justification = "JSON 序列化仅用于简单字典类型，已通过 JsonSerializerOptions 处理")]
    public async Task<MCPToolResult> CallToolAsync(string toolName, Dictionary<string, object?> arguments, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await SendRequestAsync("tools/call", new JsonObject
            {
                ["name"] = toolName,
                ["arguments"] = JsonSerializer.SerializeToNode(arguments)
            }, cancellationToken).ConfigureAwait(false);

            if (result == null)
                return new MCPToolResult { Success = false, Error = "MCP 调用失败或无响应" };

            var content = result["content"] is JsonArray arr
                ? arr.OfType<JsonObject>().FirstOrDefault()?["text"]?.GetValue<string>()
                : result.ToJsonString();

            var isError = result["isError"]?.GetValue<bool>() == true;
            return new MCPToolResult
            {
                Success = !isError,
                Content = content,
                Error = isError ? content : null
            };
        }
        catch (Exception ex)
        {
            Logger.Error("MCP HTTP 调用失败", ex, toolName);
            return new MCPToolResult { Success = false, Error = $"MCP 调用失败: {ex.Message}" };
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MCPResource>> ListResourcesAsync(CancellationToken cancellationToken = default)
    {
        var result = await SendRequestAsync("resources/list", null, cancellationToken).ConfigureAwait(false);
        if (result?["resources"] is not JsonArray resources) return Enumerable.Empty<MCPResource>();

        return resources
            .Where(r => r != null)
            .Select(r => new MCPResource
            {
                Uri = r!["uri"]?.GetValue<string>() ?? "",
                Name = r!["name"]?.GetValue<string>() ?? "",
                Description = r!["description"]?.GetValue<string>(),
                MimeType = r!["mimeType"]?.GetValue<string>()
            })
            .Where(r => r.Uri.Length > 0)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<MCPResourceContent> ReadResourceAsync(string resourceUri, CancellationToken cancellationToken = default)
    {
        var result = await SendRequestAsync("resources/read", new JsonObject
        {
            ["uri"] = resourceUri
        }, cancellationToken).ConfigureAwait(false);

        var first = result?["contents"] is JsonArray contents ? contents.FirstOrDefault() : null;
        return new MCPResourceContent
        {
            Uri = resourceUri,
            Content = first?["text"]?.GetValue<string>(),
            MimeType = first?["mimeType"]?.GetValue<string>()
        };
    }

    private async Task<JsonNode?> SendRequestAsync(string method, JsonNode? @params, CancellationToken cancellationToken)
    {
        if (_disposed) return null;

        await _rpcLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var id = Interlocked.Increment(ref _nextId);
            var request = new JsonObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = id,
                ["method"] = method
            };
            if (@params != null) request["params"] = @params;

            var endpoint = Endpoint;
            if (string.IsNullOrEmpty(endpoint)) return null;

            var message = await PostAsync(endpoint, request.ToJsonString(), cancellationToken).ConfigureAwait(false);

            return message?["error"] != null ? null : message?["result"];
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return null;
        }
        finally
        {
            _rpcLock.Release();
        }
    }

    private async Task SendNotificationAsync(string method, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        var notification = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["method"] = method
        };

        var endpoint = Endpoint;
        if (string.IsNullOrEmpty(endpoint)) return;

        // 通知无响应体：Streamable HTTP 通常回 202 Accepted，失败在此静默（与旧行为一致）
        await PostAsync(endpoint, notification.ToJsonString(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// POST 一条 JSON-RPC 消息并解析响应消息。
    /// </summary>
    /// <param name="endpoint">请求端点。</param>
    /// <param name="payload">JSON-RPC 消息体。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>解析出的 JSON-RPC 响应消息；无响应体或失败时返回 <c>null</c>。</returns>
    private async Task<JsonNode?> PostAsync(string endpoint, string payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        request.Headers.TryAddWithoutValidation(
            "Accept", StreamableHttp ? "application/json, text/event-stream" : "application/json");

        if (!string.IsNullOrEmpty(_sessionId))
            request.Headers.TryAddWithoutValidation("Mcp-Session-Id", _sessionId);

        using var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        // Streamable HTTP：服务端在初始化响应里下发会话 id
        if (response.Headers.TryGetValues("Mcp-Session-Id", out var sessionValues))
        {
            var session = sessionValues.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(session)) _sessionId = session;
        }

        // 通知类消息（202/204）无响应体
        if (response.StatusCode is System.Net.HttpStatusCode.Accepted or System.Net.HttpStatusCode.NoContent)
            return null;

        if (!response.IsSuccessStatusCode) return null;

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(body)) return null;

        var contentType = response.Content.Headers.ContentType?.MediaType;
        var text = contentType is not null &&
                   contentType.Contains("event-stream", StringComparison.OrdinalIgnoreCase)
            ? ExtractSseData(body)
            : body;

        if (string.IsNullOrWhiteSpace(text)) return null;

        try
        {
            return JsonNode.Parse(text);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 从 <c>text/event-stream</c> 文本中取出最后一条 <c>data:</c> 负载。
    /// </summary>
    /// <param name="sse">SSE 原文。</param>
    /// <returns>数据负载；无 <c>data:</c> 行时返回空串。</returns>
    private static string ExtractSseData(string sse)
    {
        var dataLines = sse
            .Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .Where(line => line.StartsWith("data:", StringComparison.Ordinal))
            .Select(line => line["data:".Length..].TrimStart())
            .ToList();

        return dataLines.Count == 0 ? string.Empty : string.Join("\n", dataLines);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _connected = false;
        _rpcLock.Dispose();
        _httpClient.Dispose();
    }
}