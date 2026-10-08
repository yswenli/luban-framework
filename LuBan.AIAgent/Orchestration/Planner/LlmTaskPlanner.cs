/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Orchestration.Planner
*文件名： LlmTaskPlanner
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：基于 LLM 的任务规划器，通过提示词引导模型生成 DAG 任务图谱
*
*****************************************************************************/
namespace LuBan.AIAgent.Orchestration.Planner;

/// <summary>
/// 基于 LLM 的任务规划器，通过提示词引导模型生成 DAG 任务图谱。
/// </summary>
public class LlmTaskPlanner : ITaskPlanner
{
    private const int MaxRetries = 1;
    private readonly IChatClient _chatClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<LuBanAgentOptions> _options;

    /// <summary>
    /// 创建 LlmTaskPlanner 实例。
    /// </summary>
    /// <param name="chatClient">聊天客户端（默认模型，作为未配置 PlannerModel 或路由失败时的回退）。</param>
    /// <param name="serviceProvider">服务提供者。</param>
    /// <param name="options">配置选项。</param>
    /// <param name="providerRouter">模型提供者路由（可选，配置 PlannerModel 后生效）。</param>
    public LlmTaskPlanner(
        IChatClient chatClient,
        IServiceProvider serviceProvider,
        IOptions<LuBanAgentOptions> options,
        IProviderRouter? providerRouter = null)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _chatClient = new TimingChatClient(
            ChatClientResilience.Wrap(
                ResolvePlannerClient(chatClient, providerRouter, options.Value.Orchestration?.PlannerModel),
                options.Value),
            "planner");
    }

    /// <summary>
    /// 按 PlannerModel 配置解析规划器使用的聊天客户端，路由失败回退注入客户端。
    /// </summary>
    /// <param name="fallback">注入的默认客户端。</param>
    /// <param name="router">模型提供者路由。</param>
    /// <param name="plannerModel">规划器模型（格式 "provider:model"）。</param>
    /// <returns>聊天客户端实例。</returns>
    private static IChatClient ResolvePlannerClient(IChatClient fallback, IProviderRouter? router, string? plannerModel)
    {
        if (string.IsNullOrEmpty(plannerModel) || router == null)
            return fallback;
        try
        {
            return router.CreateChatClient(plannerModel);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Planner 模型 '{plannerModel}' 路由失败（{ex.Message}），回退默认模型");
            return fallback;
        }
    }

    /// <summary>
    /// 以流式方式请求模型并聚合完整文本。
    /// 规划/反思响应通常耗时较长，非流式请求会把整段响应纳入单次网络超时（默认 60s）约束，
    /// 流式请求下超时仅约束单次读取，可避免长响应被整体判超时。
    /// 另：推理模型（如 glm-5）默认开启思考，会在规划阶段输出大量 reasoning 内容，
    /// 使单次规划耗时数十秒；故按 <see cref="Configuration.OrchestrationOptions.PlannerReasoningEffort"/>
    /// 传递推理强度（默认 <see langword="null"/> 表示不传该参数，推理模型建议显式设为 None 关闭思考）。
    /// </summary>
    /// <param name="prompt">系统提示词。</param>
    /// <param name="ct">取消标记。</param>
    /// <returns>模型返回的完整文本，以及单次往返的分段计时（TTFT / 总耗时）。</returns>
    private async Task<(string Text, CompletionTiming Timing)> GetCompletionAsync(string prompt, CancellationToken ct)
    {
        var effort = _options.Value.Orchestration?.PlannerReasoningEffort;
        var options = effort is null
            ? null
            : new ChatOptions { Reasoning = new ReasoningOptions { Effort = effort } };
        var effortLabel = effort?.ToString() ?? "unset";
        var sendLabel = options is null ? "not-sent" : "sent";
        Logger.Info($"[OrchDiag] planner reasoning effort={effortLabel} option={sendLabel}");

        var sb = new StringBuilder();
        var sw = Stopwatch.StartNew();
        long ttftMs = 0;
        var firstText = true;
        await foreach (var update in _chatClient
            .GetStreamingResponseAsync(new[] { new ChatMessage(ChatRole.System, prompt) }, options, ct)
            .ConfigureAwait(false))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                if (firstText)
                {
                    ttftMs = sw.ElapsedMilliseconds;
                    firstText = false;
                }
                sb.Append(update.Text);
            }
        }
        sw.Stop();

        return (sb.ToString(), new CompletionTiming(ttftMs, sw.ElapsedMilliseconds));
    }

    /// <summary>
    /// 将自然语言任务转换为 TaskGraph
    /// </summary>
    /// <param name="task"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="TaskPlanningException"></exception>

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "PlanAsync uses JSON deserialization via ToObject extension which requires reflection. This planner is only invoked at runtime when orchestration is enabled, not during AOT compilation.")]
    public async Task<TaskGraph?> PlanAsync(string task, CancellationToken ct = default)
    {
        var orchestrationOpts = _options.Value.Orchestration ?? new();
        Exception? lastError = null;
        string? lastBadResponse = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            var promptSw = Stopwatch.StartNew();
            var prompt = attempt == 0
                ? BuildPlannerPrompt(task, GetAvailableToolGroups())
                : BuildRetryPrompt(task, lastBadResponse!, lastError!);
            promptSw.Stop();

            long ttftMs = 0;
            long totalMs = 0;
            string? raw = null;
            var parseSw = Stopwatch.StartNew();
            try
            {
                var (json, timing) = await GetCompletionAsync(prompt, ct);
                ttftMs = timing.TtftMs;
                totalMs = timing.TotalMs;
                raw = json;

                if (string.IsNullOrWhiteSpace(json))
                    throw new TaskPlanningException("LLM 返回空内容");

                Logger.Info($"[OrchDiag] planner raw json (attempt={attempt + 1}): {Truncate(json, 2000)}");

                var graph = JsonSerializer.Deserialize<TaskGraph>(ExtractJson(json), PlannerJsonOptions);

                if (graph == null || graph.Nodes.Count == 0)
                    throw new TaskPlanningException("LLM 返回空图谱");

                if (graph.Nodes.Count > orchestrationOpts.MaxNodes)
                {
                    Logger.Warn($"LLM 拆解出 {graph.Nodes.Count} 个节点，超过上限 {orchestrationOpts.MaxNodes}，已截断");
                    graph.Nodes = graph.Nodes.Take(orchestrationOpts.MaxNodes).ToList();
                }

                graph.OriginalTask = task;
                graph.Source = "llm";
                graph.Parallelism = ClampParallelism(graph.Parallelism, orchestrationOpts);

                if (!graph.Validate(out var errors))
                    throw new TaskPlanningException("DAG 校验失败", errors);

                parseSw.Stop();
                Logger.Info($"[OrchDiag] planner attempt={attempt + 1} promptMs={promptSw.ElapsedMilliseconds} ttftMs={ttftMs} totalMs={totalMs} parseMs={parseSw.ElapsedMilliseconds} nodeCount={graph.Nodes.Count} ok=true");
                return graph;
            }
            catch (JsonException ex)
            {
                parseSw.Stop();
                lastError = ex;
                lastBadResponse = Truncate(raw, 1500);
                Logger.Warn($"LLM 规划第 {attempt + 1} 次尝试 JSON 解析失败: {ex.Message}");
                Logger.Info($"[OrchDiag] planner attempt={attempt + 1} promptMs={promptSw.ElapsedMilliseconds} ttftMs={ttftMs} totalMs={totalMs} parseMs={parseSw.ElapsedMilliseconds} ok=false reason=json");
            }
            catch (TaskPlanningException ex)
            {
                parseSw.Stop();
                lastError = ex;
                lastBadResponse = ex.ValidationErrors.Count > 0
                    ? string.Join("; ", ex.ValidationErrors)
                    : Truncate(raw, 1500);
                Logger.Warn($"LLM 规划第 {attempt + 1} 次尝试 DAG 校验失败: {lastBadResponse}");
                Logger.Info($"[OrchDiag] planner attempt={attempt + 1} promptMs={promptSw.ElapsedMilliseconds} ttftMs={ttftMs} totalMs={totalMs} parseMs={parseSw.ElapsedMilliseconds} ok=false reason={ex.Message}");
            }
        }

        throw new TaskPlanningException(
            $"LLM 规划失败，已重试 {MaxRetries} 次",
            lastError is TaskPlanningException tpe ? tpe.ValidationErrors : new());
    }

    /// <inheritdoc/>
    public async Task<ReflectionResult> ReflectAsync(ReplanContext context, CancellationToken ct = default)
    {
        var prompt = BuildReflectionPrompt(context);

        var (json, timing) = await GetCompletionAsync(prompt, ct);
        if (string.IsNullOrWhiteSpace(json))
            throw new TaskPlanningException("LLM 反思返回空内容");

        Logger.Info($"[OrchDiag] reflector ttftMs={timing.TtftMs} totalMs={timing.TotalMs}");
        return ParseReflectionResponse(ExtractJson(json), context);
    }

    /// <summary>
    /// 构建反思提示词。
    /// </summary>
    /// <param name="context">反思上下文。</param>
    /// <returns>提示词字符串。</returns>
    private static string BuildReflectionPrompt(ReplanContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("你是任务失败分析专家。分析失败的节点及其依赖，决定是否需要重新规划。");
        sb.AppendLine();
        sb.AppendLine($"## 用户任务");
        sb.AppendLine(context.UserGoal);
        sb.AppendLine();
        sb.AppendLine($"## 当前尝试次数：{context.Attempt}");
        sb.AppendLine();
        sb.AppendLine("## 失败节点");

        foreach (var failed in context.FailedNodes)
        {
            sb.AppendLine($"### 节点: {failed.NodeId}");
            sb.AppendLine($"- 描述: {failed.Description}");
            sb.AppendLine($"- 错误: {failed.Error}");
            if (!string.IsNullOrEmpty(failed.Output))
                sb.AppendLine($"- 输出: {failed.Output}");

            if (failed.DependencyOutputs.Count > 0)
            {
                sb.AppendLine("- 依赖节点输出:");
                foreach (var (depId, depOutput) in failed.DependencyOutputs)
                {
                    var preview = depOutput.Length > 200 ? depOutput[..200] + "..." : depOutput;
                    sb.AppendLine($"  - {depId}: {preview}");
                }
            }
            sb.AppendLine();
        }

        sb.AppendLine("## 输出格式（严格 JSON）");
        sb.AppendLine(@"{
          ""analysis"": ""失败原因分析"",
          ""fix_approach"": ""修复方案"",
          ""should_retry"": true,
          ""new_nodes"": [
            {
              ""id"": ""fix_1_step1"",
              ""description"": ""节点用途"",
              ""prompt"": ""执行 prompt，可使用 {dep:节点id} 引用前驱输出"",
              ""dependencies"": [""依赖的节点id""],
              ""toolGroups"": [""web""],
              ""isCritical"": true
            }
          ]
        }");
        sb.AppendLine();
        sb.AppendLine("请分析失败原因，决定是否重试，并生成修正节点（如果需要）。");

        return sb.ToString();
    }

    /// <summary>
    /// 解析 LLM 反思响应。
    /// </summary>
    /// <param name="json">LLM 返回的 JSON 字符串。</param>
    /// <param name="context">反思上下文。</param>
    /// <returns>解析后的反思结果。</returns>
    private static ReflectionResult ParseReflectionResponse(string json, ReplanContext context)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var result = new ReflectionResult
        {
            Analysis = root.TryGetProperty("analysis", out var a) ? a.GetString() ?? "" : "",
            FixApproach = root.TryGetProperty("fix_approach", out var f) ? f.GetString() ?? "" : "",
            ShouldRetry = TryGetBool(root, "should_retry"),
            FailedNodeIds = context.FailedNodes.Select(n => n.NodeId).ToList()
        };

        if (root.TryGetProperty("new_nodes", out var nodesEl) && nodesEl.ValueKind == JsonValueKind.Array)
        {
            var nodes = new List<TaskNode>();
            foreach (var nodeEl in nodesEl.EnumerateArray())
            {
                var node = new TaskNode
                {
                    Id = nodeEl.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "",
                    Description = nodeEl.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                    Prompt = nodeEl.TryGetProperty("prompt", out var prompt) ? prompt.GetString() ?? "" : "",
                    Role = nodeEl.TryGetProperty("role", out var role) ? role.GetString() : null,
                    IsCritical = TryGetBool(nodeEl, "isCritical")
                };

                if (nodeEl.TryGetProperty("dependencies", out var deps) && deps.ValueKind == JsonValueKind.Array)
                {
                    node.Dependencies = deps.EnumerateArray()
                        .Select(d => d.GetString() ?? "")
                        .Where(d => !string.IsNullOrEmpty(d))
                        .ToList();
                }

                if (nodeEl.TryGetProperty("toolGroups", out var tg) && tg.ValueKind == JsonValueKind.Array)
                {
                    node.ToolGroups = tg.EnumerateArray()
                        .Select(t => t.GetString() ?? "")
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();
                }

                nodes.Add(node);
            }
            result.NewNodes = nodes;
        }

        return result;
    }

    /// <summary>
    /// 安全解析布尔值，支持布尔类型和字符串类型。
    /// </summary>
    private static bool TryGetBool(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop))
            return false;

        return prop.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(prop.GetString(), out var b) && b,
            _ => false
        };
    }

    /// <summary>
    /// 将 LLM 产出的图级并行度 clamp 到合法区间。null 原样返回（表示沿用配置默认）。
    /// 硬上限 = <see cref="OrchestrationOptions.MaxParallelism"/>；若其为 0（不限），
    /// 则只 clamp 到 <c>[1, MaxNodes]</c>，避免 LLM 给出荒谬值。
    /// </summary>
    /// <param name="parallelism">LLM 解析出的并行度，可为 null。</param>
    /// <param name="options">编排配置。</param>
    /// <returns>clamp 后的并行度，或 null。</returns>
    internal static int? ClampParallelism(int? parallelism, OrchestrationOptions options)
    {
        if (parallelism is null)
            return null;

        var hardLimit = options.MaxParallelism > 0 ? options.MaxParallelism : options.MaxNodes;
        return Math.Clamp(parallelism.Value, 1, Math.Max(1, hardLimit));
    }

    /// <summary>
    /// 构建规划提示词。
    /// </summary>
    /// <param name="task">用户任务。</param>
    /// <param name="tools">可用工具组列表。</param>
    /// <returns>提示词字符串。</returns>
    private string BuildPlannerPrompt(string task, List<string> tools)
    {
        var availableTools = tools.Where(t => !string.Equals(t, "orchestration", StringComparison.OrdinalIgnoreCase)).ToList();
        var orchestration = _options.Value.Orchestration ?? new();
        var maxNodes = orchestration.MaxNodes;
        var maxParallelismHint = orchestration.MaxParallelism > 0 ? orchestration.MaxParallelism : maxNodes;
        var roleTable = BuildRoleTable();
        return $@"你是任务规划专家。将用户的复合任务拆解为 DAG 任务图谱。

## 输出格式（严格 JSON）
{{
  ""parallelism"": 1,
  ""nodes"": [
    {{
      ""id"": ""唯一标识（如 research/analyze/execute）"",
      ""description"": ""节点用途描述"",
      ""prompt"": ""执行 prompt，可使用 {{dep:节点id}} 引用前驱输出"",
      ""role"": ""角色名或 null"",
      ""dependencies"": [""依赖的节点id""],
      ""toolGroups"": [""web"" | ""filesystem"" | ""script"" | ""retrieval"" | ""localmemory"" | ""browser"" | null],
      ""isCritical"": true | false
    }}
  ]
}}

## 可用角色
{roleTable}

## 可用工具组
{string.Join(", ", availableTools)}

## 拆解原则
1. 每个节点应是独立的、可验证的子任务
2. 无依赖的节点不要强行添加依赖
3. 节点数量控制在 1-{maxNodes} 个（1 个表示普通任务，多个表示复合任务）
4. 终点节点应产出最终交付物
5. 使用 {{dep:id}} 占位符让后继节点引用前驱输出
6. 为每个节点选择合适的角色（role），若不确定可设为 null
7. toolGroups 可省略（使用角色默认工具组）或显式指定（覆盖角色默认值）；若未指定 role，则 toolGroups 必须显式指定
8. parallelism 为图级字段，表示同时执行的节点数上限，取值 1-{maxParallelismHint}；不确定时设为 1

## 用户任务
{task}";
    }

    /// <summary>
    /// 从 <see cref="SubAgentRoleRegistry"/> 动态生成可用角色表，保证提示词与运行时角色注册保持一致。
    /// 注册表不可用时返回空说明，避免规划直接失败。
    /// </summary>
    /// <returns>角色表文本（每行一个角色）。</returns>
    private string BuildRoleTable()
    {
        var roles = _serviceProvider.GetService<SubAgentRoleRegistry>()?.GetAllRoles();
        if (roles is null || roles.Count == 0)
            return "(无预置角色，请将 role 设为 null 并显式指定 toolGroups)";

        var sb = new StringBuilder();
        foreach (var role in roles)
        {
            var groups = role.DefaultToolGroups is { Count: > 0 }
                ? string.Join(", ", role.DefaultToolGroups)
                : "";
            sb.AppendLine($"- {role.Name}: 默认工具组 [{groups}]");
        }
        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// 构建重试提示词。
    /// </summary>
    /// <param name="task">用户任务。</param>
    /// <param name="lastBadResponse">上次失败的响应。</param>
    /// <param name="lastError">上次错误异常。</param>
    /// <returns>重试提示词字符串。</returns>
    private string BuildRetryPrompt(string task, string lastBadResponse, Exception lastError)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## 上次规划失败");
        sb.AppendLine($"错误：{lastError.Message}");
        sb.AppendLine();
        sb.AppendLine("## 上次的原始输出（可能已截断）");
        sb.AppendLine(string.IsNullOrWhiteSpace(lastBadResponse) ? "(无)" : lastBadResponse);
        sb.AppendLine();
        sb.AppendLine("请严格修正为符合下列 schema 的 JSON，只输出 JSON 本体，不要输出解释文字或 Markdown 代码围栏。");
        sb.AppendLine();
        sb.AppendLine(BuildPlannerPrompt(task, GetAvailableToolGroups()));
        return sb.ToString();
    }

    /// <summary>
    /// 获取所有已启用的工具组名称。
    /// </summary>
    /// <returns>工具组名称列表。</returns>
    private List<string> GetAvailableToolGroups()
        => _serviceProvider.GetRequiredService<ToolPluginRegistry>()
            .GetPlugins(null).Select(p => p.GroupName).ToList();

    /// <summary>
    /// 解析规划/反思响应时的 JSON 选项：属性名大小写不敏感。
    /// </summary>
    private static readonly JsonSerializerOptions PlannerJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// 从 LLM 原始输出中提取 JSON 主体：优先取 Markdown 代码围栏内的内容，
    /// 否则截取首个括号配平的 <c>{...}</c> 块，容忍前后散文与围栏包裹。
    /// </summary>
    /// <param name="text">LLM 原始输出。</param>
    /// <returns>用于反序列化的 JSON 子串；无法识别时原样返回。</returns>
    private static string ExtractJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var t = text.Trim();

        var fence = Regex.Match(t, @"```[ \t]*(?:json)?[ \t]*(?<body>[\s\S]*?)```", RegexOptions.IgnoreCase);
        if (fence.Success)
            t = fence.Groups["body"].Value.Trim();

        if (t.StartsWith('{') && t.EndsWith('}'))
            return t;

        var start = t.IndexOf('{');
        if (start < 0)
            return t;

        var depth = 0;
        var inString = false;
        var escaped = false;
        for (var i = start; i < t.Length; i++)
        {
            var c = t[i];
            if (inString)
            {
                if (escaped) escaped = false;
                else if (c == '\\') escaped = true;
                else if (c == '"') inString = false;
                continue;
            }

            if (c == '"') inString = true;
            else if (c == '{') depth++;
            else if (c == '}' && --depth == 0) return t.Substring(start, i - start + 1);
        }

        return t;
    }

    /// <summary>
    /// 截断长文本，用于日志与重试提示词，避免整段响应污染诊断信息。
    /// </summary>
    /// <param name="text">原始文本。</param>
    /// <param name="max">保留的最大字符数。</param>
    /// <returns>截断后的文本。</returns>
    private static string Truncate(string? text, int max)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= max)
            return text ?? "";
        return $"{text[..max]}...(已截断，总长 {text.Length})";
    }
}

/// <summary>
/// 规划/反思单次 LLM 往返的分段计时（毫秒）：TTFT 与总耗时。
/// </summary>
/// <param name="TtftMs">首字延迟（首个非空文本增量到达耗时）。</param>
/// <param name="TotalMs">整段响应总耗时。</param>
internal readonly record struct CompletionTiming(long TtftMs, long TotalMs);
