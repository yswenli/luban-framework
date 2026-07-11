namespace LuBan.Common;

/// <summary>
/// 基于IL实现的高性能反射工具类
/// </summary>
public static class FastILUtil
{
    // 缓存已生成的委托，提高性能
    private static readonly ConcurrentDictionary<string, Delegate> _delegateCache = new ConcurrentDictionary<string, Delegate>();

    /// <summary>
    /// 基于 IL 实现 type.GetMethod(methodName)?.Invoke(obj, args) ?? null，支持方法缓存以提高性能
    /// </summary>
    /// <param name="type">目标类型</param>
    /// <param name="methodName">方法名</param>
    /// <param name="obj">实例（静态方法传 null）</param>
    /// <param name="args">方法参数数组（无参数传 null）</param>
    /// <returns>方法执行结果（null 表示方法未找到或执行返回 null）</returns>
    /// <exception cref="ArgumentNullException">当 type 或 methodName 为 null 时抛出</exception>
    public static object MethodInvoke(Type type, string methodName, object? obj, object[] args)
    {
        // 参数验证
        if (type == null)
            throw new ArgumentNullException(nameof(type), "类型不能为null");
        if (string.IsNullOrEmpty(methodName))
            throw new ArgumentNullException(nameof(methodName), "方法名不能为null或空字符串");

        try
        {
            // 尝试从缓存获取委托
            string cacheKey = $"MethodInvoke_{type.FullName}_{methodName}";
            if (_delegateCache.TryGetValue(cacheKey, out var cachedDelegate))
            {
                return ((Func<Type, string, object?, object[], object>)cachedDelegate)(type, methodName, obj, args);
            }

            // 创建动态方法
            var dynamicMethod = new DynamicMethod(
                name: "FastILUtil_MethodInvoke",
                returnType: typeof(object),
                parameterTypes: new[] { typeof(Type), typeof(string), typeof(object), typeof(object[]) },
                owner: typeof(FastILUtil),
                skipVisibility: true
            );

            ILGenerator il = dynamicMethod.GetILGenerator();

            // 定义标签
            Label label_InvokeEnd = il.DefineLabel(); // 方法未找到时返回null的标签

            // 获取MethodInfo
            il.Emit(OpCodes.Ldarg_0); // 加载type参数
            il.Emit(OpCodes.Ldarg_1); // 加载methodName参数
            var getMethod = typeof(Type).GetMethod("GetMethod", new[] { typeof(string) });
            if (getMethod != null)
            {
                il.Emit(OpCodes.Callvirt, getMethod);
            }

            // 检查MethodInfo是否为null
            il.Emit(OpCodes.Dup); // 复制MethodInfo引用
            il.Emit(OpCodes.Brfalse_S, label_InvokeEnd); // 如果为null，跳转到结束标签

            // 调用方法
            il.Emit(OpCodes.Ldarg_2); // 加载obj参数
            il.Emit(OpCodes.Ldarg_3); // 加载args参数
            var invokeMethod = typeof(MethodBase).GetMethod("Invoke", new[] { typeof(object), typeof(object[]) });
            if (invokeMethod != null)
            {
                il.Emit(OpCodes.Callvirt, invokeMethod);
            }

            // 直接返回调用结果
            il.Emit(OpCodes.Ret);

            // 方法未找到时返回null
            il.MarkLabel(label_InvokeEnd);
            il.Emit(OpCodes.Pop); // 弹出栈上的null
            il.Emit(OpCodes.Ldnull); // 压入null作为返回值
            il.Emit(OpCodes.Ret);

            // 创建委托
            var invokeDelegate = (Func<Type, string, object?, object[], object>)dynamicMethod.CreateDelegate(
                typeof(Func<Type, string, object?, object[], object>)
            );

            // 缓存委托
            _delegateCache.TryAdd(cacheKey, invokeDelegate);

            return invokeDelegate(type, methodName, obj, args);
        }
        catch (Exception ex)
        {
            // 记录异常信息并抛出包装异常，提供更清晰的错误上下文
            throw new InvalidOperationException($"执行类型 '{type.FullName}' 的方法 '{methodName}' 时发生错误", ex);
        }
    }

    /// <summary>
    /// 高性能读取目标对象的指定属性值
    /// </summary>
    /// <param name="type">目标类型</param>
    /// <param name="propertyName">属性名</param>
    /// <param name="obj">实例（静态属性传 null）</param>
    /// <returns>属性值（null 表示属性未找到、无 get 方法或属性值为 null）</returns>
    /// <exception cref="ArgumentNullException">当 type 或 propertyName 为 null 时抛出</exception>
    public static object GetProperty(Type type, string propertyName, object? obj)
    {
        // 参数验证
        if (type == null)
            throw new ArgumentNullException(nameof(type), "类型不能为null");
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentNullException(nameof(propertyName), "属性名不能为null或空字符串");
        if (obj != null && !type.IsInstanceOfType(obj))
            throw new ArgumentException($"对象类型 '{obj.GetType().FullName}' 与目标类型 '{type.FullName}' 不兼容");

        try
        {
            // 尝试从缓存获取委托
            string cacheKey = $"GetProperty_{type.FullName}_{propertyName}";
            if (_delegateCache.TryGetValue(cacheKey, out var cachedDelegate))
            {
                return ((Func<Type, string, object?, object>)cachedDelegate)(type, propertyName, obj);
            }

            // 创建动态方法
            var dynamicMethod = new DynamicMethod(
                name: "FastILUtil_GetProperty",
                returnType: typeof(object),
                parameterTypes: new[] { typeof(Type), typeof(string), typeof(object) },
                owner: typeof(FastILUtil),
                skipVisibility: true
            );

            ILGenerator il = dynamicMethod.GetILGenerator();
            Label label_End = il.DefineLabel(); // 属性未找到或无法获取时返回null的标签

            // 获取PropertyInfo
            il.Emit(OpCodes.Ldarg_0); // 加载type参数
            il.Emit(OpCodes.Ldarg_1); // 加载propertyName参数
            var getPropertyMethod = typeof(Type).GetMethod("GetProperty", new[] { typeof(string) });
            if (getPropertyMethod != null)
            {
                il.Emit(OpCodes.Callvirt, getPropertyMethod);
            }

            // 检查PropertyInfo是否为null
            il.Emit(OpCodes.Dup); // 复制PropertyInfo引用
            il.Emit(OpCodes.Brfalse_S, label_End); // 如果为null，跳转到结束标签

            // 获取getter方法
            var getGetMethod = typeof(PropertyInfo).GetMethod("GetGetMethod", Type.EmptyTypes);
            if (getGetMethod != null)
            {
                il.Emit(OpCodes.Callvirt, getGetMethod);
            }

            // 检查getter是否为null
            il.Emit(OpCodes.Dup); // 复制MethodInfo引用
            il.Emit(OpCodes.Brfalse_S, label_End); // 如果为null，跳转到结束标签

            // 调用getter
            il.Emit(OpCodes.Ldarg_2); // 加载obj参数
            il.Emit(OpCodes.Ldnull); // 加载null（无参数）
            var invokeMethod = typeof(MethodBase).GetMethod("Invoke", new[] { typeof(object), typeof(object[]) });
            if (invokeMethod != null)
            {
                il.Emit(OpCodes.Callvirt, invokeMethod);
            }

            // 返回属性值
            il.Emit(OpCodes.Ret);

            // 属性未找到或无法获取时返回null
            il.MarkLabel(label_End);
            il.Emit(OpCodes.Pop); // 弹出栈上的null引用
            il.Emit(OpCodes.Ldnull); // 压入null作为返回值
            il.Emit(OpCodes.Ret);

            // 编译委托
            var getDelegate = (Func<Type, string, object?, object>)dynamicMethod.CreateDelegate(
                typeof(Func<Type, string, object?, object>)
            );

            // 缓存委托
            _delegateCache.TryAdd(cacheKey, getDelegate);

            return getDelegate(type, propertyName, obj);
        }
        catch (Exception ex)
        {
            // 记录异常信息并抛出包装异常
            throw new InvalidOperationException($"获取类型 '{type.FullName}' 的属性 '{propertyName}' 值时发生错误", ex);
        }
    }

    /// <summary>
    /// 高性能写入值到目标对象的指定属性
    /// </summary>
    /// <param name="type">目标类型</param>
    /// <param name="propertyName">属性名</param>
    /// <param name="obj">实例（静态属性传 null）</param>
    /// <param name="value">待写入的属性值（需与属性类型兼容）</param>
    /// <returns>是否写入成功（false 表示属性未找到、无 set 方法或类型不兼容）</returns>
    /// <exception cref="ArgumentNullException">当 type 或 propertyName 为 null 时抛出</exception>
    public static bool SetProperty(Type type, string propertyName, object? obj, object value)
    {
        // 参数验证
        if (type == null)
            throw new ArgumentNullException(nameof(type), "类型不能为null");
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentNullException(nameof(propertyName), "属性名不能为null或空字符串");
        if (obj != null && !type.IsInstanceOfType(obj))
            throw new ArgumentException($"对象类型 '{obj.GetType().FullName}' 与目标类型 '{type.FullName}' 不兼容");

        try
        {
            // 尝试从缓存获取委托
            string cacheKey = $"SetProperty_{type.FullName}_{propertyName}";
            if (_delegateCache.TryGetValue(cacheKey, out var cachedDelegate))
            {
                return ((Func<Type, string, object?, object, bool>)cachedDelegate)(type, propertyName, obj, value);
            }

            // 创建动态方法
            var dynamicMethod = new DynamicMethod(
                name: "FastILUtil_SetProperty",
                returnType: typeof(bool),
                parameterTypes: new[] { typeof(Type), typeof(string), typeof(object), typeof(object) },
                owner: typeof(FastILUtil),
                skipVisibility: true
            );

            ILGenerator il = dynamicMethod.GetILGenerator();
            Label label_Fail = il.DefineLabel(); // 设置失败标签（返回false）
            Label label_End = il.DefineLabel(); // 统一返回标签

            // 获取PropertyInfo
            il.Emit(OpCodes.Ldarg_0); // 加载type参数
            il.Emit(OpCodes.Ldarg_1); // 加载propertyName参数
            var getPropertyMethod = typeof(Type).GetMethod("GetProperty", new[] { typeof(string) });
            if (getPropertyMethod != null)
            {
                il.Emit(OpCodes.Callvirt, getPropertyMethod);
            }

            // 检查PropertyInfo是否为null
            il.Emit(OpCodes.Dup); // 复制PropertyInfo引用
            il.Emit(OpCodes.Brfalse_S, label_Fail); // 如果为null，跳转到失败标签

            // 获取setter方法
            var getSetMethod = typeof(PropertyInfo).GetMethod("GetSetMethod", Type.EmptyTypes);
            if (getSetMethod != null)
            {
                il.Emit(OpCodes.Callvirt, getSetMethod);
            }

            // 检查setter是否为null
            il.Emit(OpCodes.Dup); // 复制MethodInfo引用
            il.Emit(OpCodes.Brfalse_S, label_Fail); // 如果为null，跳转到失败标签

            // 调用setter
            il.Emit(OpCodes.Ldarg_2); // 加载obj参数

            // 构造参数数组：new object[] { value }
            il.Emit(OpCodes.Ldc_I4_1); // 数组长度为1
            il.Emit(OpCodes.Newarr, typeof(object)); // 创建object[]数组
            il.Emit(OpCodes.Dup); // 复制数组引用
            il.Emit(OpCodes.Ldc_I4_0); // 数组索引0
            il.Emit(OpCodes.Ldarg_3); // 加载value参数
            il.Emit(OpCodes.Stelem_Ref); // 将value存入数组索引0

            var invokeMethod = typeof(MethodBase).GetMethod("Invoke", new[] { typeof(object), typeof(object[]) });
            if (invokeMethod != null)
            {
                il.Emit(OpCodes.Callvirt, invokeMethod);
            }

            // 忽略invoke结果，设置成功
            il.Emit(OpCodes.Pop); // 弹出invoke返回值
            il.Emit(OpCodes.Ldc_I4_1); // 返回true
            il.Emit(OpCodes.Br_S, label_End); // 跳转到结束标签

            // 设置失败，返回false
            il.MarkLabel(label_Fail);
            il.Emit(OpCodes.Pop); // 弹出栈上的null引用
            il.Emit(OpCodes.Ldc_I4_0); // 返回false

            // 统一返回点
            il.MarkLabel(label_End);
            il.Emit(OpCodes.Ret);

            // 编译委托
            var setDelegate = (Func<Type, string, object?, object, bool>)dynamicMethod.CreateDelegate(
                typeof(Func<Type, string, object?, object, bool>)
            );

            // 缓存委托
            _delegateCache.TryAdd(cacheKey, setDelegate);

            return setDelegate(type, propertyName, obj, value);
        }
        catch (Exception ex)
        {
            // 记录异常信息并返回失败
            System.Diagnostics.Debug.WriteLine($"[Error] 设置类型 '{type.FullName}' 的属性 '{propertyName}' 值时发生错误: {ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// 清除方法缓存，在类型加载或重新加载时使用
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClearCache()
    {
        _delegateCache.Clear();
    }

}