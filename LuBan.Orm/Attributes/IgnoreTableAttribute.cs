/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Orm.Attributes
*文件名： IgnoreTableAttribute
*版本号： V1.0.0.0
*唯一标识：1b611322-ae32-4424-a508-144942126654
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/10/23 14:53:59
*描述：忽略表结构初始化特性
*
*=================================================
*修改标记
*修改时间：2025/10/23 14:53:59
*修改人： yswenli
*版本号： V1.0.0.0
*描述：忽略表结构初始化特性
*
*****************************************************************************/
namespace LuBan.Orm.Attributes;


/// <summary>
/// 忽略表结构初始化特性（标记在实体）
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class IgnoreTableAttribute : Attribute
{
}