/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Redis.Models
*文件名： QueueData
*版本号： V1.0.0.0
*唯一标识：4065a781-10fd-4f86-b313-6a7a9699c58b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/3/24 14:25:22
*描述：
*
*=================================================
*修改标记
*修改时间：2025/3/24 14:25:22
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Redis.Models;

/// <summary>
/// 队列数据
/// </summary>
public class QueueData
{
    /// <summary>
    /// 唯一标识
    /// </summary>
    public string Key { get; set; }
    /// <summary>
    /// 输入
    /// </summary>
    public string? Input { get; set; }
    /// <summary>
    /// 处理状态
    /// </summary>
    public EnumProcessStatus Status { get; set; }
    /// <summary>
    /// 输出
    /// </summary>
    public string? Output { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime Created { get; set; }
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? Updated { get; set; }
}
