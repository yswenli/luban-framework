/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.CloudStorage
*文件名： CloudStorageOptions
*版本号： V1.0.0.0
*唯一标识：c65e02d2-a477-4e2d-b148-812a6ebb0b1e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/9 18:00:12
*描述：云存储配置
*
*=================================================
*修改标记
*修改时间：2024/7/9 18:00:12
*修改人： yswenli
*版本号： V1.0.0.0
*描述：云存储配置
*
*****************************************************************************/
namespace LuBan.CloudStorage;

/// <summary>
/// 云存储配置
/// </summary>
public class CloudStorageOptions
{
    /// <summary>
    /// id
    /// </summary>
    [Required]
    public string Id { get; set; }
    /// <summary>
    /// key
    /// </summary>
    [Required]
    public string Key { get; set; }


    /// <summary>
    /// 供应商类型
    /// </summary>
    [Required]
    public EnumSupplierType SupplierType { get; set; }

    /// <summary>
    /// The name of the blob container in the storage account to reference.
    /// </summary>
    [Required]
    public string ContainerName { get; set; }
    /// <summary>
    /// The name of the blob container in the storage account to reference.
    /// </summary>
    [Required]
    public string Endpoint { get; set; }

    /// <summary>
    /// 超时时间
    /// </summary>
    public int TimeOut { get; set; } = 60;
    /// <summary>
    /// 最大错误重试次数
    /// </summary>
    public int MaxErrorRetry { get; set; } = 3;

    /// <summary>
    /// 是否启用下载缓存，缓存文件到服务器本地，默认为false
    /// </summary>
    public bool EnableDownloadCache { get; set; } = false;

    /// <summary>
    /// 是否启用上传缓存，缓存文件到服务器本地，默认为false
    /// </summary>
    public bool EnableUploadCache { get; set; } = false;
}
