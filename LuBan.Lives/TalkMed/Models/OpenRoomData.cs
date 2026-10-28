/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Lives.TalkMed.Models
*文件名： OpenRoomData.cs
*版本号： V1.0.0.0
*唯一标识：7daa5fad-22e4-4961-ad41-75f61285478e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:28
*描述：OpenRoomData 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:28
*修改人： yswenli
*版本号： V1.0.0.0
*描述：OpenRoomData 类
*
*****************************************************************************/

namespace LuBan.Lives.TalkMed.Models;

[DataContract]
/// <summary>
/// HostUrl 类
/// </summary>
public class HostUrl
{

    /// <summary>
    /// 主播端进入房间的直播地址（接口字段 url）。
    /// </summary>
    [DataMember(Name = "url")]
    public string Url { get; set; }

    /// <summary>
    /// 主播端进入房间的口令（接口字段 password）。
    /// </summary>
    [DataMember(Name = "password")]
    public string Password { get; set; }

    /// <summary>
    /// 主播端入口的标题（接口字段 title）。
    /// </summary>
    [DataMember(Name = "title")]
    public string Title { get; set; }
}

/// <summary>
/// 嘉宾（演讲者）端进入房间的地址与口令信息。
/// </summary>
[DataContract]
public class SpeakerUrl
{

    /// <summary>
    /// 嘉宾端进入房间的直播地址（接口字段 url）。
    /// </summary>
    [DataMember(Name = "url")]
    public string Url { get; set; }

    /// <summary>
    /// 嘉宾端进入房间的口令（接口字段 password）。
    /// </summary>
    [DataMember(Name = "password")]
    public string Password { get; set; }

    /// <summary>
    /// 嘉宾端入口的标题（接口字段 title）。
    /// </summary>
    [DataMember(Name = "title")]
    public string Title { get; set; }
}

/// <summary>
/// 观众端进入房间的地址与口令信息。
/// </summary>
[DataContract]
public class WatcherUrl
{

    /// <summary>
    /// 观众端进入房间的观看地址（接口字段 url）。
    /// </summary>
    [DataMember(Name = "url")]
    public string Url { get; set; }

    /// <summary>
    /// 观众端进入房间的口令（接口字段 password）。
    /// </summary>
    [DataMember(Name = "password")]
    public string Password { get; set; }

    /// <summary>
    /// 观众端入口的标题（接口字段 title）。
    /// </summary>
    [DataMember(Name = "title")]
    public string Title { get; set; }
}

/// <summary>
/// 创建房间接口返回的数据，包含主播、嘉宾与观众三端的入场信息。
/// </summary>
[DataContract]
public class OpenRoomData
{

    /// <summary>
    /// 房间记录 Id（接口字段 id）。
    /// </summary>
    [DataMember(Name = "id")]
    public int Id { get; set; }

    /// <summary>
    /// 房间编号（接口字段 room_id）。
    /// </summary>
    [DataMember(Name = "room_id")]
    public long RoomId { get; set; }

    /// <summary>
    /// 主播端入场信息（接口字段 host_url）。
    /// </summary>
    [DataMember(Name = "host_url")]
    public HostUrl HostUrl { get; set; }

    /// <summary>
    /// 各嘉宾端入场信息列表（接口字段 speaker_url）。
    /// </summary>
    [DataMember(Name = "speaker_url")]
    public List<SpeakerUrl> SpeakerUrl { get; set; }

    /// <summary>
    /// 观众端入场信息（接口字段 watcher_url）。
    /// </summary>
    [DataMember(Name = "watcher_url")]
    public WatcherUrl WatcherUrl { get; set; }

    /// <summary>
    /// 可用的推流或访问线路列表（接口字段 line）。
    /// </summary>
    [DataMember(Name = "line")]
    public List<string> Line { get; set; }
}