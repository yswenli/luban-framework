/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.DI.Models
*文件名： EnumInjectionActions
*版本号： V1.0.0.0
*唯一标识：958de745-c136-46db-99ac-890c5327c6df
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/13 15:37:46
*描述：服务注册方式
*
*=================================================
*修改标记
*修改时间：2025/8/13 15:37:46
*修改人： yswenli
*版本号： V1.0.0.0
*描述：服务注册方式
*
*****************************************************************************/


namespace LuBan.DI.Models;


/// <summary>
/// 服务注册方式
/// </summary>
public enum EnumInjectionActions
{
    /// <summary>
    /// 如果存在则覆盖
    /// </summary>
    [Description("存在则覆盖")]
    Add,

    /// <summary>
    /// 如果存在则跳过，默认方式
    /// </summary>
    [Description("存在则跳过")]
    TryAdd
}
