# Error Code System Redesign

## Overview

Replace the monolithic `EnumErrorCode` enum (791 lines, 100+ members) with a structured `ErrorDescriptor`-based system that provides:

- Modular classification by domain within the framework
- Extensibility for business projects to define their own error codes
- Automatic HTTP status code mapping from error category
- i18n-ready design (message resolution extensible in the future)

This is a breaking change. The existing `EnumErrorCode`, `ErrorCodeTypeAttribute`, and `ErrorCodeItemMetadataAttribute` will be removed.

## Scope

- Refactor `LuBan.Common` error handling infrastructure
- Migrate `WebApplication1` demo project error codes to the new system
- Add `WeChatErrors` to `LuBan.Wechat` and replace plain `throw new Exception(...)` call sites with `FriendlyException` (the old W-series enum members were never referenced; WeChat failures currently surface as generic HTTP 500 system errors, which hides their business semantics)
- Update `LuBan.Web.Core` middleware and filters

## Core Types

### ErrorCategory Enum

Each error code belongs to a category. The category determines the HTTP status code automatically.

| Category | HTTP Status | Meaning |
|----------|-------------|---------|
| `Validation` | 400 | Input validation failure |
| `Authentication` | 401 | Not authenticated / session expired |
| `Authorization` | 403 | Insufficient permissions |
| `NotFound` | 404 | Resource not found |
| `Conflict` | 409 | Data conflict / duplicate |
| `Business` | 422 | Business rule violation |
| `System` | 500 | System-level error |

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

### ErrorDescriptor Struct

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

### Numeric Range Allocation

Ranges are allocated **by domain**, not by category. `ErrorDescriptor.Category` is an independent dimension that determines the HTTP status code; the numeric range only groups related domains together.

| Range | Assignment |
|-------|------------|
| 10000-19999 | Core management domains (Common, User, Tenant, Dict, Menu, Role, Org, File, Task) |
| 20000-29999 | Extended system domains (Auth, Security, Print, App, Position, Notice, Config, CodeGen, Resource, Demo) |
| 30000-39999 | Infrastructure domains (Identity, Database) |
| 40000-69999 | Reserved for future framework domains |
| 70000-79999 | Reserved for additional system-level errors |
| 80000-89999 | Framework module extensions (e.g., WeChat at 81xxx) |
| 90000-99999 | Business project error codes |

Special case: `FrameworkErrors.System.Ok = 200` and `FrameworkErrors.System.InternalError = 500` intentionally use HTTP-aligned values instead of the 70000 range, because `200` is the established success convention checked by `Result<T>`'s implicit operator and API consumers.

## Framework Error Codes Organization

The single `EnumErrorCode` is replaced by `FrameworkErrors`, a static class with nested static classes per domain.

```
FrameworkErrors
+-- Common        (10000-10999)   Common validation: empty, type, captcha
+-- User          (11000-11999)   User / account: password, frozen, registration
+-- Tenant        (12000-12999)   Tenant: name duplicate, default tenant protection
+-- Dict          (13000-13999)   Dictionary: type / data CRUD
+-- Menu          (14000-14999)   Menu / permission identifier
+-- Role          (15000-15999)   Role protection / role-permission assignment
+-- Org           (16000-16999)   Organization tree
+-- File          (17000-17999)   File upload / type / size
+-- Task          (18000-18999)   Task scheduling / jobs
+-- Auth          (20000-20999)   Authentication: not logged in, kicked offline
+-- Security      (21000-21999)   Security: anti-replay, signature
+-- Print         (22000-22999)   Print templates
+-- App           (23000-23999)   Application management
+-- Position      (24000-24999)   Position / job title
+-- Notice        (25000-25999)   Notifications / announcements
+-- Config        (26000-26999)   System configuration parameters
+-- CodeGen       (27000-27999)   Code generation
+-- Resource      (28000-28999)   Resource tree
+-- Demo          (29000-29999)   Demo environment restrictions
+-- Identity      (30000-30999)   Identity / identifier conflicts
+-- Database      (31000-31999)   Database operations
+-- System        (200 / 500)     System status codes (HTTP-aligned special case)
```

### Example Structure

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
}
```

## Extension Mechanism

### Business Project Error Codes

Business projects define their own error codes as `static readonly ErrorDescriptor` fields:

```csharp
namespace WebApplication1.Errors;

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

### Registration

```csharp
// In Program.cs or startup
builder.Services.AddErrorCodes(AppErrors.All);
```

`AddErrorCodes` is an extension method on `IServiceCollection` that:

1. Creates (or reuses) the singleton `ErrorCodeRegistry`, which is **pre-populated with all `FrameworkErrors` descriptors**
2. Registers the provided business descriptors, rejecting any code that collides with framework codes or previously registered codes
3. Makes the registry available via DI for lookup scenarios (e.g., an error-code dictionary endpoint for frontend sync)

```csharp
public sealed class ErrorCodeRegistry
{
    private readonly Dictionary<int, ErrorDescriptor> _byCode = new();

    public ErrorCodeRegistry()
    {
        Register(FrameworkErrors.All); // framework codes pre-registered
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

`FrameworkErrors.All` is an `IReadOnlyList<ErrorDescriptor>` on `FrameworkErrors` enumerating every framework descriptor (built once via reflection over the nested static classes).

### Numeric Range for Extensions

| Range | Purpose |
|-------|---------|
| 80000-89999 | Framework module extensions (e.g., `LuBan.Wechat` at 81xxx; other modules take 82xxx, 83xxx, ...) |
| 90000-99999 | Business project error codes (each project allocates thousand-level segments: 90xxx, 91xxx, ...) |

### Module-Specific Error Codes (e.g., WeChat)

Module-level error codes (like WeChat) are defined within their respective modules:

```csharp
namespace LuBan.Wechat.Errors;

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

## FriendlyException Redesign

```csharp
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
    public object[]? Data { get; set; }

    private static string FormatMessage(string template, object[] args)
    {
        if (args == null || args.Length == 0) return template;
        try { return string.Format(template, args); }
        catch { return template; }
    }
}
```

Key changes from current:

- `ErrorCode: EnumErrorCode` replaced by `Error: ErrorDescriptor`
- `HttpStatusCode` auto-derived from `ErrorDescriptor.Category`
- `ValidationException` boolean removed (verified: it was only ever set inside `FriendlyError`, never read anywhere)
- `ErrorMessage: object` removed in favor of standard `Exception.Message` (only consumer was `Fail.cs`)
- Untyped exceptions (`new FriendlyException(string, ...)`) get `Error.Code = 0`. This fixes an existing bug: the old `FriendlyError.Ex(string)` set `ErrorCode = SystemOk200`, causing plain validation errors to return body `code = 200` (the success code). Untyped errors now return `code = 0`, which no longer collides with the success convention.
- `HttpStatusCode` remains settable for the rare cases that need a specific status (e.g., anti-replay returning 410 Gone); `SetStatusCode()` extension covers this.

## FriendlyError Redesign

```csharp
public static class FriendlyError
{
    public static FriendlyException Ex(ErrorDescriptor error, params object[] args)
        => new(error, args);

    // Custom message prefixed to the descriptor's message: "{message}.{error.Message}"
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

## ErrorHandlingMiddleware Changes

```csharp
// Old: always returns HTTP 200 for FriendlyException
if (ex is FriendlyException friendlyException)
{
    var message = new Fail(friendlyException.Message, (int)friendlyException.ErrorCode).ToJson();
    context.Response.StatusCode = StatusCodes.Status200OK;
    await context.Response.WriteAsync(message);
}

// New: HTTP status from ErrorDescriptor.Category
if (ex is FriendlyException friendlyException)
{
    var message = new Fail(friendlyException).ToJson();
    context.Response.StatusCode = friendlyException.HttpStatusCode;
    await context.Response.WriteAsync(message);
}
```

## API Response Format

### Success Response

```json
{
    "code": 200,
    "type": "Success",
    "message": "OK",
    "result": { ... },
    "extras": null,
    "time": "2026-08-17T10:00:00"
}
```

### Error Response

```json
{
    "code": 90001,
    "type": "Conflict",
    "message": "已存在同名或同编码项目",
    "result": null,
    "extras": null,
    "time": "2026-08-17T10:00:00"
}
```

The `type` field changes from fixed `"Success"/"Fail"` to the `ErrorCategory` name, providing semantic information for frontend handling.

### Success/Fail Class Changes

```csharp
public sealed class Success : Result
{
    public Success()
    {
        Code = 200;
        Message = "OK";
        Type = "Success";
        Time = DateTimeUtil.Now;
    }

    public Success(object? data, int code = 200)
    {
        Code = code;
        Result = data;
        Message = "OK";
        Type = "Success";
        Time = DateTimeUtil.Now;
    }
}

public sealed class Fail : Result
{
    public Fail(FriendlyException ex)
    {
        Code = ex.Error.Code;
        Message = ex.Message;
        Type = ex.Error.Category.ToString();
        Time = DateTimeUtil.Now;
    }

    public Fail(string message, int code = 500)
    {
        Code = code;
        Message = message;
        Type = "Fail";
        Time = DateTimeUtil.Now;
    }
}
```

## Files to Remove

| File | Reason |
|------|--------|
| `EnumErrorCode.cs` | Replaced by `FrameworkErrors` + `ErrorDescriptor` |
| `ErrorCodeTypeAttribute.cs` | No longer needed (was a marker for error code enums) |
| `ErrorCodeItemMetadataAttribute.cs` | No longer needed (message is embedded in `ErrorDescriptor`) |

## Files to Modify

| File | Change |
|------|--------|
| `FriendlyException.cs` | Replace `EnumErrorCode` with `ErrorDescriptor` |
| `FriendlyError.cs` | Replace `EnumErrorCode` overloads with `ErrorDescriptor` overloads |
| `Success.cs` / `Success<T>` | Remove `EnumErrorCode` constructor overloads |
| `Fail.cs` / `Fail<T>` | Add `FriendlyException` constructor, remove `EnumErrorCode` overloads |
| `ErrorHandlingMiddleware.cs` | Use `friendlyException.HttpStatusCode` instead of hardcoded 200 |
| `ApiLogAttribute.cs` | Same exception-handling change as middleware |
| `AraReplayAttacksUtil.cs` | Migrate `Ex(string, EnumErrorCode, int)` calls; see "Behavior Changes" below |
| `CloudStorage/FileHandler.cs` | Migrate `EnumErrorCode.D8001/D8002/D8003/D8006` to `FrameworkErrors.File.X` |
| `OpenApiAccessAttribute.cs` | Migrate `Ex(string, EnumErrorCode.D1011, 401)` to `Ex(string, FrameworkErrors.Auth.NotLoggedIn)` (auto-401) |
| `SwaggerController.cs` | Migrate `Ex(string, 500)` to `Ex(string, ErrorCategory.System)` |
| `OnlineUserMiddleware.cs` | No functional change required (returns fixed 401 response), but can simplify to use `ex.Error.Category` |
| `LuBan.Wechat/**/*.cs` | Replace `throw new Exception(...)` with `throw FriendlyError.Ex(WeChatErrors.X)` |
| `WebApplication1/Services/**/*.cs` | Migrate all `EnumErrorCode.X` to `FrameworkErrors.X.Y` / `AppErrors.X` |

## Files to Add

| File | Purpose |
|------|---------|
| `LuBan.Common/Errors/ErrorCategory.cs` | ErrorCategory enum + ToHttpStatus extension |
| `LuBan.Common/Errors/ErrorDescriptor.cs` | ErrorDescriptor struct |
| `LuBan.Common/Errors/FrameworkErrors.cs` | Framework error code definitions |
| `LuBan.Common/Errors/ErrorCodeRegistry.cs` | Registry for business error codes |
| `LuBan.Common/Errors/ErrorCodeServiceCollectionExtensions.cs` | AddErrorCodes extension method |
| `WebApplication1/Errors/AppErrors.cs` | Demo project error codes |
| `LuBan.Wechat/Errors/WeChatErrors.cs` | WeChat module error codes |

## API Migration Reference

| Old API (removed) | New API |
|-------------------|---------|
| `FriendlyError.Ex(EnumErrorCode.X)` | `FriendlyError.Ex(FrameworkErrors.A.B)` |
| `FriendlyError.Ex(EnumErrorCode.X, statusCode, args)` | `FriendlyError.Ex(FrameworkErrors.A.B, args)` (+ `.SetStatusCode(n)` only if a non-default status is truly required) |
| `FriendlyError.Ex(string, EnumErrorCode.X, statusCode)` | `FriendlyError.Ex(string, FrameworkErrors.A.B)` |
| `FriendlyError.Ex(string, statusCode)` | `FriendlyError.Ex(string, ErrorCategory.X)` |
| `FriendlyError.Ex(string, Exception, args)` | `FriendlyError.Ex(string, Exception, ErrorCategory.X)` (+ `.WithData(args)` for the old `Data` payload) |
| `FriendlyError.Ex(string, args)` (args went to `Data`, not the message) | `FriendlyError.Ex(string, ErrorCategory.X).WithData(args)` |
| `FriendlyError.Ex(Exception, args)` | `FriendlyError.Ex(Exception)` + `.WithData(args)` |
| `friendlyException.ErrorCode` (EnumErrorCode) | `friendlyException.Error` (ErrorDescriptor) |
| `friendlyException.ErrorMessage` | `friendlyException.Message` |
| `friendlyException.ValidationException` | `friendlyException.Error.Category == ErrorCategory.Validation` |
| `new Success(data, EnumErrorCode.X)` | `new Success(data, int)` |
| `new Fail(ex, EnumErrorCode.X)` | `new Fail(friendlyException)` |
| `new Fail(msg, EnumErrorCode.X)` | `new Fail(msg, int)` |

## Behavior Changes

Callers must be aware of these intentional behavior changes:

1. **HTTP status codes now vary.** `FriendlyException` previously always produced HTTP 200; it now produces the status mapped from its category (400/401/403/404/409/422/500). Frontend interceptors should treat HTTP status as the primary signal and `body.code` for fine-grained handling. `code == 200` still means success.

2. **`type` field semantics.** Error responses now carry the category name (`"Validation"`, `"Conflict"`, ...) instead of the fixed string `"Fail"`. `"Success"` is unchanged.

3. **Anti-replay status codes** (`AraReplayAttacksUtil`): expired timestamp changes from 410 to 400 (Validation); invalid signature changes from 400 to 401 (Authentication); missing parameters change from 200 to 400. If 410 must be preserved, use `.SetStatusCode(410)`.

4. **Untyped string exceptions** (`FriendlyError.Ex(string)`) previously returned body `code = 200` (a bug — indistinguishable from success). They now return `code = 0` with the appropriate HTTP status.

5. **WeChat module errors** previously surfaced as generic HTTP 500 ("Server API error..."). They now surface as their actual messages with category-appropriate statuses.

6. **Message format.** Old messages carried a `[D1003] ` code prefix; new messages contain only the message text. The numeric code is available separately in the response's `code` field, so no information is lost.

## Dropped Legacy Codes

| Old member | Disposition |
|------------|-------------|
| `EnumErrorCode.R2000/R2001/R2002` | **Removed, not migrated.** Verified zero references anywhere in the codebase; messages duplicate the Resource domain (`D1600-D1602`). |
| `EnumErrorCode.W1000-W1007` | **Removed, replaced by `WeChatErrors`** in the `LuBan.Wechat` module (verified zero references; the module threw plain `Exception`s). |
| `EnumErrorCode.D5003` | **Merged into `App.AppDuplicate`.** Was an exact duplicate of `D5000`. |
| `EnumErrorCode.xg1000-xg1002` | **Moved to `WebApplication1/Errors/AppErrors.cs`** as the business-extension example. |

## i18n Extension Point

The current design stores messages as inline strings. Future i18n support can be added by:

1. Changing `ErrorDescriptor.Message` to a resource key
2. Adding an `IErrorMessageResolver` interface that resolves keys to localized strings
3. `FriendlyException` constructor calls the resolver to get the localized message

No implementation in this iteration; the struct design accommodates this change without breaking the API.
