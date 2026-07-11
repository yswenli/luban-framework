/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common.Data
*文件名： MethodIfException
*版本号： V1.0.0.0
*唯一标识：b1ef7185-beaf-437b-803d-194823c29aab
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/4 16:09:23
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/4 16:09:23
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace System;


/// <summary>
/// 方法异常类
/// </summary>
public sealed class MethodIfException
{
    /// <summary>
    /// 出异常的方法
    /// </summary>
    public MethodBase ErrorMethod { get; set; }

    /// <summary>
    /// 异常特性
    /// </summary>
    public IEnumerable<IfExceptionAttribute> IfExceptionAttributes { get; set; }
}


/// <summary>
/// 异常复写特性
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class IfExceptionAttribute : Attribute
{
    /// <summary>
    /// 默认构造函数
    /// </summary>
    public IfExceptionAttribute()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="errorCode">错误编码</param>
    /// <param name="args">格式化参数</param>
    public IfExceptionAttribute(object errorCode, params object[] args)
    {
        ErrorCode = errorCode;
        Args = args;
    }

    /// <summary>
    /// 捕获特定异常类型异常（用于全局异常捕获）
    /// </summary>
    /// <param name="exceptionType"></param>
    public IfExceptionAttribute(Type exceptionType)
    {
        ExceptionType = exceptionType;
    }

    /// <summary>
    /// 错误编码
    /// </summary>
    public object ErrorCode { get; set; }

    /// <summary>
    /// 异常类型
    /// </summary>
    public Type ExceptionType { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// 格式化参数
    /// </summary>
    public object[] Args { get; set; }
}
