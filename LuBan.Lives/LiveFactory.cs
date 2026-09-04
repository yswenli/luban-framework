/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Lives
*文件名： LiveFactory.cs
*版本号： V1.0.0.0
*唯一标识：dc3ba052-b8f8-44f1-9f74-ee320a91d755
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：LiveFactory 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LiveFactory 类
*
*****************************************************************************/



namespace LuBan.Lives;

/// <summary>
/// 直播业务工厂类
/// </summary>
public static class LiveFactory
{
    /// <summary>
    /// 创建直播sdk
    /// </summary>
    /// <param name="liveEnum"></param>
    /// <returns></returns>
    public static ILiveClient Create(EnumLive liveEnum)
    {
        switch (liveEnum)
        {
            case EnumLive.TalkMed:
                return new TMLiveClient();
            case EnumLive.VZan:
                return new VZLiveClient();
            case EnumLive.YiBai:
                return new YBLiveClient();
            case EnumLive.HuiChang:
                return new HCLiveClient();
            default:
                return new TMLiveClient();
        }
    }
    /// <summary>
    /// 创建直播sdk
    /// </summary>
    /// <param name="liveEnum"></param>
    /// <param name="liveOption"></param>
    /// <returns></returns>
    public static ILiveClient Create(EnumLive liveEnum, LiveOption liveOption)
    {
        switch (liveEnum)
        {
            case EnumLive.TalkMed:
                return new TMLiveClient(liveOption);
            case EnumLive.VZan:
                return new VZLiveClient(liveOption);
            case EnumLive.YiBai:
                return new YBLiveClient(liveOption);
            case EnumLive.HuiChang:
                return new HCLiveClient(liveOption);
            default:
                return new TMLiveClient(liveOption);
        }
    }

}
