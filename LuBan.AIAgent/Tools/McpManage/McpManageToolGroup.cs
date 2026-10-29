/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.McpManage
*文件名： McpManageToolGroup
*版本号： V1.0.0.0
*唯一标识：c5733fa1-c3ea-40a0-b98a-37dde8c9e90e
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/14
*描述：MCP 管理工具实现（WriteMcpServerAsync / DeleteMcpServerAsync / TestMcpServerAsync）
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.1
*描述：写/删接入 IToolConfirmationService（Default/AcceptEdits 必确认、Plan 记计划）；
*      workspaceRoot 改可选并由宿主工作区兜底；command/args/transport 等改可选；补 [Description]
*
*****************************************************************************/

namespace LuBan.AIAgent.Tools.McpManage;

/// <summary>
/// MCP 管理工具实现。
/// </summary>
/// <remarks>
/// <see cref="WriteMcpServerAsync"/> / <see cref="DeleteMcpServerAsync"/> 属"有副作用且需用户知情"的操作，
/// 落盘前与 <c>FileSystemToolGroup</c> 一致调用 <see cref="IToolConfirmationService.EvaluateAsync"/>：
/// Default / AcceptEdits 必弹确认，Plan 模式记计划不执行，BypassPermissions 放行。
/// 确认名沿用框架约定"C# 方法名"，与 <see cref="ToolConfirmationService"/> 内置 <c>AlwaysConfirmTools</c> 对齐。
/// <para>
/// <see cref="TestMcpServerAsync"/> 不落盘、不删除，仅试连一次，故不做确认。
/// </para>
/// </remarks>
public class McpManageToolGroup
{
    private readonly MCPRegistry _registry;
    private readonly McpManageToolOptions _options;
    private readonly McpOptions _mcpOptions;
    private readonly IAgentReloadTrigger? _reloadTrigger;
    private readonly IMcpConfigValueResolver _resolver;
    private readonly IToolConfirmationService? _confirmationService;
    private readonly Func<string?>? _defaultWorkspaceRoot;

    /// <summary>
    /// 注入 MCP 注册表、管理工具选项与热重载依赖。
    /// </summary>
    /// <param name="registry">MCP 客户端注册表。</param>
    /// <param name="options">MCP 管理工具的行为选项。</param>
    /// <param name="mcpOptions">MCP 全局选项。</param>
    /// <param name="reloadTrigger">配置写入后的热重载触发器，可为 null。</param>
    /// <param name="resolver">配置值（含凭据引用）解析器，缺省用空实现。</param>
    /// <param name="confirmationService">工具调用确认服务；为 null 表示不做确认（仅供程序化直调，宿主插件必须传入）。</param>
    /// <param name="defaultWorkspaceRoot">工作区根提供者（惰性求值）；工具参数省略 workspaceRoot 时用它兜底。</param>
    public McpManageToolGroup(
        MCPRegistry registry,
        McpManageToolOptions options,
        McpOptions mcpOptions,
        IAgentReloadTrigger? reloadTrigger = null,
        IMcpConfigValueResolver? resolver = null,
        IToolConfirmationService? confirmationService = null,
        Func<string?>? defaultWorkspaceRoot = null)
    {
        _registry = registry;
        _options = options;
        _mcpOptions = mcpOptions;
        _reloadTrigger = reloadTrigger;
        _resolver = resolver ?? NullMcpConfigValueResolver.Instance;
        _confirmationService = confirmationService;
        _defaultWorkspaceRoot = defaultWorkspaceRoot;
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
    /// <param name="name">MCP 服务器名称（即配置文件名）。</param>
    /// <param name="command">stdio 传输的启动命令。</param>
    /// <param name="args">启动命令参数。</param>
    /// <param name="transport">传输方式：stdio / http / sse。</param>
    /// <param name="description">说明。</param>
    /// <param name="overwrite">是否覆盖同名配置；省略等同 false。</param>
    /// <param name="workspaceRoot">工作区根目录；省略时使用宿主当前工作区。</param>
    /// <param name="url">http / sse 的 URL。</param>
    /// <param name="headers">HTTP 头；值支持 <c>$credential:&lt;键&gt;</c>。</param>
    /// <param name="environment">stdio 环境变量；值支持 <c>$credential:&lt;键&gt;</c>。</param>
    /// <returns>写入结果。</returns>
    [Description("创建或覆盖一个工作区级 MCP 服务器配置：写入 <工作区>/.luban-agent/mcps/<name>.json，下一轮对话生效。写入前会请求用户确认。stdio 需 command，http/sse 需 url；敏感值必须用 $credential:<键> 引用，禁止明文令牌。")]
    public async Task<ToolResult<string>> WriteMcpServerAsync(
        [Description("MCP 服务器名称（同时作为配置文件名），不能含路径分隔符或 ..")]
        string name,
        [Description("stdio 传输的启动命令，如 npx；http/sse 传输可省略")]
        string? command = null,
        [Description("启动命令参数，如 [\"-y\",\"@modelcontextprotocol/server-filesystem\",\".\"]")]
        string[]? args = null,
        [Description("传输方式：stdio（默认）/ http / sse")]
        string? transport = null,
        [Description("该 MCP 服务器的用途说明（可选）")]
        string? description = null,
        [Description("同名配置已存在时必须传 true 才会覆盖；首次创建可省略（默认 false）")]
        bool overwrite = false,
        [Description("工作区根目录；省略时使用宿主当前工作区，通常无需传")]
        string? workspaceRoot = null,
        [Description("http / sse 传输的服务器 URL（stdio 传输忽略）")]
        string? url = null,
        [Description("HTTP 头，如 {\"Authorization\":\"Bearer $credential:connector:demo:token\"}；值禁止明文令牌")]
        Dictionary<string, string>? headers = null,
        [Description("stdio 进程环境变量，如 {\"API_KEY\":\"$credential:demo:apiKey\"}；值禁止明文令牌")]
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

        return await WriteConfigAsync(config, overwrite, workspaceRoot, confirmWrite: true).ConfigureAwait(false);
    }

    /// <summary>
    /// 写入 MCP 配置（程序化入口；与工具入口共用校验与落盘逻辑）。
    /// </summary>
    /// <param name="config">配置。</param>
    /// <param name="overwrite">是否允许覆盖同名配置。</param>
    /// <param name="workspaceRoot">工作区根目录；省略时使用宿主当前工作区。</param>
    /// <param name="confirmWrite">是否走工具确认（工具入口为 true；程序化调用默认 false）。</param>
    /// <returns>写入结果（配置文件的绝对路径）。</returns>
    public async Task<ToolResult<string>> WriteConfigAsync(
        McpServerConfig config, bool overwrite, string? workspaceRoot = null, bool confirmWrite = false)
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

        var root = ResolveWorkspaceRoot(workspaceRoot);
        if (root is null)
            return ToolResult.Fail<string>("未获取到工作区根目录，无法写入 MCP 配置。请传入 workspaceRoot，或确认宿主已设置工作区。");

        var mcpsDir = Path.Combine(root, ".luban-agent", "mcps");
        var configFile = Path.Combine(mcpsDir, $"{config.Name}.json");

        if (confirmWrite)
        {
            // 写操作需确认（Default / AcceptEdits 必确认；Plan 记计划；Bypass 放行）
            if (await ConfirmOrBlockAsync(nameof(WriteMcpServerAsync), configFile, new Dictionary<string, object?>
                {
                    ["name"] = config.Name,
                    ["transport"] = config.Transport,
                    ["command"] = config.Command,
                    ["url"] = config.Url,
                    ["overwrite"] = overwrite
                }).ConfigureAwait(false) is { } blocked)
            {
                return blocked;
            }
        }

        try
        {
            Directory.CreateDirectory(mcpsDir);
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(configFile, json).ConfigureAwait(false);

            _registry.LoadFromWorkspace(root);
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
    /// <param name="name">MCP 名称。</param>
    /// <param name="workspaceRoot">工作区根目录；省略时使用宿主当前工作区。</param>
    /// <returns>删除结果。</returns>
    [Description("删除一个工作区级 MCP 服务器配置（<工作区>/.luban-agent/mcps/<name>.json），下一轮对话生效。删除前会请求用户确认；内置 MCP 不可删除。")]
    public async Task<ToolResult<string>> DeleteMcpServerAsync(
        [Description("要删除的 MCP 服务器名称（对应配置文件名）")]
        string name,
        [Description("工作区根目录；省略时使用宿主当前工作区，通常无需传")]
        string? workspaceRoot = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ToolResult.Fail<string>("MCP 名称不能为空。");

        if (_registry.IsBuiltin(name))
            return ToolResult.Fail<string>($"MCP '{name}' 为内置，不可删除。");

        var root = ResolveWorkspaceRoot(workspaceRoot);
        if (root is null)
            return ToolResult.Fail<string>("未获取到工作区根目录，无法删除 MCP 配置。请传入 workspaceRoot，或确认宿主已设置工作区。");

        var configFile = Path.Combine(root, ".luban-agent", "mcps", $"{name}.json");
        if (!File.Exists(configFile))
            return ToolResult.Fail<string>($"MCP '{name}' 配置文件不存在。");

        // 删除操作始终需要确认
        if (await ConfirmOrBlockAsync(nameof(DeleteMcpServerAsync), configFile, new Dictionary<string, object?>
            {
                ["name"] = name
            }).ConfigureAwait(false) is { } blocked)
        {
            return blocked;
        }

        try
        {
            await Task.Run(() => File.Delete(configFile));

            _registry.LoadFromWorkspace(root);
            _reloadTrigger?.RequestReload();

            return ToolResult.Ok(configFile, $"MCP '{name}' 已删除。");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail<string>($"删除 MCP 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 测试 MCP 服务器连接（不持久化、不写配置）。
    /// </summary>
    /// <param name="command">stdio 启动命令。</param>
    /// <param name="args">启动命令参数。</param>
    /// <param name="transport">传输方式：stdio / http / sse。</param>
    /// <param name="timeoutSeconds">连接超时秒数；省略或 ≤0 时用全局 MCP 超时。</param>
    /// <param name="url">http / sse 的 URL。</param>
    /// <param name="headers">HTTP 头；值支持 <c>$credential:&lt;键&gt;</c>。</param>
    /// <param name="environment">stdio 环境变量；值支持 <c>$credential:&lt;键&gt;</c>。</param>
    /// <returns>测试结果。</returns>
    [Description("试连一个 MCP 服务器并返回是否连通、发现多少工具。只做连通性验证，不写入配置、不产生持久副作用，因此无需用户确认。写入配置请改用 WriteMcpServer。")]
    public async Task<ToolResult<McpTestResult>> TestMcpServerAsync(
        [Description("stdio 传输的启动命令，如 npx；http/sse 传输可省略")]
        string? command = null,
        [Description("启动命令参数")]
        string[]? args = null,
        [Description("传输方式：stdio（默认）/ http / sse")]
        string? transport = null,
        [Description("连接超时秒数；省略或 ≤0 时使用全局 MCP 超时配置")]
        int timeoutSeconds = 0,
        [Description("http / sse 传输的服务器 URL（stdio 传输忽略）")]
        string? url = null,
        [Description("HTTP 头；值必须用 $credential:<键> 引用，禁止明文令牌")]
        Dictionary<string, string>? headers = null,
        [Description("stdio 进程环境变量；值必须用 $credential:<键> 引用，禁止明文令牌")]
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
    /// 统一的确认前置检查。返回非 null 表示调用方应立即把该结果返回给 LLM。
    /// </summary>
    /// <param name="toolName">工具名（框架约定用 C# 方法名，与 AlwaysConfirmTools 默认项对齐）。</param>
    /// <param name="path">操作目标路径（用于路径级授权判定）。</param>
    /// <param name="arguments">工具参数（供确认 UI 展示）。</param>
    /// <returns>允许执行时返回 null，否则返回应直接回给 LLM 的结果。</returns>
    private async Task<ToolResult<string>?> ConfirmOrBlockAsync(
        string toolName, string path, Dictionary<string, object?> arguments)
    {
        // 未注入确认服务（程序化直调）时不做确认，保持旧行为
        if (_confirmationService is null)
            return null;

        var outcome = await _confirmationService.EvaluateAsync(toolName, path, arguments)
            .ConfigureAwait(false);

        return outcome switch
        {
            EnumConfirmationOutcome.Allowed => null,
            EnumConfirmationOutcome.Planned => ToolResult.Plan<string>(),
            _ => ToolResult.Denied<string>()
        };
    }

    /// <summary>
    /// 解析工作区根：显式参数优先，其次宿主提供者；统一返回绝对路径。
    /// </summary>
    /// <param name="workspaceRoot">显式给出的工作区根。</param>
    /// <returns>绝对路径；取不到返回 null。</returns>
    private string? ResolveWorkspaceRoot(string? workspaceRoot)
    {
        var root = !string.IsNullOrWhiteSpace(workspaceRoot) ? workspaceRoot : _defaultWorkspaceRoot?.Invoke();
        if (string.IsNullOrWhiteSpace(root))
            return null;

        try
        {
            return Path.GetFullPath(root);
        }
        catch
        {
            return root;
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