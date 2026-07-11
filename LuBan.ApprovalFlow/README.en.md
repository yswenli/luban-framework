[中文](README.md) | English

# LuBan.ApprovalFlow

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **Define workflows in JSON, drive approvals with code — a lightweight approval flow engine supporting countersign, gateway routing, HTTP callbacks, and auto-advancement.**

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.DI](../LuBan.DI/README.md) | [LuBan.EventBus](../LuBan.EventBus/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)

---

## Why Choose LuBan.ApprovalFlow?

Approval flow engines on the market are either too heavy (Activiti/Flowable require separate deployment) or too closed (SaaS platforms cannot be customized).  
LuBan.ApprovalFlow is an **embedded** approval flow engine: define workflow structure in JSON, extend node behavior with C#, no additional infrastructure needed — it runs directly in your ASP.NET Core process.

---

## Quick Preview

```csharp
// 1. Register services
services.AddApprovalFlow();

// 2. Start a flow
var engine = ServiceProviderUtil.GetRequiredService<ApprovalFlowEngine>();
var response = await engine.StartFlowAsync(new StartFlowRequest
{
    FormPayload = new Dictionary<string, object> { ["amount"] = 50000 },
    Variables = new Dictionary<string, object> { ["deptId"] = 101 }
}, definition, repository);

// 3. Process approval
await engine.ProcessApprovalAsync(new ProcessApprovalRequest
{
    RecordId = response.RecordId,
    ApproverId = "user_001",
    Approved = true,
    Comment = "Approved"
});
```

---

## Tech Stack & Dependencies

| Category | Component | Description |
|----------|-----------|-------------|
| Runtime | .NET 8.0 | ASP.NET Core FrameworkReference |
| Framework Dependencies | LuBan.Common, LuBan.DI, LuBan.EventBus, LuBan.Orm | LuBan base components |
| Serialization | System.Text.Json | Workflow definition serialization |
| Thread Pool | SimpleThreadPool | Built-in lightweight thread pool |

---

## Installation

```bash
dotnet add package LuBan.ApprovalFlow
```

---

## Feature Overview

### Core Architecture

```
FlowEngine (Singleton, manages thread pool + scheduled checks)
  └── ApprovalFlowEngine (Core engine, ~700 lines)
        ├── FlowBuilder — Loads workflow definitions from JSON, caches executors
        ├── FlowExecutor — Executes workflow steps
        ├── RuleEngine — Conditional gateway routing
        ├── AggregationEvaluator — Countersign aggregation (AND/OR/ratio)
        ├── HttpCallbackExecutor — HTTP callback notifications
        └── Node Handlers
              ├── StartNodeHandler
              ├── EndNodeHandler
              ├── TaskNodeHandler
              ├── UserNodeHandler
              ├── GatewayNodeHandler
              └── HttpNodeHandler
```

### Workflow Engine

| Component | Description |
|-----------|-------------|
| `FlowEngine` | Singleton engine manager, maintains thread pool, periodically (1 min) checks and auto-advances workflows |
| `ApprovalFlowEngine` | Core engine: start workflows, process approvals, gateway routing, event triggering |
| `FlowBuilder` | Loads workflow definitions from JSON files/strings, caches in memory |
| `FlowExecutor` | Workflow executor, binds workflow definitions and manages runtime state |
| `FlowContextAccessor` | Workflow context accessor |

### Node Handlers

| Handler | Description |
|---------|-------------|
| `StartNodeHandler` | Start node: initializes workflow |
| `EndNodeHandler` | End node: completes/rejects workflow |
| `TaskNodeHandler` | Task node: creates approval tasks |
| `UserNodeHandler` | User node: designates approvers |
| `GatewayNodeHandler` | Gateway node: conditional branch routing |
| `HttpNodeHandler` | HTTP node: calls external APIs |

### Rules & Aggregation

| Component | Description |
|-----------|-------------|
| `RuleEngine` | Conditional gateway rule engine, supports variable expressions |
| `AggregationEvaluator` | Countersign aggregation evaluator, supports AND (all pass), OR (any pass), ratio pass |
| `PlaceholderResolver` | Placeholder resolution, supports template variable substitution |

### Persistence Entities

| Entity | Description |
|--------|-------------|
| `DbApprovalFlow` | Approval flow definition |
| `DbApprovalFlowRecord` | Workflow instance record |
| `DbApprovalTask` | Approval task |
| `DbApprovalStep` | Approval step |
| `DbApprovalNodeRecord` | Node execution record |
| `DbApprovalStatistics` | Workflow statistics |
| `DbApprovalDelegation` | Approval delegation (proxy approval) |

### Extension Interfaces

| Interface | Description |
|-----------|-------------|
| `IApprovalRepository` | Persistence interface, implement your own database storage |
| `IFlowEventListener` | Workflow event listener |
| `IFlowEventHandler` | Workflow event handler |
| `IUserService` | User service, provides approver information |
| `INotificationService` | Notification service, approval message push |
| `IStatisticsUpdater` | Statistics updater |

---

## DI Registration

```csharp
// Register approval flow core services
services.AddApprovalFlow(options);

// Or auto-load JSON workflow definitions from configuration
services.AddApprovalFlow(configuration);

// Register custom persistence
services.AddApprovalRepository<MyApprovalRepository>();

// Register event listener
services.AddApprovalFlowListener<MyFlowEventListener>();

// Register user service
services.AddApprovalUserService<MyUserService>();

// Register notification service
services.AddApprovalNotificationService<MyNotificationService>();

// Register statistics updater
services.AddStatisticsUpdater<MyStatisticsUpdater>();
```

---

## Configuration

Configure in `appsettings.json` :

```json
{
  "ApprovalFlowOptions": {
    "DataDir": "data",
    "AutoApproval": true,
    "ThreadPoolSize": 10
  }
}
```

| Field | Description | Default |
|-------|-------------|---------|
| `DataDir` | Workflow definition JSON file directory | `"data"` |
| `AutoApproval` | Enable auto-approval (FlowEngine scheduled advancement) | `false` |
| `ThreadPoolSize` | Engine thread pool size | `10` |

---

## Code Examples

### Define Workflow (JSON)

```json
{
  "Key": "leave_approval",
  "Name": "Leave Approval",
  "Nodes": [
    { "Id": "start", "Type": "Start", "Name": "Start" },
    { "Id": "manager", "Type": "User", "Name": "Manager Approval", "Assignee": "${managerId}" },
    { "Id": "gateway1", "Type": "Gateway", "Name": "Amount Check" },
    { "Id": "director", "Type": "User", "Name": "Director Approval", "Assignee": "${directorId}" },
    { "Id": "end", "Type": "End", "Name": "End" }
  ],
  "Edges": [
    { "From": "start", "To": "manager" },
    { "From": "manager", "To": "gateway1" },
    { "From": "gateway1", "To": "director", "Condition": "amount > 10000" },
    { "From": "gateway1", "To": "end", "Condition": "amount <= 10000" },
    { "From": "director", "To": "end" }
  ]
}
```

### Start Workflow

```csharp
var engine = ServiceProviderUtil.GetRequiredService<ApprovalFlowEngine>();
var repository = ServiceProviderUtil.GetRequiredService<IApprovalRepository>();

// Load workflow definition
var builder = ServiceProviderUtil.GetRequiredService<FlowBuilder>();
var executor = await builder.CreateExecutorFromJsonFileAsync("data/leave_approval.json");
var definition = executor.Definition;

// Start workflow
var response = await engine.StartFlowAsync(new StartFlowRequest
{
    FormPayload = new Dictionary<string, object>
    {
        ["applicant"] = "John",
        ["days"] = 3,
        ["reason"] = "Annual leave"
    },
    Variables = new Dictionary<string, object>
    {
        ["amount"] = 5000,
        ["managerId"] = "user_002"
    }
}, definition, repository);

Console.WriteLine($"Workflow started, RecordId: {response.RecordId}");
```

### Process Approval

```csharp
var result = await engine.ProcessApprovalAsync(new ProcessApprovalRequest
{
    RecordId = 1001,
    ApproverId = "user_002",
    Approved = true,
    Comment = "Leave approved"
}, repository);
```

---

## Tips

1. **Auto-advancement**: When `AutoApproval = true`, `FlowEngine` checks in-progress workflows every minute and auto-advances to the next step
2. **JSON Hot Reload**: `FlowBuilder` supports batch loading `*.json` from a directory, automatically updating cache when files change
3. **Countersign Mode**: `AggregationEvaluator` supports AND (all pass), OR (any pass), ratio (e.g., 2/3 pass)
4. **HTTP Callback**: `HttpNodeHandler` can call external APIs within workflows for automation integration
5. **Approval Delegation**: `DbApprovalDelegation` entity supports proxy approval functionality
6. **Event-Driven**: Implement `IFlowEventListener` to listen for workflow initialization, node enter/leave, completion events

---

## License

Copyright (c) yswenli. All Rights Reserved.
