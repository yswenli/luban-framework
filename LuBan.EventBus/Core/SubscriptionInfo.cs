/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.EventBus.Core
*文件名： SubscriptionInfo
*版本号： V2.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：订阅信息模型
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V2.0.0.0
*描述：订阅信息模型
*
*****************************************************************************/
namespace LuBan.EventBus.Core;

/// <summary>
/// 订阅信息
/// </summary>
internal class SubscriptionInfo
{
    /// <summary>
    /// 处理器类型
    /// </summary>
    public Type HandlerType { get; }

    /// <summary>
    /// 是否一次性订阅
    /// </summary>
    public bool IsOnce { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="handlerType">处理器类型</param>
    /// <param name="isOnce">是否一次性订阅</param>
    public SubscriptionInfo(Type handlerType, bool isOnce)
    {
        HandlerType = handlerType;
        IsOnce = isOnce;
    }
}
