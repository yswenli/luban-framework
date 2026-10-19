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

    public McpManageToolGroup(MCPRegistry registry, McpManageToolOptions options, McpOptions mcpOptions, IAgentReloadTrigger? reloadTrigger = null)
    {
        _registry = registry;
        _options = options;
        _mcpOptions = mcpOptions;
        _reloadTrigger = reloadTrigger;
    }

    /// <summary>
    /// 创建或覆盖工作区级 MCP 服务器配置。
    /// </summary>
    public async Task<ToolResult<string>> WriteMcpServerAsync(
        string name,
        string command,
        string[]? args,
        string? transport,
        string? description,
        bool overwrite,
        string workspaceRoot)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ToolResult.Fail<string>("MCP 名称不能为空。");

        if (string.IsNullOrWhiteSpace(command))
            return ToolResult.Fail<string>("启动命令不能为空。");

        // 内置名称不可覆盖
        if (_registry.IsBuiltin(name))
            return ToolResult.Fail<string>($"MCP '{name}' 为内置保留，不可覆盖。");

        var existing = _registry.Get(name);
        if (existing != null && !overwrite)
            return ToolResult.Fail<string>($"MCP '{name}' 已存在，需设置 overwrite=true 覆盖。");

        var config = new McpServerConfig
        {
            Name = name,
            Command = command,
            Args = args?.ToList() ?? new List<string>(),
            Transport = transport ?? "stdio",
            Description = description ?? "",
            Enabled = true
        };

        var mcpsDir = Path.Combine(workspaceRoot, ".luban-agent", "mcps");
        var configFile = Path.Combine(mcpsDir, $"{name}.json");

        try
        {
            Directory.CreateDirectory(mcpsDir);
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(configFile, json);

            _registry.LoadFromWorkspace(workspaceRoot);
            _reloadTrigger?.RequestReload();

            return ToolResult.Ok(configFile, $"MCP '{name}' 配置已写入 {configFile}");
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
        string command,
        string[]? args,
        string? transport,
        int timeoutSeconds)
    {
        if (string.IsNullOrWhiteSpace(command))
            return ToolResult.Fail<McpTestResult>("启动命令不能为空。");

        var config = new McpServerConfig
        {
            Name = "test-mcp",
            Command = command,
            Args = args?.ToList() ?? new List<string>(),
            Transport = transport ?? "stdio",
            Enabled = true
        };

        var client = MCPClientFactory.Create(config);

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