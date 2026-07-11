[中文](README.md) | English

# LuBan.DI

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **"Marker interfaces + auto-scanning — say goodbye to manual DI registration."**

Are you still struggling with these DI issues?

- Having to write `services.AddScoped<IXxx, Xxx>()` for every service?
- Forgetting to register a new service and only finding out at runtime?
- Wanting AOP interception but .NET native DI doesn't support proxies?
- Confused between transient, singleton, and scoped lifetimes with inconsistent registration patterns?

If so, **LuBan.DI** is built for you.

It's a lightweight dependency injection extension library for **.NET 8** that achieves automatic service registration through marker interfaces + attribute annotations + assembly scanning. It also supports AOP proxy interception, giving your services cross-cutting concern capabilities automatically upon injection.

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.Service](../LuBan.Service/README.md) | [LuBan.AIAgent](../LuBan.AIAgent/README.md) | [LuBan.ApprovalFlow](../LuBan.ApprovalFlow/README.md)

---

## Why Choose LuBan.DI?

| Pain Point | LuBan.DI's Solution |
|---|---|
| Manual service registration is tedious | Implement `IScoped`/`ISingleton`/`ITransient` for auto-registration |
| Forgetting to register new services | `AutoInjectAllCustomerServices` scans all assemblies |
| Need AOP but .NET DI doesn't support it | `AspectDispatchProxy` runtime proxy, non-invasive interception |
| Registration order is uncontrollable | `[Injection(Order = 1)]` controls registration priority |
| Want to skip proxy for a class | `[SuppressProxy]` one-line exemption |

---

## One-Minute Preview

```csharp
using System;
using LuBan.DI.Attrs;
using LuBan.DI.Models;

// 1. Define service, implement IScoped for Scoped lifetime
public interface IUserService { string GetUserName(int id); }

public class UserService : IUserService, IScoped
{
    public string GetUserName(int id) => $"User_{id}";
}

// 2. One-line scan registration at startup
services.AutoInjectAllCustomerServices();
services.BuildProvider();

// 3. Get service anywhere
var userService = ServiceProviderUtil.GetRequiredService<IUserService>();
Console.WriteLine(userService.GetUserName(1));  // User_1
```

> No fluff, just productivity.

---

## Tech Stack

- **Target Framework**: .NET 8.0
- **Project Type**: Class Library
- **NuGet Package**: `LuBan.DI`

---

## Installation

```bash
dotnet add package LuBan.DI
```

---

## Toolbox Overview

### Marker Interfaces — Lifetime Declaration

| Interface | Namespace | Description |
|---|---|---|
| `IDependency` | `LuBan.DI.Interfaces` | Dependency marker base interface (not for direct external inheritance) |
| `ITransient` | `System` | Transient: new instance created per request |
| `IScoped` | `System` | Scoped: shared within each request scope |
| `ISingleton` | `System` | Singleton: globally unique instance |

> Your service class only needs to implement the corresponding lifetime interface, and the framework will automatically detect and register it.

### Global DI Container — `ServiceProviderUtil`

> Namespace: `System`

| Method | Description |
|---|---|
| `Services` | Get/set `IServiceCollection` |
| `InitServiceProvider(IServiceCollection)` | Initialize service collection |
| `BuildProvider()` / `BuildProvider(IServiceCollection)` | Build `IServiceProvider` |
| `GetService<T>()` | Get service (may be null) |
| `GetRequiredService<T>()` | Get service (throws if not found) |
| `GetRequiredServiceForOnce<T>(Action<IServiceCollection>)` | One-time get (suitable for testing) |
| `AutoInjectAllCustomerServices(IServiceCollection)` | Auto-scan and register all types marked with DI interfaces |

### Service Scanning — `ServiceDescriptorUtil`

> Namespace: `LuBan.DI`

| Method | Description |
|---|---|
| `AutoInjectAllCustomerServices(IServiceCollection)` | Scan all assemblies, auto-register classes implementing `IDependency` |
| `TryGetServiceLifetime(Type)` | Resolve `ServiceLifetime` from marker interface |
| `Register(IServiceCollection, Type, Type, InjectionAttribute, Type?)` | Manually register a single type |
| `AddDispatchProxy(...)` | Register AOP proxy |

### Injection Attribute — `InjectionAttribute`

> Namespace: `LuBan.DI.Attrs`

| Property | Description |
|---|---|
| `Action` | Registration mode: `Add` (override) or `TryAdd` (skip if exists) |
| `Pattern` | Registration scope: `Self`, `FirstInterface`, `SelfWithFirstInterface`, `ImplementedInterfaces`, `All` |
| `Named` | Registration alias (multi-service scenarios) |
| `Order` | Registration ordering (higher value = later registration) |
| `Proxy` | AOP proxy type (must inherit `AspectDispatchProxy`) |
| `ExceptInterfaces` | List of excluded interfaces |

### AOP Proxy

| Class/Interface | Namespace | Description |
|---|---|---|
| `AspectDispatchProxy` | `LuBan.DI.Core` | Abstract proxy base class, implements `Invoke`/`InvokeAsync`/`InvokeAsyncT<T>` |
| `IDispatchProxy` | `LuBan.Common.DI` | Proxy interception dependency interface (`Target` + `Services`) |
| `IGlobalDispatchProxy` | `LuBan.Common.DI` | Global proxy interception interface |
| `SuppressProxyAttribute` | `LuBan.DI.Attrs` | Mark on class to skip global proxy |

---

## Practical Examples

### Basic Auto-Registration

```csharp
using System;
using LuBan.DI.Attrs;

// Transient service
public class EmailService : ITransient
{
    public void Send(string to) => Console.WriteLine($"Sent to {to}");
}

// Singleton service
public class ConfigService : ISingleton
{
    public string GetConfig(string key) => $"value_of_{key}";
}

// Scoped service with interface
public interface IOrderService { void CreateOrder(int userId); }

[Injection]
public class OrderService : IOrderService, IScoped
{
    public void CreateOrder(int userId) => Console.WriteLine($"Order created for {userId}");
}
```

### Scan and Register at Startup

```csharp
using System;

// In ASP.NET Core's Program.cs or Startup.cs
var builder = WebApplication.CreateBuilder(args);

// One line scans all assemblies for classes implementing IScoped/ISingleton/ITransient
builder.Services.AutoInjectAllCustomerServices();

// Build DI container
builder.Services.BuildProvider();

var app = builder.Build();
app.Run();
```

### Manual Service Resolution

```csharp
using System;

// Get registered services anywhere (without constructor injection)
var emailService = ServiceProviderUtil.GetRequiredService<EmailService>();
emailService.Send("user@example.com");

var configService = ServiceProviderUtil.GetService<ConfigService>();
var value = configService?.GetConfig("AppName");
```

### Control Registration Behavior

```csharp
using LuBan.DI.Attrs;
using LuBan.DI.Models;

// TryAdd: skip if already registered
[Injection(EnumInjectionActions.TryAdd)]
public class DefaultLogger : ITransient
{
    public void Log(string msg) => Console.WriteLine(msg);
}

// Specify registration order
[Injection(Order = 10)]
public class PriorityService : IScoped
{
    // ...
}

// Register only self, not interfaces
[Injection(Pattern = EnumInjectionPatterns.Self)]
public class InternalWorker : ITransient
{
    // ...
}
```

### AOP Proxy Interception

```csharp
using LuBan.DI.Core;
using LuBan.DI.Attrs;
using LuBan.Common.DI;

// 1. Define proxy class
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

// 2. Specify proxy on service
[Injection(Proxy = typeof(LogDispatchProxy))]
public class UserService : IUserService, IScoped
{
    public string GetUserName(int id) => $"User_{id}";
}

// 3. Calls automatically go through proxy
var svc = ServiceProviderUtil.GetRequiredService<IUserService>();
svc.GetUserName(1);
// Output:
// [Before] GetUserName
// [After] GetUserName
```

### Skip Proxy

```csharp
using LuBan.DI.Attrs;

// Even with global proxy configured, this class won't be proxied
[SuppressProxy]
public class HealthCheckService : ITransient
{
    public bool IsHealthy() => true;
}
```

---

## Usage Tips

1. **Don't mix up namespaces**: `ITransient`/`IScoped`/`ISingleton` are in the `System` namespace; `IDependency` is in `LuBan.DI.Interfaces`; `InjectionAttribute` is in `LuBan.DI.Attrs`; `AspectDispatchProxy` is in `LuBan.DI.Core`.
2. **Scan order**: `AutoInjectAllCustomerServices` scans all types returned by `RuntimeUtil.GetTypes()`, ensure your assemblies are already loaded.
3. **`BuildProvider` goes last**: Register all services first, then call `BuildProvider()` to build the container.
4. **Proxy classes must inherit `AspectDispatchProxy`**: Also implement `IDispatchProxy` interface to get `Target` and `Services`.
5. **`[SuppressProxy]` exempts from proxying**: Add this attribute to classes you don't want proxied.
6. **`Order` controls registration sequence**: Higher values register later, useful for overriding default implementations.

---

## License

MIT License

---

**LuBan.DI** — Making .NET dependency injection more automatic, flexible, and elegant.

> If you're tired of manually registering services, add it to your toolbox.
