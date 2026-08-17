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
