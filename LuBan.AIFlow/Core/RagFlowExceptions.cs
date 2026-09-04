/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.AIFlow.Core
*文件名： RagFlowExceptions.cs
*版本号： V1.0.0.0
*唯一标识：d0d73cbf-63b2-4a8d-8357-90185193c809
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/28 14:46:52
*描述：RagFlowExceptions 类
*
*=================================================
*修改标记
*修改时间：2026/8/28 14:46:52
*修改人： yswenli
*版本号： V1.0.0.0
*描述：RagFlowExceptions 类
*
*****************************************************************************/

namespace LuBan.AIFlow.Core;

/// <summary>
/// RagFlow 客户端异常的基类。
/// 调用方可通过捕获此类型区分 RagFlow 相关错误与其他异常。
/// </summary>
public class RagFlowException : Exception
{
    /// <summary>
    /// 初始化 <see cref="RagFlowException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常消息</param>
    public RagFlowException(string message) : base(message) { }

    /// <summary>
    /// 初始化 <see cref="RagFlowException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">内部异常</param>
    public RagFlowException(string message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>
/// RagFlow 接口调用失败（网络错误、HTTP 错误状态码、接口返回业务错误码）时抛出。
/// </summary>
public class RagFlowApiException : RagFlowException
{
    /// <summary>
    /// 初始化 <see cref="RagFlowApiException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常消息</param>
    public RagFlowApiException(string message) : base(message) { }

    /// <summary>
    /// 初始化 <see cref="RagFlowApiException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">内部异常</param>
    public RagFlowApiException(string message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>
/// RagFlow 返回的数据结构不符合预期（反序列化失败、响应体缺失必要字段）时抛出。
/// </summary>
public class RagFlowDataException : RagFlowException
{
    /// <summary>
    /// 初始化 <see cref="RagFlowDataException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常消息</param>
    public RagFlowDataException(string message) : base(message) { }

    /// <summary>
    /// 初始化 <see cref="RagFlowDataException"/> 类的新实例。
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">内部异常</param>
    public RagFlowDataException(string message, Exception? innerException) : base(message, innerException) { }
}
