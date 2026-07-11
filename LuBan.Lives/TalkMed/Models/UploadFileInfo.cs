namespace LuBan.Lives.TalkMed.Models;

[DataContract]
public class UploadFileInfo
{
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "path")]
    public string Path { get; set; }

    [DataMember(Name = "url")]
    public string Url { get; set; }
}