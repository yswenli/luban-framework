/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EventBus.Models
*文件名： BaseEventData
*版本号： V2.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：事件数据基类
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V2.0.0.0
*描述：事件数据基类
*
*****************************************************************************/
namespace LuBan.EventBus.Models;

/// <summary>
/// 事件数据基类
/// </summary>
public class BaseEventData : IEventData
{
    /// <summary>
    /// 事件名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 事件发生时间
    /// </summary>
    public DateTime EventTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 事件源
    /// </summary>
    public object? EventSource { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public BaseEventData()
    {
        Name = GetType().Name;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="name">事件名称</param>
    public BaseEventData(string name)
    {
        Name = name;
    }
}
