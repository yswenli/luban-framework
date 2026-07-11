/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Reporting.Dynamic.Models
*文件名： EnumConverterType
*版本号： V1.0.0.0
*唯一标识：
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/5
*描述：转换类型枚举
*
*=================================================
*修改标记
*修改时间：2026/6/5
*修改人： yswenli
*版本号： V1.0.0.0
*描述：转换类型枚举
*
*****************************************************************************/

namespace LuBan.Reporting.Dynamic.Models;

/// <summary>
/// 转换类型枚举
/// </summary>
public enum EnumConverterType
{
    /// <summary>
    /// 不转换，原值输出
    /// </summary>
    None = 0,

    /// <summary>
    /// 简单值映射
    /// </summary>
    ValueMap = 1,

    /// <summary>
    /// Lua 脚本转换
    /// </summary>
    LuaScript = 2
}
