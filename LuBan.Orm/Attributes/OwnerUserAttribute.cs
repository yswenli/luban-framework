/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Orm.Attributes
*文件名： OwnerUserAttribute.cs
*版本号： V1.0.0.0
*唯一标识：8b48a8eb-5d4a-4a59-b642-b02a884baa23
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:29
*描述：OwnerUserAttribute 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：OwnerUserAttribute 类
*
*****************************************************************************/

namespace LuBan.Orm.Attributes;

/// <summary>
/// 所属用户数据权限
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public class OwnerUserAttribute : Attribute
{
}
