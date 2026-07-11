/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.EventBus
*文件名： IEventData
*版本号： V2.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：事件数据接口
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V2.0.0.0
*描述：事件数据接口
*
*****************************************************************************/
namespace LuBan.Common.EventBus;

/// <summary>
/// 事件数据接口
/// </summary>
public interface IEventData
{
    /// <summary>
    /// 事件名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 事件发生时间
    /// </summary>
    DateTime EventTime { get; set; }

    /// <summary>
    /// 事件源
    /// </summary>
    object? EventSource { get; set; }
}
