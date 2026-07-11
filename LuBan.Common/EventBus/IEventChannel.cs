/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.EventBus
*文件名： IEventChannel
*版本号： V2.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：事件 Channel 接口
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V2.0.0.0
*描述：事件 Channel 接口
*
*****************************************************************************/
using System.Threading.Channels;

namespace LuBan.Common.EventBus;

/// <summary>
/// 事件 Channel 基接口
/// </summary>
public interface IEventChannel : IDisposable
{
}

/// <summary>
/// 事件 Channel 接口
/// </summary>
/// <typeparam name="TEvent">事件类型</typeparam>
public interface IEventChannel<TEvent> : IEventChannel where TEvent : IEventData
{
    /// <summary>
    /// Channel 写入器
    /// </summary>
    ChannelWriter<TEvent> Writer { get; }
}
