/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Enums
*文件名： EnumJobResult
*版本号： V1.0.0.0
*唯一标识：00000000-0000-0000-0000-000000000002
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/01/13 00:00:00
*描述：作业运行结果枚举
*
*=================================================
*修改标记
*修改时间：2026/01/13 00:00:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：作业运行结果枚举
*
*****************************************************************************/
namespace LuBan.Orm.Enums;

/// <summary>
/// 作业运行结果枚举
/// </summary>
[Description("作业运行结果枚举")]
public enum EnumJobResult
{
    /// <summary>
    /// 成功
    /// </summary>
    [Description("成功")]
    Success = 0,

    /// <summary>
    /// 失败
    /// </summary>
    [Description("失败")]
    Failed = 1,
}
