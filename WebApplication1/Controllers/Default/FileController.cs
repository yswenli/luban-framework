using WebApplication1.Services;

namespace WebApplication1.Controllers.Default;


/// <summary>
/// 系统文件服务
/// </summary>
public class FileController : BaseApiController
{
    /// <summary>
    /// 测试下载文件
    /// </summary>
    /// <returns></returns>
    [HttpGet, AllowAnonymous, AllowAccess]
    public async Task<IActionResult> TestDownloadAsync()
    {
        var physicalPath = WebApp.GetPhysicalFilePath("upload/video/21878547202245.mp4");
        return await DownloadAsync(physicalPath);
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="path"></param>
    /// <param name="isPrivate"></param>
    /// <returns></returns>
    [DisplayName("上传文件"), HttpPost]
    public async Task<FileOutput?> UploadFileAsync(IFormFile? formFile, [FromQuery] string path = "", [FromQuery] bool isPrivate = false)
    {
        formFile = Request.Form?.Files?.FirstOrDefault() ?? null;
        if (formFile == null) throw FriendlyError.Ex(EnumErrorCode.D8005);
        return await FileService.Instance.UploadFileAsync(formFile, path, isPrivate);
    }


    /// <summary>
    /// 上传多文件
    /// </summary>
    /// <param name="form"></param>
    /// <param name="path"></param>
    /// <param name="isPrivate"></param>
    /// <returns></returns>
    [DisplayName("上传多文件"), HttpPost]
    public async Task<List<FileOutput>> UploadFilesAsync([Required] IFormCollection form, [FromQuery] string path = "", [FromQuery] bool isPrivate = false)
    {
        return await FileService.Instance.UploadFilesAsync(form.Files, isPrivate);
    }


    /// <summary>
    /// 上传小文件Base64
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("上传小文件Base64")]
    [HttpPost]
    public async Task<FileOutput?> UploadFileFromBase64Async([Required, FromBody] UploadFileFromBase64Input input)
    {
        return await FileService.Instance.UploadFileFromBase64Async(input);
    }

    /// <summary>
    /// 上传头像
    /// </summary>
    /// <param name="formFile"></param>
    /// <returns></returns>
    [DisplayName("上传头像"), HttpPost]
    public async Task<FileOutput> UploadAvatarAsync(IFormFile? formFile)
    {
        formFile = Request.Form?.Files?.FirstOrDefault() ?? null;
        if (formFile == null) throw FriendlyError.Ex(EnumErrorCode.D8005);
        return await FileService.Instance.UploadAvatarAsync(formFile, SessionUser.UserId);
    }

    /// <summary>
    /// 上传电子签名
    /// </summary>
    /// <param name="formFile"></param>
    /// <returns></returns>
    [DisplayName("上传电子签名"), HttpPost]
    public async Task<FileOutput> UploadSignatureAsync(IFormFile? formFile)
    {
        formFile = Request.Form?.Files?.FirstOrDefault() ?? null;
        if (formFile == null) throw FriendlyError.Ex(EnumErrorCode.D8005);
        return await FileService.Instance.UploadSignatureAsync(formFile, SessionUser.UserId);
    }

    /// <summary>
    /// 上传视频文件
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="posterTime"></param>
    /// <param name="isPrivate"></param>
    /// <returns></returns>
    [DisplayName("上传视频文件"), HttpPost]
    public async Task<VideoFileOutput> UploadVideoAsync([FromQuery, DefaultValue("00:00:01")] string posterTime, [FromQuery, DefaultValue(false)] bool isPrivate, IFormFile? formFile)
    {
        formFile = Request.Form?.Files?.FirstOrDefault() ?? null;
        if (formFile == null) throw FriendlyError.Ex(EnumErrorCode.D8005);
        return await FileService.Instance.UploadVideoAsync(formFile, posterTime, isPrivate);
    }


    /// <summary>
    /// 根据文件Id下载,
    /// 若需要权限，请自行调整逻辑
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [DisplayName("根据文件Id下载"), HttpGet("/api/File/Download/{id}"), AllowAnonymous]
    public async Task<IActionResult> DownloadAsync([Required] long id)
    {
        var fstream = await FileService.Instance.DownloadAsync(id);
        if (fstream == null)
        {
            return NotFound();
        }
        return await DownloadAsync(fstream.FileName, fstream.Stream);
    }

    /// <summary>
    /// 根据文件名称下载
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [DisplayName("根据文件名称下载"), HttpGet("/api/File/DownloadByName/{fileName}"), AllowAnonymous]
    public async Task<IActionResult> DownloadByNameAsync([Required] string fileName)
    {
        var fstream = await FileService.Instance.DownloadAsync(fileName);
        if (fstream == null)
        {
            return NotFound();
        }
        return await DownloadAsync(fstream.FileName, fstream.Stream);
    }

}
