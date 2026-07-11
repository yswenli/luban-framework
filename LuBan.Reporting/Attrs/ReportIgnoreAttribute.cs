/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Reporting.Attrs
*文件名： ReportIgnoreAtrribute
*版本号： V1.0.0.0
*唯一标识：60c42b5b-f180-419b-9d1f-c7b2f4c5dbf2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/2/24 10:39:40
*描述：忽略属性
*
*=================================================
*修改标记
*修改时间：2025/2/24 10:39:40
*修改人： yswenli
*版本号： V1.0.0.0
*描述：忽略属性
*
*****************************************************************************/
namespace LuBan.Reporting.Attrs;

/// <summary>
/// 忽略属性
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ReportIgnoreAttribute : Attribute
{

}
