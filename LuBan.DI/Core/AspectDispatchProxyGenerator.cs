/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.DI.Core
*文件名： AspectDispatchProxyGenerator
*版本号： V1.0.0.0
*唯一标识：e30c13f5-1a8a-4a4b-a045-c15861e4f735
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/13 15:41:47
*描述：异步分发代理生成器
*
*=================================================
*修改标记
*修改时间：2025/8/13 15:41:47
*修改人： yswenli
*版本号： V1.0.0.0
*描述：异步分发代理生成器
*
*****************************************************************************/

namespace LuBan.DI.Core;


/// <summary>
/// 异步分发代理生成器
/// </summary>
internal static class AspectDispatchProxyGenerator
{

    private static readonly Dictionary<Type, Dictionary<Type, Type>> s_baseTypeAndInterfaceToGeneratedProxyType = new();

    private static readonly ProxyAssembly s_proxyAssembly = new();
    private static readonly MethodInfo s_dispatchProxyInvokeMethod = IntrospectionExtensions.GetTypeInfo(typeof(AspectDispatchProxy)).GetDeclaredMethod("Invoke")!;
    private static readonly MethodInfo s_dispatchProxyInvokeAsyncMethod = IntrospectionExtensions.GetTypeInfo(typeof(AspectDispatchProxy)).GetDeclaredMethod("InvokeAsync")!;
    private static readonly MethodInfo s_dispatchProxyInvokeAsyncTMethod = IntrospectionExtensions.GetTypeInfo(typeof(AspectDispatchProxy)).GetDeclaredMethod("InvokeAsyncT")!;

    /// <summary>
    /// 生成代理实例
    /// </summary>
    /// <param name="baseType"></param>
    /// <param name="interfaceType"></param>
    /// <returns></returns>
    internal static object CreateProxyInstance(Type baseType, Type interfaceType)
    {
        var proxiedType = GetProxyType(baseType, interfaceType);
        return Activator.CreateInstance(proxiedType, new DispatchProxyHandler())!;
    }

    /// <summary>
    /// 生成代理类型
    /// </summary>
    /// <param name="baseType"></param>
    /// <param name="interfaceType"></param>
    /// <returns></returns>
    private static Type GetProxyType(Type baseType, Type interfaceType)
    {
        lock (s_baseTypeAndInterfaceToGeneratedProxyType)
        {
            if (!s_baseTypeAndInterfaceToGeneratedProxyType.TryGetValue(baseType, out var interfaceToProxy))
            {
                interfaceToProxy = new Dictionary<Type, Type>();
                s_baseTypeAndInterfaceToGeneratedProxyType[baseType] = interfaceToProxy;
            }

            if (!interfaceToProxy.TryGetValue(interfaceType, out var generatedProxy))
            {
                generatedProxy = GenerateProxyType(baseType, interfaceType);
                interfaceToProxy[interfaceType] = generatedProxy;
            }

            return generatedProxy;
        }
    }

    /// <summary>
    /// 生成代理类型
    /// </summary>
    /// <param name="baseType"></param>
    /// <param name="interfaceType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private static Type GenerateProxyType(Type baseType, Type interfaceType)
    {
        // Parameter validation is deferred until the point we need to create the proxy.
        // This prevents unnecessary overhead revalidating cached proxy types.
        var baseTypeInfo = IntrospectionExtensions.GetTypeInfo(baseType);

        // The interface type must be an interface, not a class
        if (!IntrospectionExtensions.GetTypeInfo(interfaceType).IsInterface)
        {
            // "T" is the generic parameter seen via the public contract
            throw new ArgumentException($"InterfaceType_Must_Be_Interface, {interfaceType.FullName}", nameof(interfaceType));
        }

        // The base type cannot be sealed because the proxy needs to subclass it.
        if (baseTypeInfo.IsSealed)
        {
            // "TProxy" is the generic parameter seen via the public contract
            throw new ArgumentException($"BaseType_Cannot_Be_Sealed, {baseTypeInfo.FullName}", nameof(baseType));
        }

        // The base type cannot be abstract
        if (baseTypeInfo.IsAbstract)
        {
            throw new ArgumentException($"BaseType_Cannot_Be_Abstract {baseType.FullName}", nameof(baseType));
        }

        // The base type must have a public default ctor
        if (!baseTypeInfo.DeclaredConstructors.Any(c => c.IsPublic && c.GetParameters().Length == 0))
        {
            throw new ArgumentException($"BaseType_Must_Have_Default_Ctor {baseType.FullName}", nameof(baseType));
        }

        // Create a type that derives from 'baseType' provided by caller
        var pb = s_proxyAssembly.CreateProxy("generatedProxy", baseType);

        foreach (var t in IntrospectionExtensions.GetTypeInfo(interfaceType).ImplementedInterfaces)
            pb.AddInterfaceImpl(t);

        pb.AddInterfaceImpl(interfaceType);

        var generatedProxyType = pb.CreateType();
        return generatedProxyType;
    }

    /// <summary>
    /// 代理执行上下文
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static ProxyMethodResolverContext Resolve(object[] args)
    {
        var packed = new PackedArgs(args);
        var method = s_proxyAssembly.ResolveMethodToken(packed.DeclaringType, packed.MethodToken);
        if (method.IsGenericMethodDefinition)
            method = ((MethodInfo)method).MakeGenericMethod(packed.GenericTypes);

        return new ProxyMethodResolverContext(packed, method);
    }

    /// <summary>
    /// 代理方法执行
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static object Invoke(object[] args)
    {
        var context = Resolve(args);

        // Call (protected method) DispatchProxyAsync.Invoke()
        object? returnValue = null;
        try
        {
            returnValue = s_dispatchProxyInvokeMethod.Invoke(context.Packed.DispatchProxy,
                                                                   [context.Method, context.Packed.Args])!;
            context.Packed.ReturnValue = returnValue;
        }
        catch (TargetInvocationException tie)
        {
            // 这里处理内部异常
            ExceptionDispatchInfo.Capture(tie.InnerException?.InnerException ?? tie.InnerException!).Throw();
        }

        return returnValue;
    }

    /// <summary>
    /// 代理方法执行
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static async Task InvokeAsync(object[] args)
    {
        var context = Resolve(args);
        try
        {
            var task = (Task)s_dispatchProxyInvokeAsyncMethod.Invoke(context.Packed.DispatchProxy, [context.Method, context.Packed.Args])!;
            await task;
        }
        catch (TargetInvocationException tie)
        {
            //这里处理内部异常
            ExceptionDispatchInfo.Capture(tie.InnerException?.InnerException ?? tie.InnerException!).Throw();
        }
    }

    /// <summary>
    /// 异步代理方法执行
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args"></param>
    /// <returns></returns>
    public static async Task<T?> InvokeAsync<T>(object[] args)
    {
        var context = Resolve(args);
        T? returnValue = default;
        try
        {
            var genericmethod = s_dispatchProxyInvokeAsyncTMethod.MakeGenericMethod(typeof(T));
            var task = (Task<T>)genericmethod.Invoke(context.Packed.DispatchProxy, [context.Method, context.Packed.Args])!;
            returnValue = await task.ConfigureAwait(true);
            if (returnValue != null)
                context.Packed.ReturnValue = returnValue;
        }
        catch (TargetInvocationException tie)
        {
            ExceptionDispatchInfo.Capture(tie.InnerException?.InnerException ?? tie.InnerException!).Throw();
        }
        return returnValue;
    }

}
