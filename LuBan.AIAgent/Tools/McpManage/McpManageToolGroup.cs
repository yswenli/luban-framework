/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Tools.McpManage
*文件名： McpManageToolGroup
*描述：MCP 管理工具实现（WriteMcpServerAsync / DeleteMcpServerAsync / TestMcpServerAsync）
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.MCP;
using System.Text.Json;

namespace LuBan.AIAgent.Tools.McpManage;

/// <summary>
/// MCP 管理工具实现。
/// </summary>
public class McpManageToolGroup
{
    private readonly MCPRegistry _registry;
    private readonly McpManageToolOptions _options;
    private readonly McpOptions _mcpOptions;
    private readonly IAgentReloadTrigger? _reloadTrigger;
    private readonly IMcpConfigValueResolver _resolver;

    public McpManageToolGroup(MCPRegistry registry, McpManageToolOptions options, McpOptions mcpOptions, IAgentReloadTrigger? reloadTrigger = null, IMcpConfigValueResolver? resolver = null)
    {
        _registry = registry;
        _options = options;
        _mcpOptions = mcpOptions;
        _reloadTrigger = reloadTrigger;
        _resolver = resolver ?? NullMcpConfigValueResolver.Instance;
    }

    /// <summary>
    /// 创建或覆盖工作区级 MCP 服务器配置。
    /// </summary>
    /// <remarks>
    /// <para>
    /// <paramref name="transport"/> 决定必填项：<c>stdio</c> 需 <paramref name="command"/>；
    /// <c>http</c>（Streamable HTTP）/ <c>sse</c> 需 <paramref name="url"/>。
    /// </para>
    /// <para>
    /// <paramref name="headers"/> / <paramref name="environment"/> 的值支持凭据引用
    /// <c>$credential:&lt;凭据键&gt;</c>；疑似令牌的字面量会被拒绝（见 <see cref="McpConfigSecurityValidator"/>）。
    /// </para>
    /// </remarks>
    public async Task<ToolResult<string>> WriteMcpServerAsync(
        string name,
        string? command,
        string[]? args,
        string? transport,
        string? description,
        bool overwrite,
        string workspaceRoot,
        string? url = null,
        Dictionary<string, string>? headers = null,
        Dictionary<string, string>? environment = null)
    {
        var normalizedTransport = NormalizeTransport(transport);

        if (string.IsNullOrWhiteSpace(name))
            return ToolResult.Fail<string>("MCP 名称不能为空。");

        if (!IsValidFileName(name))
            return ToolResult.Fail<string>("MCP 名称不能包含路径分隔符或非法文件名字符。");

        if (string.IsNullOrWhiteSpace(url) && string.IsNullOrWhiteSpace(command))
            return ToolResult.Fail<string>("必须提供启动命令（stdio）或 URL（http/sse）。");

        if (normalizedTransport == "stdio" && string.IsNullOrWhiteSpace(command))
            return ToolResult.Fail<string>("stdio 传输必须提供启动命令。");

        if (normalizedTransport is "http" or "sse" && string.IsNullOrWhiteSpace(url))
            return ToolResult.Fail<string>($"{normalizedTransport} 传输必须提供 URL。");

        var config = new McpServerConfig
        {
            Name = name,
            Command = command ?? "",
            Args = args?.ToList() ?? new List<string>(),
            Transport = normalizedTransport,
            Url = url ?? "",
            Headers = headers is null ? new Dictionary<string, string>() : new Dictionary<string, string>(headers),
            Environment = environment is null ? new Dictionary<string, string>() : new Dictionary<string, string>(environment),
            Description = description ?? "",
            Enabled = true
        };

        return await WriteConfigAsync(config, overwrite, workspaceRoot).ConfigureAwait(false);
    }

    /// <summary>
    /// 写入 MCP 配置（程序化入口；与工具入口共用校验与落盘逻辑）。
    /// </summary>
    /// <param name="config">配置。</param>
    /// <param name="overwrite">是否允许覆盖同名配置。</param>
    /// <param name="workspaceRoot">工作区根目录。</param>
    /// <returns>写入结果（配置文件的绝对路径）。</returns>
    public async Task<ToolResult<string>> WriteConfigAsync(McpServerConfig config, bool overwrite, string workspaceRoot)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (string.IsNullOrWhiteSpace(config.Name))
            return ToolResult.Fail<string>("MCP 名称不能为空。");

        if (!IsValidFileName(config.Name))
            return ToolResult.Fail<string>("MCP 名称不能包含路径分隔符或非法文件名字符。");

        // 内置名称不可覆盖
        if (_registry.IsBuiltin(config.Name))
            return ToolResult.Fail<string>($"MCP '{config.Name}' 为内置保留，不可覆盖。");

        var securityError = McpConfigSecurityValidator.Validate(config);
        if (securityError is not null)
            return ToolResult.Fail<string>(securityError);

        var existing = _registry.Get(config.Name);
        if (existing != null && !overwrite)
            return ToolResult.Fail<string>($"MCP '{config.Name}' 已存在，需设置 overwrite=true 覆盖。");

        var mcpsDir = Path.Combine(workspaceRoot, ".luban-agent", "mcps");
        var configFile = Path.Combine(mcpsDir, $"{config.Name}.json");

        try
        {
            Directory.CreateDirectory(mcpsDir);
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(configFile, json).ConfigureAwait(false);

            _registry.LoadFromWorkspace(workspaceRoot);
            _reloadTrigger?.RequestReload();

            return ToolResult.Ok(configFile, $"MCP '{config.Name}' 配置已写入 {configFile}");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail<string>($"写入 MCP 配置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除工作区级 MCP 服务器配置。
    /// </summary>
    public async Task<ToolResult<string>> DeleteMcpServerAsync(string name, string workspaceRoot)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ToolResult.Fail<string>("MCP 名称不能为空。");

        if (_registry.IsBuiltin(name))
            return ToolResult.Fail<string>($"MCP '{name}' 为内置，不可删除。");

        var configFile = Path.Combine(workspaceRoot, ".luban-agent", "mcps", $"{name}.json");
        if (!File.Exists(configFile))
            return ToolResult.Fail<string>($"MCP '{name}' 配置文件不存在。");

        try
        {
            await Task.Run(() => File.Delete(configFile));

            _registry.LoadFromWorkspace(workspaceRoot);
            _reloadTrigger?.RequestReload();

            return ToolResult.Ok(configFile, $"MCP '{name}' 已删除。");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail<string>($"删除 MCP 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 测试 MCP 服务器连接（不持久化）。
    /// </summary>
    public async Task<ToolResult<McpTestResult>> TestMcpServerAsync(
        string? command,
        string[]? args,
        string? transport,
        int timeoutSeconds,
        string? url = null,
        Dictionary<string, string>? headers = null,
        Dictionary<string, string>? environment = null)
    {
        var normalizedTransport = NormalizeTransport(transport);

        if (normalizedTransport == "stdio" && string.IsNullOrWhiteSpace(command))
            return ToolResult.Fail<McpTestResult>("stdio 传输必须提供启动命令。");

        if (normalizedTransport is "http" or "sse" && string.IsNullOrWhiteSpace(url))
            return ToolResult.Fail<McpTestResult>($"{normalizedTransport} 传输必须提供 URL。");

        var config = new McpServerConfig
        {
            Name = "test-mcp",
            Command = command ?? "",
            Args = args?.ToList() ?? new List<string>(),
            Transport = normalizedTransport,
            Url = url ?? "",
            Headers = headers is null ? new Dictionary<string, string>() : new Dictionary<string, string>(headers),
            Environment = environment is null ? new Dictionary<string, string>() : new Dictionary<string, string>(environment),
            Enabled = true
        };

        var client = MCPClientFactory.Create(config, _resolver);

        try
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds > 0 ? timeoutSeconds : _mcpOptions.ConnectTimeoutSeconds));
            var connected = await client.ConnectAsync(cts.Token);

            var result = new McpTestResult { Connected = connected };

            if (connected)
            {
                try
                {
                    var tools = await client.ListToolsAsync(cts.Token);
                    result.ToolCount = tools.Count();
                    result.Message = $"连接成功，发现 {result.ToolCount} 个工具。";
                }
                catch
                {
                    result.Message = "连接成功，但获取工具列表失败。";
                }
            }
            else
            {
                result.Message = "连接失败（进程未启动或超时）。";
            }

            return ToolResult.Ok(result, result.Message);
        }
        catch (Exception ex)
        {
            return ToolResult.Fail<McpTestResult>($"测试失败: {ex.Message}");
        }
        finally
        {
            if (client is IDisposable d)
            {
                try { if (client.IsConnected) await client.DisconnectAsync(); }
                catch { }
                d.Dispose();
            }
        }
    }

    /// <summary>
    /// 归一化传输方式：空值视为 <c>stdio</c>，其余小写化。
    /// </summary>
    /// <param name="transport">原始传输方式。</param>
    /// <returns>归一化后的传输方式。</returns>
    private static string NormalizeTransport(string? transport)
        => string.IsNullOrWhiteSpace(transport) ? "stdio" : transport.Trim().ToLowerInvariant();

    /// <summary>
    /// 校验名称可作为配置文件名（防路径穿越）。
    /// </summary>
    /// <param name="name">MCP 名称。</param>
    /// <returns>合法返回 <c>true</c>。</returns>
    private static bool IsValidFileName(string name)
        => name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0
           && !name.Contains("..", StringComparison.Ordinal);
}

/// <summary>MCP 测试结果。</summary>
public class McpTestResult
{
    /// <summary>是否连接成功。</summary>
    public bool Connected { get; set; }
    /// <summary>发现的工具数量。</summary>
    public int ToolCount { get; set; }
    /// <summary>结果消息。</summary>
    public string Message { get; set; } = "";
}