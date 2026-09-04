/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Errors
*文件名： ErrorCategory.cs
*版本号： V1.0.0.0
*唯一标识：3eef8f6e-7f6d-41d6-a701-7a080829303b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/17 13:13:17
*描述：ErrorCategory 类
*
*=================================================
*修改标记
*修改时间：2026/8/17 13:13:17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ErrorCategory 类
*
*****************************************************************************/

namespace LuBan.Common.Errors;

/// <summary>
/// 错误分类，决定 HTTP 响应状态码
/// </summary>
public enum ErrorCategory
{
    /// <summary>
    /// 参数校验失败 (HTTP 400)
    /// </summary>
    Validation,

    /// <summary>
    /// 认证失败，未登录 (HTTP 401)
    /// </summary>
    Authentication,

    /// <summary>
    /// 授权失败，无权限 (HTTP 403)
    /// </summary>
    Authorization,

    /// <summary>
    /// 资源不存在 (HTTP 404)
    /// </summary>
    NotFound,

    /// <summary>
    /// 数据冲突，重复等 (HTTP 409)
    /// </summary>
    Conflict,

    /// <summary>
    /// 业务逻辑错误 (HTTP 422)
    /// </summary>
    Business,

    /// <summary>
    /// 系统内部错误 (HTTP 500)
    /// </summary>
    System
}

/// <summary>
/// ErrorCategory 扩展方法
/// </summary>
public static class ErrorCategoryExtensions
{
    /// <summary>
    /// 将错误分类映射为 HTTP 状态码
    /// </summary>
    /// <param name="category">错误分类</param>
    /// <returns>对应的 HTTP 状态码</returns>
    public static int ToHttpStatus(this ErrorCategory category) => category switch
    {
        ErrorCategory.Validation => 400,
        ErrorCategory.Authentication => 401,
        ErrorCategory.Authorization => 403,
        ErrorCategory.NotFound => 404,
        ErrorCategory.Conflict => 409,
        ErrorCategory.Business => 422,
        ErrorCategory.System => 500,
        _ => 500
    };
}
