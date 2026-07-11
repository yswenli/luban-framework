[English](README.en.md) | 中文

# LuBan.DI

> **作者**: yswenli | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **"标记接口 + 自动扫描，依赖注入从此告别手动注册。"**

你是否还在为这些 DI 问题头疼？

- 每个服务都要 `services.AddScoped<IXxx, Xxx>()` 写一遍？
- 新增一个服务忘了注册，运行时才爆异常？
- 想加 AOP 拦截，却发现 .NET 原生 DI 不支持代理？
- 瞬态、单例、作用域搞不清楚，注册方式五花八门？

如果是，那 **LuBan.DI** 就是为你准备的。

它是 **.NET 8** 下的轻量级依赖注入扩展库，通过标记接口 + 特性标注 + 程序集扫描，实现服务的自动注册。还支持 AOP 代理拦截，让你的服务在注入时自动获得横切关注点能力。

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
services.AutoInjectAllCustomerServices();
services.BuildProvider();

// 3. 任何地方获取服务
var userService = ServiceProviderUtil.GetRequiredService<IUserService>();
Console.WriteLine(userService.GetUserName(1));  // User_1
```

> 没有废话，只有生产力。

---

## 技术栈

- **目标框架**：.NET 8.0
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
| `ITransient` | `System` | 瞬态：每次请求创建新实例 |
| `IScoped` | `System` | 作用域：每个请求范围内共享 |
| `ISingleton` | `System` | 单例：全局唯一实例 |

> 你的服务类只需实现对应的生命周期接口，框架会自动识别并注册。

### 全局 DI 容器 — `ServiceProviderUtil`

> 命名空间：`System`

| 方法 | 说明 |
|---|---|
| `Services` | 获取/设置 `IServiceCollection` |
| `InitServiceProvider(IServiceCollection)` | 初始化服务集合 |
| `BuildProvider()` / `BuildProvider(IServiceCollection)` | 构建 `IServiceProvider` |
| `GetService<T>()` | 获取服务（可能为 null） |
| `GetRequiredService<T>()` | 获取服务（不存在则抛异常） |
| `GetRequiredServiceForOnce<T>(Action<IServiceCollection>)` | 一次性获取（适用于测试） |
| `AutoInjectAllCustomerServices(IServiceCollection)` | 自动扫描注册所有标记了 DI 接口的类型 |

### 服务扫描 — `ServiceDescriptorUtil`

> 命名空间：`LuBan.DI`

| 方法 | 说明 |
|---|---|
| `AutoInjectAllCustomerServices(IServiceCollection)` | 扫描所有程序集，自动注册实现 `IDependency` 的类 |
| `TryGetServiceLifetime(Type)` | 根据标记接口解析 `ServiceLifetime` |
| `Register(IServiceCollection, Type, Type, InjectionAttribute, Type?)` | 手动注册单个类型 |
| `AddDispatchProxy(...)` | 注册 AOP 代理 |

### 注入特性 — `InjectionAttribute`

> 命名空间：`LuBan.DI.Attrs`

| 属性 | 说明 |
|---|---|
| `Action` | 注册方式：`Add`（覆盖）或 `TryAdd`（跳过已存在） |
| `Pattern` | 注册范围：`Self`、`FirstInterface`、`SelfWithFirstInterface`、`ImplementedInterfaces`、`All` |
| `Named` | 注册别名（多服务场景） |
| `Order` | 注册排序（值越大越后注册） |
| `Proxy` | AOP 代理类型（必须继承 `AspectDispatchProxy`） |
| `ExceptInterfaces` | 排除的接口列表 |

### AOP 代理

| 类/接口 | 命名空间 | 说明 |
|---|---|---|
| `AspectDispatchProxy` | `LuBan.DI.Core` | 抽象代理基类，实现 `Invoke`/`InvokeAsync`/`InvokeAsyncT<T>` |
| `IDispatchProxy` | `LuBan.Common.DI` | 代理拦截依赖接口（`Target` + `Services`） |
| `IGlobalDispatchProxy` | `LuBan.Common.DI` | 全局代理拦截接口 |
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

// 构建 DI 容器
builder.Services.BuildProvider();

var app = builder.Build();
app.Run();
```

### 手动获取服务

```csharp
using System;

// 在任意位置获取已注册的服务（无需构造函数注入）
var emailService = ServiceProviderUtil.GetRequiredService<EmailService>();
emailService.Send("user@example.com");

var configService = ServiceProviderUtil.GetService<ConfigService>();
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
using LuBan.DI.Core;
using LuBan.DI.Attrs;
using LuBan.Common.DI;

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

// 2. 在服务上指定代理
[Injection(Proxy = typeof(LogDispatchProxy))]
public class UserService : IUserService, IScoped
{
    public string GetUserName(int id) => $"User_{id}";
}

// 3. 调用时自动走代理
var svc = ServiceProviderUtil.GetRequiredService<IUserService>();
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

1. **命名空间别搞错**：`ITransient`/`IScoped`/`ISingleton` 在 `System` 命名空间；`IDependency` 在 `LuBan.DI.Interfaces`；`InjectionAttribute` 在 `LuBan.DI.Attrs`；`AspectDispatchProxy` 在 `LuBan.DI.Core`。
2. **扫描顺序**：`AutoInjectAllCustomerServices` 会扫描 `RuntimeUtil.GetTypes()` 返回的所有类型，确保你的程序集已被加载。
3. **`BuildProvider` 放最后**：先注册所有服务，最后调用 `BuildProvider()` 构建容器。
4. **代理类必须继承 `AspectDispatchProxy`**：同时实现 `IDispatchProxy` 接口以获取 `Target` 和 `Services`。
5. **`[SuppressProxy]` 豁免代理**：不想被代理的类加上这个特性即可。
6. **`Order` 控制注册顺序**：值越大越后注册，适用于需要覆盖默认实现的场景。

---

## 许可证

MIT License

---

**LuBan.DI** —— 让 .NET 依赖注入更自动、更灵活、更优雅。

> 如果你也受够了手动注册服务的痛苦，那就把它加进你的工具箱吧。
