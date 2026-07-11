[中文](README.md) | English

# LuBan.EventBus

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> A lightweight in-process event bus that makes inter-module communication as natural as breathing.

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.ApprovalFlow](../LuBan.ApprovalFlow/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## Why Do You Need It?

- Direct references between modules causing coupling explosion?
- Manual event subscription/unsubscription prone to omissions?
- Need async event-driven architecture without heavy middleware like RabbitMQ / Kafka?

LuBan.EventBus is built on `System.Threading.Channels`, providing a zero-dependency in-process event bus with support for generic events, scoped handler injection, one-time subscriptions, event persistence, and more.

## Quick Preview

```csharp
// Register services
builder.Services.AddEventBus();
builder.Services.AddEventHandlers(typeof(Startup).Assembly);

// Define event
public class OrderCreatedEvent : IEventData
{
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
}

// Define handler
public class OrderCreatedHandler : IEventHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent eventData)
    {
        // Handle order creation logic
        await NotifyUserAsync(eventData.OrderId);
    }
}

// Publish event
await eventBus.PublishAsync(new OrderCreatedEvent
{
    OrderId = "ORD-20260711-001",
    Amount = 299.00m
});
```

## Tech Stack

| Component | Description |
|-----------|-------------|
| `LuBan.Common` | Core interface definitions (IEventBus, IEventData, IEventHandler\<T\>) |

## Installation

```xml
<PackageReference Include="LuBan.EventBus" Version="*" />
```

## Feature Overview

### Core Capabilities

| Feature | Description |
|---------|-------------|
| Async Publish | `PublishAsync<TEvent>()` dispatches via Channel asynchronously without blocking the calling thread |
| Type-Safe Subscription | `Subscribe<TEvent, THandler>()` with generic constraints, compile-time checked |
| One-Time Subscription | `SubscribeOnce<TEvent, THandler>()` automatically unsubscribes after trigger |
| Unsubscribe | `Unsubscribe<TEvent, THandler>()` to precisely remove handlers |
| Subscriber Stats | `GetSubscriberCount<TEvent>()` to get current subscription count |

### Architecture Design

- **EventChannel\<TEvent\>** — Each event type has an independent `Channel.CreateBounded`, with background tasks continuously consuming and dispatching to handlers
- **Scoped Lifecycle** — Each event handling creates an independent scoped service instance via DI; handlers can safely inject scoped dependencies like DbContext
- **EventPersistence** — Optional event persistence support for auditing and tracing needs

### Service Registration

```csharp
// Register event bus
services.AddEventBus();

// Auto-scan all IEventHandler<T> implementations in the assembly and register them
services.AddEventHandlers(typeof(Startup).Assembly);

// Scan multiple assemblies
services.AddEventHandlers(
    typeof(Startup).Assembly,
    typeof(OrderModule).Assembly
);
```

## Usage Guide

### 1. Define Events

Event classes must implement the `IEventData` interface:

```csharp
public class UserRegisteredEvent : IEventData
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public DateTime RegisteredAt { get; set; }
}
```

### 2. Implement Handlers

Handlers implement the `IEventHandler<TEvent>` interface with DI-injected dependencies:

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

### 3. Publish Events

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

### 4. Dynamic Subscription & Unsubscription

```csharp
// Dynamic subscription
eventBus.Subscribe<UserRegisteredEvent, LogHandler>();

// One-time subscription (automatically removed after trigger)
eventBus.SubscribeOnce<UserRegisteredEvent, OnboardingHandler>();

// Check subscriber count
var count = eventBus.GetSubscriberCount<UserRegisteredEvent>();

// Unsubscribe
eventBus.Unsubscribe<UserRegisteredEvent, LogHandler>();
```

## Tips

- Each event type has its own independent Channel; different event types never block each other
- Handlers are managed with scoped lifecycle, allowing safe injection of database contexts and other scoped services
- Use `AddEventHandlers()` for auto-scanning registration to avoid manual one-by-one configuration
- Event publishing is asynchronous and does not block the main business flow
- Combine with `EventPersistence` to implement event sourcing and audit logging

## License

MIT
