/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools
*文件名： ManageToolSupport
*版本号： V1.0.0.0
*唯一标识：3d5b1f7a-8c42-4a3e-9f61-2b7c0d9a4e58
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：Skill/MCP 管理工具的公共辅助（工作区根解析、写删确认前置检查）
*
*****************************************************************************/

namespace LuBan.AIAgent.Tools;

/// <summary>
/// Skill / MCP 管理工具共用的辅助方法。
/// </summary>
/// <remarks>
/// 两类管理工具的"确认前置检查"与"工作区根解析"语义完全一致，抽到此处避免双份实现漂移：
/// <list type="bullet">
/// <item>确认名统一用 C# 方法名（与 <see cref="ToolConfirmationService"/> 内置
/// <c>AlwaysConfirmTools</c> 默认项对齐）；</item>
/// <item>工作区根显式参数优先、宿主提供者兜底，相对路径一律锚定到宿主工作区。</item>
/// </list>
/// </remarks>
internal static class ManageToolSupport
{
    /// <summary>
    /// 统一的确认前置检查。返回非 null 表示调用方应立即把该结果返回给 LLM：
    /// Plan 模式下返回"已记录计划、未执行"，被拒绝时返回"用户取消"，两者语义不可混用。
    /// </summary>
    /// <param name="confirmationService">确认服务；为 null（程序化直调）时不做确认，保持旧行为。</param>
    /// <param name="toolName">工具名（框架约定用 C# 方法名，与 AlwaysConfirmTools 默认项对齐）。</param>
    /// <param name="path">操作目标路径（用于路径级授权判定）。</param>
    /// <param name="arguments">工具参数（供确认 UI 展示）。</param>
    /// <returns>允许执行时返回 null，否则返回应直接回给 LLM 的结果。</returns>
    public static async Task<ToolResult<string>?> ConfirmOrBlockAsync(
        IToolConfirmationService? confirmationService,
        string toolName,
        string path,
        Dictionary<string, object?> arguments)
    {
        // 未注入确认服务（程序化直调）时不做确认，保持旧行为
        if (confirmationService is null)
            return null;

        var outcome = await confirmationService.EvaluateAsync(toolName, path, arguments)
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
    /// <remarks>
    /// 模型给的<b>相对路径</b>（如 <c>my-project</c>）先锚定到宿主工作区再取绝对路径，
    /// 避免按进程当前目录解析（GUI 宿主常为 bin 输出目录）而写到工作区之外。
    /// 绝对路径原样透传。
    /// </remarks>
    /// <param name="workspaceRoot">模型/调用方显式给出的工作区根。</param>
    /// <param name="defaultWorkspaceRoot">宿主工作区根提供者（惰性求值）。</param>
    /// <returns>绝对路径；取不到返回 null。</returns>
    public static string? ResolveWorkspaceRoot(string? workspaceRoot, Func<string?>? defaultWorkspaceRoot)
    {
        var explicitRoot = string.IsNullOrWhiteSpace(workspaceRoot) ? null : workspaceRoot!.Trim();
        var fallbackRoot = defaultWorkspaceRoot?.Invoke();

        string? root;
        if (explicitRoot is null)
        {
            root = fallbackRoot;
        }
        else if (!Path.IsPathFullyQualified(explicitRoot) && !string.IsNullOrWhiteSpace(fallbackRoot))
        {
            // 相对路径锚定到宿主工作区（不能用 Path.IsPathRooted：Windows 上 "/x" 也算 rooted）
            root = Path.Combine(fallbackRoot, explicitRoot);
        }
        else
        {
            root = explicitRoot;
        }

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
    /// 生成正文摘要（首若干字符 + 总长度），供确认弹窗展示；
    /// 正文往往很长，直接塞进参数会被 <see cref="IToolConfirmationService.FormatArguments"/> 截断掉其他字段。
    /// </summary>
    /// <param name="content">原始正文。</param>
    /// <param name="previewLength">摘要保留的字符数。</param>
    /// <returns>单行摘要文本。</returns>
    public static string BuildContentSummary(string? content, int previewLength = 120)
    {
        if (string.IsNullOrEmpty(content))
            return "（空）";

        var flattened = content.Replace("\r", " ").Replace("\n", " ").Trim();
        var preview = flattened.Length <= previewLength ? flattened : flattened[..previewLength] + "…";
        return $"{preview}（共 {content.Length} 字）";
    }
}