using LuBan.CloudStorage;

using System.Data;


namespace WebApplication1.Controllers.Mobile;

/// <summary>
/// 文件管理
/// </summary>
public class FileAdminController : BaseMobileController
{
    /// <summary>
    /// 导入
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [NoWebLoginAuth, AllowAnonymous]
    public Result Import([Required] IFormFile file)
    {
        var data = file.SaveAsDataTable();
        return SuccessResult(data);
    }


    /// <summary>
    /// 导出
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [NoWebLoginAuth, AllowAnonymous]
    public IActionResult Export()
    {
        return _env.Download("export.png");
    }

    /// <summary>
    /// 导出数据文件
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [NoWebLoginAuth, AllowAnonymous]
    public IActionResult ExportFile([FromQuery] bool isExcel = false)
    {
        var fileName = "export.xlsx";
        if (!isExcel)
        {
            fileName = "export.csv";
        }
        var dt = new DataTable();
        //return _env.ExportFile(dt, fileName);
        return ExportFile(dt, fileName);
    }


    /// <summary>
    /// 导出数据文件
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [NoWebLoginAuth, AllowAnonymous]
    public IActionResult ExportDataFile([FromQuery] bool isExcel = false)
    {
        var fileName = "export.xlsx";
        if (!isExcel)
        {
            fileName = "export.csv";
        }
        var list = new List<DbUser>
        {
            new DbUser()
            {
                Id = 1,
                RealName = "张三"
            }
        };
        //return _env.ExportFile(list, fileName);
        return ExportFile(list, fileName);
    }



    /// <summary>
    /// 测试上传文件大小
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult UploadFile(IFormFile file)
    {
        var fileStream = file.OpenReadStream();

        return fileStream.Download("test.jpg");
    }

    /// <summary>
    /// 测试限制上传文件大小
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>

    [HttpPost, RequestSizeLimit(2 * 1024 * 1024)]
    public IActionResult UploadFileByLimit(IFormFile file)
    {
        var fileStream = file.OpenReadStream();

        return fileStream.Download("test.jpg");
    }

    /// <summary>
    /// 测试不限制上传文件大小
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>

    [HttpPost, DisableRequestSizeLimit]
    public async Task<IActionResult> UploadFileByNonLimit(IFormFile file)
    {
        var fileStream = file.OpenReadStream();

        return await Task.FromResult(fileStream.Download("test.jpg"));
    }

    /// <summary>
    /// 示例获取流
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<Stream> GetStream()
    {
        var ms = new MemoryStream(RandomUtil.GetBytes(1024));
        return await Task.FromResult(ms);
    }

    /// <summary>
    /// 示例获取字节数组
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<byte[]> GetBytes()
    {
        return await Task.FromResult(RandomUtil.GetBytes(1024));
    }

    /// <summary>
    /// 上传多个文件到云存储
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    [HttpPost, NoWebLoginAuth, AllowAnonymous]
    public async Task<List<string>?> UploadFilesToClound([FromForm] IFormCollection form)
    {
        if (form.Files.Count == 0) return null;
        var result = new List<string>();
        foreach (var file in form.Files)
        {
            var path = "upload";
            var fileName = file.FileName;
            var cloundFileName = $"{path}/{fileName}";
            var cloudClient = CloundStorageClientFactory.Create();
            using (var ms = await file.SaveAsStreamAsync())
            {
                if (await cloudClient.UploadAsync(cloundFileName, ms))
                {
                    result.Add("/api/mobile/FileAdmin/DownloadFileFromClound?cloundFileName=" + cloundFileName.UrlEncode());
                }
                ;
            }
        }
        return result;
    }

    /// <summary>
    /// 从云存储下载文件
    /// </summary>
    /// <param name="cloundFileName"></param>
    /// <returns></returns>
    [HttpGet, NoWebLoginAuth, AllowAnonymous]
    public async Task<IActionResult> DownloadFileFromClound([Required] string cloundFileName)
    {
        var cloudClient = CloundStorageClientFactory.Create();
        var stream = await cloudClient.DownloadAsync(cloundFileName);
        if (stream != null)
            return await DownloadAsync(cloundFileName, stream);
        else
            return Content("文件不存在");
    }
}
