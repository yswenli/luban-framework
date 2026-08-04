/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
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
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Orchestration.Models;

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
    /// <param name="chatClient">聊天客户端。</param>
    /// <param name="serviceProvider">服务提供者。</param>
    /// <param name="options">配置选项。</param>
    public LlmTaskPlanner(
        IChatClient chatClient,
        IServiceProvider serviceProvider,
        IOptions<LuBanAgentOptions> options)
    {
        _chatClient = chatClient;
        _serviceProvider = serviceProvider;
        _options = options;
    }

    /// <inheritdoc/>
    public async Task<TaskGraph?> PlanAsync(string task, CancellationToken ct = default)
    {
        var orchestrationOpts = _options.Value.Orchestration ?? new();
        Exception? lastError = null;
        string? lastBadResponse = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var prompt = attempt == 0
                    ? BuildPlannerPrompt(task, GetAvailableToolGroups())
                    : BuildRetryPrompt(task, lastBadResponse!, lastError!);

                var response = await _chatClient.GetResponseAsync(
                    new[] { new ChatMessage(ChatRole.System, prompt) },
                    null,
                    ct);

                var json = response.Messages.Last().Text ?? "";
                if (string.IsNullOrWhiteSpace(json))
                    throw new TaskPlanningException("LLM 返回空内容");

                var graph = JsonSerializer.Deserialize<TaskGraph>(json, JsonOpts);

                if (graph == null || graph.Nodes.Count == 0)
                    throw new TaskPlanningException("LLM 返回空图谱");

                if (graph.Nodes.Count > orchestrationOpts.MaxNodes)
                {
                    Logger.Warn($"LLM 拆解出 {graph.Nodes.Count} 个节点，超过上限 {orchestrationOpts.MaxNodes}，已截断");
                    graph.Nodes = graph.Nodes.Take(orchestrationOpts.MaxNodes).ToList();
                }

                graph.OriginalTask = task;
                graph.Source = "llm";

                if (!graph.Validate(out var errors))
                    throw new TaskPlanningException("DAG 校验失败", errors);

                return graph;
            }
            catch (JsonException ex)
            {
                lastError = ex;
                lastBadResponse = "JSON 解析失败";
                Logger.Warn($"LLM 规划第 {attempt + 1} 次尝试 JSON 解析失败: {ex.Message}");
            }
            catch (TaskPlanningException ex)
            {
                lastError = ex;
                lastBadResponse = string.Join("; ", ex.ValidationErrors);
                Logger.Warn($"LLM 规划第 {attempt + 1} 次尝试 DAG 校验失败: {lastBadResponse}");
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

        var response = await _chatClient.GetResponseAsync(
            new[] { new ChatMessage(ChatRole.System, prompt) },
            null,
            ct);

        var json = response.Messages.Last().Text ?? "";
        if (string.IsNullOrWhiteSpace(json))
            throw new TaskPlanningException("LLM 反思返回空内容");

        return ParseReflectionResponse(json, context);
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
            ShouldRetry = root.TryGetProperty("should_retry", out var r) && r.GetBoolean(),
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
                    IsCritical = nodeEl.TryGetProperty("isCritical", out var crit) && crit.GetBoolean()
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
    /// 构建规划提示词。
    /// </summary>
    /// <param name="task">用户任务。</param>
    /// <param name="tools">可用工具组列表。</param>
    /// <returns>提示词字符串。</returns>
    private static string BuildPlannerPrompt(string task, List<string> tools)
    {
        return $@"你是任务规划专家。将用户的复合任务拆解为 DAG 任务图谱。

## 输出格式（严格 JSON）
{{
  ""nodes"": [
    {{
      ""id"": ""唯一标识（如 research/analyze/execute）"",
      ""description"": ""节点用途描述"",
      ""prompt"": ""执行 prompt，可使用 {{dep:节点id}} 引用前驱输出"",
      ""dependencies"": [""依赖的节点id""],
      ""toolGroups"": [""web"" | ""filesystem"" | null],
      ""isCritical"": true | false
    }}
  ]
}}

## 可用工具组
{string.Join(", ", tools)}

## 拆解原则
1. 每个节点应是独立的、可验证的子任务
2. 无依赖的节点不要强行添加依赖
3. 节点数量控制在 2-8 个
4. 终点节点应产出最终交付物
5. 使用 {{dep:id}} 占位符让后继节点引用前驱输出

## 用户任务
{task}";
    }

    /// <summary>
    /// 构建重试提示词。
    /// </summary>
    /// <param name="task">用户任务。</param>
    /// <param name="lastBadResponse">上次失败的响应。</param>
    /// <param name="lastError">上次错误异常。</param>
    /// <returns>重试提示词字符串。</returns>
    private static string BuildRetryPrompt(string task, string lastBadResponse, Exception lastError)
        => $"上次规划失败：{lastBadResponse}\n错误：{lastError.Message}\n\n请严格按 JSON schema 重新输出：\n{task}";

    /// <summary>
    /// 获取所有已启用的工具组名称。
    /// </summary>
    /// <returns>工具组名称列表。</returns>
    private List<string> GetAvailableToolGroups()
        => _serviceProvider.GetRequiredService<ToolPluginRegistry>()
            .GetEnabledPlugins().Select(p => p.GroupName).ToList();

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
