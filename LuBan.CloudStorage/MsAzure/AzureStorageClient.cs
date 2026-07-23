/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.CloudStorage.MsAzure
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
using Azure;
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
    /// <param name="cloudFileName"></param>
    /// <param name="localFilePath"></param>
    /// <returns></returns>
    public bool Upload(string cloudFileName, string localFilePath)
    {
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(cloudFileName);
            var retUpload = blobClient.Upload(localFilePath, overwrite: true);
            var blobClientUploadResponse = retUpload.GetRawResponse();
            if (!blobClientUploadResponse.IsError)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Azure上传文件失败", ex, cloudFileName, localFilePath);
        }
        return false;
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="localFilePath"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> UploadAsync(string cloudFileName, string localFilePath, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(cloudFileName);
            var retUpload = await blobClient.UploadAsync(localFilePath, overwrite: true, cancellationToken: ct);
            var blobClientUploadResponse = retUpload.GetRawResponse();
            if (!blobClientUploadResponse.IsError)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Azure上传文件失败", ex, cloudFileName, localFilePath);
        }
        return false;
    }

    /// <summary>
    /// 上传文件流
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    public bool Upload(string cloudFileName, Stream stream)
    {
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(cloudFileName);
            var retUpload = blobClient.Upload(stream, overwrite: true);
            var blobClientUploadResponse = retUpload.GetRawResponse();
            if (!blobClientUploadResponse.IsError)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Azure上传文件流失败", ex, cloudFileName);
        }
        return false;
    }


    /// <summary>
    /// 上传文件流
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="stream"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> UploadAsync(string cloudFileName, Stream stream, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(cloudFileName);
            var retUpload = await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: ct);
            var blobClientUploadResponse = retUpload.GetRawResponse();
            if (!blobClientUploadResponse.IsError)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Azure上传文件流失败", ex, cloudFileName);
        }
        return false;
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
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(cloudFileName);
            var retDownload = await blobClient.DownloadAsync(ct);
            return retDownload.Value.Content;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
        }
        catch (Exception ex)
        {
            Logger.Error("Azure下载文件流失败", ex, cloudFileName);
        }

        return null;
    }

    /// <summary>
    /// 下载文件内容
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<byte[]?> DownloadContentAsync(string cloudFileName, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(cloudFileName);
            var retDownload = await blobClient.DownloadAsync(ct);
            return await retDownload.Value.Content.ToBytesAsync();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
        }
        catch (Exception ex)
        {
            Logger.Error("Azure下载文件内容", ex, cloudFileName);
        }

        return null;
    }

    /// <summary>
    /// 获取文件SAS URI
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="dateTimeOffset"></param>
    /// <returns></returns>
    public async Task<string> GetSasUri(string cloudFileName, DateTimeOffset dateTimeOffset)
    {
        BlobClient blobClient = _blobContainerClient.GetBlobClient(cloudFileName);
        var builder = new BlobSasBuilder { BlobName = cloudFileName, ExpiresOn = dateTimeOffset };
        builder.SetPermissions(BlobSasPermissions.Read);
        var blobSasUri = await Task.FromResult(blobClient.GenerateSasUri(builder));
        return blobSasUri.AbsoluteUri;
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> DeleteAsync(string cloudFileName, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(cloudFileName);
            var response = await blobClient.DeleteAsync(cancellationToken: ct);
            return !response.IsError;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error("Azure删除文件失败", ex, cloudFileName);
        }
        return false;
    }

    /// <summary>
    /// Azure中是否存在文件
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> ExistAsync(string cloudFileName, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(cloudFileName);

            return await blobClient.ExistsAsync(ct);
        }
        catch (Exception ex)
        {
            Logger.Error("检查Azure文件是否存在失败", ex, cloudFileName);
        }
        return false;
    }
}
