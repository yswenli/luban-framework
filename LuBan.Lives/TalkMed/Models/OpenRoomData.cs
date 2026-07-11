namespace LuBan.Lives.TalkMed.Models;

[DataContract]
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