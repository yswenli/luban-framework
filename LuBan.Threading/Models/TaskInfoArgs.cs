/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Threading.Models
*文件名： TaskInfo
*版本号： V1.0.0.0
*唯一标识：0e295286-0910-4993-bc1d-525d9352036d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/11/19 10:07:29
*描述：任务信息
*
*=================================================
*修改标记
*修改时间：2025/11/19 10:07:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：任务信息
*
*****************************************************************************/
namespace LuBan.Threading.Models;

/// <summary>
/// 任务信息
/// </summary>
public class TaskInfoArgs
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// 队列数量
    /// </summary>
    public int QueeueCount { get; set; }
    /// <summary>
    /// 排队数量
    /// </summary>
    public int PendingCount { get; set; }
    /// <summary>
    /// 运行数量
    /// </summary>
    public int RunningCount { get; set; }
    /// <summary>
    /// 成功数量
    /// </summary>
    public int SuccessCount { get; set; }
    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailCount { get; set; }
}
