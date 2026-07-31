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
