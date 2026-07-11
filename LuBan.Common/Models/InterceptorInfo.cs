/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Models
*文件名： InterceptorInfo
*版本号： V1.0.0.0
*唯一标识：e3fbf555-cf69-41ea-9ce3-2842161863dd
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/7/3 13:24:04
*描述：拦截信息
*
*=================================================
*修改标记
*修改时间：2023/7/3 13:24:04
*修改人： yswenli
*版本号： V1.0.0.0
*描述：拦截信息
*
*****************************************************************************/
namespace LuBan.Common.Models;

/// <summary>
/// 拦截信息
/// </summary>
public class InterceptorInfo
{
    /// <summary>
    /// 控制器名称
    /// </summary>
    public string ControllerName { get; set; }
    /// <summary>
    /// 方法名称
    /// </summary>
    public string ActionName { get; set; }
    /// <summary>
    /// 方法
    /// </summary>
    public string Method { get; set; }
    /// <summary>
    /// 地址
    /// </summary>
    public string Url { get; set; }
    /// <summary>
    /// 地址栏参数
    /// </summary>
    public string Query { get; set; }
    /// <summary>
    /// 请求头
    /// </summary>
    public Dictionary<string, string> Headers { get; set; }
    /// <summary>
    /// action上的参数
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; }
}
