/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.EventBus
*文件名： IEventHandler
*版本号： V2.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：泛型事件处理器接口
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V2.0.0.0
*描述：泛型事件处理器接口
*
*****************************************************************************/
namespace LuBan.Common.EventBus;

/// <summary>
/// 泛型事件处理器接口
/// </summary>
/// <typeparam name="TEvent">事件类型</typeparam>
public interface IEventHandler<TEvent> where TEvent : IEventData
{
    /// <summary>
    /// 处理事件
    /// </summary>
    /// <param name="eventData">事件数据</param>
    /// <returns>任务</returns>
    Task HandleAsync(TEvent eventData);

    /// <summary>
    /// 处理异常（可选，默认记录日志）
    /// </summary>
    /// <param name="ex">异常信息</param>
    /// <returns>任务</returns>
    Task OnErrorAsync(Exception ex) => Task.CompletedTask;
}
