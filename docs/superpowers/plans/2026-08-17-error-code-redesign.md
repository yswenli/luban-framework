# Error Code System Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the monolithic `EnumErrorCode` enum with a structured `ErrorDescriptor`-based system that provides modular classification, automatic HTTP status mapping, and extensibility.

**Architecture:** Introduce `ErrorCategory` enum and `ErrorDescriptor` struct in `LuBan.Common`. Refactor `FriendlyException`/`FriendlyError` to use the new types. Split framework error codes into domain-organized nested classes under `FrameworkErrors`. Update middleware to use category-derived HTTP status codes. Migrate all call sites.

**Tech Stack:** C# / .NET 10 / ASP.NET Core

**Spec:** `docs/superpowers/specs/2026-08-17-error-code-redesign-design.md`

---

## Task 1: Core Types — ErrorCategory

**Files:**
- Create: `LuBan.Common/Errors/ErrorCategory.cs`

- [ ] **Step 1: Create ErrorCategory.cs with enum and extension**

```csharp
namespace LuBan.Common.Errors;

public enum ErrorCategory
{
    Validation,
    Authentication,
    Authorization,
    NotFound,
    Conflict,
    Business,
    System
}

public static class ErrorCategoryExtensions
{
    public static int ToHttpStatus(this ErrorCategory category) => category switch
    {
        ErrorCategory.Validation => 400,
        ErrorCategory.Authentication => 401,
        ErrorCategory.Authorization => 403,
        ErrorCategory.NotFound => 404,
        ErrorCategory.Conflict => 409,
        ErrorCategory.Business => 422,
        ErrorCategory.System => 500,
        _ => 500
    };
}
```

- [ ] **Step 2: Build to verify compilation**

Run: `dotnet build LuBan.Common/LuBan.Common.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add LuBan.Common/Errors/ErrorCategory.cs
git commit -m "feat(errors): add ErrorCategory enum with ToHttpStatus extension"
```

---

## Task 2: Core Types — ErrorDescriptor

**Files:**
- Create: `LuBan.Common/Errors/ErrorDescriptor.cs`

- [ ] **Step 1: Create ErrorDescriptor.cs**

```csharp
namespace LuBan.Common.Errors;

public readonly struct ErrorDescriptor : IEquatable<ErrorDescriptor>
{
    public int Code { get; }
    public string Message { get; }
    public ErrorCategory Category { get; }

    public int HttpStatusCode => Category.ToHttpStatus();

    public ErrorDescriptor(int code, string message, ErrorCategory category)
    {
        Code = code;
        Message = message;
        Category = category;
    }

    public bool Equals(ErrorDescriptor other) => Code == other.Code;
    public override bool Equals(object? obj) => obj is ErrorDescriptor other && Equals(other);
    public override int GetHashCode() => Code;
    public override string ToString() => $"[{Code}] {Message}";

    public static bool operator ==(ErrorDescriptor left, ErrorDescriptor right) => left.Equals(right);
    public static bool operator !=(ErrorDescriptor left, ErrorDescriptor right) => !left.Equals(right);
}
```

- [ ] **Step 2: Build to verify compilation**

Run: `dotnet build LuBan.Common/LuBan.Common.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add LuBan.Common/Errors/ErrorDescriptor.cs
git commit -m "feat(errors): add ErrorDescriptor struct"
```

---

## Task 3: Framework Error Codes Definition

**Files:**
- Create: `LuBan.Common/Errors/FrameworkErrors.cs`

This file is large (~200 lines). Create it with all nested domain classes.

- [ ] **Step 1: Create FrameworkErrors.cs with all domain classes**

```csharp
namespace LuBan.Common.Errors;

public static class FrameworkErrors
{
    public static class Common
    {
        public static readonly ErrorDescriptor InputEmpty = new(10001, "输入值不能为空", ErrorCategory.Validation);
        public static readonly ErrorDescriptor OutputEmpty = new(10002, "输出值不能为空", ErrorCategory.Validation);
        public static readonly ErrorDescriptor TypeIncorrect = new(10003, "类型不正确", ErrorCategory.Validation);
        public static readonly ErrorDescriptor CaptchaError = new(10004, "验证码错误", ErrorCategory.Validation);
        public static readonly ErrorDescriptor IdEmpty = new(10005, "Id不能为空", ErrorCategory.Validation);
        public static readonly ErrorDescriptor ParamEmpty = new(10006, "输入的参数不能为空", ErrorCategory.Validation);
        public static readonly ErrorDescriptor PhoneEmpty = new(10007, "请输入手机号", ErrorCategory.Validation);
        public static readonly ErrorDescriptor PhoneInvalid = new(10008, "请输入正确的手机号", ErrorCategory.Validation);
    }

    public static class User
    {
        public static readonly ErrorDescriptor PasswordIncorrect = new(11001, "密码不正确", ErrorCategory.Authentication);
        public static readonly ErrorDescriptor OldPasswordWrong = new(11002, "旧密码输入错误", ErrorCategory.Validation);
        public static readonly ErrorDescriptor AccountFrozen = new(11003, "账号已冻结", ErrorCategory.Authentication);
        public static readonly ErrorDescriptor AccountNotFound = new(11004, "账号不存在", ErrorCategory.NotFound);
        public static readonly ErrorDescriptor AccountExists = new(11005, "账号已存在", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor UserNotRegistered = new(11006, "用户未注册", ErrorCategory.NotFound);
        public static readonly ErrorDescriptor RecordNotFound = new(11007, "记录不存在", ErrorCategory.NotFound);
        public static readonly ErrorDescriptor DataExists = new(11008, "数据已存在", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor DataInUse = new(11009, "数据不存在或含有关联引用，禁止删除", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor DuplicateOrInvalidData = new(11010, "重复数据或记录含有不存在数据", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor CannotDeleteSelf = new(11011, "非法操作，禁止删除自己", ErrorCategory.Business);
        public static readonly ErrorDescriptor NoPermissionOnData = new(11012, "没有权限操作该数据", ErrorCategory.Authorization);
        public static readonly ErrorDescriptor AdminPasswordProtected = new(11013, "测试数据禁止更改用户【admin】密码", ErrorCategory.Authorization);
        public static readonly ErrorDescriptor NoPermission = new(11014, "没有权限", ErrorCategory.Authorization);
        public static readonly ErrorDescriptor CannotDeleteSuperAdmin = new(11015, "禁止删除超级管理员", ErrorCategory.Authorization);
        public static readonly ErrorDescriptor CannotModifySuperAdminStatus = new(11016, "禁止修改超级管理员状态", ErrorCategory.Authorization);
        public static readonly ErrorDescriptor CannotDeleteAdmin = new(11017, "禁止删除管理员", ErrorCategory.Authorization);
    }

    public static class Tenant
    {
        public static readonly ErrorDescriptor DefaultTenantStatusLocked = new(12001, "默认租户状态禁止修改", ErrorCategory.Authorization);
        public static readonly ErrorDescriptor CannotCreateDbType = new(12002, "禁止创建此类型的数据库", ErrorCategory.Business);
        public static readonly ErrorDescriptor TenantDisabled = new(12003, "租户已禁用", ErrorCategory.Authentication);
        public static readonly ErrorDescriptor CannotDeleteDefaultTenant = new(12004, "禁止删除默认租户", ErrorCategory.Authorization);
        public static readonly ErrorDescriptor TenantNameDuplicate = new(12005, "已存在同名的租户", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor TenantAdminDuplicate = new(12006, "已存在同名的租户管理员", ErrorCategory.Conflict);
    }

    public static class Dict
    {
        public static readonly ErrorDescriptor DictTypeNotFound = new(13000, "字典类型不存在", ErrorCategory.NotFound);
        public static readonly ErrorDescriptor DictTypeDuplicate = new(13001, "字典类型已存在,名称或编码重复", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor DictTypeHasChildren = new(13002, "字典类型下面有字典值禁止删除", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor DictDataDuplicate = new(13003, "字典值已存在,名称或编码重复", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor DictDataNotFound = new(13004, "字典值不存在", ErrorCategory.NotFound);
        public static readonly ErrorDescriptor DictStatusError = new(13005, "字典状态错误", ErrorCategory.Business);
    }

    public static class Menu
    {
        public static readonly ErrorDescriptor MenuExists = new(14000, "菜单已存在", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor RouteEmpty = new(14001, "路由地址为空", ErrorCategory.Validation);
        public static readonly ErrorDescriptor OpenMethodEmpty = new(14002, "打开方式为空", ErrorCategory.Validation);
        public static readonly ErrorDescriptor PermIdentifierEmpty = new(14003, "权限标识格式为空", ErrorCategory.Validation);
        public static readonly ErrorDescriptor PermIdentifierFormatError = new(14004, "权限标识格式错误 如xxx:xxx", ErrorCategory.Validation);
        public static readonly ErrorDescriptor PermNotFound = new(14005, "权限不存在", ErrorCategory.NotFound);
        public static readonly ErrorDescriptor ParentMenuIsSelf = new(14006, "父级菜单不能为当前节点", ErrorCategory.Validation);
        public static readonly ErrorDescriptor CannotMoveRoot = new(14007, "不能移动根节点", ErrorCategory.Business);
        public static readonly ErrorDescriptor ParentSameAsSelf = new(14008, "禁止本节点与父节点相同", ErrorCategory.Validation);
        public static readonly ErrorDescriptor ParentMenuNotFound = new(14009, "父菜单不存在", ErrorCategory.NotFound);
    }

    public static class Role
    {
        public static readonly ErrorDescriptor CannotAssignRoleToAdmin = new(15001, "禁止为管理员分配角色", ErrorCategory.Authorization);
        public static readonly ErrorDescriptor CannotAssignPermToSuperAdmin = new(15002, "禁止为超级管理员角色分配权限", ErrorCategory.Authorization);
        public static readonly ErrorDescriptor CannotDeleteAdminRole = new(15003, "禁止删除系统管理员角色", ErrorCategory.Authorization);
        public static readonly ErrorDescriptor CannotModifyAdminRole = new(15004, "禁止修改系统管理员角色", ErrorCategory.Authorization);
        public static readonly ErrorDescriptor CannotAssignPermToAdminRole = new(15005, "禁止为系统管理员角色分配权限", ErrorCategory.Authorization);
        public static readonly ErrorDescriptor CannotAssignRoleToSuperAdmin = new(15006, "禁止为超级管理员分配角色", ErrorCategory.Authorization);
    }

    public static class Org
    {
        public static readonly ErrorDescriptor ParentOrgNotFound = new(16000, "父机构不存在", ErrorCategory.NotFound);
        public static readonly ErrorDescriptor ParentOrgIsSelf = new(16001, "当前机构Id不能与父机构Id相同", ErrorCategory.Validation);
        public static readonly ErrorDescriptor OrgDuplicate = new(16002, "已有相同组织机构,编码或名称相同", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor NoPermissionOnOrg = new(16003, "没有权限操作机构", ErrorCategory.Authorization);
        public static readonly ErrorDescriptor OrgHasUsers = new(16004, "该机构下有用户禁止删除", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor SubOrgHasUsers = new(16005, "附属机构下有用户禁止删除", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor OnlyAddChild = new(16006, "只能增加下级机构", ErrorCategory.Business);
        public static readonly ErrorDescriptor ChildOrgHasUsers = new(16007, "下级机构下有用户禁止删除", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor CannotDeleteTenantDefaultOrg = new(16008, "租户默认机构禁止删除", ErrorCategory.Authorization);
        public static readonly ErrorDescriptor CannotAddRootOrg = new(16009, "禁止增加根节点机构", ErrorCategory.Business);
    }

    public static class File
    {
        public static readonly ErrorDescriptor FileNotFound = new(17000, "文件不存在", ErrorCategory.NotFound);
        public static readonly ErrorDescriptor FileTypeNotAllowed = new(17001, "不允许的文件类型", ErrorCategory.Validation);
        public static readonly ErrorDescriptor FileTooLarge = new(17002, "文件超过允许大小", ErrorCategory.Validation);
        public static readonly ErrorDescriptor FileExtError = new(17003, "文件后缀错误", ErrorCategory.Validation);
        public static readonly ErrorDescriptor FileExists = new(17004, "文件已存在", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor FileNameEmpty = new(17005, "上传文件名不能为空", ErrorCategory.Validation);
        public static readonly ErrorDescriptor FileContentEmpty = new(17006, "上传文件不能为空", ErrorCategory.Validation);
    }

    public static class Task
    {
        public static readonly ErrorDescriptor TaskNameDuplicate = new(18001, "已存在同名任务调度", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor TaskNotFound = new(18002, "任务调度不存在", ErrorCategory.NotFound);
        public static readonly ErrorDescriptor ScriptEmpty = new(18003, "脚本代码不能为空", ErrorCategory.Validation);
        public static readonly ErrorDescriptor JobDetailMissing = new(18004, "脚本代码中的作业类，需要定义 [JobDetail] 特性", ErrorCategory.Validation);
        public static readonly ErrorDescriptor JobIdMismatch = new(18005, "作业编号需要与脚本代码中的作业类 [JobDetail('jobId')] 一致", ErrorCategory.Validation);
        public static readonly ErrorDescriptor CannotModifyJobId = new(18006, "禁止修改作业编号", ErrorCategory.Business);
        public static readonly ErrorDescriptor JobExecFailed = new(18007, "执行作业失败", ErrorCategory.Business);
    }

    public static class Auth
    {
        public static readonly ErrorDescriptor NotLoggedIn = new(20001, "非法操作，未登录", ErrorCategory.Authentication);
        public static readonly ErrorDescriptor KickedOffline = new(20002, "已将其他地方登录账号下线", ErrorCategory.Authentication);
    }

    public static class Security
    {
        public static readonly ErrorDescriptor DuplicateOperation = new(21001, "当前操作重复", ErrorCategory.Validation);
        public static readonly ErrorDescriptor OperationExpired = new(21002, "当前操作过期", ErrorCategory.Validation);
        public static readonly ErrorDescriptor InvalidSignature = new(21003, "签名不正确", ErrorCategory.Authentication);
    }

    public static class Print
    {
        public static readonly ErrorDescriptor PrintTemplateDuplicate = new(22001, "已存在同名打印模板", ErrorCategory.Conflict);
    }

    public static class App
    {
        public static readonly ErrorDescriptor AppDuplicate = new(23001, "已存在同名或同编码应用", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor OnlyOneActiveSystem = new(23002, "默认激活系统只能有一个", ErrorCategory.Business);
        public static readonly ErrorDescriptor AppHasMenus = new(23003, "该应用下有菜单禁止删除", ErrorCategory.Conflict);
    }

    public static class Position
    {
        public static readonly ErrorDescriptor PositionDuplicate = new(24001, "已存在同名或同编码职位", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor PositionHasUsers = new(24002, "该职位下有用户禁止删除", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor CannotModifyPosition = new(24003, "无权修改本职位", ErrorCategory.Authorization);
        public static readonly ErrorDescriptor PositionNotFound = new(24004, "职位不存在", ErrorCategory.NotFound);
    }

    public static class Notice
    {
        public static readonly ErrorDescriptor NoticeStatusError = new(25001, "通知公告状态错误", ErrorCategory.Business);
        public static readonly ErrorDescriptor NoticeDeleteFailed = new(25002, "通知公告删除失败", ErrorCategory.Business);
        public static readonly ErrorDescriptor NoticeMustBeDraft = new(25003, "通知公告编辑失败，类型必须为草稿", ErrorCategory.Validation);
        public static readonly ErrorDescriptor NoticeOnlyByPublisher = new(25004, "通知公告操作失败，非发布者不能进行操作", ErrorCategory.Authorization);
    }

    public static class Config
    {
        public static readonly ErrorDescriptor ConfigDuplicate = new(26001, "已存在同名或同编码参数配置", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor CannotDeleteSystemParam = new(26002, "禁止删除系统参数", ErrorCategory.Authorization);
    }

    public static class CodeGen
    {
        public static readonly ErrorDescriptor TemplateGenerated = new(27001, "该表代码模板已经生成过", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor TypeNotFound = new(27002, "该类型不存在", ErrorCategory.NotFound);
        public static readonly ErrorDescriptor FieldNotFound = new(27003, "该字段不存在", ErrorCategory.NotFound);
        public static readonly ErrorDescriptor NotEnumType = new(27004, "该类型不是枚举类型", ErrorCategory.Validation);
        public static readonly ErrorDescriptor EntityNotFound = new(27005, "该实体不存在", ErrorCategory.NotFound);
    }

    public static class Resource
    {
        public static readonly ErrorDescriptor ParentResourceNotFound = new(28001, "父资源不存在", ErrorCategory.NotFound);
        public static readonly ErrorDescriptor ParentResourceIsSelf = new(28002, "当前资源Id不能与父资源Id相同", ErrorCategory.Validation);
        public static readonly ErrorDescriptor ResourceDuplicate = new(28003, "已有相同编码或名称", ErrorCategory.Conflict);
        public static readonly ErrorDescriptor FuncDuplicate = new(28004, "已存在同名功能或同名程序及插件", ErrorCategory.Conflict);
    }

    public static class Demo
    {
        public static readonly ErrorDescriptor DemoEnvReadOnly = new(29001, "演示环境禁止修改数据", ErrorCategory.Authorization);
    }

    public static class Identity
    {
        public static readonly ErrorDescriptor IdentityDuplicate = new(30001, "身份标识已存在", ErrorCategory.Conflict);
    }

    public static class Database
    {
        public static readonly ErrorDescriptor NoDataColumns = new(31000, "请添加数据列", ErrorCategory.Validation);
        public static readonly ErrorDescriptor TableNotFound = new(31001, "数据表不存在", ErrorCategory.NotFound);
        public static readonly ErrorDescriptor DuplicateFieldName = new(31002, "不允许添加相同字段名", ErrorCategory.Validation);
    }

    public static class System
    {
        public static readonly ErrorDescriptor Ok = new(200, "操作成功", ErrorCategory.System);
        public static readonly ErrorDescriptor InternalError = new(500, "系统异常，详情请在系统日志中查阅", ErrorCategory.System);
    }

    private static readonly IReadOnlyList<ErrorDescriptor> _all = typeof(FrameworkErrors)
        .GetNestedTypes()
        .SelectMany(t => t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
        .Where(f => f.FieldType == typeof(ErrorDescriptor))
        .Select(f => (ErrorDescriptor)f.GetValue(null)!)
        .ToList();

    public static IReadOnlyList<ErrorDescriptor> All => _all;
}
```

- [ ] **Step 2: Build to verify compilation**

Run: `dotnet build LuBan.Common/LuBan.Common.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add LuBan.Common/Errors/FrameworkErrors.cs
git commit -m "feat(errors): add FrameworkErrors with all domain error codes"
```

---

## Task 4: FriendlyException Redesign

**Files:**
- Modify: `LuBan.Common/Errors/FriendlyException.cs`

- [ ] **Step 1: Rewrite FriendlyException.cs**

Replace the entire file content with:

```csharp
namespace System;

using LuBan.Common.Errors;

public class FriendlyException : Exception
{
    public FriendlyException(ErrorDescriptor error, params object[] args)
        : base(FormatMessage(error.Message, args))
    {
        Error = error;
        HttpStatusCode = error.HttpStatusCode;
    }

    public FriendlyException(string customMessage, ErrorDescriptor error, params object[] args)
        : base($"{customMessage}.{FormatMessage(error.Message, args)}")
    {
        Error = error;
        HttpStatusCode = error.HttpStatusCode;
    }

    public FriendlyException(string message, ErrorCategory category = ErrorCategory.Business)
        : base(message)
    {
        Error = new ErrorDescriptor(0, message, category);
        HttpStatusCode = category.ToHttpStatus();
    }

    public FriendlyException(string message, Exception innerException, ErrorCategory category = ErrorCategory.System)
        : base(message, innerException)
    {
        Error = new ErrorDescriptor(0, message, category);
        HttpStatusCode = category.ToHttpStatus();
    }

    public ErrorDescriptor Error { get; }
    public int HttpStatusCode { get; set; }
    public new object[]? Data { get; set; }

    private static string FormatMessage(string template, object[] args)
    {
        if (args == null || args.Length == 0) return template;
        try { return string.Format(template, args); }
        catch { return template; }
    }
}
```

- [ ] **Step 2: Build to verify compilation**

Run: `dotnet build LuBan.Common/LuBan.Common.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add LuBan.Common/Errors/FriendlyException.cs
git commit -m "refactor(errors): redesign FriendlyException to use ErrorDescriptor"
```

---

## Task 5: FriendlyError Redesign

**Files:**
- Modify: `LuBan.Common/Errors/FriendlyError.cs`

- [ ] **Step 1: Rewrite FriendlyError.cs**

Replace the entire file content with:

```csharp
namespace System;

using LuBan.Common.Errors;

public static class FriendlyError
{
    public static FriendlyException Ex(ErrorDescriptor error, params object[] args)
        => new(error, args);

    public static FriendlyException Ex(string message, ErrorDescriptor error, params object[] args)
        => new(message, error, args);

    public static FriendlyException Ex(string message, ErrorCategory category = ErrorCategory.Business)
        => new(message, category);

    public static FriendlyException Ex(string message, Exception exception, ErrorCategory category = ErrorCategory.System)
        => new(message, exception, category);

    public static FriendlyException Ex(Exception exception)
        => new(exception.Message, exception, ErrorCategory.System);

    public static FriendlyException SetStatusCode(this FriendlyException exception, int statusCode)
    {
        exception.HttpStatusCode = statusCode;
        return exception;
    }

    public static FriendlyException WithData(this FriendlyException exception, params object[] data)
    {
        exception.Data = data;
        return exception;
    }
}
```

- [ ] **Step 2: Build to verify compilation**

Run: `dotnet build LuBan.Common/LuBan.Common.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add LuBan.Common/Errors/FriendlyError.cs
git commit -m "refactor(errors): redesign FriendlyError to use ErrorDescriptor"
```

---

## Task 6: Success/Fail Response Types

**Files:**
- Modify: `LuBan.Common/Data/Success.cs`
- Modify: `LuBan.Common/Data/Fail.cs`

- [ ] **Step 1: Update Success.cs — remove EnumErrorCode overloads**

Find and remove the constructor that takes `EnumErrorCode`:

```csharp
// REMOVE this constructor:
public Success(object? d, EnumErrorCode code) : this(d, (int)code)
{
}
```

And in `Success<T>`:

```csharp
// REMOVE this constructor:
public Success(T t, EnumErrorCode code) : this(t, (int)code)
{
}
```

- [ ] **Step 2: Update Fail.cs — add FriendlyException constructor and remove EnumErrorCode**

Add new constructor and remove EnumErrorCode overloads:

```csharp
// In Fail class, ADD:
public Fail(FriendlyException ex)
{
    Code = ex.Error.Code;
    Message = ex.Message;
    Type = ex.Error.Category.ToString();
    Time = DateTimeUtil.Now;
}

// REMOVE these constructors:
public Fail(Exception? ex, EnumErrorCode code) : this(ex, (int)code) { }
public Fail(string msg, EnumErrorCode code) : this(msg, (int)code) { }

// In Fail<T> class, ADD:
public Fail(FriendlyException ex)
{
    Code = ex.Error.Code;
    Message = ex.Message;
    Type = ex.Error.Category.ToString();
    Time = DateTimeUtil.Now;
}

// REMOVE these constructors:
public Fail(Exception? ex, EnumErrorCode code) : this(ex, (int)code) { }
public Fail(string msg, EnumErrorCode code) : this(msg, (int)code) { }
```

- [ ] **Step 3: Build to verify compilation**

Run: `dotnet build LuBan.Common/LuBan.Common.csproj`
Expected: Build errors from remaining EnumErrorCode references (expected — will fix in migration tasks)

- [ ] **Step 4: Commit**

```bash
git add LuBan.Common/Data/Success.cs LuBan.Common/Data/Fail.cs
git commit -m "refactor(data): update Success/Fail for new error system"
```

---

## Task 7: ErrorCodeRegistry and DI Integration

**Files:**
- Create: `LuBan.Common/Errors/ErrorCodeRegistry.cs`
- Create: `LuBan.Common/Errors/ErrorCodeServiceCollectionExtensions.cs`

- [ ] **Step 1: Create ErrorCodeRegistry.cs**

```csharp
namespace LuBan.Common.Errors;

public sealed class ErrorCodeRegistry
{
    private readonly Dictionary<int, ErrorDescriptor> _byCode = new();

    public ErrorCodeRegistry()
    {
        Register(FrameworkErrors.All);
    }

    public void Register(IEnumerable<ErrorDescriptor> descriptors)
    {
        foreach (var d in descriptors)
        {
            if (!_byCode.TryAdd(d.Code, d))
                throw new InvalidOperationException($"Duplicate error code: {d.Code}");
        }
    }

    public ErrorDescriptor? FindByCode(int code)
        => _byCode.GetValueOrDefault(code);
}
```

- [ ] **Step 2: Create ErrorCodeServiceCollectionExtensions.cs**

```csharp
namespace LuBan.Common.Errors;

using Microsoft.Extensions.DependencyInjection;

public static class ErrorCodeServiceCollectionExtensions
{
    public static IServiceCollection AddErrorCodes(this IServiceCollection services, IEnumerable<ErrorDescriptor> descriptors)
    {
        services.AddSingleton<ErrorCodeRegistry>(sp =>
        {
            var registry = new ErrorCodeRegistry();
            registry.Register(descriptors);
            return registry;
        });
        return services;
    }

    public static IServiceCollection AddErrorCodes(this IServiceCollection services)
    {
        services.AddSingleton<ErrorCodeRegistry>();
        return services;
    }
}
```

- [ ] **Step 3: Build to verify compilation**

Run: `dotnet build LuBan.Common/LuBan.Common.csproj`
Expected: Build succeeded

- [ ] **Step 4: Commit**

```bash
git add LuBan.Common/Errors/ErrorCodeRegistry.cs LuBan.Common/Errors/ErrorCodeServiceCollectionExtensions.cs
git commit -m "feat(errors): add ErrorCodeRegistry and DI extensions"
```

---

## Task 8: Middleware — ErrorHandlingMiddleware

**Files:**
- Modify: `LuBan.Web.Core/Attributes/ErrorHandlingMiddleware.cs`

- [ ] **Step 1: Update HandleExceptionAsync to use HttpStatusCode**

Find the FriendlyException handling block (around line 83-93) and replace:

```csharp
// OLD:
if (ex is FriendlyException friendlyException)
{
    var message = new Fail(friendlyException.Message, (int)friendlyException.ErrorCode).ToJson();
    if (message.IsNotNullOrEmpty())
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsync(message);
        stopwatch.Stop();
        return;
    }
}

// NEW:
if (ex is FriendlyException friendlyException)
{
    var message = new Fail(friendlyException).ToJson();
    if (message.IsNotNullOrEmpty())
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = friendlyException.HttpStatusCode;
        await context.Response.WriteAsync(message);
        stopwatch.Stop();
        return;
    }
}
```

- [ ] **Step 2: Build to verify compilation**

Run: `dotnet build LuBan.Web.Core/LuBan.Web.Core.csproj`
Expected: Build errors from EnumErrorCode references (expected — will fix in migration tasks)

- [ ] **Step 3: Commit**

```bash
git add LuBan.Web.Core/Attributes/ErrorHandlingMiddleware.cs
git commit -m "refactor(middleware): use HttpStatusCode from FriendlyException"
```

---

## Task 9: Middleware — ApiLogAttribute

**Files:**
- Modify: `LuBan.Web.Core/Attributes/ApiLogAttribute.cs`

- [ ] **Step 1: Update FriendlyException handling in OnExceptionAsync**

Find the block (around line 133-141) and replace:

```csharp
// OLD:
if (context.Exception is FriendlyException friendlyException)
{
    var message = new Fail(friendlyException.Message, (int)friendlyException.ErrorCode).ToJson();
    context.HttpContext.Response.StatusCode = StatusCodes.Status200OK;
    // ...
}

// NEW:
if (context.Exception is FriendlyException friendlyException)
{
    var message = new Fail(friendlyException).ToJson();
    context.HttpContext.Response.StatusCode = friendlyException.HttpStatusCode;
    // ...
}
```

- [ ] **Step 2: Build to verify compilation**

Run: `dotnet build LuBan.Web.Core/LuBan.Web.Core.csproj`
Expected: Build errors (expected)

- [ ] **Step 3: Commit**

```bash
git add LuBan.Web.Core/Attributes/ApiLogAttribute.cs
git commit -m "refactor(middleware): update ApiLogAttribute for new error system"
```

---

## Task 10: Migrate AraReplayAttacksUtil

**Files:**
- Modify: `LuBan.Common/AraReplayAttacksUtil.cs`

- [ ] **Step 1: Add using directive at top**

```csharp
using LuBan.Common.Errors;
```

- [ ] **Step 2: Replace all EnumErrorCode.D0001 usages**

Find each `EnumErrorCode.D0001` and replace with `FrameworkErrors.Common.InputEmpty`:

```csharp
// Example (line ~177):
throw FriendlyError.Ex("The required parameter cannot be empty.", FrameworkErrors.Common.InputEmpty);
```

Apply to all D0001 occurrences (lines 177, 208, 215, 221, 234, 243, 251, 256, 261, 268).

- [ ] **Step 3: Replace EnumErrorCode.P0001**

```csharp
// (line ~234):
throw FriendlyError.Ex("The current operation cannot be repeated.", FrameworkErrors.Security.DuplicateOperation);
```

- [ ] **Step 4: Replace EnumErrorCode.P0002**

```csharp
// (lines ~208, ~215):
throw FriendlyError.Ex("The timestamp of the current operation has expired.", FrameworkErrors.Security.OperationExpired);
```

Note: If HTTP 410 is required, add `.SetStatusCode(410)`.

- [ ] **Step 5: Replace EnumErrorCode.P0003**

```csharp
// (line ~275):
throw FriendlyError.Ex("The current operation signature error.", FrameworkErrors.Security.InvalidSignature);
```

- [ ] **Step 6: Build to verify**

Run: `dotnet build LuBan.Common/LuBan.Common.csproj`
Expected: Build succeeded for this file

- [ ] **Step 7: Commit**

```bash
git add LuBan.Common/AraReplayAttacksUtil.cs
git commit -m "refactor: migrate AraReplayAttacksUtil to new error system"
```

---

## Task 11: Migrate CloudStorage/FileHandler

**Files:**
- Modify: `LuBan.CloudStorage/FileHandler.cs`

- [ ] **Step 1: Add using directive**

```csharp
using LuBan.Common.Errors;
```

- [ ] **Step 2: Replace D8001, D8002, D8003, D8006**

```csharp
// D8001 → FrameworkErrors.File.FileTypeNotAllowed
// D8002 → FrameworkErrors.File.FileTooLarge
// D8003 → FrameworkErrors.File.FileExtError
// D8006 → FrameworkErrors.File.FileContentEmpty
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build LuBan.CloudStorage/LuBan.CloudStorage.csproj`
Expected: Build succeeded

- [ ] **Step 4: Commit**

```bash
git add LuBan.CloudStorage/FileHandler.cs
git commit -m "refactor: migrate CloudStorage FileHandler to new error system"
```

---

## Task 12: Migrate OpenApiAccessAttribute

**Files:**
- Modify: `LuBan.Web.Core/Attributes/OpenApiAccessAttribute.cs`

- [ ] **Step 1: Add using directive**

```csharp
using LuBan.Common.Errors;
```

- [ ] **Step 2: Replace D1011 usage**

```csharp
// OLD:
throw FriendlyError.Ex("Unauthorized", EnumErrorCode.D1011, 401);

// NEW:
throw FriendlyError.Ex("Unauthorized", FrameworkErrors.Auth.NotLoggedIn);
// Note: HTTP 401 is auto-derived from Authentication category
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build LuBan.Web.Core/LuBan.Web.Core.csproj`
Expected: Build progressed

- [ ] **Step 4: Commit**

```bash
git add LuBan.Web.Core/Attributes/OpenApiAccessAttribute.cs
git commit -m "refactor: migrate OpenApiAccessAttribute to new error system"
```

---

## Task 13: Migrate SwaggerController

**Files:**
- Modify: `LuBan.Web.Core/Swagger/SwaggerController.cs`

- [ ] **Step 1: Replace Ex(string, 500) calls**

```csharp
// OLD:
throw FriendlyError.Ex($"...", 500);

// NEW:
throw FriendlyError.Ex($"...", ErrorCategory.System);
```

- [ ] **Step 2: Build to verify**

Run: `dotnet build LuBan.Web.Core/LuBan.Web.Core.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add LuBan.Web.Core/Swagger/SwaggerController.cs
git commit -m "refactor: migrate SwaggerController to new error system"
```

---

## Task 14: WeChat Module — WeChatErrors

**Files:**
- Create: `LuBan.Wechat/Errors/WeChatErrors.cs`

- [ ] **Step 1: Create WeChatErrors.cs**

```csharp
namespace LuBan.Wechat.Errors;

using LuBan.Common.Errors;

public static class WeChatErrors
{
    public static readonly ErrorDescriptor TokenFailed = new(81001, "通过code获取微信token失败", ErrorCategory.Business);
    public static readonly ErrorDescriptor AccessTokenFailed = new(81002, "获取微信accessToken失败", ErrorCategory.Business);
    public static readonly ErrorDescriptor ProfileFailed = new(81003, "获取微信昵称头像失败", ErrorCategory.Business);
    public static readonly ErrorDescriptor NotFollowed = new(81004, "未关注微信公众号", ErrorCategory.Business);
    public static readonly ErrorDescriptor SendFailed = new(81005, "发送微信消息失败", ErrorCategory.Business);
    public static readonly ErrorDescriptor TemplateSendFailed = new(81006, "发送微信模板消息失败", ErrorCategory.Business);
    public static readonly ErrorDescriptor PrepayFailed = new(81007, "微信预付下单失败", ErrorCategory.Business);
    public static readonly ErrorDescriptor OrderInfoFailed = new(81008, "获取微信订单信息失败", ErrorCategory.Business);
}
```

- [ ] **Step 2: Build to verify**

Run: `dotnet build LuBan.Wechat/LuBan.Wechat.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add LuBan.Wechat/Errors/WeChatErrors.cs
git commit -m "feat(wechat): add WeChatErrors definitions"
```

---

## Task 15: WeChat Module — Migrate Exception Throws

**Files:**
- Modify: `LuBan.Wechat/WechatCorpClient.cs`
- Modify: `LuBan.Wechat/CorpProvider/WechatCorpSuiteClient.cs`
- Modify: `LuBan.Wechat/CorpProvider/WechatCorpAppClient.cs`

- [ ] **Step 1: Add using directives to each file**

```csharp
using LuBan.Common.Errors;
using LuBan.Wechat.Errors;
```

- [ ] **Step 2: Replace throw new Exception with FriendlyError.Ex**

In `WechatCorpClient.cs`:
```csharp
// OLD:
throw new Exception($"获取企业微信AccessToken失败:code:{result.ErrorCode},msg:{result.ErrorMessage}");
// NEW:
throw FriendlyError.Ex($"获取企业微信AccessToken失败:code:{result.ErrorCode},msg:{result.ErrorMessage}", WeChatErrors.AccessTokenFailed);
```

Apply similar changes to all `throw new Exception(...)` occurrences in the WeChat module files.

- [ ] **Step 3: Build to verify**

Run: `dotnet build LuBan.Wechat/LuBan.Wechat.csproj`
Expected: Build succeeded

- [ ] **Step 4: Commit**

```bash
git add LuBan.Wechat/
git commit -m "refactor(wechat): migrate to FriendlyException with WeChatErrors"
```

---

## Task 16: Demo Project — AppErrors

**Files:**
- Create: `WebApplication1/Errors/AppErrors.cs`

- [ ] **Step 1: Create AppErrors.cs**

```csharp
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
```

- [ ] **Step 2: Build to verify**

Run: `dotnet build WebApplication1/WebApplication1.csproj`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add WebApplication1/Errors/AppErrors.cs
git commit -m "feat(demo): add AppErrors for business error codes"
```

---

## Task 17: Demo Project — Migrate Services

**Files:**
- Modify: `WebApplication1/Services/**/*.cs` (all service files using EnumErrorCode)

- [ ] **Step 1: Add using directive to each service file**

```csharp
using LuBan.Common.Errors;
using WebApplication1.Errors;
```

- [ ] **Step 2: Replace EnumErrorCode.X with FrameworkErrors/AppErrors**

Example mappings:
- `EnumErrorCode.D0009` → `FrameworkErrors.User.AccountNotFound`
- `EnumErrorCode.D1001` → `FrameworkErrors.User.CannotDeleteSelf`
- `EnumErrorCode.D1003` → `FrameworkErrors.User.AccountExists`
- `EnumErrorCode.D1014` → `FrameworkErrors.User.CannotDeleteSuperAdmin`
- `EnumErrorCode.D1015` → `FrameworkErrors.User.CannotModifySuperAdminStatus`
- `EnumErrorCode.D1022` → `FrameworkErrors.User.CannotAssignRoleToSuperAdmin`
- `EnumErrorCode.D1023` → `FrameworkErrors.Tenant.CannotDeleteDefaultTenant`
- `EnumErrorCode.D1300` → `FrameworkErrors.Tenant.TenantNameDuplicate`
- `EnumErrorCode.D1301` → `FrameworkErrors.Tenant.TenantAdminDuplicate`
- `EnumErrorCode.D3000` → `FrameworkErrors.Dict.DictTypeNotFound`
- `EnumErrorCode.D3001` → `FrameworkErrors.Dict.DictTypeDuplicate`
- `EnumErrorCode.D3005` → `FrameworkErrors.Dict.DictStatusError`
- `EnumErrorCode.D1002` → `FrameworkErrors.User.RecordNotFound`
- `EnumErrorCode.D1006` → `FrameworkErrors.User.DataExists`
- `EnumErrorCode.D1016` → `FrameworkErrors.User.NoPermission`
- `EnumErrorCode.D1019` → `FrameworkErrors.Role.CannotDeleteAdminRole`
- `EnumErrorCode.D8000` → `FrameworkErrors.File.FileNotFound`
- `EnumErrorCode.D8005` → `FrameworkErrors.File.FileNameEmpty`

- [ ] **Step 3: Build to verify**

Run: `dotnet build WebApplication1/WebApplication1.csproj`
Expected: Build succeeded

- [ ] **Step 4: Commit**

```bash
git add WebApplication1/Services/
git commit -m "refactor(demo): migrate all services to new error system"
```

---

## Task 18: Delete Legacy Files

**Files:**
- Delete: `LuBan.Common/Errors/EnumErrorCode.cs`
- Delete: `LuBan.Common/Errors/ErrorCodeTypeAttribute.cs`
- Delete: `LuBan.Common/Errors/ErrorCodeItemMetadataAttribute.cs`

- [ ] **Step 1: Delete the files**

```bash
rm LuBan.Common/Errors/EnumErrorCode.cs
rm LuBan.Common/Errors/ErrorCodeTypeAttribute.cs
rm LuBan.Common/Errors/ErrorCodeItemMetadataAttribute.cs
```

- [ ] **Step 2: Build entire solution**

Run: `dotnet build`
Expected: Build succeeded (all migrations complete)

- [ ] **Step 3: Commit**

```bash
git add -A
git commit -m "refactor: remove legacy EnumErrorCode and attributes"
```

---

## Task 19: Final Verification

- [ ] **Step 1: Build entire solution**

Run: `dotnet build`
Expected: Build succeeded with no errors

- [ ] **Step 2: Run tests if available**

Run: `dotnet test`
Expected: All tests pass

- [ ] **Step 3: Manual smoke test**

Start the demo application and verify:
1. Login works (or shows appropriate error)
2. API error responses have correct HTTP status codes (400/401/403/404)
3. Response `type` field shows category names

- [ ] **Step 4: Final commit if any fixes needed**

```bash
git add -A
git commit -m "fix: address any final migration issues"
```

---

## Self-Review Checklist

After completing all tasks:

1. **Spec coverage:** All sections of the spec implemented?
   - ErrorCategory enum ✓
   - ErrorDescriptor struct ✓
   - FrameworkErrors with all domains ✓
   - FriendlyException redesign ✓
   - FriendlyError redesign ✓
   - Success/Fail changes ✓
   - Middleware changes ✓
   - ErrorCodeRegistry ✓
   - All migrations ✓
   - Legacy files deleted ✓

2. **Placeholder scan:** No TBD/TODO in plan?

3. **Type consistency:** All type names match between tasks?
   - ErrorCategory ✓
   - ErrorDescriptor ✓
   - FrameworkErrors nested classes ✓
   - WeChatErrors ✓
   - AppErrors ✓