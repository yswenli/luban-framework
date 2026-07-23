/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.CloudStorage
*文件名： UploadOptions
*版本号： V1.0.0.0
*唯一标识：e8b3e32d-57f8-416d-b9db-78e7814a9084
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/27 18:58:48
*描述：文件上传配置选项
*
*=================================================
*修改标记
*修改时间：2023/12/27 18:58:48
*修改人： yswenli
*版本号： V1.0.0.0
*描述：文件上传配置选项
*
*****************************************************************************/
namespace LuBan.CloudStorage;

/// <summary>
/// 文件上传配置选项
/// </summary>
public sealed class UploadOptions
{
    /// <summary>
    /// 路径
    /// </summary>
    public string Path { get; set; } = "upload";

    /// <summary>
    /// 上传文件大小，
    ///  Defaults to 134,217,728 bytes, which is approximately 128MB.
    /// </summary>
    public long MaxSize { get; set; } = 134217728;
    /// <summary>
    /// 缓冲区大小，64K
    /// </summary>
    public int MemoryBufferThreshold { get; set; } = 65536;

    /// <summary>
    /// 上传文件扩展名限制
    /// </summary>
    public List<string> ExtensionNames { get; set; } = new()
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".pdf",
        ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".zip", ".rar", ".7z", ".mp4", ".avi", ".mp3"
    };

    /// <summary>
    /// 启用文件MD5验证
    /// </summary>
    /// <remarks>防止重复上传</remarks>
    public bool EnableMd5 { get; set; }

    /// <summary>
    /// 是否启用云存储
    /// </summary>
    public bool EnableCloudStorage { get; set; } = false;

    /// <summary>
    /// 集成云存储配置
    /// </summary>
    public CloudStorageOptions? CloudStorageOptions
    {
        get; set;
    }
}
