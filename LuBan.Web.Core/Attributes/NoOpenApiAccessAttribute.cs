/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Attributes
*文件名： NoOpenApiAccessAttribute
*版本号： V1.0.0.0
*唯一标识：fe9eae53-b88e-4d95-b051-58f6cfa1d2da
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/4/10 10:32:14
*描述：
*
*=================================================
*修改标记
*修改时间：2025/4/10 10:32:14
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Web.Core.Attributes;

/// <summary>
/// 标记无需开放接口认证，
/// 对应的 OpenApiAccessAttribute
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class NoOpenApiAccessAttribute : Attribute
{

}
