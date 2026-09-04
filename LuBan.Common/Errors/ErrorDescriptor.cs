/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Errors
*文件名： ErrorDescriptor.cs
*版本号： V1.0.0.0
*唯一标识：5ee0351c-3530-48b8-ad92-815a792c6062
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/17 13:14:12
*描述：ErrorDescriptor 类
*
*=================================================
*修改标记
*修改时间：2026/8/17 13:14:12
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ErrorDescriptor 类
*
*****************************************************************************/

namespace LuBan.Common.Errors;

/// <summary>
/// 错误描述符，包含错误码、消息、分类和自动推导的 HTTP 状态码。
/// 替代原 EnumErrorCode，支持按领域模块化组织错误码。
/// </summary>
public readonly struct ErrorDescriptor : IEquatable<ErrorDescriptor>
{
    /// <summary>
    /// 错误码（业务数字码）
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// 错误分类
    /// </summary>
    public ErrorCategory Category { get; }

    /// <summary>
    /// 由 Category 自动推导的 HTTP 状态码
    /// </summary>
    public int HttpStatusCode => Category.ToHttpStatus();

    /// <summary>
    /// 创建错误描述符
    /// </summary>
    /// <param name="code">错误码</param>
    /// <param name="message">错误消息</param>
    /// <param name="category">错误分类</param>
    public ErrorDescriptor(int code, string message, ErrorCategory category)
    {
        Code = code;
        Message = message;
        Category = category;
    }

    /// <inheritdoc/>
    public bool Equals(ErrorDescriptor other) => Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ErrorDescriptor other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Code;

    /// <inheritdoc/>
    public override string ToString() => $"[{Code}] {Message}";

    /// <summary>
    /// 相等判断
    /// </summary>
    public static bool operator ==(ErrorDescriptor left, ErrorDescriptor right) => left.Equals(right);

    /// <summary>
    /// 不等判断
    /// </summary>
    public static bool operator !=(ErrorDescriptor left, ErrorDescriptor right) => !left.Equals(right);
}
