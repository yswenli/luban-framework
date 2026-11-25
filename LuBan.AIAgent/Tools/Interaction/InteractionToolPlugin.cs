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
*命名空间：LuBan.AIAgent.Tools.Interaction
*文件名： InteractionToolPlugin
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/11/24
*描述：用户交互工具插件：向用户提出 1～4 个选择题并等待作答
*
*****************************************************************************/
using LuBan.AIAgent.Protocol;

namespace LuBan.AIAgent.Tools.Interaction;

/// <summary>
/// 用户交互工具插件。该组「总是注入」：子代理与编排节点同样需要向用户提问，
/// 而显式工具组列表（角色默认组/配置默认组）不会自动带上本组。
/// </summary>
public class InteractionToolPlugin : ILuBanToolPlugin
{
    /// <inheritdoc />
    public string GroupName => "interaction";

    /// <inheritdoc />
    public string? Description => "用户交互工具：向用户提出选择题并等待作答";

    /// <inheritdoc />
    /// <remarks>
    /// 显式工具组筛选时也强制并入本组，保证子代理/编排节点能向用户提问。
    /// </remarks>
    public bool IsAlwaysInjected => true;

    /// <inheritdoc />
    public IReadOnlyList<AIFunction> GetTools(DiContainer sp, ToolGroupOptions? toolsOptions = null)
    {
        var group = new InteractionToolGroup(sp.GetRequiredService<IUserInteractionService>());
        return
        [
            AIFunctionFactoryHelper.Create(
                group, nameof(InteractionToolGroup.AskUserQuestionAsync), name: "AskUserQuestion")
        ];
    }

    /// <inheritdoc />
    public bool IsEnabled(LuBanAgentOptions options) => true;
}

/// <summary>
/// 用户交互工具组。
/// </summary>
/// <param name="interaction">用户交互服务（DI 注入）。</param>
public class InteractionToolGroup(IUserInteractionService interaction)
{
    private readonly IUserInteractionService _interaction = interaction
        ?? throw new ArgumentNullException(nameof(interaction));

    /// <summary>
    /// 向用户提出 1～4 个问题并等待作答。
    /// </summary>
    /// <param name="questionsJson">
    /// 问题列表 JSON。数组或 <c>{"questions":[...]}</c> 均可；每项形如
    /// <c>{"question":"问题正文","header":"短标题","multiSelect":false,"options":[{"label":"选项","description":"说明"}]}</c>。
    /// </param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>逐题作答结果（JSON）；用户取消或宿主无通道时 outcome 不为 answered。</returns>
    [Description("""
        向用户提问并等待回答，用于信息不足、需要用户拍板或存在多种合理实现时。
        参数 questionsJson 是 JSON：{"questions":[{"question":"问题正文","header":"短标题(≤12字)","multiSelect":false,"options":[{"label":"选项文案","description":"选项说明"}]}]}。
        限制：1～4 个问题；每题 2～4 个选项；问题正文不得重复，同一题的选项文案不得重复（界面会自动追加「其它」自由输入，无需自定义该项）。
        返回 {"outcome":"answered|cancelled|unavailable","answers":[{"question":"...","labels":["..."]}]}。
        用户取消（cancelled）不是拒绝执行，请基于现有信息继续并在回复中说明，不要重复追问同一问题。
        """)]
    public async Task<ToolResult<string>> AskUserQuestionAsync(
        [Description("问题列表 JSON，形如 {\"questions\":[{...}]}")] string questionsJson,
        CancellationToken cancellationToken = default)
    {
        if (!UserQuestionParser.TryParse(questionsJson, out var questions, out var parseError))
            return ToolResult.Fail<string>(parseError!);

        var result = await _interaction.AskAsync(questions, cancellationToken).ConfigureAwait(false);

        var payload = AgentEventProtocol.SerializePayload(new
        {
            outcome = result.Outcome switch
            {
                UserQuestionOutcome.Answered => "answered",
                UserQuestionOutcome.Cancelled => "cancelled",
                _ => "unavailable"
            },
            answers = result.Answers is null
                ? null
                : result.Answers.Select(a => new { question = a.Question, labels = a.Labels }),
            message = result.Message
        });

        return ToolResult.Ok(payload, result.Message);
    }
}

/// <summary>
/// <c>questionsJson</c> 的解析与约束校验。约束与界面的能力上限对齐：
/// 问题 1～4 个、每题选项 2～4 个，避免模型下发界面无法承载的结构。
/// </summary>
internal static class UserQuestionParser
{
    /// <summary>单次提问的问题数上限。</summary>
    public const int MaxQuestions = 4;

    /// <summary>单题的选项数上限。</summary>
    public const int MaxOptions = 4;

    /// <summary>单题的选项数下限。</summary>
    public const int MinOptions = 2;

    /// <summary>
    /// 解析并校验问题列表。
    /// </summary>
    /// <param name="json">模型给出的 JSON（数组或 <c>{"questions":[...]}</c>）。</param>
    /// <param name="questions">解析结果。</param>
    /// <param name="error">校验失败时的中文提示（供模型自我修正）。</param>
    /// <returns>解析成功返回 true。</returns>
    public static bool TryParse(
        string? json, out IReadOnlyList<UserQuestion> questions, out string? error)
    {
        questions = [];
        error = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            error = "参数 questionsJson 不能为空。请传 {\"questions\":[{...}]} 形式的 JSON。";
            return false;
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            error = $"questionsJson 不是合法 JSON：{ex.Message}";
            return false;
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("questions", out var nested))
            {
                root = nested;
            }

            if (root.ValueKind != JsonValueKind.Array)
            {
                error = "questionsJson 必须是数组，或 {\"questions\":[...]} 对象。";
                return false;
            }

            var items = root.EnumerateArray().ToList();
            if (items.Count is < 1 or > MaxQuestions)
            {
                error = $"一次只能提 {MaxQuestions} 个以内的问题（当前 {items.Count} 个）。";
                return false;
            }

            var parsed = new List<UserQuestion>(items.Count);
            // 问题正文是答案回链的唯一键（宿主按正文逐题匹配），重复正文会让答案无法与问题对应
            var seen = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var i = 0; i < items.Count; i++)
            {
                if (!TryParseQuestion(items[i], i, out var question, out error))
                    return false;

                if (seen.TryGetValue(question.Question, out var firstIndex))
                {
                    error = $"第 {i + 1} 个问题与第 {firstIndex + 1} 个问题正文重复，请合并或改写后再提问。";
                    return false;
                }

                seen[question.Question] = i;
                parsed.Add(question);
            }

            questions = parsed;
            return true;
        }
    }

    /// <summary>解析单个问题。</summary>
    private static bool TryParseQuestion(
        JsonElement item, int index, out UserQuestion question, out string? error)
    {
        question = null!;
        error = null;
        var position = index + 1;

        if (item.ValueKind != JsonValueKind.Object)
        {
            error = $"第 {position} 个问题必须是对象。";
            return false;
        }

        var text = ReadString(item, "question");
        if (string.IsNullOrWhiteSpace(text))
        {
            error = $"第 {position} 个问题缺少 question 正文。";
            return false;
        }

        if (!item.TryGetProperty("options", out var optionsElement)
            || optionsElement.ValueKind != JsonValueKind.Array)
        {
            error = $"第 {position} 个问题缺少 options 数组。";
            return false;
        }

        var optionItems = optionsElement.EnumerateArray().ToList();
        if (optionItems.Count is < MinOptions or > MaxOptions)
        {
            error = $"第 {position} 个问题需要 {MinOptions}～{MaxOptions} 个选项（当前 {optionItems.Count} 个）。";
            return false;
        }

        var options = new List<UserQuestionOption>(optionItems.Count);
        foreach (var optionItem in optionItems)
        {
            if (optionItem.ValueKind != JsonValueKind.Object)
            {
                error = $"第 {position} 个问题的选项必须是对象（含 label）。";
                return false;
            }

            var label = ReadString(optionItem, "label");
            if (string.IsNullOrWhiteSpace(label))
            {
                error = $"第 {position} 个问题存在缺少 label 的选项。";
                return false;
            }

            // 界面按文案判选中（回传的也是文案），同一题内重复会让各端「点一个亮两个」
            var trimmed = label.Trim();
            if (options.Any(o => string.Equals(o.Label, trimmed, StringComparison.Ordinal)))
            {
                error = $"第 {position} 个问题存在重复选项「{trimmed}」，请改成可区分的文案。";
                return false;
            }

            options.Add(new UserQuestionOption(trimmed, ReadString(optionItem, "description")));
        }

        var multiSelect = item.TryGetProperty("multiSelect", out var multi)
            && multi.ValueKind == JsonValueKind.True;

        question = new UserQuestion(text.Trim(), options, ReadString(item, "header"), multiSelect);
        return true;
    }

    /// <summary>读取字符串属性；缺失或类型不符时返回 null。</summary>
    private static string? ReadString(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}