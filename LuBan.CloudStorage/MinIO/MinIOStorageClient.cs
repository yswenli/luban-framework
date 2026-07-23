/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.CloudStorage.MinIO
*文件名： MinIOStorageClient
*版本号： V1.0.0.0
*唯一标识：de890703-0a86-4c03-b6e7-21061c4a1540
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/5/26 15:15:09
*描述：
*
*=================================================
*修改标记
*修改时间：2025/5/26 15:15:09
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Encryption;

using System.IO.Pipelines;

namespace LuBan.CloudStorage.MinIO;

/// <summary>
/// minio客户端，
/// https://www.minio.org.cn/docs/minio/linux/developers/dotnet/minio-dotnet.html，
/// https://www.minio.org.cn/docs/minio/linux/developers/dotnet/API.html#
/// </summary>
public class MinIOStorageClient : ICloudStorageClient
{

    CloudStorageOptions _option;
    IMinioClient _minioClient;

    /// <summary>
    /// minio客户端
    /// </summary>
    /// <param name="options"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public MinIOStorageClient(CloudStorageOptions options)
    {
        _option = options;
        if (_option.ContainerName.IsNullOrEmpty()) throw new ArgumentNullException("ContainerName is bucketName, bucketName configuration cannot be empty");

        _minioClient = new MinioClient()
            .WithEndpoint(_option.Endpoint)
            .WithCredentials(_option.Id, _option.Key)
            .WithSSL(true)
            .Build();
    }
    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> DeleteAsync(string cloudFileName, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var args = new RemoveObjectArgs()
        .WithBucket(_option.ContainerName)
        .WithObject(cloudFileName);
        await _minioClient.RemoveObjectAsync(args);
        return true;
    }

    /// <summary>
    /// 下载文件流
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<Stream?> DownloadAsync(string cloudFileName, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var ms = new MemoryStream();
        var args = new GetObjectArgs()
            .WithBucket(_option.ContainerName)
            .WithObject(cloudFileName)
            .WithCallbackStream(async (stream, cancellationToken) =>
            {
                await stream.CopyToAsync(ms, cancellationToken);
            });

        await _minioClient.GetObjectAsync(args);
        ms.Position = 0;
        return ms;
    }
    /// <summary>
    /// 下载文件内容，适合较小文件
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<byte[]?> DownloadContentAsync(string cloudFileName, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            using var memoryStream = new MemoryStream();
            var args = new GetObjectArgs()
                .WithBucket(_option.ContainerName)
                .WithObject(cloudFileName)
                .WithCallbackStream(async (stream, cancellationToken) =>
                {
                    await stream.CopyToAsync(memoryStream, cancellationToken);
                });

            await _minioClient.GetObjectAsync(args);
            return memoryStream.ToArray();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> ExistAsync(string cloudFileName, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var statObjectArgs = new StatObjectArgs()
        .WithBucket(_option.ContainerName)
        .WithObject(cloudFileName);
        var result = await _minioClient.StatObjectAsync(statObjectArgs);
        return result != null && result.ObjectName.IsNotNullOrEmpty();
    }

    /// <summary>
    /// 获取签名地址
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="dateTimeOffset"></param>
    /// <returns></returns>
    public async Task<string> GetSasUri(string cloudFileName, DateTimeOffset dateTimeOffset)
    {
        var expiry = (int)(dateTimeOffset - DateTimeOffset.UtcNow).TotalSeconds;
        var args = new PresignedGetObjectArgs()
                                      .WithBucket(_option.ContainerName)
                                      .WithObject(cloudFileName)
                                      .WithExpiry(Math.Max(expiry, 1));
        return await _minioClient.PresignedGetObjectAsync(args);
    }

    /// <summary>
    /// 上传
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="localFilePath"></param>
    /// <returns></returns>
    public bool Upload(string cloudFileName, string localFilePath)
    {
        return Task.Run(async () => await UploadAsync(cloudFileName, localFilePath)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 上传
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    public bool Upload(string cloudFileName, Stream stream)
    {
        return Task.Run(async () => await UploadAsync(cloudFileName, stream)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 上传
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="localFilePath"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> UploadAsync(string cloudFileName, string localFilePath, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var aesEncryption = Aes.Create();
        aesEncryption.KeySize = 256;
        aesEncryption.GenerateKey();
        var ssec = new SSEC(aesEncryption.Key);

        var args = new PutObjectArgs()
        .WithBucket(_option.ContainerName)
        .WithObject(cloudFileName)
        .WithFileName(localFilePath)
        .WithContentType("application/octet-stream")
        .WithServerSideEncryption(ssec);
        var result = await _minioClient.PutObjectAsync(args);
        return result != null && result.ObjectName.IsNotNullOrEmpty();
    }

    /// <summary>
    /// 上传
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="stream"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> UploadAsync(string cloudFileName, Stream stream, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var aesEncryption = Aes.Create();
        aesEncryption.KeySize = 256;
        aesEncryption.GenerateKey();
        var ssec = new SSEC(aesEncryption.Key);

        var args = new PutObjectArgs()
        .WithBucket(_option.ContainerName)
        .WithObject(cloudFileName)
        .WithStreamData(stream)
        .WithContentType("application/octet-stream")
        .WithServerSideEncryption(ssec);
        var result = await _minioClient.PutObjectAsync(args);
        return result != null && result.ObjectName.IsNotNullOrEmpty();
    }
}
