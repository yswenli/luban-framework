/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.CloudStorage
*文件名： ICloudStorage
*版本号： V1.0.0.0
*唯一标识：b6011828-2c8f-4e54-a1ab-04afae208c2c
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/7/9 18:24:09
*描述：云存储接口
*
*=================================================
*修改标记
*修改时间：2024/7/9 18:24:09
*修改人： yswenli
*版本号： V1.0.0.0
*描述：云存储接口
*
*****************************************************************************/
namespace LuBan.CloudStorage;

/// <summary>
/// 云存储接口
/// </summary>
public interface ICloudStorageClient
{

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="localFilePath"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    bool Upload(string cloudFileName, string localFilePath);
    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="localFilePath"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> UploadAsync(string cloudFileName, string localFilePath, CancellationToken ct = default);


    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    bool Upload(string cloudFileName, Stream stream);

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="stream"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> UploadAsync(string cloudFileName, Stream stream, CancellationToken ct = default);


    /// <summary>
    /// 下载文件流，适合较大文件
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<Stream?> DownloadAsync(string cloudFileName, CancellationToken ct = default);


    /// <summary>
    /// 下载文件内容，适合较小文件
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<byte[]?> DownloadContentAsync(string cloudFileName, CancellationToken ct = default);



    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(string cloudFileName, CancellationToken ct = default);

    /// <summary>
    /// 获取文件SAS URI
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="dateTimeOffset"></param>
    /// <returns></returns>
    Task<string> GetSasUri(string cloudFileName, DateTimeOffset dateTimeOffset);

    /// <summary>
    /// 判断文件是否存在
    /// </summary>
    /// <param name="cloudFileName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<bool> ExistAsync(string cloudFileName, CancellationToken ct = default);
}
