namespace WebApplication1.Models;


public class FileInput : BaseIdInput
{
    /// <summary>
    /// 文件名称
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// 文件Url
    /// </summary>
    public string? Url { get; set; }
}

public class PageFileInput : BasePageInput
{
    /// <summary>
    /// 文件名称
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
}

public class DeleteFileInput : BaseIdInput
{
}

public class UploadFileFromBase64Input
{
    /// <summary>
    /// 文件内容
    /// </summary>
    public string FileDataBase64 { get; set; }

    /// <summary>
    /// 文件类型( "image/jpeg",)
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// 文件名称
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// 保存路径
    /// </summary>
    public string Path { get; set; }
    /// <summary>
    /// 是否私有
    /// </summary>
    public bool IsPrivate { get; set; } = false;
}



public class FileOutput
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 提供者
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name => Id + Suffix;

    /// <summary>
    /// URL
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// 大小
    /// </summary>
    public string SizeKb { get; set; }

    /// <summary>
    /// 后缀
    /// </summary>
    public string Suffix { get; set; }

    /// <summary>
    /// 路径
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// 文件名称
    /// </summary>
    public string FileName { get; set; }
}


/// <summary>
/// 视频文件输出类
/// </summary>
public class VideoFileOutput : FileOutput
{

    /// <summary>
    /// PosterUrl
    /// </summary>
    public string PosterUrl { get; set; }
}