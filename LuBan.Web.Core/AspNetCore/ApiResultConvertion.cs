/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.AspNetCore
*文件名： ApiResultConvertion
*版本号： V1.0.0.0
*唯一标识：261f9445-8d20-4bf0-b701-3782ef66a8fb
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/8/1 17:33:17
*描述：接口返回值统一处理转换
*
*=================================================
*修改标记
*修改时间：2024/8/1 17:33:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：接口返回值统一处理转换
*
*****************************************************************************/
namespace LuBan.Web.Core.AspNetCore;

/// <summary>
/// 接口返回值统一处理转换
/// </summary>
public static class ApiResultConvertion
{
    /// <summary>
    /// 转换返回的格式
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task<IActionResult?> ConvertToUnifiedResult(this ActionExecutedContext? context)
    {
        if (context == null) return null;
        IActionResult result = new EmptyResult();
        //空值处理
        if (context.Result == null || context.Result is EmptyResult emptyResult)
        {
            result = new EmptyResult();
        }
        //如果返回值是变量类型，则转换成统一格式
        else if (context.Result is ObjectResult objectResult
            && objectResult != null)
        {
            var value = objectResult.Value;
            //框架内置类型
            if (value is Result)
            {
                await UrlUpdateHelper.UpdateUrlsAsync(value);

                result = new JsonResult(objectResult.Value)
                {
                    ContentType = "application/json; charset=utf-8",
                    StatusCode = objectResult.StatusCode ?? 200
                };
            }
            //流处理
            else if (value is Stream stream)
            {
                result = new FileStreamResult(stream, "application/octet-stream");
            }
            //字节数组处理
            else if (value is byte[] bytes)
            {
                result = new FileContentResult(bytes, "application/octet-stream");
            }
            //其它数据类型
            else
            {
                if (value == null)
                {
                    var data = new Success(value);
                    result = new JsonResult(data)
                    {
                        ContentType = "application/json; charset=utf-8",
                        StatusCode = objectResult.StatusCode ?? 200
                    };
                }
                else
                {
                    var noApiResultConvertion = value.GetCustomAttribute<NoApiResultConvertAtrribute>() ?? null;
                    if (noApiResultConvertion == null)
                    {
                        // 更新value中的URL
                        await UrlUpdateHelper.UpdateUrlsAsync(value);

                        var data = new Success(value);
                        result = new JsonResult(data)
                        {
                            ContentType = "application/json; charset=utf-8",
                            StatusCode = objectResult.StatusCode ?? 200
                        };
                    }
                }

                return result;
            }
        }
        //纯文本处理
        else if (context.Result is ContentResult contentResult)
        {
            if (contentResult.ContentType.IsNullOrEmpty())
            {
                contentResult.ContentType = "text/plain; charset=utf-8";
            }
            contentResult.StatusCode = 200;
            result = contentResult;
        }
        //mvc内置json处理
        else if (context.Result is JsonResult jsonResult)
        {
            jsonResult.ContentType = "application/json; charset=utf-8";
            result = jsonResult;
        }
        //其它：视图、重定向、文件、状态值等
        else
        {
            result = context.Result;
        }
        return result;
    }
}
