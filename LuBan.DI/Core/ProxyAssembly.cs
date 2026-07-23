/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.DI.Core
*文件名： ProxyAssembly
*版本号： V1.0.0.0
*唯一标识：431c5549-9b1f-4c4c-a669-0ebf5fbe3f3e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/13 15:48:53
*描述：代理组件集
*
*=================================================
*修改标记
*修改时间：2025/8/13 15:48:53
*修改人： yswenli
*版本号： V1.0.0.0
*描述：代理组件集
*
*****************************************************************************/
namespace LuBan.DI.Core;

/// <summary>
/// 代理组件集
/// </summary>
internal class ProxyAssembly
{
    internal AssemblyBuilder _ab;
    private readonly ModuleBuilder _mb;
    private int _typeId = 0;

    private readonly Dictionary<MethodBase, int> _methodToToken = new();

    private readonly List<MethodBase> _methodsByToken = new();
    private readonly HashSet<string> _ignoresAccessAssemblyNames = new();
    private ConstructorInfo _ignoresAccessChecksToAttributeConstructor = null!;

    /// <summary>
    /// 代理组件集
    /// </summary>
    public ProxyAssembly()
    {
        var access = AssemblyBuilderAccess.Run;
        var assemblyName = new AssemblyName("ProxyBuilder2")
        {
            Version = new Version(1, 0, 0)
        };
        _ab = AssemblyBuilder.DefineDynamicAssembly(assemblyName, access);
        _mb = _ab.DefineDynamicModule("LuBanProxyModule");
    }

    /// <summary>
    /// 获取或创建 IgnoresAccessChecksAttribute 特性的 ConstructorInfo（构造函数信息）.
    /// 此特性会在动态程序集中同时进行定义和引用，以便能够访问其他程序集中的内部类型（internal types）。
    /// </summary>
    internal ConstructorInfo IgnoresAccessChecksAttributeConstructor
    {
        get
        {
            if (_ignoresAccessChecksToAttributeConstructor == null)
            {
                var attributeTypeInfo = GenerateTypeInfoOfIgnoresAccessChecksToAttribute();
                _ignoresAccessChecksToAttributeConstructor = attributeTypeInfo.DeclaredConstructors.Single();
            }

            return _ignoresAccessChecksToAttributeConstructor;
        }
    }

    /// <summary>
    /// 获取或创建 IgnoresAccessChecksAttribute 特性的 ConstructorInfo（构造函数信息）.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="proxyBaseType"></param>
    /// <returns></returns>
    public ProxyBuilder CreateProxy(string name, Type proxyBaseType)
    {
        var nextId = Interlocked.Increment(ref _typeId);
        var tb = _mb.DefineType(name + "_" + nextId, TypeAttributes.Public, proxyBaseType);
        return new ProxyBuilder(this, tb, proxyBaseType);
    }

    /// <summary>
    /// 获取或创建 IgnoresAccessChecksToAttribute 特性的 TypeInfo（类型信息）.
    /// </summary>
    /// <returns></returns>
    private TypeInfo GenerateTypeInfoOfIgnoresAccessChecksToAttribute()
    {
        var attributeTypeBuilder =
            _mb.DefineType("System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute",
                           TypeAttributes.Public | TypeAttributes.Class,
                           typeof(Attribute));

        // Create backing field as:
        // private string assemblyName;
        var assemblyNameField =
            attributeTypeBuilder.DefineField("assemblyName", typeof(string), FieldAttributes.Private);

        // Create ctor as:
        // public IgnoresAccessChecksToAttribute(string)
        var constructorBuilder = attributeTypeBuilder.DefineConstructor(MethodAttributes.Public,
                                                     CallingConventions.HasThis,
                                                     new Type[] { assemblyNameField.FieldType });

        var il = constructorBuilder.GetILGenerator();

        // Create ctor body as:
        // assemblyName = {ctor parameter 0}
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg, 1);
        il.Emit(OpCodes.Stfld, assemblyNameField);

        // return
        il.Emit(OpCodes.Ret);

        // Define property as:
        // public string AssemblyName {get { return assemblyName; } }
        var getterPropertyBuilder = attributeTypeBuilder.DefineProperty(
                                               "AssemblyName",
                                               PropertyAttributes.None,
                                               CallingConventions.HasThis,
                                               returnType: typeof(string),
                                               parameterTypes: null);

        var getterMethodBuilder = attributeTypeBuilder.DefineMethod(
                                               "get_AssemblyName",
                                               MethodAttributes.Public,
                                               CallingConventions.HasThis,
                                               returnType: typeof(string),
                                               parameterTypes: null);

        // Generate body:
        // return assemblyName;
        il = getterMethodBuilder.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, assemblyNameField);
        il.Emit(OpCodes.Ret);

        // Generate the AttributeUsage attribute for this attribute type:
        // [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
        var attributeUsageTypeInfo = IntrospectionExtensions.GetTypeInfo(typeof(AttributeUsageAttribute));

        // Find the ctor that takes only AttributeTargets
        var attributeUsageConstructorInfo =
            attributeUsageTypeInfo.DeclaredConstructors
                .Single(c => c.GetParameters().Length == 1 &&
                             c.GetParameters()[0].ParameterType == typeof(AttributeTargets));

        // Find the property to set AllowMultiple
        var allowMultipleProperty =
            attributeUsageTypeInfo.DeclaredProperties
                .Single(f => string.Equals(f.Name, "AllowMultiple"));

        // Create a builder to construct the instance via the ctor and property
        CustomAttributeBuilder customAttributeBuilder =
            new(attributeUsageConstructorInfo,
                                        new object[] { AttributeTargets.Assembly },
                                        new PropertyInfo[] { allowMultipleProperty },
                                        new object[] { true });

        // Attach this attribute instance to the newly defined attribute type
        attributeTypeBuilder.SetCustomAttribute(customAttributeBuilder);

        // Make the TypeInfo real so the constructor can be used.
        return attributeTypeBuilder.CreateTypeInfo();
    }

    // Generates an instance of the IgnoresAccessChecksToAttribute to
    // identify the given assembly as one which contains internal types
    // the dynamic assembly will need to reference.
    internal void GenerateInstanceOfIgnoresAccessChecksToAttribute(string assemblyName)
    {
        // Add this assembly level attribute:
        // [assembly: System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute(assemblyName)]
        var attributeConstructor = IgnoresAccessChecksAttributeConstructor;
        CustomAttributeBuilder customAttributeBuilder =
            new(attributeConstructor, new object[] { assemblyName });
        _ab.SetCustomAttribute(customAttributeBuilder);
    }

    // Ensures the type we will reference from the dynamic assembly
    // is visible.  Non-public types need to emit an attribute that
    // allows access from the dynamic assembly.
    internal void EnsureTypeIsVisible(Type type)
    {
        var typeInfo = IntrospectionExtensions.GetTypeInfo(type);
        if (!typeInfo.IsVisible)
        {
            var assemblyName = DynamicUtil.GetAssemblyName(typeInfo);
            if (!_ignoresAccessAssemblyNames.Contains(assemblyName))
            {
                GenerateInstanceOfIgnoresAccessChecksToAttribute(assemblyName);
                _ignoresAccessAssemblyNames.Add(assemblyName);
            }
        }
    }

    /// <summary>
    /// Generates a proxy assembly that can be used to invoke methods on
    /// </summary>
    /// <param name="method"></param>
    /// <param name="type"></param>
    /// <param name="token"></param>
    internal void GetTokenForMethod(MethodBase method, out Type type, out int token)
    {
        type = method.DeclaringType ?? throw new ArgumentNullException(nameof(method));
        if (!_methodToToken.TryGetValue(method, out token))
        {
            _methodsByToken.Add(method);
            token = _methodsByToken.Count - 1;
            _methodToToken[method] = token;
        }
    }

    internal MethodBase ResolveMethodToken(int token)
    {
        return _methodsByToken[token];
    }
}
