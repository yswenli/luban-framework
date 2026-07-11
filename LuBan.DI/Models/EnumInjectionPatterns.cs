/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.DI.Models
*文件名： EnumInjectionPatterns
*版本号： V1.0.0.0
*唯一标识：d1782948-2af9-4e35-adec-4322d917d102
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/13 15:38:53
*描述：注册范围
*
*=================================================
*修改标记
*修改时间：2025/8/13 15:38:53
*修改人： yswenli
*版本号： V1.0.0.0
*描述：注册范围
*
*****************************************************************************/
namespace LuBan.DI.Models;


/// <summary>
/// 注册范围
/// </summary>
public enum EnumInjectionPatterns
{
    /// <summary>
    /// 只注册自己
    /// </summary>
    [Description("只注册自己")]
    Self,

    /// <summary>
    /// 第一个接口
    /// </summary>
    [Description("只注册第一个接口")]
    FirstInterface,

    /// <summary>
    /// 自己和第一个接口，默认值
    /// </summary>
    [Description("自己和第一个接口")]
    SelfWithFirstInterface,

    /// <summary>
    /// 所有接口
    /// </summary>
    [Description("所有接口")]
    ImplementedInterfaces,

    /// <summary>
    /// 注册自己包括所有接口
    /// </summary>
    [Description("自己包括所有接口")]
    All
}
