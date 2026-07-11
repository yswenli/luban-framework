/****************************************************************************
*Copyright @ YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Consts
*文件名： HttpStatusConst
*版本号： V1.0.0.0
*唯一标识：06860cda-5dc5-445a-89b1-b28c5ec78193
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/12/19 10:11:03
*描述：http状态码
*
*=================================================
*修改标记
*修改时间：2024/12/19 10:11:03
*修改人： yswenli
*版本号： V1.0.0.0
*描述：http状态码
*
*****************************************************************************/
namespace LuBan.Common.Consts;

/// <summary>  
/// HTTP状态码常量类，包含常见的HTTP状态码及其描述。  
/// </summary>  
[Const("http状态码")]
public class HttpStatusConst
{
    /// <summary>
    /// 表示ok
    /// </summary>
    public const int Status200OK = 200;

    /// <summary>  
    /// HTTP状态码 100：继续。  
    /// 表示客户端应继续其请求。  
    /// </summary>  
    public const int Status100Continue = 100;

    /// <summary>  
    /// HTTP状态码 413：请求实体过大。  
    /// 表示请求的实体数据大小超过了服务器的处理能力。  
    /// </summary>  
    public const int Status413RequestEntityTooLarge = 413;

    /// <summary>  
    /// HTTP状态码 413：有效负载过大。  
    /// 表示请求的有效负载大小超过了服务器的处理能力。  
    /// </summary>  
    public const int Status413PayloadTooLarge = 413;

    /// <summary>  
    /// HTTP状态码 414：请求URI过长。  
    /// 表示请求的URI长度超过了服务器的处理能力。  
    /// </summary>  
    public const int Status414RequestUriTooLong = 414;

    /// <summary>  
    /// HTTP状态码 414：URI过长。  
    /// 表示请求的URI长度超过了服务器的处理能力。  
    /// </summary>  
    public const int Status414UriTooLong = 414;

    /// <summary>  
    /// HTTP状态码 415：不支持的媒体类型。  
    /// 表示服务器无法处理请求中提交的媒体格式。  
    /// </summary>  
    public const int Status415UnsupportedMediaType = 415;

    /// <summary>  
    /// HTTP状态码 416：请求范围不满足。  
    /// 表示客户端请求的范围无效。  
    /// </summary>  
    public const int Status416RequestedRangeNotSatisfiable = 416;

    /// <summary>  
    /// HTTP状态码 416：范围不满足。  
    /// 表示客户端请求的范围无效。  
    /// </summary>  
    public const int Status416RangeNotSatisfiable = 416;

    /// <summary>  
    /// HTTP状态码 417：期望失败。  
    /// 表示服务器无法满足客户端的期望值。  
    /// </summary>  
    public const int Status417ExpectationFailed = 417;

    /// <summary>  
    /// HTTP状态码 418：我是一个茶壶。  
    /// 表示服务器拒绝冲泡咖啡，因为它是一个茶壶（愚人节彩蛋）。  
    /// </summary>  
    public const int Status418ImATeapot = 418;

    /// <summary>  
    /// HTTP状态码 419：身份验证超时。  
    /// 表示客户端的身份验证已过期。  
    /// </summary>  
    public const int Status419AuthenticationTimeout = 419;

    /// <summary>  
    /// HTTP状态码 421：错误的请求。  
    /// 表示请求被定向到无法生成响应的服务器。  
    /// </summary>  
    public const int Status421MisdirectedRequest = 421;

    /// <summary>  
    /// HTTP状态码 422：不可处理的实体。  
    /// 表示服务器理解请求实体，但无法处理其中的内容。  
    /// </summary>  
    public const int Status422UnprocessableEntity = 422;

    /// <summary>  
    /// HTTP状态码 423：锁定。  
    /// 表示目标资源已被锁定。  
    /// </summary>  
    public const int Status423Locked = 423;

    /// <summary>  
    /// HTTP状态码 424：依赖失败。  
    /// 表示由于之前的请求失败，当前请求也无法完成。  
    /// </summary>  
    public const int Status424FailedDependency = 424;

    /// <summary>  
    /// HTTP状态码 426：需要升级。  
    /// 表示客户端应切换到更高版本的协议。  
    /// </summary>  
    public const int Status426UpgradeRequired = 426;

    /// <summary>  
    /// HTTP状态码 428：需要前提条件。  
    /// 表示服务器要求请求满足某些条件。  
    /// </summary>  
    public const int Status428PreconditionRequired = 428;

    /// <summary>  
    /// HTTP状态码 429：请求过多。  
    /// 表示客户端发送的请求次数超过了服务器的限制。  
    /// </summary>  
    public const int Status429TooManyRequests = 429;

    /// <summary>  
    /// HTTP状态码 431：请求头字段过大。  
    /// 表示请求头字段的大小超过了服务器的限制。  
    /// </summary>  
    public const int Status431RequestHeaderFieldsTooLarge = 431;

    /// <summary>  
    /// HTTP状态码 451：因法律原因不可用。  
    /// 表示请求的资源因法律原因不可用。  
    /// </summary>  
    public const int Status451UnavailableForLegalReasons = 451;

    /// <summary>  
    /// HTTP状态码 499：客户端关闭请求。  
    /// 表示客户端在服务器处理请求之前关闭了连接。  
    /// </summary>  
    public const int Status499ClientClosedRequest = 499;
    /// <summary>
    /// 表示代接口代码处理出现异常
    /// </summary>
    public const int Status500InternalServerError = 500;
}
