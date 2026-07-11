[中文](README.md) | English

# LuBan.Qingflow

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **All-in-one Qingflow Open API client — app data, approval flows, user management, reports & charts, all in a single class.**

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)

---

## Why Choose LuBan.Qingflow?

Qingflow is a low-code platform that can be deeply integrated with external systems via Open API. However, the official API docs are scattered, token management is cumbersome, and paginated queries require manual looping...  
LuBan.Qingflow wraps the complete Qingflow API client, automatically manages AccessToken caching and refresh, supports generic data mapping, letting you operate Qingflow data directly with C# objects.

---

## Quick Preview

```csharp
// Get Qingflow client (singleton, auto-reads config)
var client = OpenApiClient.Instance;

// Get app data list
var result = await client.GetAppDataListAsync(new GetAppDataListInput
{
    AppId = "your_app_id",
    PageIndex = 1,
    PageSize = 50
});

// Get strongly-typed data
var orders = await client.GetPagedModelListAsync<OrderModel>(new GetAppDataListInput
{
    AppId = "your_app_id",
    PageIndex = 1,
    PageSize = 50
});
```

---

## Tech Stack & Dependencies

| Category | Component | Description |
|----------|-----------|-------------|
| Runtime | .NET 8.0 | — |
| Framework Dependencies | LuBan.Common | LuBan base components (HTTP, caching, serialization, etc.) |

---

## Installation

```bash
dotnet add package LuBan.Qingflow
```

---

## Configuration

Add to `appsettings.json` or configuration center:

```json
{
  "QingflowOptions": {
    "Domain": "https://api.qingflow.com",
    "WorkspaceId": "your_workspace_id",
    "WorkspaceSecret": "your_workspace_secret",
    "SuperAdminToken": "your_super_admin_token"
  }
}
```

| Field | Description |
|-------|-------------|
| `Domain` | Qingflow API domain (public cloud/private cloud/DingTalk, etc.) |
| `WorkspaceId` | Workspace ID |
| `WorkspaceSecret` | Workspace secret (AccessToken/Secret) |
| `SuperAdminToken` | Super admin token (for app list retrieval and other admin operations) |

---

## Feature Overview

### Core Classes

| Class | Description |
|-------|-------------|
| `OpenApiClient` | Qingflow API client (singleton, ~825 lines), wraps all API calls |
| `QingflowOptions` | Configuration class: domain, workspace, secret, app list |
| `QingflowAppOptions` | Qingflow app configuration (AppId + AppName) |

### API Capability Matrix

| Domain | Method | Description |
|--------|--------|-------------|
| **App Management** | `GetAppListAsync()` | Get app list (requires super admin token) |
| **App Data** | `GetAppDataListAsync(input)` | Get app data list (supports pagination, filtering, full pull) |
| | `GetAppDataByIdAsync(appDataId)` | Get single data record detail |
| | `GetTitleValueCollectionListAsync(input)` | Get data list (title-value collection format) |
| | `GetTitleValueCollectionByIdAsync(appDataId)` | Get single data record (title-value collection format) |
| | `GetModelByIdAsync<T>(appDataId)` | Get single data record (strongly-typed generic) |
| | `GetPagedModelListAsync<T>(input)` | Get paginated data list (strongly-typed generic) |
| | `DeleteAppDataAsync(input)` | Delete app data |
| **Date Range** | `GetLastestAppDataAsync(input)` | Get data by date range |
| | `GetLastestModelAsync<T>(input)` | Get data by date range (strongly-typed) |
| **Table Data** | `GetModelListForTabletAsync<T>(input)` | Get table sub-table data |
| | `GetModelListWithTableDataAsync<T1, T2>(input)` | Get main table + sub-table associated data |
| **Approval Flow** | `GetAuditFlowListAsync(dataId)` | Get data approval flow list |
| | `GetLatestAuditFlowAsync(dataId, nodeName)` | Get latest approval status |
| **User Management** | `GetUserAsync(userId)` | Get user information |
| **Reports & Charts** | `GetChartDataAsync(input)` | Get report chart data |
| | `GetChartDataByDatetimeAsync(input)` | Get chart data by date range |
| **Patient Data** | `GetPatientRegistInfosAsync(input)` | Get patient registration info |
| | `GetAppDataByIdCardAsync(input)` | Query data by ID card |
| | `GetModelByIdCardAsync<T>(input)` | Query data by ID card (strongly-typed) |

---

## Code Examples

### Basic Usage

```csharp
// Initialize client (auto-reads from Nacos/appsettings)
var client = OpenApiClient.Instance;

// Or manually specify configuration
var client = new OpenApiClient(new QingflowOptions
{
    Domain = "https://api.qingflow.com",
    WorkspaceId = "ws_001",
    WorkspaceSecret = "secret_xxx",
    SuperAdminToken = "admin_token_xxx"
});
```

### Get App Data (Strongly-typed)

```csharp
// Define data model (implement IAppData interface)
public class OrderModel : IAppData
{
    public long AppDataId { get; set; }
    public string OrderNo { get; set; }
    public decimal Amount { get; set; }
    public string CustomerName { get; set; }
}

// Paginated query
var result = await client.GetPagedModelListAsync<OrderModel>(new GetAppDataListInput
{
    AppId = "app_001",
    PageIndex = 1,
    PageSize = 50,
    Queries = new List<AppDataQuery>
    {
        new AppDataQuery { QueId = 1, SearchKey = "keyword" }
    }
});

foreach (var order in result.Result.Result)
{
    Console.WriteLine($"{order.OrderNo}: {order.Amount}");
}
```

### Full Data Pull

```csharp
// IsAll = true auto-paginates to pull all data
var result = await client.GetAppDataListAsync(new GetAppDataListInput
{
    AppId = "app_001",
    PageIndex = 1,
    PageSize = 100,
    IsAll = true
});

Console.WriteLine($"Retrieved {result.Result.Result.Count} records in total");
```

### Query by Date Range

```csharp
var data = await client.GetLastestAppDataAsync(new GetLastestAppDataInput
{
    AppId = "app_001",
    FromDateTime = DateTime.Parse("2026-01-01"),
    ToDateTime = DateTime.Parse("2026-07-01"),
    PageIndex = 1,
    PageSize = 100,
    IsAll = true
});
```

### Query Approval Flow Status

```csharp
// Get approval flow for a data record
var flowInfo = await client.GetAuditFlowListAsync("data_id_xxx");

// Get latest approval node
var latest = await client.GetLatestAuditFlowAsync("data_id_xxx", nodeName: "Manager Approval");
if (latest != null)
{
    Console.WriteLine($"Current node: {latest.AuditNodeName}, Status: {latest.ApplyStatus}");
}
```

### Get Chart Data

```csharp
var chartData = await client.GetChartDataAsync(new GetChartDataListInput
{
    ChartKey = "chart_001",
    Filter = new ChartDataListInputFilter
    {
        PageNum = 1,
        PageSize = 50
    }
});
```

### Delete Data

```csharp
await client.DeleteAppDataAsync(new DeleteAppDataInput
{
    AppId = "app_001",
    AppDataId = 12345
});
```

---

## Tips

1. **Auto Token Management**: `OpenApiClient` automatically caches AccessToken and refreshes 20 minutes before expiration — no manual handling needed
2. **Token Expiry Retry**: When encountering error code `49300` (Token expired), automatically clears cache and retries
3. **Full Pull**: Set `IsAll = true` to auto-paginate until all data is retrieved
4. **Strongly-typed Mapping**: Implement `IAppData` interface to directly get C# objects via generic methods
5. **Singleton Pattern**: `OpenApiClient.Instance` is globally unique; constructor auto-loads app list
6. **Super Admin Permissions**: Admin APIs like `GetAppListAsync()` require `SuperAdminToken`; regular tokens cannot call them

---

## License

Copyright (c) yswenli. All Rights Reserved.
