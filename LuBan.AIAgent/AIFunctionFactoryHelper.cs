/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent
*文件名： AIFunctionFactoryHelper
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：AIFunction 工厂辅助类，支持显式方法名注册工具
*
*****************************************************************************/

namespace LuBan.AIAgent;

/// <summary>
/// AIFunctionFactory 辅助方法，支持显式方法名注册工具，避免运行时反射枚举 <see cref="Type.GetMethods()"/>。
/// </summary>
internal static class AIFunctionFactoryHelper
{
    /// <summary>
    /// 需要做工作区根锚定/兜底的路径参数名（无默认值时，模型漏传会让参数绑定直接抛 ArgumentException）。
    /// </summary>
    private static readonly string[] PathParameterNames = ["rootPath", "path", "sourcePath", "destPath"];

    /// <summary>
    /// 为指定实例的某个公开实例方法创建 <see cref="AIFunction"/>。
    /// </summary>
    /// <typeparam name="T">工具分组类型</typeparam>
    /// <param name="instance">工具分组实例</param>
    /// <param name="methodName">方法名（建议使用 nameof）</param>
    /// <param name="workspaceRootProvider">
    /// 工作区根目录提供者；设置后对名为 rootPath/path（含 sourcePath/destPath）且无默认值的路径参数生效：
    /// <list type="bullet">
    /// <item>模型显式给出的<b>相对路径（含 "."）</b>一律锚定到工作区根，避免按进程当前目录解析
    /// （GUI 宿主常为 bin 输出目录）导致"工作区里的文件查不出来"；绝对路径原样透传。</item>
    /// <item>参数缺省/空白时是否以工作区根兜底，由 <paramref name="fallbackToWorkspaceRootWhenMissing"/> 决定。</item>
    /// </list>
    /// 采用惰性求值（调用工具时才读取），避免工作区切换后取到过期值。
    /// </param>
    /// <param name="name">
    /// 显式工具名（对 LLM 暴露的名称）。null 表示沿用方法名。
    /// 用于把 C# 方法名（PlanTaskAsync）映射为契约名（plan_task）。
    /// </param>
    /// <param name="fallbackToWorkspaceRootWhenMissing">
    /// 参数缺省/空白时是否以工作区根兜底。<b>仅"只读发现类"工具应为 true</b>
    /// （如 ListDirectory/GetWorkspaceOverview/SearchFiles/Grep）；面向具体文件/目录的读写删改工具
    /// 须为 false，否则模型漏传 path 会被静默替换成工作区根，产生"路径被误识别为目录"
    /// "返回工作区统计"等误导结果（缺参由 <see cref="RequiredArgumentGuardAIFunction"/> 统一提示）。
    /// 注意：
    /// <list type="bullet">
    /// <item>无论该值为何，只要提供了 <paramref name="workspaceRootProvider"/>，"相对路径锚定"始终生效。</item>
    /// <item>该值只决定是否打 <see cref="WorkspaceRootFallbackAIFunction"/> 标记（即是否跳过必填参数守卫），
    /// 不改变参数绑定的取值：绑定层对"缺参/不可解释为字符串"一律回落工作区根（绝不返回 null），
    /// 以保证缺参时守卫先返回中文提示、而非在工具内 <c>new FileInfo(null)</c> 抛英文异常。</item>
    /// </list>
    /// </param>
    /// <returns>AIFunction 实例</returns>
    public static AIFunction Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(
        T instance, string methodName, Func<string?>? workspaceRootProvider = null, string? name = null,
        bool fallbackToWorkspaceRootWhenMissing = true) where T : class
    {
        var method = typeof(T).GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            ?? throw new ArgumentException($"方法 {methodName} 在类型 {typeof(T).Name} 上不存在", nameof(methodName));

        if (workspaceRootProvider == null && name == null)
        {
            return AIFunctionFactory.Create(method, instance);
        }

        var options = new AIFunctionFactoryOptions { Name = name };
        if (workspaceRootProvider != null)
        {
            options.ConfigureParameterBinding = parameter =>
            {
                if (parameter.HasDefaultValue || !IsPathParameter(parameter.Name))
                {
                    return default;
                }

                return new AIFunctionFactoryOptions.ParameterBindingOptions
                {
                    BindParameter = (parameterInfo, arguments) =>
                    {
                        // 参数值来自模型返回的 JSON 反序列化，运行时通常是 JsonElement 而非 string；
                        // 统一走 ToolArgumentValue 解释，避免与守卫的"是否已提供"判断出现二义（曾致 path 绑定为 null）。
                        var provided = ToolArgumentValue.TryGet(arguments, parameterInfo.Name!, out var value)
                            && ToolArgumentValue.TryGetString(value, out var text)
                            && !string.IsNullOrWhiteSpace(text)
                                ? text
                                : null;

                        if (provided != null)
                        {
                            // 模型显式给出的路径：相对路径（含 "."）锚定到工作区根，
                            // 避免按进程 CWD 解析（GUI 宿主常为 bin 输出目录）导致工作区文件"查不到"。
                            return ResolveRelativeToWorkspaceRoot(provided, workspaceRootProvider);
                        }

                        // 走到这里说明"缺参"或"提供了无法解释为字符串的值"。
                        // 防御：不得返回 null——参数值一旦为 null，工具内的 new FileInfo(null) 等会抛
                        // ArgumentNullException(Parameter 'fileName')。故统一回落到工作区根（或进程当前目录）。
                        var root = workspaceRootProvider();
                        return string.IsNullOrWhiteSpace(root) ? Environment.CurrentDirectory : root;
                    }
                };
            };
        }

        var function = AIFunctionFactory.Create(method, instance, options);

        // 仅"缺省兜底"语义的工具打标记，供 BuildTools 识别"已具备工作区根兜底、无需必填参数守卫"；
        // 只做相对路径锚定（缺参仍需守卫）的工具不打标记。
        return workspaceRootProvider != null && fallbackToWorkspaceRootWhenMissing
            ? new WorkspaceRootFallbackAIFunction(function)
            : function;
    }

    /// <summary>
    /// 判断参数名是否属于需要工作区根兜底/锚定的路径参数。
    /// </summary>
    private static bool IsPathParameter(string? name)
        => name != null && PathParameterNames.Any(p => string.Equals(p, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// 把模型给出的相对路径（含 "."）锚定到工作区根；绝对路径原样返回。
    /// 无工作区根或路径非法时原样返回，交由工具原有错误处理。
    /// </summary>
    private static string ResolveRelativeToWorkspaceRoot(string path, Func<string?> workspaceRootProvider)
    {
        try
        {
            if (Path.IsPathFullyQualified(path))
                return path;

            var root = workspaceRootProvider();
            if (string.IsNullOrWhiteSpace(root))
                return path;

            return Path.GetFullPath(Path.Combine(root, path));
        }
        catch
        {
            return path;
        }
    }
}
