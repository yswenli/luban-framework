using LuBan.AIAgent.Attachments;

namespace LuBan.AIAgent.Hosting;

/// <summary>
/// 附件路径归一：把宿主 UI 传入的附件路径与正文中的 <c>@引用</c> 合并为最终附件列表，
/// 并返回剥离引用后的正文。解析规则由 <see cref="IAgentAttachmentOrchestrator.ExtractReferences"/> 提供，
/// 因此 CLI / Codex / A2A / Web 等所有宿主行为一致。
/// </summary>
public static class AgentAttachmentResolver
{
    /// <summary>
    /// 归一本次请求：UI 路径优先、去重（大小写不敏感）、保持出现顺序。
    /// </summary>
    /// <param name="request">原始请求。</param>
    /// <param name="orchestrator">附件编排（提供 <c>@引用</c> 解析）。</param>
    /// <returns>剥离引用后的正文与合并去重后的路径列表。</returns>
    public static (string Text, IReadOnlyList<string> Paths) Resolve(
        AgentRunRequest request,
        IAgentAttachmentOrchestrator orchestrator)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(orchestrator);

        var (text, referenced) = orchestrator.ExtractReferences(request.Input);
        return (text ?? string.Empty, Merge(request.AttachmentPaths, referenced));
    }

    /// <summary>合并两路路径：前者优先，空项丢弃，按归一化绝对路径去重。</summary>
    private static IReadOnlyList<string> Merge(
        IReadOnlyList<string>? uiPaths,
        IReadOnlyList<string>? referenced)
    {
        var result = new List<string>((uiPaths?.Count ?? 0) + (referenced?.Count ?? 0));
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 即使只有 UI 路径也要归一与去重：宿主的路径可能带空白、相对路径或大小写重复
        Append(uiPaths);
        Append(referenced);
        return result;

        void Append(IReadOnlyList<string>? paths)
        {
            if (paths is null) return;
            foreach (var path in paths)
            {
                var normalized = Normalize(path);
                if (normalized.Length == 0) continue;
                if (seen.Add(normalized))
                    result.Add(normalized);
            }
        }
    }

    /// <summary>路径归一：去空白并转绝对路径；无法归一（非法字符等）时退回原文本。</summary>
    private static string Normalize(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var trimmed = path.Trim();
        try { return Path.GetFullPath(trimmed); }
        catch { return trimmed; }
    }
}