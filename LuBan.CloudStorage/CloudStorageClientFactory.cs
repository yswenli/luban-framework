/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.CloudStorage
*文件名： CloudStorageClientFactory
*版本号： V1.0.0.0
*唯一标识：fb295f29-a54e-4ac5-b705-0a232b207db9
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/9 18:23:29
*描述：云存储工厂
*
*=================================================
*修改标记
*修改时间：2024/7/9 18:23:29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：云存储工厂
*
*****************************************************************************/

namespace LuBan.CloudStorage;

using System.Collections.Concurrent;

/// <summary>
/// 云存储工厂
/// </summary>
public static class CloudStorageClientFactory
{
    static ConcurrentDictionary<EnumSupplierType, ICloudStorageClient> _cache;

    /// <summary>
    /// 云存储工厂
    /// </summary>
    static CloudStorageClientFactory()
    {
        _cache = new ConcurrentDictionary<EnumSupplierType, ICloudStorageClient>();
    }

    /// <summary>
    /// 创建云存储
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public static ICloudStorageClient Create(CloudStorageOptions? config)
    {
        if (config == null) throw new ArgumentNullException("CloudStorageConfig初始化不能为空");
        
        return _cache.GetOrAdd(config.SupplierType, type =>
        {
            return type switch
            {
                EnumSupplierType.Aliyun => new AliyunStorageClient(config),
                EnumSupplierType.Azure => new AzureStorageClient(config),
                EnumSupplierType.MinIO => new MinIOStorageClient(config),
                _ => throw new ArgumentOutOfRangeException($"SupplierType:{type}未实现")
            };
        });
    }


    /// <summary>
    /// 创建云存储
    /// </summary>
    /// <returns></returns>
    public static ICloudStorageClient Create() => Create(NacosConfigUtil.Read<UploadOptions>()?.CloudStorageOptions);
}
