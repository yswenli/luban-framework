/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.DI.Core
*文件名： ProxyBuilder
*版本号： V1.0.0.0
*唯一标识：4d9fb6f8-1e74-4b70-910f-754ad9b0ac14
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/13 15:49:33
*描述：代理构建器
*
*=================================================
*修改标记
*修改时间：2025/8/13 15:49:33
*修改人： yswenli
*版本号： V1.0.0.0
*描述：代理构建器
*
*****************************************************************************/
namespace LuBan.DI.Core;

/// <summary>
/// 代理构建器
/// </summary>
internal class ProxyBuilder
{
    private const int InvokeActionFieldAndCtorParameterIndex = 0;

    private static readonly MethodInfo s_delegateInvoke;
    private static readonly MethodInfo s_delegateInvokeAsync;
    private static readonly MethodInfo s_delegateinvokeAsyncT;

    static ProxyBuilder()
    {
        s_delegateInvoke = typeof(DispatchProxyHandler)!.GetMethod("InvokeHandle")!;
        s_delegateInvokeAsync = typeof(DispatchProxyHandler)!.GetMethod("InvokeAsyncHandle")!;
        s_delegateinvokeAsyncT = typeof(DispatchProxyHandler)!.GetMethod("InvokeAsyncHandleT")!;
    }

    private readonly ProxyAssembly _assembly;
    private readonly TypeBuilder _tb;
    private readonly Type _proxyBaseType;
    private readonly List<FieldBuilder> _fields;

    internal ProxyBuilder(ProxyAssembly assembly, TypeBuilder tb, Type proxyBaseType)
    {
        _assembly = assembly;
        _tb = tb;
        _proxyBaseType = proxyBaseType;

        _fields = new List<FieldBuilder>
    {
        tb.DefineField("_handler", typeof(DispatchProxyHandler), FieldAttributes.Private)
    };
    }

    private static bool IsGenericTask(Type type)
    {
        var current = type;
        while (current != null)
        {
            if (IntrospectionExtensions.GetTypeInfo(current).IsGenericType && current.GetGenericTypeDefinition() == typeof(Task<>))
                return true;
            current = IntrospectionExtensions.GetTypeInfo(current).BaseType;
        }
        return false;
    }

    private void Complete()
    {
        var args = new Type[_fields.Count];
        for (var i = 0; i < args.Length; i++)
        {
            args[i] = _fields[i].FieldType;
        }

        var cb = _tb.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, args);
        var il = cb.GetILGenerator();

        // chained ctor call
        var baseCtor = IntrospectionExtensions.GetTypeInfo(_proxyBaseType).DeclaredConstructors.SingleOrDefault(c => c.IsPublic && c.GetParameters().Length == 0);
        if (baseCtor != null)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, baseCtor);
        }

        // store all the fields
        for (var i = 0; i < args.Length; i++)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg, i + 1);
            il.Emit(OpCodes.Stfld, _fields[i]);
        }

        il.Emit(OpCodes.Ret);
    }

    internal Type CreateType()
    {
        Complete();
        return _tb.CreateTypeInfo().AsType();
    }

    internal void AddInterfaceImpl(Type iface)
    {
        // If necessary, generate an attribute to permit visibility
        // to internal types.
        _assembly.EnsureTypeIsVisible(iface);

        _tb.AddInterfaceImplementation(iface);

        // AccessorMethods -> Metadata mappings.
        var propertyMap = new Dictionary<MethodInfo, PropertyAccessorInfo>(MethodInfoEqualityComparer.Instance);
        foreach (var pi in iface.GetRuntimeProperties())
        {
            var ai = new PropertyAccessorInfo(pi.GetMethod, pi.SetMethod);
            if (pi.GetMethod != null)
                propertyMap[pi.GetMethod] = ai;
            if (pi.SetMethod != null)
                propertyMap[pi.SetMethod] = ai;
        }

        var eventMap = new Dictionary<MethodInfo, EventAccessorInfo>(MethodInfoEqualityComparer.Instance);
        foreach (var ei in iface.GetRuntimeEvents())
        {
            var ai = new EventAccessorInfo(ei.AddMethod, ei.RemoveMethod, ei.RaiseMethod);
            if (ei.AddMethod != null)
                eventMap[ei.AddMethod] = ai;
            if (ei.RemoveMethod != null)
                eventMap[ei.RemoveMethod] = ai;
            if (ei.RaiseMethod != null)
                eventMap[ei.RaiseMethod] = ai;
        }

        foreach (var mi in iface.GetRuntimeMethods())
        {
            // 排除静态方法
            if (mi.IsStatic) continue;

            var mdb = AddMethodImpl(mi);
            if (propertyMap.TryGetValue(mi, out var associatedProperty))
            {
                if (MethodInfoEqualityComparer.Instance.Equals(associatedProperty.InterfaceGetMethod, mi))
                    associatedProperty.GetMethodBuilder = mdb;
                else
                    associatedProperty.SetMethodBuilder = mdb;
            }

            if (eventMap.TryGetValue(mi, out var associatedEvent))
            {
                if (MethodInfoEqualityComparer.Instance.Equals(associatedEvent.InterfaceAddMethod, mi))
                    associatedEvent.AddMethodBuilder = mdb;
                else if (MethodInfoEqualityComparer.Instance.Equals(associatedEvent.InterfaceRemoveMethod, mi))
                    associatedEvent.RemoveMethodBuilder = mdb;
                else
                    associatedEvent.RaiseMethodBuilder = mdb;
            }
        }

        foreach (var pi in iface.GetRuntimeProperties())
        {
            if (pi == null) continue;
            var method = pi.GetMethod ?? pi.SetMethod;
            if (method == null) continue;
            var ai = propertyMap[method];
            var pb = _tb.DefineProperty(pi.Name, pi.Attributes, pi.PropertyType, pi.GetIndexParameters().Select(p => p.ParameterType).ToArray());
            if (ai.GetMethodBuilder != null)
                pb.SetGetMethod(ai.GetMethodBuilder);
            if (ai.SetMethodBuilder != null)
                pb.SetSetMethod(ai.SetMethodBuilder);
        }

        foreach (var ei in iface.GetRuntimeEvents())
        {
            var method = ei.AddMethod ?? ei.RemoveMethod;
            if (method == null) continue;
            var ai = eventMap[method];
            if (ei?.EventHandlerType == null) continue;
            var eb = _tb.DefineEvent(ei.Name, ei.Attributes, ei.EventHandlerType);
            if (ai.AddMethodBuilder != null)
                eb.SetAddOnMethod(ai.AddMethodBuilder);
            if (ai.RemoveMethodBuilder != null)
                eb.SetRemoveOnMethod(ai.RemoveMethodBuilder);
            if (ai.RaiseMethodBuilder != null)
                eb.SetRaiseMethod(ai.RaiseMethodBuilder);
        }
    }

    private MethodBuilder AddMethodImpl(MethodInfo mi)
    {
        var parameters = mi.GetParameters();
        var paramTypes = ParamTypes(parameters, false);

        var mdb = _tb.DefineMethod(mi.Name, MethodAttributes.Public | MethodAttributes.Virtual, mi.ReturnType, paramTypes);
        if (mi.ContainsGenericParameters)
        {
            var ts = mi.GetGenericArguments();
            var ss = new string[ts.Length];
            for (var i = 0; i < ts.Length; i++)
            {
                ss[i] = ts[i].Name;
            }
            var genericParameters = mdb.DefineGenericParameters(ss);
            for (var i = 0; i < genericParameters.Length; i++)
            {
                genericParameters[i].SetGenericParameterAttributes(IntrospectionExtensions.GetTypeInfo(ts[i]).GenericParameterAttributes);
            }
        }
        var il = mdb.GetILGenerator();

        ParametersArray args = new(il, paramTypes);

        // object[] args = new object[paramCount];
        il.Emit(OpCodes.Nop);
        var argsArr = new GenericArray<object>(il, ParamTypes(parameters, true).Length);

        for (var i = 0; i < parameters.Length; i++)
        {
            // args[i] = argi;
            if (!parameters[i].IsOut)
            {
                argsArr.BeginSet(i);
                args.Get(i);
                argsArr.EndSet(parameters[i].ParameterType);
            }
        }

        // object[] packed = new object[PackedArgs.PackedTypes.Length];
        GenericArray<object> packedArr = new(il, PackedArgs.PackedTypes.Length);

        // packed[PackedArgs.DispatchProxyPosition] = this;
        packedArr.BeginSet(PackedArgs.DispatchProxyPosition);
        il.Emit(OpCodes.Ldarg_0);
        packedArr.EndSet(typeof(AspectDispatchProxy));

        // packed[PackedArgs.DeclaringTypePosition] = typeof(iface);
        var Type_GetTypeFromHandle = typeof(Type).GetRuntimeMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });
        if (Type_GetTypeFromHandle != null)
        {

            _assembly.GetTokenForMethod(mi, out var declaringType, out var methodToken);
            packedArr.BeginSet(PackedArgs.DeclaringTypePosition);
            il.Emit(OpCodes.Ldtoken, declaringType);
            il.Emit(OpCodes.Call, Type_GetTypeFromHandle);
            packedArr.EndSet(typeof(object));
            // packed[PackedArgs.MethodTokenPosition] = iface method token;
            packedArr.BeginSet(PackedArgs.MethodTokenPosition);
            il.Emit(OpCodes.Ldc_I4, methodToken);
            packedArr.EndSet(typeof(int));
        }


        // packed[PackedArgs.ArgsPosition] = args;
        packedArr.BeginSet(PackedArgs.ArgsPosition);
        argsArr.Load();
        packedArr.EndSet(typeof(object[]));

        // packed[PackedArgs.GenericTypesPosition] = mi.GetGenericArguments();
        if (mi.ContainsGenericParameters)
        {
            packedArr.BeginSet(PackedArgs.GenericTypesPosition);
            var genericTypes = mi.GetGenericArguments();
            GenericArray<Type> typeArr = new(il, genericTypes.Length);
            for (var i = 0; i < genericTypes.Length; ++i)
            {
                typeArr.BeginSet(i);
                il.Emit(OpCodes.Ldtoken, genericTypes[i]);
                if (Type_GetTypeFromHandle != null)
                    il.Emit(OpCodes.Call, Type_GetTypeFromHandle);
                typeArr.EndSet(typeof(Type));
            }
            typeArr.Load();
            packedArr.EndSet(typeof(Type[]));
        }

        for (var i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].ParameterType.IsByRef)
            {
                args.BeginSet(i);
                argsArr.Get(i);
                args.EndSet(i, typeof(object));
            }
        }

        var invokeMethod = s_delegateInvoke;
        if (mi.ReturnType == typeof(Task))
        {
            invokeMethod = s_delegateInvokeAsync;
        }
        if (IsGenericTask(mi.ReturnType))
        {
            var returnTypes = mi.ReturnType.GetGenericArguments();
            invokeMethod = s_delegateinvokeAsyncT.MakeGenericMethod(returnTypes);
        }

        // Call AsyncDispatchProxyGenerator.Invoke(object[]), InvokeAsync or InvokeAsyncT
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, _fields[InvokeActionFieldAndCtorParameterIndex]);
        packedArr.Load();
        il.Emit(OpCodes.Callvirt, invokeMethod);
        if (mi.ReturnType != typeof(void))
        {
            Convert(il, typeof(object), mi.ReturnType, false);
        }
        else
        {
            il.Emit(OpCodes.Pop);
        }

        il.Emit(OpCodes.Ret);

        _tb.DefineMethodOverride(mdb, mi);
        return mdb;
    }

    private static Type[] ParamTypes(ParameterInfo[] parms, bool noByRef)
    {
        var types = new Type[parms.Length];
        for (var i = 0; i < parms.Length; i++)
        {
            types[i] = parms[i].ParameterType;
            if (noByRef && types[i].IsByRef)
            {
                var val = types[i].GetElementType();
                if (val != null)
                    types[i] = val;
            }
        }
        return types;
    }

    // TypeCode does not exist in ProjectK or ProjectN.
    // This lookup method was copied from PortableLibraryThunks\Internal\PortableLibraryThunks\System\TypeThunks.cs
    // but returns the integer value equivalent to its TypeCode enum.
    private static int GetTypeCode(Type type)
    {
        if (type == null)
            return 0;   // TypeCode.Empty;

        if (type == typeof(bool))
            return 3;   // TypeCode.Boolean;

        if (type == typeof(char))
            return 4;   // TypeCode.Char;

        if (type == typeof(sbyte))
            return 5;   // TypeCode.SByte;

        if (type == typeof(byte))
            return 6;   // TypeCode.Byte;

        if (type == typeof(short))
            return 7;   // TypeCode.Int16;

        if (type == typeof(ushort))
            return 8;   // TypeCode.UInt16;

        if (type == typeof(int))
            return 9;   // TypeCode.Int32;

        if (type == typeof(uint))
            return 10;  // TypeCode.UInt32;

        if (type == typeof(long))
            return 11;  // TypeCode.Int64;

        if (type == typeof(ulong))
            return 12;  // TypeCode.UInt64;

        if (type == typeof(float))
            return 13;  // TypeCode.Single;

        if (type == typeof(double))
            return 14;  // TypeCode.Double;

        if (type == typeof(decimal))
            return 15;  // TypeCode.Decimal;

        if (type == typeof(DateTime))
            return 16;  // TypeCode.DateTime;

        if (type == typeof(string))
            return 18;  // TypeCode.String;

        if (IntrospectionExtensions.GetTypeInfo(type).IsEnum)
            return GetTypeCode(Enum.GetUnderlyingType(type));

        return 1;   // TypeCode.Object;
    }

    private static readonly OpCode[] s_convOpCodes = new OpCode[] {
    OpCodes.Nop,//Empty = 0,
    OpCodes.Nop,//Object = 1,
    OpCodes.Nop,//DBNull = 2,
    OpCodes.Conv_I1,//Boolean = 3,
    OpCodes.Conv_I2,//Char = 4,
    OpCodes.Conv_I1,//SByte = 5,
    OpCodes.Conv_U1,//Byte = 6,
    OpCodes.Conv_I2,//Int16 = 7,
    OpCodes.Conv_U2,//UInt16 = 8,
    OpCodes.Conv_I4,//Int32 = 9,
    OpCodes.Conv_U4,//UInt32 = 10,
    OpCodes.Conv_I8,//Int64 = 11,
    OpCodes.Conv_U8,//UInt64 = 12,
    OpCodes.Conv_R4,//Single = 13,
    OpCodes.Conv_R8,//Double = 14,
    OpCodes.Nop,//Decimal = 15,
    OpCodes.Nop,//DateTime = 16,
    OpCodes.Nop,//17
    OpCodes.Nop,//String = 18,
};

    private static readonly OpCode[] s_ldindOpCodes = new OpCode[] {
    OpCodes.Nop,//Empty = 0,
    OpCodes.Nop,//Object = 1,
    OpCodes.Nop,//DBNull = 2,
    OpCodes.Ldind_I1,//Boolean = 3,
    OpCodes.Ldind_I2,//Char = 4,
    OpCodes.Ldind_I1,//SByte = 5,
    OpCodes.Ldind_U1,//Byte = 6,
    OpCodes.Ldind_I2,//Int16 = 7,
    OpCodes.Ldind_U2,//UInt16 = 8,
    OpCodes.Ldind_I4,//Int32 = 9,
    OpCodes.Ldind_U4,//UInt32 = 10,
    OpCodes.Ldind_I8,//Int64 = 11,
    OpCodes.Ldind_I8,//UInt64 = 12,
    OpCodes.Ldind_R4,//Single = 13,
    OpCodes.Ldind_R8,//Double = 14,
    OpCodes.Nop,//Decimal = 15,
    OpCodes.Nop,//DateTime = 16,
    OpCodes.Nop,//17
    OpCodes.Ldind_Ref,//String = 18,
};

    private static readonly OpCode[] s_stindOpCodes = new OpCode[] {
    OpCodes.Nop,//Empty = 0,
    OpCodes.Nop,//Object = 1,
    OpCodes.Nop,//DBNull = 2,
    OpCodes.Stind_I1,//Boolean = 3,
    OpCodes.Stind_I2,//Char = 4,
    OpCodes.Stind_I1,//SByte = 5,
    OpCodes.Stind_I1,//Byte = 6,
    OpCodes.Stind_I2,//Int16 = 7,
    OpCodes.Stind_I2,//UInt16 = 8,
    OpCodes.Stind_I4,//Int32 = 9,
    OpCodes.Stind_I4,//UInt32 = 10,
    OpCodes.Stind_I8,//Int64 = 11,
    OpCodes.Stind_I8,//UInt64 = 12,
    OpCodes.Stind_R4,//Single = 13,
    OpCodes.Stind_R8,//Double = 14,
    OpCodes.Nop,//Decimal = 15,
    OpCodes.Nop,//DateTime = 16,
    OpCodes.Nop,//17
    OpCodes.Stind_Ref,//String = 18,
};

    private static void Convert(ILGenerator il, Type source, Type target, bool isAddress)
    {
        if (target == source)
            return;

        var sourceTypeInfo = IntrospectionExtensions.GetTypeInfo(source);
        var targetTypeInfo = IntrospectionExtensions.GetTypeInfo(target);

        if (source.IsByRef)
        {
            var argType = source.GetElementType();
            if (argType != null)
            {
                Ldind(il, argType);
                Convert(il, argType, target, isAddress);
            }
            return;
        }
        if (targetTypeInfo.IsValueType)
        {
            if (sourceTypeInfo.IsValueType)
            {
                var opCode = s_convOpCodes[GetTypeCode(target)];
                il.Emit(opCode);
            }
            else
            {
                il.Emit(OpCodes.Unbox, target);
                if (!isAddress)
                    Ldind(il, target);
            }
        }
        else if (targetTypeInfo.IsAssignableFrom(sourceTypeInfo))
        {
            if (sourceTypeInfo.IsValueType || source.IsGenericParameter)
            {
                if (isAddress)
                    Ldind(il, source);
                il.Emit(OpCodes.Box, source);
            }
        }
        else
        {
            if (target.IsGenericParameter)
            {
                il.Emit(OpCodes.Unbox_Any, target);
            }
            else
            {
                il.Emit(OpCodes.Castclass, target);
            }
        }
    }

    private static void Ldind(ILGenerator il, Type type)
    {
        var opCode = s_ldindOpCodes[GetTypeCode(type)];
        if (!opCode.Equals(OpCodes.Nop))
        {
            il.Emit(opCode);
        }
        else
        {
            il.Emit(OpCodes.Ldobj, type);
        }
    }

    private static void Stind(ILGenerator il, Type type)
    {
        var opCode = s_stindOpCodes[GetTypeCode(type)];
        if (!opCode.Equals(OpCodes.Nop))
        {
            il.Emit(opCode);
        }
        else
        {
            il.Emit(OpCodes.Stobj, type);
        }
    }

    private class ParametersArray
    {
        private readonly ILGenerator _il;
        private readonly Type[] _paramTypes;

        internal ParametersArray(ILGenerator il, Type[] paramTypes)
        {
            _il = il;
            _paramTypes = paramTypes;
        }

        internal void Get(int i)
        {
            _il.Emit(OpCodes.Ldarg, i + 1);
        }

        internal void BeginSet(int i)
        {
            _il.Emit(OpCodes.Ldarg, i + 1);
        }

        internal void EndSet(int i, Type stackType)
        {
            var argType = _paramTypes[i].GetElementType();
            if (argType == null) return;
            Convert(_il, stackType, argType, false);
            Stind(_il, argType);
        }
    }

    private class GenericArray<T>
    {
        private readonly ILGenerator _il;
        private readonly LocalBuilder _lb;

        internal GenericArray(ILGenerator il, int len)
        {
            _il = il;
            _lb = il.DeclareLocal(typeof(T[]));

            il.Emit(OpCodes.Ldc_I4, len);
            il.Emit(OpCodes.Newarr, typeof(T));
            il.Emit(OpCodes.Stloc, _lb);
        }

        internal void Load()
        {
            _il.Emit(OpCodes.Ldloc, _lb);
        }

        internal void Get(int i)
        {
            _il.Emit(OpCodes.Ldloc, _lb);
            _il.Emit(OpCodes.Ldc_I4, i);
            _il.Emit(OpCodes.Ldelem_Ref);
        }

        internal void BeginSet(int i)
        {
            _il.Emit(OpCodes.Ldloc, _lb);
            _il.Emit(OpCodes.Ldc_I4, i);
        }

        internal void EndSet(Type stackType)
        {
            Convert(_il, stackType, typeof(T), false);
            _il.Emit(OpCodes.Stelem_Ref);
        }
    }

    private sealed class PropertyAccessorInfo
    {
        public MethodInfo InterfaceGetMethod { get; }
        public MethodInfo InterfaceSetMethod { get; }
        public MethodBuilder GetMethodBuilder { get; set; }
        public MethodBuilder SetMethodBuilder { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="interfaceGetMethod"></param>
        /// <param name="interfaceSetMethod"></param>
        public PropertyAccessorInfo(MethodInfo? interfaceGetMethod, MethodInfo? interfaceSetMethod)
        {
            if (interfaceGetMethod != null)
                InterfaceGetMethod = interfaceGetMethod;
            if (interfaceSetMethod != null)
                InterfaceSetMethod = interfaceSetMethod;
        }
    }

    private sealed class EventAccessorInfo
    {
        public MethodInfo? InterfaceAddMethod { get; }
        public MethodInfo? InterfaceRemoveMethod { get; }
        public MethodInfo? InterfaceRaiseMethod { get; }
        public MethodBuilder AddMethodBuilder { get; set; }
        public MethodBuilder RemoveMethodBuilder { get; set; }
        public MethodBuilder RaiseMethodBuilder { get; set; }

        public EventAccessorInfo(MethodInfo? interfaceAddMethod, MethodInfo? interfaceRemoveMethod, MethodInfo? interfaceRaiseMethod)
        {
            InterfaceAddMethod = interfaceAddMethod;
            InterfaceRemoveMethod = interfaceRemoveMethod;
            InterfaceRaiseMethod = interfaceRaiseMethod;
        }
    }

    private sealed class MethodInfoEqualityComparer : EqualityComparer<MethodInfo>
    {
        public static readonly MethodInfoEqualityComparer Instance = new();

        private MethodInfoEqualityComparer()
        {
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public override sealed bool Equals(MethodInfo? left, MethodInfo? right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null)
                return right == null;
            else if (right == null)
                return false;

            // This assembly should work in netstandard1.3,
            // so we cannot use MemberInfo.MetadataToken here.
            // Therefore, it compares honestly referring ECMA-335 I.8.6.1.6 Signature Matching.
            if (!Equals(left.DeclaringType, right.DeclaringType))
                return false;

            if (!Equals(left.ReturnType, right.ReturnType))
                return false;

            if (left.CallingConvention != right.CallingConvention)
                return false;

            if (left.IsStatic != right.IsStatic)
                return false;

            if (left.Name != right.Name)
                return false;

            var leftGenericParameters = left.GetGenericArguments();
            var rightGenericParameters = right.GetGenericArguments();
            if (leftGenericParameters.Length != rightGenericParameters.Length)
                return false;

            for (var i = 0; i < leftGenericParameters.Length; i++)
            {
                if (!Equals(leftGenericParameters[i], rightGenericParameters[i]))
                    return false;
            }

            var leftParameters = left.GetParameters();
            var rightParameters = right.GetParameters();
            if (leftParameters.Length != rightParameters.Length)
                return false;

            for (var i = 0; i < leftParameters.Length; i++)
            {
                if (!Equals(leftParameters[i].ParameterType, rightParameters[i].ParameterType))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// This method is not used in this implementation.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override sealed int GetHashCode(MethodInfo obj)
        {
            if (obj == null)
                return 0;
            var hashCode = obj.DeclaringType!.GetHashCode();
            hashCode ^= obj.Name.GetHashCode();
            foreach (var parameter in obj.GetParameters())
            {
                hashCode ^= parameter.ParameterType.GetHashCode();
            }

            return hashCode;
        }
    }
}
