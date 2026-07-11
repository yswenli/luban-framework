/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Errors
*文件名： FriendlyError
*版本号： V1.0.0.0
*唯一标识：ab6f6d7b-31fa-4bc5-8fde-2bd67d4d5ac2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/15 11:29:39
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/15 11:29:39
*修改人： yswenli
*版本号： V1.0.0.0
*描述：抛异常静态类
*
*****************************************************************************/
namespace System;

/// <summary>
/// 抛异常静态类
/// </summary>
public static class FriendlyError
{
    /// <summary>
    /// 抛出错误码异常
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static FriendlyException Ex(string msg, params object[] args)
    {
        return new FriendlyException(msg, EnumErrorCode.SystemOk200, HttpStatusConst.Status200OK)
        {
            ValidationException = true,
            Data = args
        };
    }
    /// <summary>
    /// 抛出错误码异常
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static FriendlyException Ex(string msg, Exception exception, params object[] args)
    {
        return new FriendlyException(msg, exception)
        {
            ErrorCode = EnumErrorCode.SystemOk200,
            HttpStatusCode = HttpStatusConst.Status500InternalServerError,
            ValidationException = false,
            Data = args
        };
    }

    /// <summary>
    /// 抛出错误码异常
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static FriendlyException Ex(Exception exception, params object[] args)
    {
        return Ex(exception.Message, exception, args);
    }

    /// <summary>
    /// 抛出错误码异常
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static FriendlyException Ex(string errorMessage, int statusCode = HttpStatusConst.Status200OK)
    {
        var friendlyException = new FriendlyException(errorMessage, EnumErrorCode.SystemOk200, statusCode)
        {
            ValidationException = true
        };
        return friendlyException;
    }

    /// <summary>
    /// 抛出错误码异常
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="statusCode"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static FriendlyException Ex(EnumErrorCode errorCode, int statusCode = HttpStatusConst.Status200OK, params object[] args)
    {
        var (code, msg) = GetErrorCodeMessage(errorCode, args);
        var friendlyException = new FriendlyException(msg, code, statusCode)
        {
            ValidationException = true
        };
        return friendlyException;
    }
    /// <summary>
    /// 抛出错误码异常
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <param name="errorCode"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static FriendlyException Ex(string errorMessage, EnumErrorCode errorCode, int statusCode = HttpStatusConst.Status200OK)
    {
        var (code, msg) = GetErrorCodeMessage(errorCode);
        var friendlyException = new FriendlyException($"{errorMessage}.{msg}", errorCode, statusCode)
        {
            ValidationException = true
        };
        return friendlyException;
    }
    /// <summary>
    /// 抛出错误码异常
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    public static FriendlyException Ex(string errorMessage) => Ex(errorMessage, HttpStatusConst.Status200OK);

    /// <summary>
    /// 获取错误码字符串
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <param name="errorCode"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private static string MontageErrorMessage(string? errorMessage, EnumErrorCode errorCode, params object[] args)
    {
        var msg = (errorCode < 0
            ? string.Empty
            : $"[{errorCode}] ") + errorMessage;
        if (args == null || args.Length < 1 || args[0] == null)
        {
            return msg;
        }
        return $"{msg} ,args:{args.ToJson()}";
    }

    /// <summary>
    /// 获取错误码消息
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private static (EnumErrorCode code, string msg) GetErrorCodeMessage(EnumErrorCode errorCode, params object[] args)
    {
        var errorInfo = HandleEnumErrorCode(errorCode);
        // 字符串格式化
        return (errorCode, MontageErrorMessage(errorInfo.Value, errorCode, args));
    }

    /// <summary>
    /// 处理枚举类型错误码
    /// </summary>
    /// <param name="errorCode">错误码</param>
    /// <returns></returns>
    private static (object? Key, string? Value) HandleEnumErrorCode(object errorCode)
    {
        // 获取类型
        var errorType = errorCode.GetType();
        var fieldName = Enum.GetName(errorType!, errorCode);
        if (string.IsNullOrEmpty(fieldName)) return (null, null);
        var fieldinfo = errorType.GetField(fieldName);
        if (fieldinfo != null && fieldinfo.IsDefined(typeof(ErrorCodeItemMetadataAttribute), true))
        {
            return GetErrorCodeItemInformation(fieldinfo);
        }
        return (Key: null, Value: null);
    }

    /// <summary>
    /// 获取错误代码信息
    /// </summary>
    /// <param name="fieldInfo">字段对象</param>
    /// <returns>(object key, object value)</returns>
    private static (object? Key, string? Value) GetErrorCodeItemInformation(FieldInfo fieldInfo)
    {
        var errorCodeItemMetadata = fieldInfo.GetCustomAttribute<ErrorCodeItemMetadataAttribute>();
        if (errorCodeItemMetadata != null)
            return (errorCodeItemMetadata.ErrorCode ?? fieldInfo.Name, string.Format(errorCodeItemMetadata.ErrorMessage, errorCodeItemMetadata.Args));
        return (Key: null, Value: null);
    }


    /// <summary>
    /// 设置异常状态码
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static FriendlyException SetStatusCode(this FriendlyException exception, int statusCode = HttpStatusConst.Status500InternalServerError)
    {
        exception.HttpStatusCode = statusCode;
        return exception;
    }


    /// <summary>
    /// 设置额外数据
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="data"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static FriendlyException Ex(this FriendlyException exception, int statusCode = HttpStatusConst.Status500InternalServerError, params object[] data)
    {
        exception.Data = data;
        exception.HttpStatusCode = statusCode;
        return exception;
    }
}
