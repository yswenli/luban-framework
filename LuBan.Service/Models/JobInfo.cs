/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common.Service.Models
*文件名： JobInfo
*版本号： V1.0.0.0
*唯一标识：36c4d34e-1aec-4d23-a22c-c65ca5db6183
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/9/11 14:37:09
*描述：
*
*=================================================
*修改标记
*修改时间：2023/9/11 14:37:09
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Service.Models;

/// <summary>
/// 任务信息
/// </summary>
public class JobInfo
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 类型
    /// </summary>
    public string Type { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; set; }
    /// <summary>
    /// 最近启动时间
    /// </summary>
    public DateTime? LastStarted { get; set; }
    /// <summary>
    /// 最近停止时间
    /// </summary>
    public DateTime? LastStoped { get; set; }
    /// <summary>
    /// 下次运行时间
    /// </summary>
    public DateTimeOffset? NextRunTime { get; set; }
    /// <summary>
    /// 运行次数
    /// </summary>
    public long RunTimes { get; set; }
    /// <summary>
    /// 异常次数
    /// </summary>
    public long ErrorTimes { get; set; }
    /// <summary>
    /// 最后一次异常信息
    /// </summary>
    public string LastError { get; set; }
}
