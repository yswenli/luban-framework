/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common.Data
*文件名： FriendlyException
*版本号： V1.0.0.0
*唯一标识：bf379418-cb3a-4d5a-8c56-785f5a2c1459
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 15:25:35
*描述：框架友好异常类
*
*=================================================
*修改标记
*修改时间：2023/12/4 15:25:35
*修改人： yswenli
*版本号： V1.0.0.0
*描述：框架友好异常类
*
*****************************************************************************/
namespace System;

/// <summary>
/// 框架友好异常类
/// </summary>
public class FriendlyException : Exception
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public FriendlyException() : base()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="ex"></param>
    public FriendlyException(string msg, Exception ex) : base(msg, ex)
    {

    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message"></param>
    /// <param name="errorCode"></param>
    /// <param name="httpStatusCode"></param>
    public FriendlyException(string message, EnumErrorCode errorCode, int httpStatusCode = HttpStatusConst.Status200OK) : base(message)
    {
        ErrorMessage = message;
        ErrorCode = errorCode;
        HttpStatusCode = httpStatusCode;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message"></param>
    /// <param name="errorCode"></param>
    /// <param name="innerException"></param>
    /// <param name="httpStatusCode"></param>
    public FriendlyException(string message, EnumErrorCode errorCode, Exception innerException, int httpStatusCode = HttpStatusConst.Status200OK) : base(message, innerException)
    {
        ErrorMessage = message;
        ErrorCode = errorCode;
        HttpStatusCode = httpStatusCode;
    }

    /// <summary>
    /// 错误码
    /// </summary>
    public EnumErrorCode ErrorCode { get; set; }

    /// <summary>
    /// 错误消息（支持 Object 对象）
    /// </summary>
    public object ErrorMessage { get; set; }

    /// <summary>
    /// 状态码,
    /// HttpStatusConst
    /// </summary>
    public int HttpStatusCode { get; set; } = HttpStatusConst.Status200OK;

    /// <summary>
    /// 是否是数据验证异常
    /// </summary>
    public bool ValidationException { get; set; } = false;

    /// <summary>
    /// 额外数据
    /// </summary>
    public new object[] Data { get; set; }
}
