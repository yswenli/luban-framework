/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Orm.Enums
*文件名： EnumJobStatus
*版本号： V1.0.0.0
*唯一标识：00000000-0000-0000-0000-000000000001
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/01/13 00:00:00
*描述：作业运行状态枚举
*
*=================================================
*修改标记
*修改时间：2026/01/13 00:00:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：作业运行状态枚举
*
*****************************************************************************/
namespace LuBan.Orm.Enums;

/// <summary>
/// 作业运行状态枚举
/// </summary>
[Description("作业运行状态枚举")]
public enum EnumJobStatus
{
    /// <summary>
    /// 运行中
    /// </summary>
    [Description("运行中")]
    Running = 0,

    /// <summary>
    /// 未运行
    /// </summary>
    [Description("未运行")]
    NotRunning = 1,
}
