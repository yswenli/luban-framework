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
using System.Text.Json;

namespace LuBan.EventBus.Core;

/// <summary>
/// 事件持久化服务，使用 LocalCacheUtil
/// </summary>
public class EventPersistence
{
    private const string KEY_LIST_PREFIX = "eventbus:keys:";
    private const int MAX_LOAD_COUNT = 1000;
    private static readonly object _lock = new();

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
            var eventKey = $"eventbus:{typeof(TEvent).Name}:{Guid.NewGuid():N}";
            LocalCacheUtil.Set(eventKey, eventData, TimeSpan.FromHours(24));

            var keyListKey = KEY_LIST_PREFIX + typeof(TEvent).Name;

            lock (_lock)
            {
                var keysJson = LocalCacheUtil.Get<string>(keyListKey);
                var keys = string.IsNullOrEmpty(keysJson)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(keysJson) ?? new List<string>();

                keys.Add(eventKey);
                LocalCacheUtil.Set(keyListKey, JsonSerializer.Serialize(keys), TimeSpan.FromHours(24));
            }

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
            var keyListKey = KEY_LIST_PREFIX + typeof(TEvent).Name;
            var keysJson = LocalCacheUtil.Get<string>(keyListKey);
            if (string.IsNullOrEmpty(keysJson))
                return new List<TEvent>();

            var keys = JsonSerializer.Deserialize<List<string>>(keysJson) ?? new List<string>();
            var events = new List<TEvent>();

            foreach (var key in keys.Take(MAX_LOAD_COUNT))
            {
                var eventData = LocalCacheUtil.Get<TEvent>(key);
                if (eventData != null)
                    events.Add(eventData);
            }

            Logger.Debug($"EventPersistence: 加载事件 {typeof(TEvent).Name} 共 {events.Count} 条");
            return events;
        });
    }
}
