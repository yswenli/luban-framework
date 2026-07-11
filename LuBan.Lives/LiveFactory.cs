

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
