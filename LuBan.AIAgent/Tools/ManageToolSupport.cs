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
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.1
*描述：ResolveWorkspaceRoot 增加包含性校验：相对路径锚定后不得越出宿主工作区，
*      隔离宿主（IsolateWorkspaceByUser）下绝对路径同样受此约束
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
/// <item>工作区根显式参数优先、宿主提供者兜底，相对路径一律锚定到宿主工作区；
/// 开启按用户隔离的宿主连绝对路径也要求落在工作区内。</item>
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
    /// 避免按进程当前目录解析（GUI 宿主常为 bin 输出目录）而写到工作区之外；
    /// 锚定结果必须仍落在宿主工作区内，<c>../other-user</c> 这类穿越会被拒绝。
    /// <para>
    /// 绝对路径默认原样透传（单用户宿主存在「跨项目传绝对路径」的既有用法）；
    /// 但当 <paramref name="requireContainment"/> 为 true（宿主开启按用户隔离工作区）时，
    /// 绝对路径同样必须落在宿主工作区内，否则隔离可被 <c>D:\ws\users\&lt;其他用户&gt;</c> 绕过。
    /// </para>
    /// </remarks>
    /// <param name="workspaceRoot">模型/调用方显式给出的工作区根。</param>
    /// <param name="defaultWorkspaceRoot">宿主工作区根提供者（惰性求值）。</param>
    /// <param name="requireContainment">是否要求绝对路径也落在宿主工作区内（多用户隔离宿主传 true）。</param>
    /// <returns>
    /// <c>Root</c>：解析出的绝对路径（取不到为 null）；
    /// <c>Error</c>：越界时的可直接回给 LLM 的提示（无错误为 null）。
    /// </returns>
    public static (string? Root, string? Error) ResolveWorkspaceRoot(
        string? workspaceRoot, Func<string?>? defaultWorkspaceRoot, bool requireContainment = false)
    {
        var explicitRoot = string.IsNullOrWhiteSpace(workspaceRoot) ? null : workspaceRoot!.Trim();
        var fallbackRoot = defaultWorkspaceRoot?.Invoke();

        // 未显式给出：完全信任宿主（它不会把自己指到工作区外）
        if (explicitRoot is null)
            return (GetFullPathOrSelf(fallbackRoot), null);

        var anchor = string.IsNullOrWhiteSpace(fallbackRoot) ? null : GetFullPathOrSelf(fallbackRoot);

        // 绝对路径：默认透传；隔离宿主要求必须仍在工作区内
        if (Path.IsPathFullyQualified(explicitRoot))
        {
            var absolute = GetFullPathOrSelf(explicitRoot);
            if (requireContainment && anchor is not null && absolute is not null
                && !IsWithinRoot(anchor, absolute))
            {
                return (null, BuildOutOfRangeError(explicitRoot, anchor));
            }

            return (absolute, null);
        }

        // 相对路径：无宿主基准时无法判断是否越界，按进程当前目录处理（保持旧行为）
        if (anchor is null)
            return (GetFullPathOrSelf(explicitRoot), null);

        var candidate = GetFullPathOrSelf(Path.Combine(anchor, explicitRoot));
        if (candidate is null)
            return (GetFullPathOrSelf(explicitRoot), null);

        // 锚定后必须仍在宿主工作区内：挡住 ../ 穿越到 users/<其他用户>
        if (!IsWithinRoot(anchor, candidate))
            return (null, BuildOutOfRangeError(explicitRoot, anchor));

        return (candidate, null);
    }

    /// <summary>
    /// 越界提示：直接回给 LLM，指明合法范围，避免它反复试探。
    /// </summary>
    /// <param name="explicitRoot">模型给的原始值。</param>
    /// <param name="anchor">宿主工作区（已归一化）。</param>
    /// <returns>提示文本。</returns>
    private static string BuildOutOfRangeError(string explicitRoot, string anchor)
        => $"workspaceRoot 不能越出宿主工作区：{explicitRoot}（工作区为 {anchor}）。" +
           "请省略该参数，或使用工作区内的路径。";

    /// <summary>
    /// 取绝对路径；空白/null 返回 null，非法路径原样返回（交由后续 IO 报错）。
    /// </summary>
    private static string? GetFullPathOrSelf(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        try
        {
            return Path.GetFullPath(path);
        }
        catch
        {
            return path;
        }
    }

    /// <summary>
    /// 判断路径是否落在根之下（含相等）。
    /// </summary>
    /// <remarks>
    /// 必须带目录分隔符比较，否则 <c>C:\ws-evil</c> 会被误判成在 <c>C:\ws</c> 内。
    /// </remarks>
    private static bool IsWithinRoot(string root, string path)
    {
        var normalizedRoot = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedPath = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (string.Equals(normalizedRoot, normalizedPath, StringComparison.OrdinalIgnoreCase))
            return true;

        // 根为盘符根（C:\）时自身已带分隔符，避免拼出 "C:\\"
        var prefix = normalizedRoot.EndsWith(Path.DirectorySeparatorChar)
            ? normalizedRoot
            : normalizedRoot + Path.DirectorySeparatorChar;

        return normalizedPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
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