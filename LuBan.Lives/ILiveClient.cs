/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Lives
*文件名： ILiveClient.cs
*版本号： V1.0.0.0
*唯一标识：f8182362-550d-46b8-8a45-2f4db1ec2526
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：ILiveClient 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ILiveClient 类
*
*****************************************************************************/

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
