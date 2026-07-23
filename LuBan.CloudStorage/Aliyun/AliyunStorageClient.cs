/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.CloudStorage.Aliyun
*文件名： AliyunStorageClient
*版本号： V1.0.0.0
*唯一标识：08f9cb9e-d336-49f5-82f3-f9cd41496164
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/9 18:48:43
*描述：阿里云存储
*
*=================================================
*修改标记
*修改时间：2024/7/9 18:48:43
*修改人： yswenli
*版本号： V1.0.0.0
*描述：阿里云存储
*
*****************************************************************************/
using Aliyun.OSS;
using Aliyun.OSS.Common;

namespace LuBan.CloudStorage.Aliyun;

/// <summary>
/// 阿里云存储
/// </summary>
public class AliyunStorageClient : ICloudStorageClient
{
    CloudStorageOptions _option;
    OssClient _ossClient;


    /// <summary>
    /// 阿里云存储
    /// </summary>
    /// <param name="option"></param>
    public AliyunStorageClient(CloudStorageOptions option)
    {
        _option = option;

        if (_option.ContainerName.IsNullOrEmpty()) throw new ArgumentNullException("ContainerName is bucketName, bucketName configuration cannot be empty");

        // 配置OssClient  
        var config = new ClientConfiguration
        {
            //ConnectionTimeout = _option.TimeOut, // 连接超时  
            //MaxErrorRetry = _option.MaxErrorRetry, // 最大重试次数 
            SignatureVersion = SignatureVersion.V4
        };
        //_ossClient = new OssClient($"https://{option.Endpoint}", option.Id, option.Key);
        _ossClient = new OssClient($"https://{option.Endpoint}", option.Id, option.Key, config);
        var region = _option.Endpoint.Substring("oss-", ".");
        _ossClient.SetRegion(region);
    }

    #region bucket
    /// <summary>
    /// 创建桶空间
    /// </summary>
    /// <param name="bucketName"></param>
    /// <returns></returns>
    public bool CreateBucket(string bucketName)
    {
        var bucket = _ossClient.CreateBucket(bucketName);
        return (bucket != null && bucket.Name.IsNotNullOrEmpty());
    }
    /// <summary>
    /// 删除桶空间
    /// </summary>
    /// <param name="bucketName"></param>
    public void DeleteBucket(string bucketName)
    {
        _ossClient.DeleteBucket(bucketName);
    }

    /// <summary>
    /// 列举当前账号下的所有存储空间
    /// </summary>
    /// <returns></returns>
    public List<string> GetBucketList()
    {
        List<string> result = new List<string>();
        var buckets = _ossClient.ListBuckets();
        if (buckets != null)
        {
            foreach (var item in buckets)
            {
                result.Add(item.Name);
            }
        }
        return result;
    }

    /// <summary>
    /// 判断存储空间是否存在
    /// </summary>
    /// <param name="bucketName"></param>
    /// <returns></returns>
    public bool BucketExists(string bucketName)
    {
        return _ossClient.DoesBucketExist(bucketName);
    }

    #endregion


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
            var result = await Task.Run(() => _ossClient.GetObject(_option.ContainerName, cloudFileName));
            if (result != null && result.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                return result.Content;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(new Exception("找不到此文件：" + cloudFileName, ex));
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
            var result = await Task.Run(() => _ossClient.GetObject(_option.ContainerName, cloudFileName));
            if (result != null && result.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                var stream = result.Content;
                return await stream.ToBytesAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(new Exception("找不到此文件：" + cloudFileName, ex));
        }
        return null;
    }

    /// <summary>
    /// 生成临时访问链接
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="dateTimeOffset"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<string> GetSasUri(string cloudFileName, DateTimeOffset dateTimeOffset)
    {
        try
        {
            var result = await Task.Run(() => _ossClient.GeneratePresignedUri(_option.ContainerName, cloudFileName, dateTimeOffset.UtcDateTime));
            return result?.ToString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            Logger.Error(new Exception("生成临时访问链接失败：" + cloudFileName, ex));
        }
        return string.Empty;
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
            var result = _ossClient.PutObject(_option.ContainerName, cloudFileName, localFilePath);
            if (result != null && result.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Error("上传文件到OSS失败", ex, cloudFileName, localFilePath);
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
            var result = await Task.Run(() => _ossClient.PutObject(_option.ContainerName, cloudFileName, localFilePath));
            if (result != null && result.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Error("上传文件到OSS失败", ex, cloudFileName, localFilePath);
        }
        return false;
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    public bool Upload(string cloudFileName, Stream stream)
    {
        try
        {
            if (!_ossClient.DoesObjectExist(_option.ContainerName, cloudFileName))
            {
                var result = _ossClient.PutObject(_option.ContainerName, cloudFileName, stream);
                if (result != null && result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("上传文件到OSS失败", ex, cloudFileName);
        }
        return false;
    }

    /// <summary>
    /// 上传文件
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
            if (!_ossClient.DoesObjectExist(_option.ContainerName, cloudFileName))
            {
                var result = await Task.Run(() => _ossClient.PutObject(_option.ContainerName, cloudFileName, stream));
                if (result != null && result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("上传文件到OSS失败", ex, cloudFileName);
        }
        return false;
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
            var result = _ossClient.DeleteObject(_option.ContainerName, cloudFileName);
            if (result != null && result.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Error("从OSS删除文件失败", ex, cloudFileName);
        }
        return await Task.FromResult(false);
    }

    /// <summary>
    /// OSS文件是否存在
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> ExistAsync(string cloudFileName, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            return _ossClient.DoesObjectExist(_option.ContainerName, cloudFileName);
        }
        catch (Exception ex)
        {
            Logger.Error("检查OSS文件是否存在失败", ex, cloudFileName);
        }
        return await Task.FromResult(false);
    }
}
