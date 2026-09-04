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

    [DataMember(Name = "url")]
    public string Url { get; set; }

    [DataMember(Name = "password")]
    public string Password { get; set; }

    [DataMember(Name = "title")]
    public string Title { get; set; }
}

[DataContract]
public class SpeakerUrl
{

    [DataMember(Name = "url")]
    public string Url { get; set; }

    [DataMember(Name = "password")]
    public string Password { get; set; }

    [DataMember(Name = "title")]
    public string Title { get; set; }
}

[DataContract]
public class WatcherUrl
{

    [DataMember(Name = "url")]
    public string Url { get; set; }

    [DataMember(Name = "password")]
    public string Password { get; set; }

    [DataMember(Name = "title")]
    public string Title { get; set; }
}

[DataContract]
public class OpenRoomData
{

    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "room_id")]
    public long RoomId { get; set; }

    [DataMember(Name = "host_url")]
    public HostUrl HostUrl { get; set; }

    [DataMember(Name = "speaker_url")]
    public List<SpeakerUrl> SpeakerUrl { get; set; }

    [DataMember(Name = "watcher_url")]
    public WatcherUrl WatcherUrl { get; set; }

    [DataMember(Name = "line")]
    public List<string> Line { get; set; }
}