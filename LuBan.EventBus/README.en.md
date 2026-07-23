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

LuBan.EventBus is built on `System.Threading.Channels`, providing a zero-dependency in-process event bus with support for generic events, scoped handler injection, event persistence, and more.

## Quick Preview

```csharp
// Register services
builder.Services.AddEventBus();
builder.Services.AddEventHandlers(typeof(Startup).Assembly);

// Define event (inheriting BaseEventData)
public class OrderCreatedEvent : BaseEventData
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
| Auto-Discovery | `AddEventHandlers()` auto-scans and registers all `IEventHandler<T>` implementations |
| Type Safety | Generic constraints ensure compile-time checking of event-handler matching |
| Event Persistence | Optional `EventPersistence` support for auditing and tracing needs |
| Independent Channels | Each event type has its own Channel; different types never block each other |

### Architecture Design

- **EventChannel\<TEvent\>** — Each event type has an independent `Channel.CreateBounded`, with background tasks continuously consuming and dispatching to handlers
- **Scoped Lifecycle** — Each event handling creates an independent scoped service instance via DI; handlers can safely inject scoped dependencies like DbContext
- **EventPersistence** — Optional event persistence support based on `LocalCacheUtil`, supporting event save and load

### Configuration Options

| Option | Default | Description |
|--------|---------|-------------|
| `MaxQueueCapacity` | 10000 | Maximum Channel capacity |
| `EnablePersistence` | true | Whether to enable event persistence |
| `FullMode` | 0 | Degradation strategy when Channel is full: 0=Wait (block), 1=DropNewest, 2=DropOldest |

### Service Registration

```csharp
// Register event bus (with default configuration)
services.AddEventBus();

// Register event bus (with custom configuration)
services.AddEventBus(options =>
{
    options.MaxQueueCapacity = 5000;
    options.EnablePersistence = false;
    options.FullMode = 1; // Drop newest event when Channel is full
});

// Auto-scan all IEventHandler<T> implementations in the assembly and register them
services.AddEventHandlers(typeof(Startup).Assembly);

// Scan multiple assemblies
services.AddEventHandlers(
    typeof(Startup).Assembly,
    typeof(OrderModule).Assembly
);

// Without parameters, auto-scans all loaded assemblies
services.AddEventHandlers();
```

## Usage Guide

### 1. Define Events

Event classes must implement the `IEventData` interface, or inherit from the `BaseEventData` base class:

```csharp
// Option 1: Implement interface
public class UserRegisteredEvent : IEventData
{
    public string Name { get; set; }
    public DateTime EventTime { get; set; }
    public object? EventSource { get; set; }
    
    public string UserId { get; set; }
    public string Email { get; set; }
}

// Option 2: Inherit base class (recommended)
public class UserRegisteredEvent : BaseEventData
{
    public string UserId { get; set; }
    public string Email { get; set; }
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

    // Optional: implement error handling
    public Task OnErrorAsync(Exception ex)
    {
        // Log error
        return Task.CompletedTask;
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
            Email = email
        });
    }
}
```

### 4. Event Persistence

When persistence is enabled, events are automatically saved to `LocalCacheUtil` (supports file persistence):

```csharp
// Publish event (auto-persisted)
await eventBus.PublishAsync(new OrderCreatedEvent { ... });

// Load saved events
var persistence = serviceProvider.GetRequiredService<EventPersistence>();
var savedEvents = await persistence.LoadAsync<OrderCreatedEvent>();
```

## Tips

- Each event type has its own independent Channel; different event types never block each other
- Handlers are managed with scoped lifecycle, allowing safe injection of database contexts and other scoped services
- Use `AddEventHandlers()` for auto-scanning registration to avoid manual one-by-one configuration
- Event publishing is asynchronous and does not block the main business flow
- Combine with `EventPersistence` to implement event sourcing and audit logging
- `BaseEventData` provides base properties like `Name`, `EventTime`, and `EventSource`

## License

MIT
