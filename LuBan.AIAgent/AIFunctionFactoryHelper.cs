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
    /// 需要兜底的路径参数名（无默认值时，模型漏传会让参数绑定直接抛 ArgumentException）。
    /// </summary>
    private static readonly string[] PathParameterNames = ["rootPath", "path"];

    /// <summary>
    /// 为指定实例的某个公开实例方法创建 <see cref="AIFunction"/>。
    /// </summary>
    /// <typeparam name="T">工具分组类型</typeparam>
    /// <param name="instance">工具分组实例</param>
    /// <param name="methodName">方法名（建议使用 nameof）</param>
    /// <param name="workspaceRootProvider">
    /// 工作区根目录提供者；仅在参数无默认值且名为 rootPath/path 时生效。
    /// 采用惰性求值（调用工具时才读取），避免工作区切换后取到过期值。
    /// </param>
    /// <returns>AIFunction 实例</returns>
    public static AIFunction Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(
        T instance, string methodName, Func<string?>? workspaceRootProvider = null) where T : class
    {
        var method = typeof(T).GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            ?? throw new ArgumentException($"方法 {methodName} 在类型 {typeof(T).Name} 上不存在", nameof(methodName));

        if (workspaceRootProvider == null)
        {
            return AIFunctionFactory.Create(method, instance);
        }

        var options = new AIFunctionFactoryOptions
        {
            ConfigureParameterBinding = parameter =>
            {
                if (parameter.HasDefaultValue || !IsPathParameter(parameter.Name))
                {
                    return default;
                }

                return new AIFunctionFactoryOptions.ParameterBindingOptions
                {
                    BindParameter = (parameterInfo, arguments) =>
                    {
                        if (arguments.TryGetValue(parameterInfo.Name!, out var value)
                            && value is string provided
                            && !string.IsNullOrWhiteSpace(provided))
                        {
                            return provided;
                        }

                        var root = workspaceRootProvider();
                        return string.IsNullOrWhiteSpace(root) ? Environment.CurrentDirectory : root;
                    }
                };
            }
        };

        return AIFunctionFactory.Create(method, instance, options);
    }

    /// <summary>
    /// 判断参数名是否属于需要工作区根兜底的路径参数。
    /// </summary>
    private static bool IsPathParameter(string? name)
        => name != null && PathParameterNames.Any(p => string.Equals(p, name, StringComparison.OrdinalIgnoreCase));
}
