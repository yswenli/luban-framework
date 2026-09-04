/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：WebApplication1.Models.Vos
*文件名： FileInput.cs
*版本号： V1.0.0.0
*唯一标识：8c670944-d835-48fc-a8ba-a54382f2abed
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：FileInput 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：FileInput 类
*
*****************************************************************************/

namespace WebApplication1.Models.Vos;


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