namespace System;

using LuBan.Common.Errors;

public class FriendlyException : Exception
{
    public FriendlyException(ErrorDescriptor error, params object[] args)
        : base(FormatMessage(error.Message, args))
    {
        Error = error;
        HttpStatusCode = error.HttpStatusCode;
    }

    public FriendlyException(string customMessage, ErrorDescriptor error, params object[] args)
        : base(customMessage)
    {
        Error = error;
        HttpStatusCode = error.HttpStatusCode;
    }

    public FriendlyException(string message, ErrorCategory category = ErrorCategory.Business)
        : base(message)
    {
        Error = new ErrorDescriptor(0, message, category);
        HttpStatusCode = category.ToHttpStatus();
    }

    public FriendlyException(string message, Exception innerException, ErrorCategory category = ErrorCategory.System)
        : base(message, innerException)
    {
        Error = new ErrorDescriptor(0, message, category);
        HttpStatusCode = category.ToHttpStatus();
    }

    public ErrorDescriptor Error { get; }
    public int HttpStatusCode { get; set; }

    private static string FormatMessage(string template, object[] args)
    {
        if (args == null || args.Length == 0) return template;
        try { return string.Format(template, args); }
        catch { return template; }
    }
}