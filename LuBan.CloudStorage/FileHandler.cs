/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.CloudStorage
*文件名： FileHandler
*版本号： V1.0.0.0
*唯一标识：58b8eca9-a298-42e5-bca5-56b82f77cced
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/9 15:55:17
*描述：文件处理
*
*=================================================
*修改标记
*修改时间：2024/8/9 15:55:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：文件处理
*
*****************************************************************************/

using LuBan.VideoKit;

namespace LuBan.CloudStorage;

/// <summary>
/// 文件处理
/// </summary>
public partial class FileHandler : IScoped
{
    UploadOptions _uploadOptions;
    ICloudStorageClient _cloudStorageClient;
    readonly BaseRepository<DbFile> _sysFileRep;

    /// <summary>
    /// 文件处理
    /// </summary>
    /// <exception cref="Exception"></exception>
    public FileHandler()
    {
        var options = NacosConfigUtil.Read<UploadOptions>();
        if (options == null)
        {
            throw new Exception("在配置文件中找不到UploadOptions");
        }
        _uploadOptions = options;
        _cloudStorageClient = CloundStorageClientFactory.Create();
        _sysFileRep = new BaseRepository<DbFile>();
    }


    /// <summary>
    /// 保存文件流
    /// </summary>
    /// <param name="folderName"></param>
    /// <param name="fileName"></param>
    /// <param name="stream"></param>
    /// <param name="localFilePath"></param>
    /// <returns></returns>
    bool SaveFile(string folderName, string fileName, Stream stream, string localFilePath)
    {
        try
        {
            if (_uploadOptions.EnableCloudStorage)
            {
                folderName = folderName.Replace("\\", "/");

                if (_uploadOptions.CloudStorageOptions != null && _uploadOptions.CloudStorageOptions.EnableUploadCache)
                {
                    if (!FileUtil.Exists(localFilePath))
                    {
                        FileUtil.SaveStream(stream, localFilePath);
                    }
                    return _cloudStorageClient.Upload($"{folderName}/{fileName}", localFilePath);
                }
                else
                {
                    return _cloudStorageClient.Upload($"{folderName}/{fileName}", stream);
                }
            }
            else
            {
                using var fs = new FileStream(localFilePath, FileMode.Create, FileAccess.Write);
                stream.CopyTo(fs);
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
        return false;
    }

    /// <summary>
    /// 保存文件流
    /// </summary>
    /// <param name="folderName"></param>
    /// <param name="fileName"></param>
    /// <param name="stream"></param>
    /// <param name="localFilePath"></param>
    /// <returns></returns>
    async Task<bool> SaveFileAsync(string folderName, string fileName, Stream stream, string localFilePath)
    {
        try
        {
            if (_uploadOptions.EnableCloudStorage)
            {
                folderName = folderName.Replace("\\", "/");

                if (_uploadOptions.CloudStorageOptions != null && _uploadOptions.CloudStorageOptions.EnableUploadCache)
                {
                    if (!FileUtil.Exists(localFilePath))
                    {
                        var fs = await FileUtil.SaveStreamAsync(stream, localFilePath);
                        fs?.Dispose();
                    }
                    return await _cloudStorageClient.UploadAsync($"{folderName}/{fileName}", localFilePath);
                }
                else
                {
                    return await _cloudStorageClient.UploadAsync($"{folderName}/{fileName}", stream);
                }
            }
            else
            {
                using var fs = new FileStream(localFilePath, FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(fs);
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
        return false;
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="folderName"></param>
    /// <param name="fileName"></param>
    /// <param name="tempFile"></param>
    /// <param name="localFilePath"></param>
    /// <returns></returns>
    async Task<bool> SaveFileAsync(string folderName, string fileName, TempFile tempFile, string localFilePath)
    {
        try
        {
            if (_uploadOptions.EnableCloudStorage)
            {
                folderName = folderName.Replace("\\", "/");

                if (_uploadOptions.CloudStorageOptions != null && _uploadOptions.CloudStorageOptions.EnableUploadCache)
                {
                    using var fs = new FileStream(localFilePath, FileMode.Create, FileAccess.Write);
                    using var ts = tempFile.Open();
                    await ts.CopyToAsync(fs);
                    return await _cloudStorageClient.UploadAsync($"{folderName}/{fileName}", localFilePath);
                }
                else
                {
                    using var ts = tempFile.Open();
                    return await _cloudStorageClient.UploadAsync($"{folderName}/{fileName}", ts);
                }
            }
            else
            {
                using var fs = new FileStream(localFilePath, FileMode.Create, FileAccess.Write);
                using var ts = tempFile.Open();
                await ts.CopyToAsync(fs);
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
        return false;
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="folderName"></param>
    /// <param name="fileName"></param>
    /// <param name="fileBytes"></param>
    /// <param name="localFilePath"></param>
    public bool SaveSmallFile(string folderName, string fileName, byte[] fileBytes, string localFilePath)
    {
        using var ms = new MemoryStream(fileBytes);
        ms.Seek(0, SeekOrigin.Begin);
        return SaveFile(folderName, fileName, ms, localFilePath);
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="folderName"></param>
    /// <param name="fileName"></param>
    /// <param name="fileBytes"></param>
    /// <param name="localFilePath"></param>
    public async Task<bool> SaveSmallFileAsync(string folderName, string fileName, byte[] fileBytes, string localFilePath)
    {
        using var ms = new MemoryStream(fileBytes);
        ms.Seek(0, SeekOrigin.Begin);
        return await SaveFileAsync(folderName, fileName, ms, localFilePath);
    }

    /// <summary>
    /// 获取文件内容。
    /// 当文件较小时，使用此方法
    /// </summary>
    /// <param name="folderName"></param>
    /// <param name="fileName"></param>
    /// <param name="localFilePath"></param>
    /// <returns></returns>
    public async Task<byte[]?> GetSmallFileContentAsync(string folderName, string fileName, string localFilePath)
    {
        if (_uploadOptions.EnableCloudStorage)
        {
            folderName = folderName.Replace("\\", "/");
            return await _cloudStorageClient.DownloadContentAsync($"{folderName}/{fileName}");
        }
        else
        {
            return await FileUtil.ReadAsync(localFilePath);
        }
    }

    /// <summary>
    /// 获取文件流。
    /// 当文件较大时，使用此方法
    /// </summary>
    /// <param name="folderName"></param>
    /// <param name="fileName"></param>
    /// <param name="localFilePath"></param>
    /// <returns></returns>
    public async Task<Stream?> GetFileStreamAsync(string folderName, string fileName, string localFilePath)
    {
        if (_uploadOptions.EnableCloudStorage)
        {
            //将网络流转换成内存流
            folderName = folderName.Replace("\\", "/");
            if (_uploadOptions.CloudStorageOptions != null && _uploadOptions.CloudStorageOptions.EnableDownloadCache)
            {
                if (!FileUtil.Exists(localFilePath))
                {
                    using var stream = await _cloudStorageClient.DownloadAsync($"{folderName}/{fileName}");
                    return await FileUtil.SaveStreamAsync(stream, localFilePath);
                }
                else
                {
                    return FileUtil.Open(localFilePath);
                }
            }
            else
            {
                return await _cloudStorageClient.DownloadAsync($"{folderName}/{fileName}");
            }
        }
        else
        {
            return FileUtil.Open(localFilePath);
        }
    }

    /// <summary>
    /// 获取文件。
    /// 当文件较大时，使用此方法
    /// </summary>
    /// <param name="folderName"></param>
    /// <param name="fileName"></param>
    /// <param name="localFilePath"></param>
    /// <returns></returns>
    public async Task<TempFile?> GetFileAsync(string folderName, string fileName, string localFilePath)
    {
        if (_uploadOptions.EnableCloudStorage)
        {
            //将网络流转换成内存流
            folderName = folderName.Replace("\\", "/");
            using var stream = await _cloudStorageClient.DownloadAsync($"{folderName}/{fileName}");
            if (stream == null) return new TempFile(localFilePath);
            return await stream.DonwloadAsync();
        }
        else
        {
            return new TempFile(localFilePath);
        }
    }


    /// <summary>
    /// 根据配置验证文件
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="fileSize"></param>
    (long SizeKb, string Suffix) ValidateFile(string fileName, long fileSize)
    {
        if (fileName.IsNullOrEmpty()) throw FriendlyError.Ex(EnumErrorCode.D8006);

        var sizeKb = (long)(fileSize / 1024.0);

        if (_uploadOptions.MaxSize <= 0 || sizeKb > _uploadOptions.MaxSize)
            throw FriendlyError.Ex(EnumErrorCode.D8002);

        var suffix = Path.GetExtension(fileName).ToLower();

        if (suffix.IsNullOrEmpty())
            throw FriendlyError.Ex(EnumErrorCode.D8003);

        if (_uploadOptions.ExtensionNames == null
            || _uploadOptions.ExtensionNames.Count < 1
            || !_uploadOptions.ExtensionNames.Contains(suffix))
            throw FriendlyError.Ex(EnumErrorCode.D8001);

        return (sizeKb, suffix);
    }


    /// <summary>
    /// 获取正则表达式
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"(\{.+?})")]
    private static partial Regex GetRegex();
    /// <summary>
    /// 获取正则表达式
    /// </summary>
    /// <param name="txt"></param>
    /// <returns></returns>
    private static MatchCollection GetRegexImpl(string txt) => GetRegex().Matches(txt);


    /// <summary>
    /// 上传小文件
    /// </summary>
    /// <param name="domain"></param>
    /// <param name="rootPath"></param>
    /// <param name="bytes"></param>
    /// <param name="fileName"></param>
    /// <param name="length"></param>
    /// <param name="savePath"></param>
    /// <param name="isPrivate"></param>
    /// <returns></returns>
    public async Task<DbFile?> HandleUploadFileAsync(string domain, string rootPath, byte[] bytes, string fileName, long length, string? savePath, bool isPrivate = false)
    {
        var (sizeKb, suffix) = ValidateFile(fileName, length);

        if (savePath.IsNotNullOrEmpty())
        {
            savePath = savePath.Replace("..", "").Replace("\\", "/");
        }
        else
        {
            savePath = _uploadOptions.Path;
            var match = GetRegexImpl(savePath);
            match.ToList().ForEach(a =>
            {
                var str = DateTime.Now.ToString(a.ToString().Substring(1, a.Length - 2)); // 每天一个目录
                savePath = savePath.Replace(a.ToString(), str);
            });
        }

        // 判断是否重复上传的文件
        var fileMd5 = BitConverter.ToString(bytes.GetMD5()).Replace("-", "");

        using var locker = await LockerBuilder.Default.CreateAsync($"FileUpload_{fileMd5}");
        var oldFile = await _sysFileRep.FirstAsync(q => q.FileMd5 == fileMd5);
        if (oldFile != null && oldFile.Id > 0)
        {
            return oldFile;
        }
        var newFile = new DbFile
        {
            Id = YitIdHelper.NextId(),
            BucketName = _uploadOptions.EnableCloudStorage ? _uploadOptions?.CloudStorageOptions?.ContainerName ?? "" : "Local",
            FileName = fileName,
            Suffix = suffix,
            SizeKb = sizeKb.ToString(),
            FilePath = savePath,
            FileMd5 = fileMd5,
            Provider = "",
            IsPrivate = isPrivate
        };
        var finalName = newFile.Id + newFile.Suffix;
        var filePath = Path.Combine(rootPath, savePath);
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        var realFile = Path.Combine(filePath, finalName);

        var result = await SaveSmallFileAsync(savePath, finalName, bytes, realFile);
        if (!result)
        {
            return null;
        }
        newFile.Url = $"{domain}/api/File/Download/{newFile.Id}";
        await _sysFileRep.InsertAsync(newFile);
        return newFile;
    }

    /// <summary>
    /// 上传较大文件
    /// </summary>
    /// <param name="domain"></param>
    /// <param name="rootPath"></param>
    /// <param name="stream"></param>
    /// <param name="fileName"></param>
    /// <param name="length"></param>
    /// <param name="savePath"></param>
    /// <param name="isPrivate"></param>
    /// <returns></returns>
    public async Task<DbFile?> HandleUploadFileAsync(string domain, string rootPath, Stream stream, string fileName, long length, string? savePath, bool isPrivate = false, CancellationToken ct = default)
    {
        var (sizeKb, suffix) = ValidateFile(fileName, length);

        if (savePath.IsNotNullOrEmpty())
        {
            savePath = savePath.Replace("..", "").Replace("\\", "/");
        }
        else
        {
            savePath = _uploadOptions.Path;
            var match = GetRegexImpl(savePath);
            match.ToList().ForEach(a =>
            {
                var str = DateTime.Now.ToString(a.ToString().Substring(1, a.Length - 2)); // 每天一个目录
                savePath = savePath.Replace(a.ToString(), str);
            });
        }

        // 流式计算MD5，避免将整个流加载到内存
        using (stream)
        {
            var fileMd5 = await ComputeMd5StreamAsync(stream, ct);
            stream.Position = 0;
            
            using var locker = await LockerBuilder.Default.CreateAsync($"FileUpload_{fileMd5}");
            var oldFile = await _sysFileRep.FirstAsync(q => q.FileMd5 == fileMd5);
            if (oldFile != null && oldFile.Id > 0)
            {
                return oldFile;
            }
            var newFile = new DbFile
            {
                Id = YitIdHelper.NextId(),
                BucketName = _uploadOptions.EnableCloudStorage ? _uploadOptions?.CloudStorageOptions?.ContainerName ?? "" : "Local",
                FileName = fileName,
                Suffix = suffix,
                SizeKb = sizeKb.ToString(),
                FilePath = savePath,
                FileMd5 = fileMd5,
                Provider = "",
                IsPrivate = isPrivate
            };
            var finalName = newFile.Id + newFile.Suffix;
            var filePath = Path.Combine(rootPath, savePath);
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            var realFile = Path.Combine(filePath, finalName);
            stream.Position = 0;
            var result = await SaveFileAsync(savePath, finalName, stream, realFile);
            if (!result)
            {
                return null;
            }
            newFile.Url = $"{domain}/api/File/DownloadByName/{newFile.FileName}";
            await _sysFileRep.InsertAsync(newFile);
            return newFile;
        }
    }

    /// <summary>
    /// 上传视频文件
    /// </summary>
    /// <param name="domain"></param>
    /// <param name="rootPath"></param>
    /// <param name="videoContentStream"></param>
    /// <param name="fileName"></param>
    /// <param name="length"></param>
    /// <param name="savePath"></param>
    /// <param name="posterTime"></param>
    /// <param name="isPrivate"></param>
    /// <returns></returns>
    public async Task<DbFile?> HandleUploadVideoFileAsync(string domain,
        string rootPath,
        Stream videoContentStream, string fileName,
        long length, string? savePath,
        string posterTime = "00:00:01",
        bool isPrivate = false)
    {
        var (sizeKb, suffix) = ValidateFile(fileName, length);

        if (savePath.IsNotNullOrEmpty())
        {
            savePath = savePath.Replace("..", "").Replace("\\", "/");
        }
        else
        {
            savePath = _uploadOptions.Path;
            var match = GetRegexImpl(savePath);
            match.ToList().ForEach(a =>
            {
                var str = DateTime.Now.ToString(a.ToString().Substring(1, a.Length - 2));
                savePath = savePath.Replace(a.ToString(), str);
            });
        }

        var fileMd5 = await ComputeMd5StreamAsync(videoContentStream);
        if (videoContentStream.CanSeek)
            videoContentStream.Position = 0;

        using var locker = await LockerBuilder.Default.CreateAsync($"FileUpload_{fileMd5}");
        var oldFile = await _sysFileRep.FirstAsync(q => q.FileMd5 == fileMd5);
        if (oldFile != null && oldFile.Id > 0)
        {
            return oldFile;
        }
        var newFile = new DbFile
        {
            Id = YitIdHelper.NextId(),
            BucketName = _uploadOptions.EnableCloudStorage ? _uploadOptions?.CloudStorageOptions?.ContainerName ?? "" : "Local",
            FileName = fileName,
            Suffix = suffix,
            SizeKb = sizeKb.ToString(),
            FilePath = savePath,
            FileMd5 = fileMd5,
            Provider = "",
            IsPrivate = isPrivate
        };
        var finalName = newFile.Id + newFile.Suffix;
        var filePath = Path.Combine(rootPath, savePath);
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        var realFile = Path.Combine(filePath, finalName);
        var result = await SaveFileAsync(savePath, finalName, videoContentStream, realFile);
        if (!result)
        {
            return null;
        }
        newFile.Url = $"{domain}/api/File/DownloadByName/{newFile.FileName}";
        var enableVideoThumbnail = NacosConfigUtil.Read<bool>("HostingOptions:EnableVideoThumbnail");
        if (enableVideoThumbnail == true && VideoUtil.FFmpegPath.IsNotNullOrEmpty())
        {
            VideoUtil.ExtractPoster(realFile, posterTime, out byte[] posterContent);
            if (posterContent != null && posterContent.Length > 0)
            {
                var posterInfo = await HandleUploadFileAsync(domain, rootPath, posterContent, "poster.jpg".GetNewFilePath(), posterContent.Length, string.Empty, isPrivate);
                if (posterInfo != null)
                {
                    newFile.PosterUrl = posterInfo?.Url;
                }
            }
        }
        await _sysFileRep.InsertAsync(newFile);
        return newFile;
    }

    /// <summary>
    /// 从文件流获取MD5值
    /// </summary>
    /// <param name="formFileStream"></param>
    /// <returns></returns>
    public static string GetMd5ForStream(Stream formFileStream)
    {
        if (formFileStream.CanSeek)
            formFileStream.Seek(0, SeekOrigin.Begin);
        var md5 = MD5Util.GetMD5Str(formFileStream);
        if (formFileStream.CanSeek)
            formFileStream.Seek(0, SeekOrigin.Begin);
        return md5;
    }

    /// <summary>
    /// 流式计算MD5，避免将整个流加载到内存
    /// </summary>
    /// <param name="stream">输入流</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>MD5哈希字符串</returns>
    public static async Task<string> ComputeMd5StreamAsync(Stream stream, CancellationToken ct = default)
    {
        using var md5 = MD5.Create();
        var buffer = new byte[81920]; // 80KB buffer
        int bytesRead;
        
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
        {
            md5.TransformBlock(buffer, 0, bytesRead, null, 0);
            ct.ThrowIfCancellationRequested();
        }
        
        md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return BitConverter.ToString(md5.Hash!).Replace("-", "").ToLower();
    }
}
