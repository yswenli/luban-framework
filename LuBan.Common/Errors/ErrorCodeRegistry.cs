/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Errors
*文件名： ErrorCodeRegistry.cs
*版本号： V1.0.0.0
*唯一标识：e952c7ab-7317-486b-8cd7-95de4b813ca2
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/17 13:20:40
*描述：ErrorCodeRegistry 类
*
*=================================================
*修改标记
*修改时间：2026/8/17 13:20:40
*修改人： yswenli
*版本号： V1.0.0.0
*描述：ErrorCodeRegistry 类
*
*****************************************************************************/

namespace LuBan.Common.Errors;

/// <summary>
/// 错误码注册表。自动加载框架内置错误码（FrameworkErrors.All），
/// 并支持注册业务项目自定义错误码。重复错误码会抛出异常。
/// </summary>
public sealed class ErrorCodeRegistry
{
    private readonly Dictionary<int, ErrorDescriptor> _byCode = new();

    /// <summary>
    /// 创建注册表，自动加载 FrameworkErrors.All
    /// </summary>
    public ErrorCodeRegistry()
    {
        Register(FrameworkErrors.All);
    }

    /// <summary>
    /// 注册错误描述符集合
    /// </summary>
    /// <param name="descriptors">要注册的错误描述符</param>
    /// <exception cref="InvalidOperationException">当错误码重复时抛出</exception>
    public void Register(IEnumerable<ErrorDescriptor> descriptors)
    {
        foreach (var d in descriptors)
        {
            if (!_byCode.TryAdd(d.Code, d))
                throw new InvalidOperationException($"Duplicate error code: {d.Code}");
        }
    }

    /// <summary>
    /// 根据错误码查找错误描述符
    /// </summary>
    /// <param name="code">错误码</param>
    /// <returns>对应的错误描述符，未找到返回 null</returns>
    public ErrorDescriptor? FindByCode(int code)
        => _byCode.GetValueOrDefault(code);
}
