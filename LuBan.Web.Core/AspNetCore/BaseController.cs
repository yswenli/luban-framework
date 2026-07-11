/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core.AspNetCore
*文件名： BaseController
*版本号： V1.0.0.0
*唯一标识：647416bb-c6b2-4821-b0bf-c9dd09999052
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2021/8/19 11:15:10
*描述：LuBan.Web.Core.AspNetCore 基础控制器类
*
*=================================================
*修改标记
*修改时间：2024/8/19 11:15:10
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBan.Web.Core.AspNetCore 基础控制器类
*
*****************************************************************************/

namespace LuBan.Web.Core.AspNetCore;

/// <summary>
/// LuBan.Web.Core.AspNetCore 基础控制器类
/// </summary>
[ApiResultConvertion]
public abstract class BaseController : Controller
{
    /// <summary>
    /// aspnetcore环境参数
    /// </summary>
    protected IWebHostEnvironment _env;

    /// <summary>
    /// 应用配置参数
    /// </summary>
    protected HostingOptions _hostingOptions;

    /// <summary>
    /// 框架中基础控制器类
    /// </summary>
    public BaseController()
    {
        _env = WebApp.WebHostEnvironment;
        _hostingOptions = HostingOptions.Default;
        if (_hostingOptions == null || _hostingOptions.AppOptions == null)
        {
            throw new Exception("配置文件读取失败");
        }
    }

    /// <summary>
    /// 目录路径
    /// </summary>
    protected string ContentPath
    {
        get
        {
            return _env.ContentRootPath;
        }
    }

    /// <summary>
    /// 根据相对地址中获取完整路径
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    protected string GetMapPath(string filePath)
    {
        return WebApp.GetPhysicalPath(filePath);
    }

    /// <summary>
    /// 导入数据，支持excel csv tsv等文件
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    protected DataTable SaveAsDataTable(IFormFile file)
    {
        return file.SaveAsDataTable();
    }

    /// <summary>
    /// 导入数据，支持excel csv tsv等文件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="file"></param>
    /// <returns></returns>
    protected List<T> SaveAsModels<T>(IFormFile file) where T : class, new()
    {
        return file.SaveAsModels<T>();
    }

    /// <summary>
    /// 导出列表数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="fileName"></param>
    /// <param name="columnNameList"></param>
    /// <param name="splitStr"></param>
    /// <param name="namePairs"></param>
    /// <returns></returns>
    protected IActionResult ExportFile<T>(IEnumerable<T> data,
            string fileName,
            IEnumerable<string>? columnNameList = null,
            string splitStr = ",",
            IEnumerable<NamePair>? namePairs = null) where T : class, new()
    {
        return _env.ExportFile(data, fileName, columnNameList, splitStr, namePairs);
    }

    /// <summary>
    /// 导出DataTable数据
    /// </summary>
    /// <param name="data"></param>
    /// <param name="fileName"></param>
    /// <param name="columnNameList"></param>
    /// <param name="splitStr"></param>
    /// <returns></returns>
    protected IActionResult ExportFile(DataTable data,
            string fileName,
            IEnumerable<string>? columnNameList = null,
            string splitStr = ",")
    {
        return _env.ExportFile(data, fileName, columnNameList, splitStr);
    }


    /// <summary>
    /// 客户端ip
    /// </summary>
    protected string ClientIP
    {
        get
        {
            return HttpContext.GetClientIp();
        }
    }


    /// <summary>
    /// 客户端ip和区域
    /// </summary>
    protected IPRegion IPRegion
    {
        get
        {
            return IPRegion.Parse(ClientIP);
        }
    }

    /// <summary>
    /// 判断是否是ajax
    /// </summary>
    /// <returns></returns>
    protected bool IsAjax()
    {
        return Request.IsAjaxRequest();
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    protected string? Serialize<T>(T t)
    {
        if (t == null) return null;
        return t.ToJson();
    }

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    protected T? Deserialize<T>(string json)
    {
        return SerializeUtil.Deserialize<T>(json);
    }


    /// <summary>
    /// 返回成功的结果
    /// </summary>
    /// <param name="t"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    protected Success SuccessResult(dynamic t, int code = 200)
    {
        return new Success(t);
    }

    /// <summary>
    /// 返回成功的结果
    /// </summary>
    /// <param name="t"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    protected Success SuccessResult(dynamic t, EnumErrorCode code) => SuccessResult(t, (int)code);

    /// <summary>
    /// 返回成功的结果
    /// </summary>
    /// <returns></returns>
    protected Success SuccessResult()
    {
        return new Success();
    }

    /// <summary>
    /// 返回错误的结果
    /// </summary>
    /// <returns></returns>
    protected Fail ErrorResult()
    {
        return new Fail();
    }

    /// <summary>
    /// 返回错误的结果
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    protected Fail ErrorResult(string msg, int code = 999)
    {
        return new Fail(msg, code);
    }

    /// <summary>
    /// 返回错误的结果
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    protected Fail ErrorResult(string msg, EnumErrorCode code) => ErrorResult(msg, (int)code);

    /// <summary>
    /// 返回错误的结果
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    protected Fail ErrorResult(Exception ex, int code = 999)
    {
        return new Fail(ex, code);
    }

    /// <summary>
    /// 返回错误的结果
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    protected Fail ErrorResult(Exception ex, EnumErrorCode code) => ErrorResult(ex, (int)code);


    #region aspnetcore 其它内容


    /// <summary>
    /// 返回指定内容结果
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="text"></param>
    /// <param name="contentType"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    protected async Task<IActionResult> ContentAsync(string text, string contentType = "text/plain", int statusCode = 200)
    {
        var contentResult = new ContentResult()
        {
            Content = text,
            ContentType = contentType,
            StatusCode = statusCode
        };
        return await Task.FromResult(contentResult);
    }

    /// <summary>
    /// 返回空结果
    /// </summary>
    /// <param name="controller"></param>
    /// <returns></returns>
    protected async Task<IActionResult> EmptyAsync()
    {
        return await Task.FromResult(NoContent());
    }

    /// <summary>
    /// 下载文件，
    /// 用于几m以下的小文件
    /// </summary>
    /// <param name="fileContent">下载时的文件名称</param>
    /// <param name="fileContent">内容字节数组</param>
    /// <returns></returns>
    protected async Task<IActionResult> DownloadAsync(string fileDownloadName,
        byte[] fileContent)
    {
        var memoryStream = new MemoryStream(fileContent);
        return await DownloadAsync(fileDownloadName, memoryStream);
    }

    /// <summary>
    /// 下载文件，
    /// 支持range请求头
    /// </summary>
    /// <param name="physicsFullFileName">内容文件地址</param>
    /// <returns></returns>
    protected async Task<IActionResult> DownloadAsync(string physicsFullFileName)
    {
        var fileInfo = new FileInfo(physicsFullFileName);
        if (!fileInfo.Exists) return NotFound();
        var fileStream = new FileStream(physicsFullFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await DownloadAsync(fileInfo.Name, fileStream);
    }


    /// <summary>
    /// 下载文件,
    /// 支持range请求头
    /// </summary>
    /// <param name="fileDownloadName"></param>
    /// <param name="fileStream"></param>
    /// <returns></returns>
    protected async Task<IActionResult> DownloadAsync(string fileDownloadName, Stream fileStream)
    {
        var contentType = FileTypeUtil.GetHttpContentType(fileDownloadName);
        var length = fileStream.Length;
        try
        {
            if (Request.Headers.ContainsKey("Range"))
            {
                var rangeHeader = Request.Headers["Range"].ToString();
                var rangeMatch = Regex.Match(rangeHeader, @"bytes=(\d+)-(\d+)?");

                if (!rangeMatch.Success)
                    return BadRequest("Invalid Range header");

                long start = long.Parse(rangeMatch.Groups[1].Value);
                long end = rangeMatch.Groups[2].Success ?
                    long.Parse(rangeMatch.Groups[2].Value) :
                    length - 1;

                // 验证范围有效性
                if (start < 0 || end >= length || start > end)
                    return BadRequest("Invalid range");

                // 设置范围响应头
                Response.Headers[HeaderNames.ContentRange] = $"bytes {start}-{end}/{length}";
                Response.Headers[HeaderNames.AcceptRanges] = "bytes";
                Response.StatusCode = StatusCodes.Status206PartialContent;
                if (!fileStream.CanSeek) throw new Exception("支持range请求的文件流必须启用UploadOptions:CloudStorageOptions:EnableDownloadCache:true");
                fileStream.Seek(start, SeekOrigin.Begin);
                return await Task.FromResult(new FileStreamResult(fileStream, contentType)
                {
                    EnableRangeProcessing = true,
                    FileDownloadName = Path.GetFileName(fileDownloadName)
                });
            }

            // 普通下载（强制附件模式）
            Response.Headers[HeaderNames.ContentDisposition] =
                $"attachment; filename=\"{Path.GetFileName(fileDownloadName)}\"";

            return await Task.FromResult(new FileStreamResult(fileStream, contentType)
            {
                EnableRangeProcessing = true
            });
        }
        catch
        {
            fileStream.Dispose();
            throw;
        }
    }

    /// <summary>
    /// 返回跳转结果
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    protected async Task<IActionResult> RedirectAsync(string url)
    {
        return await Task.FromResult(new RedirectResult(url));
    }

    /// <summary>
    /// 返回禁止访问结果
    /// </summary>
    /// <param name="data"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    protected async Task<IActionResult> ForbidAsync()
    {
        return await Task.FromResult(Forbid());
    }
    /// <summary>
    /// 返回未登录结果
    /// </summary>
    /// <returns></returns>
    protected async Task<IActionResult> UnauthAsync()
    {
        return await Task.FromResult(Unauthorized());
    }
    /// <summary>
    /// 返回200结果
    /// </summary>
    /// <returns></returns>
    protected async Task<IActionResult> OKAsync()
    {
        return await Task.FromResult(Ok());
    }
    /// <summary>
    /// 返回404结果
    /// </summary>
    /// <returns></returns>
    protected async Task<IActionResult> NotFoundAsync()
    {
        return await Task.FromResult(NotFound());
    }

    #endregion

    #region jwt

    /// <summary>
    /// 创建快捷的jwt token
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="tenantId"></param>
    /// <param name="openId"></param>
    /// <param name="tokenExpire"></param>
    /// <returns></returns>
    protected string CreateJwtToken(long userId, long tenantId, string openId, int tokenExpire = 7200)
    {
        return JwtExtention.CreateJwtToken(userId, tenantId, openId, tokenExpire);
    }

    /// <summary>
    /// 创建快捷的jwt token
    /// </summary>
    /// <param name="user"></param>
    /// <param name="openId"></param>
    /// <param name="tokenExpire"></param>
    /// <returns></returns>
    protected string CreateJwtToken(DbUser user, string openId, int tokenExpire = 7200)
    {
        return JwtExtention.CreateJwtToken(user, openId, tokenExpire);
    }

    /// <summary>
    /// 创建快捷的jwt token
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="tokenExpire"></param>
    /// <returns></returns>
    public static string CreateJwtToken(long userId, int tokenExpire = 7200)
    {
        return JwtExtention.CreateJwtToken(userId, tokenExpire);
    }

    #endregion

    #region sse

    /// <summary>
    /// 发送SSE流数据
    /// </summary>
    /// <param name="msg">SSE数据流</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    protected static async Task SendSseStreamAsync(string msg, CancellationToken cancellationToken = default)
    {
        using var ss = new SseStream(msg);
        await ss.SendAsync(cancellationToken);
    }



    /// <summary>
    /// 发送SSE流数据
    /// </summary>
    /// <param name="stream">SSE数据流</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    protected static async Task SendSseStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        using var ss = new SseStream(stream);
        await ss.SendAsync(cancellationToken);
    }

    #endregion

    #region RedisClient

    /// <summary>
    /// 获取RedisClient
    /// </summary>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    protected RedisClient GetRedisClient(string sectionName = "RedisOptions")
    {
        var redisOptions = NacosConfigUtil.Read<RedisOptions>(sectionName);
        if (redisOptions == null) throw new Exception($"在appsettings.json内未找到名称为{sectionName}的redis配置");
        return RedisClientBuilder.Build(redisOptions);
    }

    #endregion


    /// <summary>
    /// 获取服务缓存
    /// </summary>
    protected IServiceCache ServiceCache
    {
        get
        {
            return ServiceCacheExtention.ServiceCache;
        }
    }

}
