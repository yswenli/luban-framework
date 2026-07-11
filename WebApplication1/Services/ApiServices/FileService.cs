using LuBan.CloudStorage;

using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using WebApplication1.Models.Vos;

namespace WebApplication1.Services;

/// <summary>
/// 系统文件服务
/// </summary>
public partial class FileService : BaseService<FileService>
{
    private readonly DbRepository<DbFile> _sysFileRep;
    private readonly UploadOptions _uploadOptions;
    private readonly FileHandler _fileHandler;

    /// <summary>
    /// 系统文件服务
    /// </summary>
    public FileService()
    {
        _sysFileRep = new DbRepository<DbFile>();
        _uploadOptions = ConfigUtil.Read<UploadOptions>() ?? throw new Exception("uploadOptions can not be null");
        _fileHandler = new FileHandler();
    }

    /// <summary>
    /// 获取文件分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedList<DbFile>> PageAsync(PageFileInput input)
    {
        return await _sysFileRep
            .WhereIF(!string.IsNullOrWhiteSpace(input.FileName), u => u.FileName!.Contains(input.FileName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StartTime.ToString()) && !string.IsNullOrWhiteSpace(input.EndTime.ToString()),
                        u => u.CreateTime >= input.StartTime && u.CreateTime <= input.EndTime)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input);
    }

    /// <summary>
    /// 获取目录
    /// </summary>
    /// <param name="savePath"></param>
    /// <returns></returns>
    private string GetSavePath(string? savePath)
    {
        var env = EnvironmentUtil.GetEnvironment() ?? "dev";
        if (savePath.IsNullOrEmpty())
        {
            savePath = _uploadOptions.Path;
            MatchCollection match = GetRegexImpl(savePath);
            match.ToList().ForEach(delegate (Match a)
            {
                string newValue = DateTime.Now.ToString(a.ToString().Substring(1, a.Length - 2));
                savePath = savePath.Replace(a.ToString(), newValue);
            });
            return savePath.Replace("upload/", "upload/" + EnvironmentUtil.GetEnvironment() + "/").Replace("..", "").Replace("\\", "/"); ;
        }
        else
        {
            savePath = savePath.Replace(".", "").Replace("~", "").Replace("/", "").Replace("\\", "");
            return Path.Combine("upload", EnvironmentUtil.GetEnvironment(), savePath).Replace("..", "").Replace("\\", "/");
        }
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="file"></param>
    /// <param name="savePath"></param>
    /// <param name="isPrivate"></param>
    /// <returns></returns>
    public async Task<FileOutput?> UploadFileAsync(IFormFile file, string savePath, bool isPrivate = false)
    {
        var sysFile = await HandleUploadFile(file, savePath, isPrivate);
        if (sysFile == null) return null;
        return sysFile.Adapt<FileOutput>();
    }

    /// <summary>
    /// 上传小图片Base64
    /// </summary>
    /// <param name="strBase64"></param>
    /// <param name="fileName"></param>
    /// <param name="savePath"></param>
    /// <param name="isPrivate"></param>
    /// <returns></returns>
    public async Task<FileOutput?> UploadSmallFileFromBase64Async(string strBase64, string fileName, string? savePath, bool isPrivate = false)
    {
        byte[] fileData = Convert.FromBase64String(strBase64);
        var sysFile = await new FileHandler().HandleUploadFileAsync(HostingOptions.Default.Domain, WebApp.WebHostEnvironment?.ContentRootPath ?? "", fileData, fileName, (int)fileData.Length, GetSavePath(savePath), isPrivate);
        if (sysFile == null) return null;
        return sysFile.Adapt<FileOutput>();
    }


    /// <summary>
    /// 上传文件Base64
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<FileOutput?> UploadFileFromBase64Async(UploadFileFromBase64Input input)
    {
        return await UploadSmallFileFromBase64Async(input.FileDataBase64, input.FileName, input.Path, input.IsPrivate);
    }


    /// <summary>
    /// 上传多文件
    /// </summary>
    /// <param name="files"></param>
    /// <param name="isPrivate"></param>
    /// <returns></returns>
    public async Task<List<FileOutput>> UploadFilesAsync([Required] IFormFileCollection files, bool isPrivate = false)
    {
        var filelist = new List<FileOutput>();
        foreach (var file in files)
        {
            var data = await UploadFileAsync(file, "", isPrivate);
            if (data != null)
                filelist.Add(data);
        }
        return filelist;
    }

    /// <summary>
    /// 根据文件Id下载文件
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<FStream?> DownloadAsync(long id, int userId = 0)
    {
        var file = await _sysFileRep.FirstAsync(u => u.Id == id);
        if (file == null || file.Id < 1) return null;
        //若需要权限，请自行调整逻辑
        //if (file.IsPrivate)
        //{
        //    var user = await _sysFileRep.Change<DbUser>().FirstAsync(u => u.Id == userId);
        //    if (user == null || user.Id < 1) return new JsonResult(ErrorResult("私有文件访问权限不足"));
        //    if (userId != file.CreateUserId && userId != 1300000000101)
        //    {
        //        return new JsonResult(ErrorResult("私有文件访问权限不足"));
        //    }
        //}
        var fileName = HttpUtility.UrlEncode(file.FileName, Encoding.GetEncoding("UTF-8"));
        if (fileName.IsNullOrEmpty()) return null;

        var finalName = $"{file.Id}{file.Suffix}";
        var filePath = Path.Combine(file.FilePath ?? "", finalName);
        var physicalFilePath = Path.Combine(WebApp.WebHostEnvironment.ContentRootPath, filePath);
        var cloudFileName = file.FilePath + "/" + finalName;
        var stream = await _fileHandler.GetFileStreamAsync(file.FilePath ?? filePath, finalName, physicalFilePath);

        if (stream == null) return null;

        return new FStream(fileName, stream);
    }

    /// <summary>
    /// 根据文件名下载文件
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public async Task<FStream?> DownloadAsync(string fileName)
    {
        var file = await _sysFileRep.FirstAsync(u => u.FileName == fileName);
        if (file == null || file.Id < 1) return null;

        fileName = HttpUtility.UrlEncode(file.FileName, Encoding.GetEncoding("UTF-8")) ?? "";
        if (fileName.IsNullOrEmpty()) return null;

        //若需要权限，请自行调整逻辑
        //if (file.IsPrivate)
        //{
        //    var user = await _sysFileRep.Change<DbUser>().FirstAsync(u => u.Id == userId);
        //    if (user == null || user.Id < 1) return new JsonResult(ErrorResult("私有文件访问权限不足"));
        //    if (userId != file.CreateUserId && userId != 1300000000101)
        //    {
        //        return new JsonResult(ErrorResult("私有文件访问权限不足"));
        //    }
        //}

        var finalName = $"{file.Id}{file.Suffix}";
        var filePath = Path.Combine(file.FilePath ?? "", finalName);
        var physicalFilePath = Path.Combine(WebApp.WebHostEnvironment.ContentRootPath, filePath);
        var cloudFileName = file.FilePath + "/" + finalName;
        var stream = await _fileHandler.GetFileStreamAsync(file.FilePath ?? "", finalName, physicalFilePath);
        if (stream == null) return null;

        return new FStream(fileName, stream);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> DeleteFileAsync(DeleteFileInput input)
    {
        var file = await _sysFileRep.FirstAsync(u => u.Id == input.Id);
        if (file != null)
        {
            var finalName = $"{file.Id}{file.Suffix}";
            var filePath = Path.Combine(WebApp.WebHostEnvironment.ContentRootPath, file.FilePath ?? "", finalName);
            if (File.Exists(filePath))
                File.Delete(filePath);
            if (file.BucketName != "Local")
            {
                var cloudFileName = file.FilePath + "/" + finalName;
                await CloundStorageClientFactory.Create().DeleteAsync(cloudFileName);
            }
            return await _sysFileRep.DeleteAsync(file); ;
        }
        return false;
    }


    /// <summary>
    /// 更新文件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<bool> UpdateFileAsync(FileInput input)
    {
        var isExist = await _sysFileRep.IsAnyAsync(u => u.Id == input.Id);
        if (!isExist) throw FriendlyError.Ex(EnumErrorCode.D8000);
        return await _sysFileRep.UpdateAsync(u => new DbFile() { FileName = input.FileName }, u => u.Id == input.Id);
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="file"></param>
    /// <param name="savePath"></param>
    /// <param name="isPrivate"></param>
    /// <returns></returns>
    private async Task<DbFile?> HandleUploadFile(IFormFile file, string savePath, bool isPrivate = false)
    {
        if (file == null) throw FriendlyError.Ex(EnumErrorCode.D8000);
        using var fileStream = file.OpenReadStream();
        if (fileStream == null) throw FriendlyError.Ex(EnumErrorCode.D8000);
        var fileName = file.FileName;
        return await new FileHandler().HandleUploadFileAsync(HostingOptions.Default.Domain, WebApp.WebHostEnvironment?.ContentRootPath ?? "", fileStream.ToBytes() ?? [], fileName, (int)fileStream.Length, GetSavePath(savePath), isPrivate);
    }


    /// <summary>
    /// 上传视频文件
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="savePath"></param>
    /// <param name="posterTime"></param>
    /// <param name="isPrivate"></param>
    /// <returns></returns>
    private async Task<VideoFileOutput?> HandleUploadVideoFile(IFormFile formFile, string savePath, string posterTime = "00:00:01", bool isPrivate = false)
    {
        if (formFile == null) throw FriendlyError.Ex(EnumErrorCode.D8005);
        using var videoContentStream = formFile.OpenReadStream() ?? throw FriendlyError.Ex(EnumErrorCode.D8005);
        var fileName = formFile.FileName;
        var dbFileInfo = await new FileHandler().HandleUploadVideoFileAsync(HostingOptions.Default.Domain, WebApp.WebHostEnvironment?.ContentRootPath ?? "", videoContentStream, fileName, (int)videoContentStream.Length, GetSavePath(savePath), posterTime, isPrivate);
        return dbFileInfo?.Adapt<VideoFileOutput>();
    }

    /// <summary>
    /// 获取正则表达式
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"(\{.+?})")]
    private partial Regex GetRegex();
    /// <summary>
    /// 获取正则表达式
    /// </summary>
    /// <param name="txt"></param>
    /// <returns></returns>
    private MatchCollection GetRegexImpl(string txt) => GetRegex().Matches(txt);

    /// <summary>
    /// 上传头像
    /// </summary>
    /// <param name="file"></param>
    /// <param name="userID"></param>
    /// <returns></returns>
    public async Task<FileOutput> UploadAvatarAsync(IFormFile file, long userID)
    {
        var sysUserRep = _sysFileRep.ChangeRepository<DbUser>();
        var user = await sysUserRep.FirstAsync(u => u.Id == userID);
        if (user == null) throw FriendlyError.Ex("上传头像失败，不存此用户");
        // 删除当前用户已有头像
        if (!string.IsNullOrWhiteSpace(user.Avatar))
        {
            var fileId = Path.GetFileNameWithoutExtension(user.Avatar);
            if (long.TryParse(fileId, out long id))
            {
                await DeleteFileAsync(new DeleteFileInput { Id = id });
            }
        }
        var res = await UploadFileAsync(file, "avatar", true) ?? throw FriendlyError.Ex("上传头像失败");
        await sysUserRep.UpdateAsync(u => new DbUser() { Avatar = res.Url }, u => u.Id == user.Id);
        return res;
    }


    /// <summary>
    /// 上传电子签名
    /// </summary>
    /// <param name="file"></param>
    /// <param name="userID"></param>
    /// <returns></returns>
    public async Task<FileOutput> UploadSignatureAsync(IFormFile file, long userID)
    {
        var sysUserRep = _sysFileRep.ChangeRepository<DbUser>();
        var user = await sysUserRep.FirstAsync(u => u.Id == userID);
        if (user == null) throw FriendlyError.Ex("上传头像失败，不存此用户");
        // 删除当前用户已有电子签名
        if (!string.IsNullOrWhiteSpace(user.Signature) && user.Signature.EndsWith(".png"))
        {
            var fileId = Path.GetFileNameWithoutExtension(user.Signature);
            await DeleteFileAsync(new DeleteFileInput { Id = long.Parse(fileId) });
        }
        var res = await UploadFileAsync(file, "signature", true) ?? throw FriendlyError.Ex("上传电子签名失败");
        await sysUserRep.UpdateAsync(u => new DbUser() { Signature = res.Url }, u => u.Id == user.Id);
        return res;
    }

    /// <summary>
    /// 上传视频
    /// </summary>
    /// <param name="file"></param>
    /// <param name="posterTime"></param>
    /// <param name="isPrivate"></param>
    /// <returns></returns>
    public async Task<VideoFileOutput> UploadVideoAsync(IFormFile file, string posterTime, bool isPrivate)
    {
        var videoFile = await HandleUploadVideoFile(file, "video", posterTime, isPrivate);
        if (videoFile == null) throw FriendlyError.Ex("上传视频失败");
        return videoFile;
    }

}