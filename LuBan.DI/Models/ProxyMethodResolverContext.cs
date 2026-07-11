/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
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
*描述：
*
*=================================================
*修改标记
*修改时间：2025/8/13 15:47:59
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.DI.Models;

internal class ProxyMethodResolverContext
{
    public PackedArgs Packed { get; }
    public MethodBase Method { get; }

    public ProxyMethodResolverContext(PackedArgs packed, MethodBase method)
    {
        Packed = packed;
        Method = method;
    }
}
