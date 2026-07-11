/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Errors
*文件名： ErrorCodeItemMetadataAttribute
*版本号： V1.0.0.0
*唯一标识：d3ebb044-c7f3-4324-81b7-6e80af7a4bc6
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/12/15 11:32:20
*描述：
*
*=================================================
*修改标记
*修改时间：2023/12/15 11:32:20
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
namespace System;


/// <summary>
/// 异常元数据特性
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class ErrorCodeItemMetadataAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="errorMessage">错误消息</param>
    /// <param name="args">格式化参数</param>
    public ErrorCodeItemMetadataAttribute(string errorMessage, params object[] args)
    {
        ErrorMessage = errorMessage;
        Args = args;
    }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// 错误码
    /// </summary>
    public object ErrorCode { get; set; }

    /// <summary>
    /// 格式化参数
    /// </summary>
    public object[] Args { get; set; }
}
