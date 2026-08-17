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
