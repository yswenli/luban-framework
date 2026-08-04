namespace LuBan.AIAgent;

/// <summary>
/// AIFunctionFactory 辅助方法，支持显式方法名注册工具，避免运行时反射枚举 <see cref="Type.GetMethods()"/>。
/// </summary>
internal static class AIFunctionFactoryHelper
{
    /// <summary>
    /// 为指定实例的某个公开实例方法创建 <see cref="AIFunction"/>。
    /// </summary>
    /// <typeparam name="T">工具分组类型</typeparam>
    /// <param name="instance">工具分组实例</param>
    /// <param name="methodName">方法名（建议使用 nameof）</param>
    /// <returns>AIFunction 实例</returns>
    public static AIFunction Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(
        T instance, string methodName) where T : class
    {
        var method = typeof(T).GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            ?? throw new ArgumentException($"方法 {methodName} 在类型 {typeof(T).Name} 上不存在", nameof(methodName));

        return AIFunctionFactory.Create(method, instance);
    }
}
