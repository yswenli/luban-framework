/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Threading
*文件名： PoolTaskStatus
*版本号： V1.0.0.0
*唯一标识：5950ae7c-cc2a-4c39-89b1-9e6f8218d42c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/5/21 13:50:10
*描述：任务状态
*
*=================================================
*修改标记
*修改时间：2025/5/21 13:50:10
*修改人： yswenli
*版本号： V1.0.0.0
*描述：任务状态
*
*****************************************************************************/


namespace LuBan.Threading;

/// <summary>
/// 任务状态
/// </summary>
[Description("任务状态")]
public enum PoolTaskStatus
{
    /// <summary>
    /// 等待中
    /// </summary>
    [Description("等待中")]
    Pending = 0,
    /// <summary>
    /// 运行中
    /// </summary>
    [Description("运行中")]
    Running = 1,
    /// <summary>
    /// 成功
    /// </summary>
    [Description("成功")]
    Success = 2,
    /// <summary>
    /// 失败
    /// </summary>
    [Description("失败")]
    Failed = 3
}
