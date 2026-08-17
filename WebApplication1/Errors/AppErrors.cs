namespace WebApplication1.Errors;

using LuBan.Common.Errors;

public static class AppErrors
{
    public static readonly ErrorDescriptor ProjectDuplicate = new(90001, "已存在同名或同编码项目", ErrorCategory.Conflict);
    public static readonly ErrorDescriptor IdNumberDuplicate = new(90002, "已存在相同证件号码人员", ErrorCategory.Conflict);
    public static readonly ErrorDescriptor TestDataNotFound = new(90003, "检测数据不存在", ErrorCategory.NotFound);

    public static IReadOnlyList<ErrorDescriptor> All => new[]
    {
        ProjectDuplicate, IdNumberDuplicate, TestDataNotFound
    };
}
