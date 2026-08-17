namespace LuBan.Common.Errors;

public enum ErrorCategory
{
    Validation,
    Authentication,
    Authorization,
    NotFound,
    Conflict,
    Business,
    System
}

public static class ErrorCategoryExtensions
{
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