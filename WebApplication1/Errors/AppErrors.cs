namespace WebApplication1.Errors;

using LuBan.Common.Errors;

/// <summary>
/// 业务项目自定义错误码定义（90001-90003）。
/// 通过 services.AddErrorCodes(AppErrors.All) 注册到依赖注入。
/// </summary>
public static class AppErrors
{
    /// <summary>已存在同名或同编码项目</summary>
    public static readonly ErrorDescriptor ProjectDuplicate = new(90001, "已存在同名或同编码项目", ErrorCategory.Conflict);

    /// <summary>已存在相同证件号码人员</summary>
    public static readonly ErrorDescriptor IdNumberDuplicate = new(90002, "已存在相同证件号码人员", ErrorCategory.Conflict);

    /// <summary>检测数据不存在</summary>
    public static readonly ErrorDescriptor TestDataNotFound = new(90003, "检测数据不存在", ErrorCategory.NotFound);

    /// <summary>
    /// 所有业务错误码集合（用于注册到 ErrorCodeRegistry）
    /// </summary>
    public static IReadOnlyList<ErrorDescriptor> All => new[]
    {
        ProjectDuplicate, IdNumberDuplicate, TestDataNotFound
    };
}
