/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：System
*文件名： BaseJobService
*版本号： V1.0.0.0
*唯一标识：3f77aebb-df19-47e5-9d8c-f23ff30b39f6
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/11/24 14:03:00
*描述：LuBan.Framework 后台任务基类
*
*=================================================
*修改标记
*修改时间：2022/11/24 14:03:00
*修改人： yswen
*版本号： V1.0.0.0
*描述：LuBan.Framework 后台任务基类
*
*****************************************************************************/
namespace System;

/// <summary>
/// LuBan.Framework 后台任务基类
/// </summary>
public abstract class BaseJobService : BaseBackgroundService, IJob
{
    /// <summary>
    /// 后台工作基类
    /// </summary>
    /// <param name="intervalTime">间隔时长(ms),小于1的间隔表示只执行一次</param>
    /// <param name="sequentially">是否按顺序执行</param>
    /// <param name="userLog">启用日志</param>
    public BaseJobService(int intervalTime = 60 * 1000, bool sequentially = true, bool userLog = false)
        : base(intervalTime, sequentially, userLog)
    {
    }
    /// <summary>
    /// 后台工作基类，按时间点循环执行
    /// </summary>
    /// <param name="hour"></param>
    /// <param name="minute"></param>
    /// <param name="second"></param>
    /// <param name="once"></param>
    /// <param name="sequentially"></param>
    /// <param name="userLog"></param>
    public BaseJobService(int hour, int minute, int second, bool once = false, bool sequentially = true, bool userLog = false) : base(hour, minute, second, once, userLog)
    {

    }
    /// <summary>
    /// 后台工作基类，按时间点循环执行
    /// </summary>
    /// <param name="hourMiniteSeconds"></param>
    /// <param name="once"></param>
    /// <param name="sequentially"></param>
    /// <param name="userLog"></param>
    public BaseJobService(string hourMiniteSeconds, bool once = false, bool sequentially = true, bool userLog = false) : base(hourMiniteSeconds, once, sequentially, userLog)
    {

    }
}
