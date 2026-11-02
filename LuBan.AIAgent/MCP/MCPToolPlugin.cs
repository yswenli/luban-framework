/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
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
    public IReadOnlyList<AIFunction> GetTools(DiContainer sp, ToolGroupOptions? toolsOptions = null)
    {
        var registry = sp.GetService<MCPRegistry>();
        if (registry == null) return Array.Empty<AIFunction>();

        var confirmationService = sp.GetRequiredService<IToolConfirmationService>();
        var clients = registry.GetAll();
        var tools = new List<AIFunction>();

        // 仅当存在 MCP 客户端时才暴露资源类工具，避免无服务器时空注册两个无效工具。
        if (clients.Count > 0)
        {
            tools.Add(new McpListResourcesAIFunction(registry, confirmationService));
            tools.Add(new McpReadResourceAIFunction(registry, confirmationService));
        }

        foreach (var client in clients.Where(c => c.IsConnected))
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
    /// 大小写不敏感地读取工具参数值。
    /// </summary>
    /// <param name="arguments">工具参数。</param>
    /// <param name="name">参数名。</param>
    /// <returns>参数值的字符串表示；不存在时返回 null。</returns>
    private static string? GetArg(AIFunctionArguments arguments, string name)
    {
        foreach (var kv in arguments)
        {
            if (string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase))
                return kv.Value?.ToString();
        }
        return null;
    }

    /// <summary>
    /// MCP 工具包装为 AIFunction，名称加 mcp_{client}_{tool} 前缀避免冲突
    /// </summary>
    private sealed class MCPToolAIFunction : AIFunction
    {
        private readonly IMCPClient _client;
        private readonly MCPTool _tool;
        private readonly string _name;
        private readonly IToolConfirmationService _confirmationService;

        /// <summary>
        /// 把单个 MCP 工具包装为 AIFunction，工具名加 <c>mcp_{client}_{tool}</c> 前缀避免冲突。
        /// </summary>
        /// <param name="client">提供该工具的 MCP 客户端。</param>
        /// <param name="tool">待包装的 MCP 工具定义。</param>
        /// <param name="confirmationService">调用前的确认服务。</param>
        public MCPToolAIFunction(IMCPClient client, MCPTool tool, IToolConfirmationService confirmationService)
        {
            _client = client;
            _tool = tool;
            _confirmationService = confirmationService;
            _name = Sanitize($"mcp_{client.Name}_{tool.Name}");
        }

        /// <summary>
        /// 暴露给模型的工具名，已替换非法字符并带 MCP 客户端前缀。
        /// </summary>
        public override string Name => _name;

        /// <summary>
        /// 工具描述，直接取自 MCP 工具自身的元数据。
        /// </summary>
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
    internal sealed class McpListResourcesAIFunction : AIFunction
    {
        private readonly MCPRegistry _registry;
        private readonly IToolConfirmationService _confirmationService;

        /// <summary>
        /// 构造「列出 MCP 资源」工具。
        /// </summary>
        /// <param name="registry">用于枚举已连接 MCP 客户端的注册表。</param>
        /// <param name="confirmationService">调用前的确认服务。</param>
        public McpListResourcesAIFunction(MCPRegistry registry, IToolConfirmationService confirmationService)
        {
            _registry = registry;
            _confirmationService = confirmationService;
        }

        /// <summary>
        /// 工具名，固定为 <c>mcp_list_resources</c>。
        /// </summary>
        public override string Name => "mcp_list_resources";

        /// <summary>
        /// 工具描述文字，说明可按 client 过滤、缺省遍历全部服务器。
        /// </summary>
        public override string Description => "列出已连接 MCP 服务器可用的资源（client 可选，缺省遍历全部）。";

        /// <summary>
        /// 工具入参结构，仅含可选的 <c>client</c> 过滤字段。
        /// </summary>
        public override JsonElement JsonSchema => JsonDocument.Parse(
            """{"type":"object","properties":{"client":{"type":"string","description":"MCP 服务器名称，缺省全部"}}}""").RootElement.Clone();

        protected override async ValueTask<object?> InvokeCoreAsync(
            AIFunctionArguments arguments, CancellationToken cancellationToken)
        {
            var args = new Dictionary<string, object?>(arguments);
            var outcome = await _confirmationService.EvaluateAsync(Name, null, args);
            if (outcome == EnumConfirmationOutcome.Planned) return ToolResult.Plan<string>();
            if (outcome != EnumConfirmationOutcome.Allowed) return ToolResult.Denied<string>();

            var clientFilter = GetArg(arguments, "client");
            var clients = _registry.GetAll().Where(x => x.IsConnected).ToList();
            if (!string.IsNullOrWhiteSpace(clientFilter))
            {
                clients = clients.Where(x => x.Name.Equals(clientFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                if (clients.Count == 0)
                    return ToolResult.Fail<string>($"未找到 MCP 服务器: {clientFilter}。");
            }

            var list = new List<object>();
            var failedClients = 0;
            foreach (var client in clients)
            {
                IEnumerable<MCPResource> resources;
                try { resources = await client.ListResourcesAsync(cancellationToken); }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
                catch (Exception ex)
                {
                    Logger.Warn($"列出 MCP 资源失败（client={client.Name}）: {ex.Message}");
                    failedClients++;
                    continue;
                }

                foreach (var r in resources)
                    list.Add(new { client = client.Name, uri = r.Uri, name = r.Name, description = r.Description, mimeType = r.MimeType });
            }

            if (list.Count == 0)
            {
                return failedClients > 0
                    ? ToolResult.Fail<string>($"读取 MCP 资源失败：{failedClients} 个服务器调用出错。")
                    : ToolResult.Ok<string>("当前没有可用的 MCP 资源。");
            }

            return ToolResult.Ok<string>(JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    /// <summary>
    /// 读取指定 MCP 资源。
    /// </summary>
    internal sealed class McpReadResourceAIFunction : AIFunction
    {
        /// <summary>
        /// 单次返回资源内容的最大字符数，超出则截断，避免撑爆对话上下文。
        /// </summary>
        private const int MaxContentCharacters = 12000;

        private readonly MCPRegistry _registry;
        private readonly IToolConfirmationService _confirmationService;

        /// <summary>
        /// 构造「读取指定 MCP 资源」工具。
        /// </summary>
        /// <param name="registry">用于按名称解析 MCP 客户端的注册表。</param>
        /// <param name="confirmationService">调用前的确认服务。</param>
        public McpReadResourceAIFunction(MCPRegistry registry, IToolConfirmationService confirmationService)
        {
            _registry = registry;
            _confirmationService = confirmationService;
        }

        /// <summary>
        /// 工具名，固定为 <c>mcp_read_resource</c>。
        /// </summary>
        public override string Name => "mcp_read_resource";

        /// <summary>
        /// 工具描述文字，说明需提供 client 与 uri。
        /// </summary>
        public override string Description => "读取指定 MCP 资源的完整内容（需 client 与 uri）。";

        /// <summary>
        /// 工具入参结构，强制要求 <c>client</c> 与 <c>uri</c> 两个字段。
        /// </summary>
        public override JsonElement JsonSchema => JsonDocument.Parse(
            """{"type":"object","required":["client","uri"],"properties":{"client":{"type":"string"},"uri":{"type":"string"}}}""").RootElement.Clone();

        protected override async ValueTask<object?> InvokeCoreAsync(
            AIFunctionArguments arguments, CancellationToken cancellationToken)
        {
            var args = new Dictionary<string, object?>(arguments);
            var outcome = await _confirmationService.EvaluateAsync(Name, null, args);
            if (outcome == EnumConfirmationOutcome.Planned) return ToolResult.Plan<string>();
            if (outcome != EnumConfirmationOutcome.Allowed) return ToolResult.Denied<string>();

            var clientName = GetArg(arguments, "client");
            var uri = GetArg(arguments, "uri");
            if (string.IsNullOrWhiteSpace(clientName) || string.IsNullOrWhiteSpace(uri))
                return ToolResult.Fail<string>("参数缺失：需要 client 与 uri。");

            var client = _registry.Get(clientName!);
            if (client == null) return ToolResult.Fail<string>($"未找到 MCP 服务器: {clientName}。");
            if (!client.IsConnected) return ToolResult.Fail<string>($"MCP 服务器 {clientName} 未连接。");

            try
            {
                var content = await client.ReadResourceAsync(uri!, cancellationToken);
                var text = content?.Content ?? "（资源内容为空）";
                if (text.Length > MaxContentCharacters)
                    text = text[..MaxContentCharacters] + "\n\n[内容已截断]";
                return ToolResult.Ok<string>(text);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ToolResult.Fail<string>($"读取 MCP 资源失败: {ex.Message}");
            }
        }
    }
}
