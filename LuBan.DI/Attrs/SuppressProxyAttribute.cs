/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common.DI
*文件名： SuppressProxyAttribute
*版本号： V1.0.0.0
*唯一标识：dd4fe7df-fb5d-4483-afb1-9c09aff48e57
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/6 15:21:53
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/6 15:21:53
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.DI.Attrs;

/// <summary>
/// 跳过全局代理
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SuppressProxyAttribute : Attribute
{
}
