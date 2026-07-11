/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Enums
*文件名： EnumJobCreateType
*版本号： V1.0.0.0
*唯一标识：5f50ad00-4f2f-4d1c-91f4-ff7f787e410c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 14:10:25
*描述：作业创建类型枚举
*
*=================================================
*修改标记
*修改时间：2023/12/4 14:10:25
*修改人： yswenli
*版本号： V1.0.0.0
*描述：作业创建类型枚举
*
*****************************************************************************/
namespace LuBan.Orm.Enums;

/// <summary>
/// 作业创建类型枚举
/// </summary>
[Description("作业创建类型枚举")]
public enum EnumJobCreateType
{
    /// <summary>
    /// 内置
    /// </summary>
    [Description("内置")]
    BuiltIn = 0,

    /// <summary>
    /// 脚本
    /// </summary>
    [Description("脚本")]
    Script = 1,

    /// <summary>
    /// HTTP请求
    /// </summary>
    [Description("HTTP请求")]
    Http = 2,
}
