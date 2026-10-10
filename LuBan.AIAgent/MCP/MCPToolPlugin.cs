/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.MCP
*文件名： MCPToolPlugin
*版本号： V1.0.0.0
*唯一标识：c815034e-eda2-40a8-aed7-16a667718d17
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/29
*描述：将已连接 MCP 客户端的工具并入 Agent 工具注册表
*
*=================================================
*修改标记
*修改时间：2026/7/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：将已连接 MCP 客户端的工具并入 Agent 工具注册表
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace LuBan.AIAgent.MCP;

/// <summary>
/// 将已连接 MCP 客户端的工具并入 Agent 工具注册表（GroupName="mcp"）
/// </summary>
public class MCPToolPlugin : ILuBanToolPlugin
{
    /// <inheritdoc />
    public string GroupName => "mcp";

    /// <inheritdoc />
    public string? Description => "MCP 外部服务器工具";

    /// <inheritdoc />
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp, ToolGroupOptions? toolsOptions = null)
    {
        var registry = sp.GetService<MCPRegistry>();
        if (registry == null) return Array.Empty<AIFunction>();

        var confirmationService = sp.GetRequiredService<IToolConfirmationService>();
        var tools = new List<AIFunction>
        {
            new McpListResourcesAIFunction(registry, confirmationService),
            new McpReadResourceAIFunction(registry, confirmationService),
        };

        foreach (var client in registry.GetAll().Where(c => c.IsConnected))
        {
            IEnumerable<MCPTool> mcpTools;
            try
            {
                mcpTools = client.ListToolsAsync().GetAwaiter().GetResult();
            }
            catch
            {
                continue;
            }

            foreach (var tool in mcpTools)
            {
                tools.Add(new MCPToolAIFunction(client, tool, confirmationService));
            }
        }
        return tools;
    }

    /// <inheritdoc />
    public bool IsEnabled(LuBanAgentOptions options) => true;

    /// <summary>
    /// MCP 工具包装为 AIFunction，名称加 mcp_{client}_{tool} 前缀避免冲突
    /// </summary>
    private sealed class MCPToolAIFunction : AIFunction
    {
        private readonly IMCPClient _client;
        private readonly MCPTool _tool;
        private readonly string _name;
        private readonly IToolConfirmationService _confirmationService;

        public MCPToolAIFunction(IMCPClient client, MCPTool tool, IToolConfirmationService confirmationService)
        {
            _client = client;
            _tool = tool;
            _confirmationService = confirmationService;
            _name = Sanitize($"mcp_{client.Name}_{tool.Name}");
        }

        public override string Name => _name;

        public override string Description => _tool.Description;

        public override JsonElement JsonSchema
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_tool.InputSchema))
                {
                    return JsonDocument.Parse("{}").RootElement.Clone();
                }
                return JsonDocument.Parse(_tool.InputSchema).RootElement.Clone();
            }
        }

        protected override async ValueTask<object?> InvokeCoreAsync(
            AIFunctionArguments arguments, CancellationToken cancellationToken)
        {
            var outcome = await _confirmationService.EvaluateAsync(_name, null,
                new Dictionary<string, object?>(arguments));
            if (outcome == EnumConfirmationOutcome.Planned)
                return ToolResult.Plan<string>();
            if (outcome != EnumConfirmationOutcome.Allowed)
                return ToolResult.Denied<string>();

            var result = await _client.CallToolAsync(
                _tool.Name,
                new Dictionary<string, object?>(arguments),
                cancellationToken);
            return result.Success ? result.Content : $"MCP 工具调用失败: {result.Error}";
        }

        private static string Sanitize(string name)
            => new(name.Select(c => char.IsLetterOrDigit(c) || c is '_' or '-' ? c : '_').ToArray());
    }

    /// <summary>
    /// 列出 MCP 资源（可选按 client 过滤）。
    /// </summary>
    private sealed class McpListResourcesAIFunction : AIFunction
    {
        private readonly MCPRegistry _registry;
        private readonly IToolConfirmationService _confirmationService;

        public McpListResourcesAIFunction(MCPRegistry registry, IToolConfirmationService confirmationService)
        {
            _registry = registry;
            _confirmationService = confirmationService;
        }

        public override string Name => "mcp_list_resources";

        public override string Description => "列出已连接 MCP 服务器可用的资源（client 可选，缺省遍历全部）。";

        public override JsonElement JsonSchema => JsonDocument.Parse(
            """{"type":"object","properties":{"client":{"type":"string","description":"MCP 服务器名称，缺省全部"}}}""").RootElement.Clone();

        protected override async ValueTask<object?> InvokeCoreAsync(
            AIFunctionArguments arguments, CancellationToken cancellationToken)
        {
            var args = new Dictionary<string, object?>(arguments);
            var outcome = await _confirmationService.EvaluateAsync(Name, null, args);
            if (outcome == EnumConfirmationOutcome.Planned) return ToolResult.Plan<string>();
            if (outcome != EnumConfirmationOutcome.Allowed) return ToolResult.Denied<string>();

            var clientFilter = arguments.TryGetValue("client", out var c) ? c?.ToString() : null;
            var clients = _registry.GetAll().Where(x => x.IsConnected);
            if (!string.IsNullOrWhiteSpace(clientFilter))
                clients = clients.Where(x => x.Name.Equals(clientFilter, StringComparison.OrdinalIgnoreCase));

            var list = new List<object>();
            foreach (var client in clients)
            {
                IEnumerable<MCPResource> resources;
                try { resources = await client.ListResourcesAsync(cancellationToken); }
                catch { continue; }
                foreach (var r in resources)
                    list.Add(new { client = client.Name, uri = r.Uri, name = r.Name, description = r.Description, mimeType = r.MimeType });
            }

            return list.Count == 0
                ? "当前没有可用的 MCP 资源。"
                : JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <summary>
    /// 读取指定 MCP 资源。
    /// </summary>
    private sealed class McpReadResourceAIFunction : AIFunction
    {
        private readonly MCPRegistry _registry;
        private readonly IToolConfirmationService _confirmationService;

        public McpReadResourceAIFunction(MCPRegistry registry, IToolConfirmationService confirmationService)
        {
            _registry = registry;
            _confirmationService = confirmationService;
        }

        public override string Name => "mcp_read_resource";

        public override string Description => "读取指定 MCP 资源的完整内容（需 client 与 uri）。";

        public override JsonElement JsonSchema => JsonDocument.Parse(
            """{"type":"object","required":["client","uri"],"properties":{"client":{"type":"string"},"uri":{"type":"string"}}}""").RootElement.Clone();

        protected override async ValueTask<object?> InvokeCoreAsync(
            AIFunctionArguments arguments, CancellationToken cancellationToken)
        {
            var args = new Dictionary<string, object?>(arguments);
            var outcome = await _confirmationService.EvaluateAsync(Name, null, args);
            if (outcome == EnumConfirmationOutcome.Planned) return ToolResult.Plan<string>();
            if (outcome != EnumConfirmationOutcome.Allowed) return ToolResult.Denied<string>();

            var clientName = arguments.TryGetValue("client", out var c) ? c?.ToString() : null;
            var uri = arguments.TryGetValue("uri", out var u) ? u?.ToString() : null;
            if (string.IsNullOrWhiteSpace(clientName) || string.IsNullOrWhiteSpace(uri))
                return "参数缺失：需要 client 与 uri。";

            var client = _registry.Get(clientName!);
            if (client == null) return $"未找到 MCP 服务器: {clientName}。";
            if (!client.IsConnected) return $"MCP 服务器 {clientName} 未连接。";

            try
            {
                var content = await client.ReadResourceAsync(uri!, cancellationToken);
                return content?.Content ?? "（资源内容为空）";
            }
            catch (Exception ex)
            {
                return $"读取 MCP 资源失败: {ex.Message}";
            }
        }
    }
}
