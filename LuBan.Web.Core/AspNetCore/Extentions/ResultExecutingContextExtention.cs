/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Web.Core.AspNetCore.Extentions
*文件名： ResultExecutingContextExtention
*版本号： V1.0.0.0
*唯一标识：3ae5bbe4-2f55-4b4c-81b3-a8b6dde806f7
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/12/4 17:28:13
*描述：当前执行上下文扩展
*
*=================================================
*修改标记
*修改时间：2025/12/4 17:28:13
*修改人： yswenli
*版本号： V1.0.0.0
*描述：当前执行上下文扩展
*
*****************************************************************************/
namespace LuBan.Web.Core.AspNetCore.Extentions;

/// <summary>
/// 当前执行上下文扩展
/// </summary>
public static class ResultExecutingContextExtention
{
    /// <summary>
    /// 获取值
    /// </summary>
    /// <param name="resultContext"></param>
    /// <returns></returns>
    public static string? GetResultLogText(this ResultExecutingContext resultContext)
    {
        IActionResult actionResult = resultContext.Result;
        string? result = "";
        try
        {
            if (actionResult != null)
            {
                if (actionResult is BadRequestObjectResult br && br != null)
                {
                    return $"请求发生异常,statuscode:{br.StatusCode},value:{SerializeUtil.Serialize(br.Value ?? "")}";
                }

                if (actionResult is FileResult fr)
                {
                    result = $"下载文件：{fr.FileDownloadName}";
                }
                else if (actionResult is ViewResult vr)
                {
                    result = $"访问页面:{resultContext.HttpContext.Request.Path}";
                }
                else if (actionResult is ContentResult cr)
                {
                    result = cr.Content;
                }
                else if (actionResult is RedirectResult rr)
                {
                    result = $"跳转地址：{rr.Url}";
                }
                else if (actionResult is JsonResult jr)
                {
                    if (jr != null && jr.Value != null)
                    {
                        if (jr.Value is Stream || jr.Value is byte[])
                        {
                            result = "数据流";
                        }
                        else
                        {
                            if (jr.Value is string json)
                            {
                                result = json.Substring(0, 10240);
                            }
                            else
                            {
                                result = SerializeUtil.Serialize(jr.Value);
                            }
                        }
                    }
                }
                else if (actionResult is ObjectResult objData)
                {
                    if (objData != null && objData.Value != null)
                    {
                        if (objData.Value is Stream || objData.Value is byte[])
                        {
                            result = "数据流";
                        }
                        else
                        {
                            result = SerializeUtil.Serialize(objData.Value);
                        }
                    }
                }
                else
                {
                    result = SerializeUtil.Serialize(actionResult);
                }
            }
        }
        catch { }
        return result;
    }
}
