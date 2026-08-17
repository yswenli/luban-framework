namespace System;

using LuBan.Common.Errors;

public static class FriendlyError
{
    public static FriendlyException Ex(ErrorDescriptor error, params object[] args)
        => new(error, args);

    public static FriendlyException Ex(string message, ErrorDescriptor error, params object[] args)
        => new(message, error, args);

    public static FriendlyException Ex(string message, ErrorCategory category = ErrorCategory.Business)
        => new(message, category);

    public static FriendlyException Ex(string message, Exception exception, ErrorCategory category = ErrorCategory.System)
        => new(message, exception, category);

    public static FriendlyException Ex(Exception exception)
        => new(exception.Message, exception, ErrorCategory.System);

    public static FriendlyException SetStatusCode(this FriendlyException exception, int statusCode)
    {
        exception.HttpStatusCode = statusCode;
        return exception;
    }

    public static FriendlyException WithData(this FriendlyException exception, params object[] data)
    {
        exception.Data = data;
        return exception;
    }
}