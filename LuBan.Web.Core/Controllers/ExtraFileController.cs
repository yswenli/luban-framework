/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.Controls
*文件名： BigFileController
*版本号： V1.0.0.0
*唯一标识：00638a4e-c3e2-4d51-af7d-1b577f8f6599
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/9/10 17:19:24
*描述：大文件控制器
*
*=================================================
*修改标记
*修改时间：2024/9/10 17:19:24
*修改人： yswenli
*版本号： V1.0.0.0
*描述：大文件控制器
*
*****************************************************************************/

namespace LuBan.Web.Core.Controllers;

/// <summary>
/// 大文件控制器
/// </summary>
public sealed class ExtraFileController : BaseApiController
{
    static readonly int _bufferSize = 10 * 1024 * 1024;
    static readonly UploadOptions _uploadOptions = ConfigUtil.Read<UploadOptions>() ?? throw new Exception("上传配置文件不存在");

    /// <summary>
    /// 上传大文件
    /// </summary>
    /// <returns></returns>
    [DisableRequestSizeLimit, HttpPost, SwaggerUIUploadFiles, ClearFormValueModelBinding]
    public async Task<Result> Upload(CancellationToken ct)
    {
        var tempFiles = new List<string>();
        try
        {
            var result = new List<string>();
            var request = HttpContext.Request;
            var boundary = request.GetMultipartBoundary();
            if (string.IsNullOrEmpty(boundary))
            {
                return ErrorResult("boundary is null");
            }

            using (var bodyStream = request.Body)
            {
                var reader = new MultipartReader(boundary, bodyStream, _bufferSize);
                var section = await reader.ReadNextSectionAsync(ct);
                while (section != null && !ct.IsCancellationRequested)
                {
                    ContentDispositionHeaderValue? header = section?.GetContentDispositionHeader() ?? null;
                    if (header != null && (header.FileName.HasValue || header.FileNameStar.HasValue))
                    {
                        var fileSection = section?.AsFileSection();
                        var originalFileName = fileSection?.FileName;
                        
                        if (!ValidateExtension(originalFileName))
                        {
                            section = await reader.ReadNextSectionAsync(ct);
                            continue;
                        }

                        var tempFilePath = Path.GetTempFileName();
                        tempFiles.Add(tempFilePath);

                        if (fileSection?.FileStream != null)
                        {
                            using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                            {
                                await fileSection.FileStream.CopyToAsync(fs, _bufferSize, ct);
                            }

                            var fileInfo = new FileInfo(tempFilePath);
                            var fileSize = fileInfo.Length;

                            if (!ValidateFileSize(fileSize))
                            {
                                FileUtil.Delete(tempFilePath);
                                tempFiles.Remove(tempFilePath);
                                section = await reader.ReadNextSectionAsync(ct);
                                continue;
                            }

                            using var fs2 = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read);
                            var dbFile = await ServiceProviderUtil.GetRequiredService<FileHandler>().HandleUploadFileAsync(
                                HostingOptions.Default.Domain,
                                WebApp.WebHostEnvironment?.ContentRootPath ?? "",
                                fs2,
                                originalFileName ?? "unknown",
                                fileSize,
                                null,
                                false,
                                ct);

                            if (dbFile != null)
                            {
                                result.Add(dbFile.Url ?? $"/api/File/Download/{dbFile.Id}");
                            }

                            FileUtil.Delete(tempFilePath);
                            tempFiles.Remove(tempFilePath);
                        }
                    }
                    section = await reader.ReadNextSectionAsync(ct);
                }
            }
            return SuccessResult(result);
        }
        catch (OperationCanceledException)
        {
            foreach (var tempFile in tempFiles)
            {
                if (FileUtil.Exists(tempFile))
                    FileUtil.Delete(tempFile);
            }
            return ErrorResult("上传已取消");
        }
        catch (Exception ex)
        {
            foreach (var tempFile in tempFiles)
            {
                if (FileUtil.Exists(tempFile))
                    FileUtil.Delete(tempFile);
            }
            return ErrorResult(ex);
        }
    }

    private static bool ValidateExtension(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return false;
        
        var extension = Path.GetExtension(fileName).ToLower();
        if (string.IsNullOrEmpty(extension)) return false;
        
        if (_uploadOptions.ExtensionNames == null || _uploadOptions.ExtensionNames.Count == 0)
            return true;
        
        return _uploadOptions.ExtensionNames.Contains(extension);
    }

    private static bool ValidateFileSize(long fileSize)
    {
        var sizeKb = (long)(fileSize / 1024.0);
        return _uploadOptions.MaxSize <= 0 || sizeKb <= _uploadOptions.MaxSize;
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Download(string fileName)
    {
        var filePath = Path.Combine(WebApp.HostingOptions.AppOptions.StaticPaths[0], fileName);
        if (!IsPathSafe(filePath))
        {
            return BadRequest("Invalid file path");
        }
        
        if (FileUtil.Exists(filePath)) { return PhysicalFile(filePath, "application/octet-stream", fileName); }
        return NotFound();
    }

    private static bool IsPathSafe(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        
        var fullPath = Path.GetFullPath(path);
        var basePath = Path.GetFullPath(WebApp.GetPhysicalFilePath(""));
        
        return fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase);
    }
}
