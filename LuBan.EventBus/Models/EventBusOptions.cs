/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EventBus.Models
*文件名： EventBusOptions
*版本号： V2.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：事件总线配置选项
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V2.0.0.0
*描述：事件总线配置选项
*
*****************************************************************************/
namespace LuBan.EventBus.Models;

/// <summary>
/// 事件总线配置选项
/// </summary>
public class EventBusOptions
{
    /// <summary>
    /// Channel 最大容量（默认 10000）
    /// </summary>
    public int MaxQueueCapacity { get; set; } = 10000;

    /// <summary>
    /// 是否启用持久化（默认 true）
    /// </summary>
    public bool EnablePersistence { get; set; } = true;

    /// <summary>
    /// 轮询间隔（毫秒，默认 100）
    /// </summary>
    public int Sensitivities { get; set; } = 100;

    /// <summary>
    /// 最大并发处理数（默认 4）
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = 4;
}
