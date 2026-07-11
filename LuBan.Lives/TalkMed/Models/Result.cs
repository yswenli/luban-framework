

namespace LuBan.Lives.TalkMed.Models;

[DataContract]
public class Result<T>
{
    [DataMember(Name = "code")]
    public int Code { get; set; }

    [DataMember(Name = "data")]
    public T Data { get; set; }

    [DataMember(Name = "message")]
    public string Message { get; set; }
}