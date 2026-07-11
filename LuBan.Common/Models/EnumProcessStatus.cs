/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Models
*文件名： EnumProcessStatus
*版本号： V1.0.0.0
*唯一标识：14c75eab-fe49-4a7f-bf04-93979319bbba
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/4/1 15:12:41
*描述：处理状态
*
*=================================================
*修改标记
*修改时间：2025/4/1 15:12:41
*修改人： yswenli
*版本号： V1.0.0.0
*描述：处理状态
*
*****************************************************************************/
namespace LuBan.Common.Models;


/// <summary>
/// 处理状态
/// </summary>
[Description("处理状态")]
public enum EnumProcessStatus
{
    /// <summary>
    /// 未开始
    /// </summary>
    [Description("未开始")]
    NotStart = 0,
    /// <summary>
    /// 运行中
    /// </summary>
    [Description("运行中")]
    Running = 1,
    /// <summary>
    /// 已完成
    /// </summary>
    [Description("已完成")]
    Completed = 2,
    /// <summary>
    /// 已取消
    /// </summary>
    [Description("已取消")]
    Canceled = 3,
    /// <summary>
    /// 已失败
    /// </summary>
    [Description("已失败")]
    Failed = 4
}
