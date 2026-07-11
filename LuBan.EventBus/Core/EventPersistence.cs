/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EventBus.Core
*文件名： EventPersistence
*版本号： V2.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：事件持久化服务
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V2.0.0.0
*描述：事件持久化服务
*
*****************************************************************************/
namespace LuBan.EventBus.Core;

/// <summary>
/// 事件持久化服务，使用 LocalCacheUtil
/// </summary>
public class EventPersistence
{
    private readonly EventBusOptions _options;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">配置选项</param>
    public EventPersistence(IOptions<EventBusOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// 保存事件数据
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="eventData">事件数据</param>
    /// <returns>任务</returns>
    public async Task SaveAsync<TEvent>(TEvent eventData) where TEvent : IEventData
    {
        await Task.Run(() =>
        {
            var key = $"eventbus:{typeof(TEvent).Name}:{eventData.EventTime:yyyyMMddHHmmss}";
            LocalCacheUtil.Set(key, eventData, TimeSpan.FromHours(24));
            Logger.Debug($"EventPersistence: 保存事件 {typeof(TEvent).Name}");
        });
    }

    /// <summary>
    /// 加载事件数据
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <returns>事件列表</returns>
    public async Task<IList<TEvent>> LoadAsync<TEvent>() where TEvent : IEventData
    {
        return await Task.Run(() =>
        {
            // 由于 LocalCacheUtil 不支持按前缀查询，返回空列表
            return new List<TEvent>();
        });
    }
}
