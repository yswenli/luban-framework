/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Web.Core.Swagger
*文件名： MarkDownLoader
*版本号： V1.0.0.0
*唯一标识：d465cb78-dcd7-4dd6-8666-d3999c0ac715
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 11:12:41
*描述：Swagger生成markdown
*
*=====================================================================
*修改标记
*修改时间：2022/6/21 11:12:41
*修改人： walle.wen
*版本号： V1.0.0.0
*描述：Swagger生成markdown
*
*****************************************************************************/
namespace LuBan.Web.Core.Swagger;

/// <summary>
/// Swagger生成markdown
/// </summary>
[ApiController]
[SwaggerHidden]
public class SwaggerController : ControllerBase
{

    /// <summary>
    /// 生成markdown文档
    /// </summary>
    /// <param name="swaggerDocGenerator"></param>
    /// <param name="environment"></param>
    /// <param name="documents"></param>
    /// <returns></returns>
    [HttpGet("download")]
    public IActionResult DownloadDoc([FromServices] ISwaggerDocGenerator swaggerDocGenerator, [FromServices] IWebHostEnvironment environment, [FromQuery, Required(ErrorMessage = "请在地址栏中输入参数 documents，且参数 documents 的值仅支持\"default\",\"admin\",\"mobile\",\"internal\",\"open\"")] string documents)
    {
        try
        {
            var bytes = swaggerDocGenerator.GetSwaggerDocContent(documents);
            var mime = "application/octet-stream";
            var name = $"{documents}-http-api-document.md";
            return new FileContentResult(bytes, mime) { FileDownloadName = name };
        }
        catch (Exception ex)
        {
            throw FriendlyError.Ex($"请在地址栏中输入参数 ?documents=，且参数 documents 的值仅支持\"default\",\"admin\",\"mobile\",\"internal\",\"open\";ex:{ex}", 500);
        }
    }


    /// <summary>
    /// 生成js sdk
    /// </summary>
    /// <param name="swaggerDocGenerator"></param>
    /// <param name="environment"></param>
    /// <param name="documents"></param>
    /// <returns></returns>
    [HttpGet("jssdk")]
    public IActionResult GenerateJsSdk([FromServices] ISwaggerDocGenerator swaggerDocGenerator, [FromServices] IWebHostEnvironment environment, [FromQuery, Required(ErrorMessage = "请在地址栏中输入参数 documents，且参数 documents 的值仅支持\"default\",\"admin\",\"mobile\",\"internal\",\"open\"")] string documents)
    {
        try
        {
            var bytes = swaggerDocGenerator.GetSwaggerJsSdk(documents);
            var mime = "application/octet-stream";
            var name = $"{documents}-api-sdk.js";
            return new FileContentResult(bytes, mime) { FileDownloadName = name };
        }
        catch (Exception ex)
        {
            throw FriendlyError.Ex($"请在地址栏中输入参数 ?documents=，且参数 documents 的值仅支持\"default\",\"admin\",\"mobile\",\"internal\",\"open\";ex:{ex}", 500);
        }
    }
}
