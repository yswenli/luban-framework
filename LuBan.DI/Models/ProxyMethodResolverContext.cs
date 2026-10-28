/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.DI.Models
*文件名： ProxyMethodResolverContext
*版本号： V1.0.0.0
*唯一标识：7b8b4702-4492-4417-8b37-2c11bb59246b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/13 15:47:59
*描述：ProxyMethodResolverContext 类
*
*=================================================
*修改标记
*修改时间：2025/8/13 15:47:59
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ProxyMethodResolverContext 类
*
*****************************************************************************/
namespace LuBan.DI.Models;

/// <summary>
/// ProxyMethodResolverContext 类
/// </summary>

internal class ProxyMethodResolverContext
{
    /// <summary>
    /// 代理调用时传入的打包参数，含代理对象、方法令牌、实参与返回值槽位。
    /// </summary>
    public PackedArgs Packed { get; }
    /// <summary>
    /// 由打包参数中的方法令牌解析得到、并已按泛型实参实例化的方法元数据。
    /// </summary>
    public MethodBase Method { get; }

    /// <summary>
    /// 用打包参数与已解析的方法元数据创建代理方法执行上下文。
    /// </summary>
    /// <param name="packed">代理调用传入的打包参数。</param>
    /// <param name="method">解析后用于执行的方法元数据。</param>
    public ProxyMethodResolverContext(PackedArgs packed, MethodBase method)
    {
        Packed = packed;
        Method = method;
    }
}
