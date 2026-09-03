namespace LuBan.Common.Errors;

/// <summary>
/// 框架内置错误码定义。按业务领域分组（嵌套静态类），
/// 每个 ErrorDescriptor 包含错误码、消息和分类（自动推导 HTTP 状态码）。
/// <para>错误码范围：Common 10xxx, User 11xxx, Tenant 12xxx, Dict 13xxx,
/// Menu 14xxx, Role 15xxx, Org 16xxx, File 17xxx, Task 18xxx,
/// Auth 20xxx, Security 21xxx, ... System 500</para>
/// </summary>
public static class FrameworkErrors
{
    /// <summary>通用错误（10001-10008）</summary>
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

        public static readonly ErrorDescriptor DataNotFound = new(31003, "数据不存在", ErrorCategory.NotFound);
    }

    public static class System
    {
        public static readonly ErrorDescriptor InternalError = new(500, "系统异常，详情请在系统日志中查阅", ErrorCategory.System);
    }

    [global::System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("System.Diagnostics.CodeAnalysis", "IL2070", Justification = "Reflection over nested types; library is not trim/AOT targeted.")]
    private static readonly IReadOnlyList<ErrorDescriptor> _all = typeof(FrameworkErrors)
        .GetNestedTypes(global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static)
        .SelectMany(t => t.GetFields(global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static))
        .Where(f => f.FieldType == typeof(ErrorDescriptor))
        .Select(f => (ErrorDescriptor)f.GetValue(null)!)
        .ToList();

    public static IReadOnlyList<ErrorDescriptor> All => _all;
}