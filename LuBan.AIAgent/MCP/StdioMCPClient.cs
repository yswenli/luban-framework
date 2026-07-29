/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.MCP
*文件名： StdioMCPClient
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/28
*描述：基于 stdio JSON-RPC 的外部 MCP 客户端
*
*****************************************************************************/
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LuBan.AIAgent.MCP;

/// <summary>
/// 基于 stdio JSON-RPC 2.0 的外部 MCP 客户端
/// </summary>
public class StdioMCPClient : IMCPClient, IDisposable, IAsyncDisposable
{
    private readonly Configuration.McpServerConfig _config;
    private readonly SemaphoreSlim _rpcLock = new(1, 1);
    private Process? _process;
    private int _nextId;

    /// <summary>
    /// 创建 StdioMCPClient 实例
    /// </summary>
    public StdioMCPClient(Configuration.McpServerConfig config)
    {
        _config = config;
    }

    /// <inheritdoc />
    public string Name => _config.Name;

    /// <inheritdoc />
    public string Description => _config.Description;

    /// <inheritdoc />
    public bool IsConnected => _process is { HasExited: false };

    /// <inheritdoc />
    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected) return true;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = _config.Command,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            foreach (var arg in _config.Args)
            {
                psi.ArgumentList.Add(arg);
            }

            _process = Process.Start(psi);
            if (_process == null) return false;

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

            if (initResult == null)
            {
                await DisconnectAsync();
                return false;
            }

            await SendNotificationAsync("notifications/initialized");
            return true;
        }
        catch
        {
            await DisconnectAsync();
            return false;
        }
    }

    /// <inheritdoc />
    public Task DisconnectAsync()
    {
        if (_process != null)
        {
            try
            {
                if (!_process.HasExited) _process.Kill(entireProcessTree: true);
            }
            catch { }
            finally
            {
                _process.Dispose();
                _process = null;
            }
        }
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
    public async Task<MCPToolResult> CallToolAsync(
        string toolName, Dictionary<string, object?> arguments, CancellationToken cancellationToken = default)
    {
        var args = new JsonObject();
        foreach (var (key, value) in arguments)
        {
            args[key] = ToJsonNode(value);
        }

        var result = await SendRequestAsync("tools/call", new JsonObject
        {
            ["name"] = toolName,
            ["arguments"] = args
        }, cancellationToken);

        if (result == null)
        {
            return new MCPToolResult { Success = false, Error = "MCP 调用失败或无响应" };
        }

        var isError = result["isError"]?.GetValue<bool>() == true;
        var content = result["content"] is JsonArray arr
            ? string.Join("\n", arr.Select(c => c?["text"]?.GetValue<string>()).Where(t => t != null))
            : result.ToJsonString();

        return new MCPToolResult
        {
            Success = !isError,
            Content = isError ? null : content,
            Error = isError ? content : null
        };
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

    /// <summary>
    /// 发送 JSON-RPC 请求并等待匹配 id 的响应（跳过通知与其他消息）
    /// </summary>
    private async Task<JsonNode?> SendRequestAsync(string method, JsonNode? @params, CancellationToken cancellationToken)
    {
        if (!IsConnected) return null;

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

            await _process!.StandardInput.WriteLineAsync(request.ToJsonString());
            await _process.StandardInput.FlushAsync();

            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await _process.StandardOutput.ReadLineAsync(cancellationToken);
                if (line == null) return null;

                JsonNode? message;
                try { message = JsonNode.Parse(line); }
                catch { continue; }

                var responseId = message?["id"];
                if (responseId != null && JsonNode.DeepEquals(responseId, JsonValue.Create(id)))
                {
                    return message!["error"] != null ? null : message["result"];
                }
            }
            return null;
        }
        catch (OperationCanceledException)
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

    /// <summary>
    /// 发送 JSON-RPC 通知（无 id，不等待响应）
    /// </summary>
    private async Task SendNotificationAsync(string method)
    {
        if (!IsConnected) return;
        var notification = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["method"] = method
        };
        await _process!.StandardInput.WriteLineAsync(notification.ToJsonString());
        await _process.StandardInput.FlushAsync();
    }

    /// <summary>
    /// 将参数值转换为 JsonNode（兼容 JsonElement 与基础类型）
    /// </summary>
    private static JsonNode? ToJsonNode(object? value) => value switch
    {
        null => null,
        JsonNode node => node.DeepClone(),
        JsonElement element => JsonNode.Parse(element.GetRawText()),
        _ => JsonValue.Create(value)
    };

    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
        _rpcLock.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        _rpcLock.Dispose();
        GC.SuppressFinalize(this);
    }
}
