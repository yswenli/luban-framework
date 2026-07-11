/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Web.Core.Models
*文件名： DataResult
*版本号： V1.0.0.0
*唯一标识：a4f6f4c0-cf3e-40b9-ac86-fe186995f7cc
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/9/13 15:44:16
*描述：
*
*=================================================
*修改标记
*修改时间：2023/9/13 15:44:16
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace LuBan.Web.Core.Models;

/// <summary>
/// mvc行为结果
/// </summary>
public class DataResult : ContentResult, IActionResult
{

    /// <summary>
    /// mvc行为结果
    /// </summary>
    public DataResult()
    {

    }

    /// <summary>
    /// mvc行为结果
    /// </summary>
    /// <param name="content"></param>
    /// <param name="statusCode"></param>
    /// <param name="contenType"></param>
    public DataResult(string content, int statusCode, string contenType = "text/html; charset=utf-8")
    {
        Content = content;
        StatusCode = statusCode;
        ContentType = contenType;
    }

    /// <summary>
    /// mvc行为结果
    /// </summary>
    /// <param name="content"></param>
    public DataResult(string content) : this(content, 200)
    {

    }

    /// <summary>
    /// mvc行为结果
    /// </summary>
    /// <param name="content"></param>
    public DataResult(object content) : this(content.ToJson() ?? "", 200, "application/json; charset=utf-8")
    {

    }

}
