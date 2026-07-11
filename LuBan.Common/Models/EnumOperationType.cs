/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Models
*文件名： EnumOperationType
*版本号： V1.0.0.0
*唯一标识：71e6b939-71ca-43dd-afd0-e111d040140f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/11/7 10:54:49
*描述：操作类型枚举
*
*=================================================
*修改标记
*修改时间：2025/11/7 10:54:49
*修改人： yswenli
*版本号： V1.0.0.0
*描述：操作类型枚举
*
*****************************************************************************/
namespace LuBan.Common.Models;

/// <summary>
/// 操作类型枚举
/// </summary>
[Description("操作类型枚举")]
public enum EnumOperationType
{
    [Description("未知")]
    Unknown = 0,
    [Description("查询")]
    Query = 1,
    [Description("插入")]
    Insert = 2,
    [Description("更新")]
    Update = 3,
    [Description("删除")]
    Delete = 4
}
