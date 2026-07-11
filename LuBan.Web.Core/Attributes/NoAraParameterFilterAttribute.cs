/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Auth
*文件名： NoAraParameterFilterAttribute
*版本号： V1.0.0.0
*唯一标识：d0506d06-5f42-4f1d-a587-9b72f7f5bf87
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/4/22 14:36:19
*描述：标记无需安全参数较验过滤器
*
*=================================================
*修改标记
*修改时间：2025/4/22 14:36:19
*修改人： yswenli
*版本号： V1.0.0.0
*描述：标记无需安全参数较验过滤器
*
*****************************************************************************/
namespace LuBan.Web.Core.Attributes;


/// <summary>
/// 标记无需安全参数较验过滤器
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class NoAraParameterFilterAttribute : Attribute
{

}
