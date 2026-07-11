/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.DI.Core
*文件名： PackedArgs
*版本号： V1.0.0.0
*唯一标识：ccf2ccc6-6cae-4126-9c4b-46918a54c7dc
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/13 15:46:06
*描述：包参数
*
*=================================================
*修改标记
*修改时间：2025/8/13 15:46:06
*修改人： yswenli
*版本号： V1.0.0.0
*描述：包参数
*
*****************************************************************************/
namespace LuBan.DI.Models;

/// <summary>
/// 包参数
/// </summary>
internal class PackedArgs
{
    internal const int DispatchProxyPosition = 0;
    internal const int DeclaringTypePosition = 1;
    internal const int MethodTokenPosition = 2;
    internal const int ArgsPosition = 3;
    internal const int GenericTypesPosition = 4;
    internal const int ReturnValuePosition = 5;

    internal static readonly Type[] PackedTypes = new Type[] { typeof(object), typeof(Type), typeof(int), typeof(object[]), typeof(Type[]), typeof(object) };

    private readonly object[] _args;

    /// <summary>
    /// 包参数
    /// </summary>
    internal PackedArgs() : this(new object[PackedTypes.Length])
    {
    }
    /// <summary>
    /// 包参数
    /// </summary>
    /// <param name="args"></param>
    internal PackedArgs(object[] args)
    {
        _args = args;
    }

    internal AspectDispatchProxy DispatchProxy => (AspectDispatchProxy)_args[DispatchProxyPosition];
    internal Type DeclaringType => (Type)_args[DeclaringTypePosition];
    internal int MethodToken => (int)_args[MethodTokenPosition];
    internal object[] Args => (object[])_args[ArgsPosition];
    internal Type[] GenericTypes => (Type[])_args[GenericTypesPosition];

    internal object ReturnValue
    { /*get { return args[ReturnValuePosition]; }*/ set { _args[ReturnValuePosition] = value; } }
}
