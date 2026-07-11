namespace LuBan.Lives;

/// <summary>
/// 直播sdk
/// </summary>
public interface ILiveClient
{
    /// <summary>
    /// 获取直播地址
    /// </summary>
    /// <param name="channelId"></param>
    /// <param name="secret"></param>
    /// <param name="userId"></param>
    /// <param name="name"></param>
    /// <param name="avatar"></param>
    /// <returns></returns>
    string GetLiveUrl(string channelId, string secret, string userId, string name, string avatar);
}
