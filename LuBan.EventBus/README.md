[English](README.en.md) | 中文

# LuBan.EventBus

> **作者**: yswenli | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> 轻量级进程内事件总线，让模块间通信像呼吸一样自然。

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.ApprovalFlow](../LuBan.ApprovalFlow/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## 为什么需要它？

- 模块之间直接引用导致耦合度爆炸？
- 手动管理事件订阅/取消订阅容易遗漏？
- 需要异步事件驱动但又不想引入 RabbitMQ / Kafka 这样的重量级中间件？

LuBan.EventBus 基于 `System.Threading.Channels` 构建，提供零外部依赖的进程内事件总线，支持泛型事件、Scoped 处理器注入、一次性订阅、事件持久化等特性。

## 快速预览

```csharp
// 注册服务
builder.Services.AddEventBus();
builder.Services.AddEventHandlers(typeof(Startup).Assembly);

// 定义事件
public class OrderCreatedEvent : IEventData
{
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
}

// 定义处理器
public class OrderCreatedHandler : IEventHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent eventData)
    {
        // 处理订单创建逻辑
        await NotifyUserAsync(eventData.OrderId);
    }
}

// 发布事件
await eventBus.PublishAsync(new OrderCreatedEvent
{
    OrderId = "ORD-20260711-001",
    Amount = 299.00m
});
```

## 技术栈

| 组件 | 说明 |
|------|------|
| `LuBan.Common` | 核心接口定义（IEventBus、IEventData、IEventHandler\<T\>） |

## 安装

```xml
<PackageReference Include="LuBan.EventBus" Version="*" />
```

## 功能总览

### 核心能力

| 功能 | 说明 |
|------|------|
| 异步发布 | `PublishAsync<TEvent>()` 基于 Channel 异步分发，不阻塞调用线程 |
| 类型安全订阅 | `Subscribe<TEvent, THandler>()` 泛型约束，编译期检查 |
| 一次性订阅 | `SubscribeOnce<TEvent, THandler>()` 触发后自动取消 |
| 取消订阅 | `Unsubscribe<TEvent, THandler>()` 精确移除处理器 |
| 订阅统计 | `GetSubscriberCount<TEvent>()` 获取当前订阅数量 |

### 架构设计

- **EventChannel\<TEvent\>** — 每个事件类型独立 `Channel.CreateBounded`，后台任务持续消费并分发到处理器
- **Scoped 生命周期** — 每次事件处理通过 DI 创建独立的 Scoped 服务实例，处理器可安全注入 DbContext 等 Scoped 依赖
- **EventPersistence** — 可选事件持久化支持，满足审计与回溯需求

### 服务注册

```csharp
// 注册事件总线
services.AddEventBus();

// 自动扫描程序集中的所有 IEventHandler<T> 实现并注册
services.AddEventHandlers(typeof(Startup).Assembly);

// 扫描多个程序集
services.AddEventHandlers(
    typeof(Startup).Assembly,
    typeof(OrderModule).Assembly
);
```

## 使用指南

### 1. 定义事件

事件类需实现 `IEventData` 接口：

```csharp
public class UserRegisteredEvent : IEventData
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public DateTime RegisteredAt { get; set; }
}
```

### 2. 实现处理器

处理器实现 `IEventHandler<TEvent>` 接口，通过 DI 注入依赖：

```csharp
public class SendWelcomeEmailHandler : IEventHandler<UserRegisteredEvent>
{
    private readonly IEmailService _emailService;

    public SendWelcomeEmailHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task HandleAsync(UserRegisteredEvent eventData)
    {
        await _emailService.SendWelcomeAsync(eventData.Email);
    }
}
```

### 3. 发布事件

```csharp
public class UserService
{
    private readonly IEventBus _eventBus;

    public UserService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task RegisterAsync(string email)
    {
        var user = await CreateUserAsync(email);

        await _eventBus.PublishAsync(new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = email,
            RegisteredAt = DateTime.UtcNow
        });
    }
}
```

### 4. 动态订阅与取消

```csharp
// 动态订阅
eventBus.Subscribe<UserRegisteredEvent, LogHandler>();

// 一次性订阅（触发后自动移除）
eventBus.SubscribeOnce<UserRegisteredEvent, OnboardingHandler>();

// 查看订阅数
var count = eventBus.GetSubscriberCount<UserRegisteredEvent>();

// 取消订阅
eventBus.Unsubscribe<UserRegisteredEvent, LogHandler>();
```

## 小贴士

- 每个事件类型拥有独立的 Channel，不同类型的事件互不阻塞
- 处理器通过 Scoped 生命周期管理，可安全注入数据库上下文等 Scoped 服务
- 使用 `AddEventHandlers()` 自动扫描注册，避免手动逐个配置
- 事件发布是异步的，不会阻塞主业务流程
- 结合 `EventPersistence` 可实现事件溯源与审计日志

## 许可证

MIT
