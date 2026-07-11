/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.CloudStorage.Aliyun
*文件名： AzureStorageClient
*版本号： V1.0.0.0
*唯一标识：e7a85595-28e0-4352-a764-c67a90d90330
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/9 18:30:18
*描述：微软云
*
*=================================================
*修改标记
*修改时间：2024/7/9 18:30:18
*修改人： yswenli
*版本号： V1.0.0.0
*描述：微软云
*
*****************************************************************************/
using Azure.Core.Pipeline;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace LuBan.CloudStorage.MsAzure;

/// <summary>
/// 微软云
/// </summary>
public class AzureStorageClient : ICloudStorageClient
{
    CloudStorageOptions _config;
    BlobContainerClient _blobContainerClient;

    /// <summary>
    /// 微软云
    /// </summary>
    /// <param name="config"></param>
    public AzureStorageClient(CloudStorageOptions config)
    {
        if (config.Endpoint.IsNullOrEmpty())
        {
            config.Endpoint = "core.chinacloudapi.cn";
        }
        _config = config;
        var cnnStr = $"DefaultEndpointsProtocol=https;AccountName={_config.Id};AccountKey={_config.Key};EndpointSuffix={config.Endpoint}";

        var option = new BlobClientOptions();
        option.Retry.MaxRetries = _config.MaxErrorRetry;
        option.Retry.MaxDelay = TimeSpan.FromSeconds(3);
        option.Transport = new HttpClientTransport(new HttpClient() { Timeout = TimeSpan.FromSeconds(_config.TimeOut) });
        _blobContainerClient = new BlobContainerClient(cnnStr, _config.ContainerName, option);

    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="cloundFileName"></param>
    /// <param name="localFilePath"></param>
    /// <returns></returns>
    public bool Upload(string cloundFileName, string localFilePath)
    {
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(cloundFileName);
            if (!blobClient.Exists())
            {
                var retUpload = blobClient.Upload(localFilePath);
                var blobClientUploadResponse = retUpload.GetRawResponse();
                if (!blobClientUploadResponse.IsError)
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Azure上传文件失败", ex, cloundFileName, localFilePath);
        }
        return false;
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="cloundFileName"></param>
    /// <param name="localFilePath"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> UploadAsync(string cloundFileName, string localFilePath, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(cloundFileName);
            if (!await blobClient.ExistsAsync(ct))
            {
                var retUpload = await blobClient.UploadAsync(localFilePath, ct);
                var blobClientUploadResponse = retUpload.GetRawResponse();
                if (!blobClientUploadResponse.IsError)
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Azure上传文件失败", ex, cloundFileName, localFilePath);
        }
        return false;
    }

    /// <summary>
    /// 上传文件流
    /// </summary>
    /// <param name="cloundFileName"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    public bool Upload(string cloundFileName, Stream stream)
    {
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(cloundFileName);
            if (!blobClient.Exists())
            {
                var retUpload = blobClient.Upload(stream);
                var blobClientUploadResponse = retUpload.GetRawResponse();
                if (!blobClientUploadResponse.IsError)
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Azure上传文件流失败", ex, cloundFileName);
        }
        return false;
    }


    /// <summary>
    /// 上传文件流
    /// </summary>
    /// <param name="cloundFileName"></param>
    /// <param name="stream"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> UploadAsync(string cloundFileName, Stream stream, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(cloundFileName);
            if (!await blobClient.ExistsAsync(ct))
            {
                var retUpload = await blobClient.UploadAsync(stream, ct);
                var blobClientUploadResponse = retUpload.GetRawResponse();
                if (!blobClientUploadResponse.IsError)
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Azure上传文件流失败", ex, cloundFileName);
        }
        return false;
    }

    /// <summary>
    /// 下载文件流
    /// </summary>
    /// <param name="cloundFileName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<Stream?> DownloadAsync(string cloundFileName, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(cloundFileName);

            if (await blobClient.ExistsAsync(ct))
            {
                var retDownload = await blobClient.DownloadAsync(ct);
                var blobClientDownloadResponse = retDownload.GetRawResponse();
                if (!blobClientDownloadResponse.IsError)
                {
                    return retDownload.Value.Content;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Azure下载文件流失败", ex, cloundFileName);
        }

        return null;
    }

    /// <summary>
    /// 下载文件内容
    /// </summary>
    /// <param name="cloundFileName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<byte[]?> DownloadContentAsync(string cloundFileName, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(cloundFileName);

            if (await blobClient.ExistsAsync(ct))
            {
                var retDownload = await blobClient.DownloadAsync(ct);
                var blobClientDownloadResponse = retDownload.GetRawResponse();
                if (!blobClientDownloadResponse.IsError)
                {
                    return await retDownload.Value.Content.ToBytesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Azure下载文件内容", ex, cloundFileName);
        }

        return null;
    }

    /// <summary>
    /// 获取文件SAS URI
    /// </summary>
    /// <param name="cloundFileName"></param>
    /// <param name="dateTimeOffset"></param>
    /// <returns></returns>
    public async Task<string> GetSasUri(string cloundFileName, DateTimeOffset dateTimeOffset)
    {
        BlobClient blobClient = _blobContainerClient.GetBlobClient(cloundFileName);
        var builder = new BlobSasBuilder { BlobName = cloundFileName, ExpiresOn = dateTimeOffset };
        builder.SetPermissions(BlobSasPermissions.Read);
        var blobSasUri = await Task.FromResult(blobClient.GenerateSasUri(builder));
        return blobSasUri.AbsoluteUri;
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="cloundFileName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> DeleteAsync(string cloundFileName, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(cloundFileName);

            if (await blobClient.ExistsAsync(ct))
            {
                var response = await blobClient.DeleteAsync(cancellationToken: ct);
                if (!response.IsError)
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Azure删除文件失败", ex, cloundFileName);
        }
        return false;
    }

    /// <summary>
    /// Azure中是否存在文件
    /// </summary>
    /// <param name="cloundFileName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> ExistAsync(string cloundFileName, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(cloundFileName);

            return await blobClient.ExistsAsync(ct);
        }
        catch (Exception ex)
        {
            Logger.Error("检查Azure文件是否存在失败", ex, cloundFileName);
        }
        return false;
    }
}
