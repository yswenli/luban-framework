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
*描述：基于 HTTP/SSE 的 MCP 客户端实现
*
*=================================================
*修改标记
*修改时间：2026/8/4
*修改人： yswenli
*版本号： V1.0.0.0
*描述：基于 HTTP/SSE 的 MCP 客户端实现
*
*****************************************************************************/
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LuBan.AIAgent.Configuration;

namespace LuBan.AIAgent.MCP;

/// <summary>
/// 基于 HTTP/SSE 的 MCP 客户端实现
/// </summary>
public class HttpMCPClient : IMCPClient, IDisposable
{
    private readonly McpServerConfig _config;
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _rpcLock = new(1, 1);
    private int _nextId;
    private bool _connected;
    private bool _disposed;

    private static readonly TimeSpan RpcTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 创建 HttpMCPClient 实例
    /// </summary>
    public HttpMCPClient(McpServerConfig config)
    {
        _config = config;
        _httpClient = new HttpClient { Timeout = RpcTimeout };
    }

    /// <inheritdoc />
    public string Name => _config.Name;

    /// <inheritdoc />
    public string Description => _config.Description;

    /// <inheritdoc />
    public bool IsConnected => _connected && !_disposed;

    /// <inheritdoc />
    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) return false;
        if (_connected) return true;

        try
        {
            var initResult = await SendRequestAsync("initialize", new JsonObject
            {
                ["protocolVersion"] = "2024-11-05",
                ["capabilities"] = new JsonObject(),
                ["clientInfo"] = new JsonObject
                {
                    ["name"] = "luban-ai-agent",
                    ["version"] = "1.0"
                }
            }, cancellationToken);

            if (initResult == null) return false;

            await SendNotificationAsync("notifications/initialized", cancellationToken);
            _connected = true;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public Task DisconnectAsync()
    {
        _connected = false;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MCPTool>> ListToolsAsync(CancellationToken cancellationToken = default)
    {
        var result = await SendRequestAsync("tools/list", null, cancellationToken);
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
            }, cancellationToken);

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
        var result = await SendRequestAsync("resources/list", null, cancellationToken);
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
        }, cancellationToken);

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

        await _rpcLock.WaitAsync(cancellationToken);
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

            var baseUrl = _config.Args.FirstOrDefault() ?? _config.Command;
            if (string.IsNullOrEmpty(baseUrl)) return null;

            var endpoint = baseUrl.TrimEnd('/') + "/message";

            var content = new StringContent(request.ToJsonString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);

            if (!response.IsSuccessStatusCode) return null;

            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            var message = JsonNode.Parse(responseText);
            return message?["error"] != null ? null : message["result"];
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

        var baseUrl = _config.Args.FirstOrDefault() ?? _config.Command;
        if (string.IsNullOrEmpty(baseUrl)) return;

        var endpoint = baseUrl.TrimEnd('/') + "/message";

        var content = new StringContent(notification.ToJsonString(), Encoding.UTF8, "application/json");
        await _httpClient.PostAsync(endpoint, content, cancellationToken);
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