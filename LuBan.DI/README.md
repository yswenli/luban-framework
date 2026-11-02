[English](README.en.md) | 中文

# LuBan.DI

> **作者**: yswenli | **联系邮箱**: yswenli@outlook.com | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **"标记接口 + 自动扫描，依赖注入从此告别手动注册。"**

你是否还在为这些 DI 问题头疼？

- 每个服务都要 `services.AddScoped<IXxx, Xxx>()` 写一遍？
- 新增一个服务忘了注册，运行时才爆异常？
- 想加 AOP 拦截，却发现 .NET 原生 DI 不支持代理？
- 瞬态、单例、作用域搞不清楚，注册方式五花八门？

如果是，那 **LuBan.DI** 就是为你准备的。

它是 **.NET 10** 下的轻量级依赖注入扩展库，通过标记接口 + 特性标注 + 程序集扫描，实现服务的自动注册。还支持 AOP 代理拦截，让你的服务在注入时自动获得横切关注点能力。

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.Service](../LuBan.Service/README.md) | [LuBan.AIAgent](../LuBan.AIAgent/README.md) | [LuBan.ApprovalFlow](../LuBan.ApprovalFlow/README.md)

---

## 为什么选择 LuBan.DI？

| 痛点 | LuBan.DI 的解法 |
|---|---|
| 手动注册服务太繁琐 | 实现 `IScoped`/`ISingleton`/`ITransient` 即自动注册 |
| 新增服务忘记注册 | `AutoInjectAllCustomerServices` 扫描所有程序集 |
| 需要 AOP 但 .NET DI 不支持 | `AspectDispatchProxy` 运行时代理，无侵入拦截 |
| 注册顺序不可控 | `[Injection(Order = 1)]` 控制注册优先级 |
| 想跳过某个类的代理 | `[SuppressProxy]` 一行豁免 |

---

## 一分钟预览

```csharp
using System;
using LuBan.DI.Attrs;
using LuBan.DI.Models;

// 1. 定义服务，实现 IScoped 即代表 Scoped 生命周期
public interface IUserService { string GetUserName(int id); }

public class UserService : IUserService, IScoped
{
    public string GetUserName(int id) => $"User_{id}";
}

// 2. 启动时一行扫描注册
var di = new DiContainer();
di.AutoInjectAllCustomerServices();
di.Build().UseAsGlobal();

// 3. 任何地方获取服务（扫描只注册实现类自身，按接口解析需显式注册接口）
var userService = DiContainer.Instance.GetRequiredService<UserService>();
Console.WriteLine(userService.GetUserName(1));  // User_1
```

> 没有废话，只有生产力。

---

## DiContainer（统一容器）

`LuBan.DI.DiContainer` 是框架统一的 DI 容器，同时实现 `IServiceCollection`（注册）、
`IServiceProvider`（解析）、`IServiceScopeFactory`（互操作）与 `IDisposable` / `IAsyncDisposable`。

```csharp
var di = new DiContainer();
di.AddSingleton<IFoo, Foo>();
di.AddScoped<IBar>(d => new Bar(d.GetRequiredService<IFoo>()));
di.Build();                                  // 默认 ValidateScopes + ValidateOnBuild，幂等

using var scope = di.CreateScope();          // 子容器，永远从根派生
var bar = scope.GetRequiredService<IBar>();
```

- 宿主自己 `new` 出来的根容器才允许 `Build()`；`services.AsDiContainer()` / `provider.AsDiContainer()` 是借用视图，不可 `Build()`。
- 包装外部 provider 的视图不持有、不释放被包装对象。
- `DiContainer.Instance` 是全局容器入口；全进程只有这一个静态入口，没有第二套转发层。
- 未构建时 `GetService` 返回 `null`，`GetRequiredService` 抛 `InvalidOperationException`。

---

## 技术栈

- **目标框架**：.NET 10.0
- **项目类型**：类库（Class Library）
- **NuGet 包**：`LuBan.DI`

---

## 安装

```bash
dotnet add package LuBan.DI
```

---

## 工具箱全览

### 标记接口 — 生命周期声明

| 接口 | 命名空间 | 说明 |
|---|---|---|
| `IDependency` | `LuBan.DI.Interfaces` | 依赖标记基接口（禁止外部直接继承） |
| `ITransient` | `LuBan.DI` | 瞬态：每次请求创建新实例 |
| `IScoped` | `LuBan.DI` | 作用域：每个请求范围内共享 |
| `ISingleton` | `LuBan.DI` | 单例：全局唯一实例 |

> 你的服务类只需实现对应的生命周期接口，框架会自动识别并注册。

### 全局 DI 容器 — `DiContainer.Instance`

> 命名空间：`LuBan.DI`

| 成员 | 说明 |
|---|---|
| `DiContainer.Instance` | 全局容器（同时实现 `IServiceCollection` / `IServiceProvider` / `IServiceScopeFactory`） |
| `GetService<T>()` / `GetService(Type)` | 获取服务（未注册或未构建返回 null） |
| `GetRequiredService<T>()` / `GetRequiredService(Type)` | 获取服务（未注册或未构建抛异常） |
| `GetServices<T>()` | 获取某个类型的全部注册 |
| `ResetServices(IServiceCollection)` | 接管外部服务集合（如 ASP.NET 的 `builder.Services`） |
| `Build()` | 幂等构建（默认开启 `ValidateScopes` + `ValidateOnBuild`） |
| `Rebuild()` | 每次重建并释放旧 provider |
| `CreateScope()` | 派生子容器（永远从根派生） |
| `UseAsGlobal()` | 把当前实例设为全局容器 |

### 服务扫描 — `ServiceDescriptorUtil`

> 命名空间：`LuBan.DI`

| 方法 | 说明 |
|---|---|
| `AutoInjectAllCustomerServices(this IServiceCollection)` | 扫描所有程序集，自动注册实现 `IDependency` 的类 |
| `TryGetServiceLifetime(Type)` | 根据标记接口解析 `ServiceLifetime` |
| `Register(IServiceCollection, Type, Type, InjectionAttribute, Type?)` | 手动注册单个类型 |
| `RegisterService(IServiceCollection, Type, Type, InjectionAttribute)` | 扫描入口：按 `Pattern` 决定注册范围（当前只注册实现类自身） |
| `AddDispatchProxy(...)` | 注册 AOP 代理 |

### 注入特性 — `InjectionAttribute`

> 命名空间：`LuBan.DI.Attrs`

| 属性 | 说明 |
|---|---|
| `Action` | 注册方式：`Add` 或 `TryAdd`；当前实现中两者行为一致——已存在同类型注册即跳过，均不覆盖 |
| `Pattern` | 注册范围：`Self`、`FirstInterface`、`SelfWithFirstInterface`、`ImplementedInterfaces`、`All`；当前实现对所有取值都只注册实现类自身，接口注册尚未生效 |
| `Named` | 注册别名（多服务场景；预留，当前未生效） |
| `Order` | 注册排序（值越大越后注册） |
| `Proxy` | AOP 代理类型（必须继承 `AspectDispatchProxy`；仅在显式传入接口的 `Register` 重载中生效） |
| `ExceptInterfaces` | 排除的接口列表（预留，当前未生效） |

### AOP 代理

| 类/接口 | 命名空间 | 说明 |
|---|---|---|
| `AspectDispatchProxy` | `LuBan.DI.Core` | 抽象代理基类，实现 `Invoke`/`InvokeAsync`/`InvokeAsyncT<T>` |
| `IDispatchProxy` | `LuBan.DI.Interfaces` | 代理拦截依赖接口（`Target` + `Services`） |
| `IGlobalDispatchProxy` | `LuBan.DI.Interfaces` | 全局代理拦截接口 |
| `SuppressProxyAttribute` | `LuBan.DI.Attrs` | 标注在类上，跳过全局代理 |

---

## 实战示例

### 基础自动注册

```csharp
using System;
using LuBan.DI.Attrs;

// 瞬态服务
public class EmailService : ITransient
{
    public void Send(string to) => Console.WriteLine($"Sent to {to}");
}

// 单例服务
public class ConfigService : ISingleton
{
    public string GetConfig(string key) => $"value_of_{key}";
}

// 作用域服务，带接口
public interface IOrderService { void CreateOrder(int userId); }

[Injection]
public class OrderService : IOrderService, IScoped
{
    public void CreateOrder(int userId) => Console.WriteLine($"Order created for {userId}");
}
```

### 启动时扫描注册

```csharp
using System;

// ASP.NET Core 的 Program.cs 或 Startup.cs
var builder = WebApplication.CreateBuilder(args);

// 一行扫描所有程序集中实现了 IScoped/ISingleton/ITransient 的类
builder.Services.AutoInjectAllCustomerServices();

// 构建全局 DI 容器
var di = new DiContainer(builder.Services).UseAsGlobal();
di.Rebuild();   // 每次重建；若只想构建一次可用 di.Build()（幂等、默认开启校验）

var app = builder.Build();
app.Run();
```

### 手动获取服务

```csharp
using System;

// 在任意位置获取已注册的服务（无需构造函数注入）
var emailService = DiContainer.Instance.GetRequiredService<EmailService>();
emailService.Send("user@example.com");

var configService = DiContainer.Instance.GetService<ConfigService>();
var value = configService?.GetConfig("AppName");
```

### 控制注册方式

```csharp
using LuBan.DI.Attrs;
using LuBan.DI.Models;

// TryAdd：如果已注册则跳过
[Injection(EnumInjectionActions.TryAdd)]
public class DefaultLogger : ITransient
{
    public void Log(string msg) => Console.WriteLine(msg);
}

// 指定注册顺序
[Injection(Order = 10)]
public class PriorityService : IScoped
{
    // ...
}

// 只注册自己，不注册接口
[Injection(Pattern = EnumInjectionPatterns.Self)]
public class InternalWorker : ITransient
{
    // ...
}
```

### AOP 代理拦截

```csharp
using System.Reflection;
using LuBan.DI.Core;
using LuBan.DI.Attrs;
using LuBan.DI.Interfaces;

// 1. 定义代理类
public class LogDispatchProxy : AspectDispatchProxy, IDispatchProxy
{
    public object Target { get; set; }
    public IServiceProvider Services { get; set; }

    public override object Invoke(MethodInfo method, object[] args)
    {
        Console.WriteLine($"[Before] {method.Name}");
        var result = method.Invoke(Target, args);
        Console.WriteLine($"[After] {method.Name}");
        return result;
    }

    public override async Task InvokeAsync(MethodInfo method, object[] args)
    {
        Console.WriteLine($"[Before Async] {method.Name}");
        await (Task)method.Invoke(Target, args);
        Console.WriteLine($"[After Async] {method.Name}");
    }

    public override async Task<T> InvokeAsyncT<T>(MethodInfo method, object[] args)
    {
        Console.WriteLine($"[Before Async<T>] {method.Name}");
        var result = await (Task<T>)method.Invoke(Target, args);
        Console.WriteLine($"[After Async<T>] {method.Name}");
        return result;
    }
}

// 2. 接口 + 代理需显式注册：扫描路径只注册实现类自身，
//    类上的 [Injection(Proxy = ...)] 不会注册接口，也就不会产生代理
public class UserService : IUserService, IScoped
{
    public string GetUserName(int id) => $"User_{id}";
}

var di = new DiContainer();
ServiceDescriptorUtil.Register(di, typeof(IScoped), typeof(UserService),
    new InjectionAttribute { Proxy = typeof(LogDispatchProxy) }, typeof(IUserService));
di.Build().UseAsGlobal();

// 3. 调用时自动走代理
var svc = DiContainer.Instance.GetRequiredService<IUserService>();
svc.GetUserName(1);
// 输出:
// [Before] GetUserName
// [After] GetUserName
```

### 跳过代理

```csharp
using LuBan.DI.Attrs;

// 即使全局配置了代理，这个类也不会被代理
[SuppressProxy]
public class HealthCheckService : ITransient
{
    public bool IsHealthy() => true;
}
```

---

## 使用小贴士

1. **命名空间别搞错**：`ITransient`/`IScoped`/`ISingleton` 在 `LuBan.DI`；`IDependency` 在 `LuBan.DI.Interfaces`；`InjectionAttribute` 在 `LuBan.DI.Attrs`；`AspectDispatchProxy` 在 `LuBan.DI.Core`。
2. **扫描顺序**：`AutoInjectAllCustomerServices` 会扫描 `RuntimeUtil.GetTypes()` 返回的所有类型，确保你的程序集已被加载。
3. **先注册、后构建**：注册完成后调用 `Build()`（幂等）或 `Rebuild()`（每次重建），再调用 `UseAsGlobal()` 把容器设为全局入口。
4. **代理类必须继承 `AspectDispatchProxy`**：同时实现 `IDispatchProxy` 接口以获取 `Target` 和 `Services`。
5. **`[SuppressProxy]` 豁免代理**：不想被代理的类加上这个特性即可。
6. **`Order` 控制注册顺序**：值越大越后注册；由于已存在同类型即跳过，「先注册者胜出」，需要替换实现请改注册顺序或直接移除旧注册。

---

## 许可证

Copyright (c) yswenli.

Licensed under the [Apache License 2.0](../LICENSE).

---

**LuBan.DI** —— 让 .NET 依赖注入更自动、更灵活、更优雅。

> 如果你也受够了手动注册服务的痛苦，那就把它加进你的工具箱吧。
