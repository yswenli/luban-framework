using System.Text;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.LocalMemory;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Rules.BuiltIn;

/// <summary>
/// 记忆自动召回规则：每轮对话前（context-build）检索相关长期记忆并注入上下文
/// </summary>
public class MemoryRecallRule : RuleBase
{
    private readonly ILocalMemoryService? _memory;
    private readonly IOptions<LuBanAgentOptions> _options;

    /// <summary>
    /// 创建记忆召回规则
    /// </summary>
    public MemoryRecallRule(ILocalMemoryService? memory, IOptions<LuBanAgentOptions> options)
    {
        _memory = memory;
        _options = options;
    }

    /// <summary>
    /// 规则 ID
    /// </summary>
    public override string Id => "memory-recall";

    /// <summary>
    /// 规则名称
    /// </summary>
    public override string Name => "记忆自动召回";

    /// <summary>
    /// 规则描述
    /// </summary>
    public override string Description => "每轮对话前自动检索相关长期记忆并注入上下文";

    /// <summary>
    /// 规则优先级
    /// </summary>
    public override int Priority => 50;

    /// <inheritdoc />
    public override bool IsApplicable(RuleContext context) => context.ActionType == "context-build";

    /// <inheritdoc />
    public override async Task<RuleResult> ExecuteAsync(RuleContext context)
    {
        var result = RuleResult.AllowResult();

        if (_memory == null || !_options.Value.Tools.LocalMemory.RecallEnabled)
            return result;

        var query = context.UserInput;
        if (string.IsNullOrWhiteSpace(query))
            return result;

        try
        {
            var memOpts = _options.Value.Tools.LocalMemory;
            var found = await _memory.SearchAsync(query, topK: memOpts.RecallTopK);
            var relevant = found.Where(r => r.Score >= memOpts.RecallMinScore).ToList();
            if (relevant.Count == 0)
                return result;

            var sb = new StringBuilder();
            sb.AppendLine("[记忆上下文]");
            foreach (var r in relevant)
                sb.AppendLine($"- {r.Content}（类别: {r.Category}，时间: {r.CreatedAt:yyyy-MM-dd}）");
            result.Inject.Add(sb.ToString().TrimEnd());
        }
        catch (Exception ex)
        {
            Logger.Error("记忆自动召回失败", ex, query);
        }
        return result;
    }
}
