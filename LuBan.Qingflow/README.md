[English](README.en.md) | 中文

# LuBan.Qingflow

> **作者**: yswenli | **联系邮箱**: yswenli@outlook.com | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **轻流 Open API 一站式客户端——应用数据、审批流程、用户管理、报表图表，一个类全部搞定。**

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)

---

## 为什么选择 LuBan.Qingflow？

轻流（Qingflow）是低代码平台，通过 Open API 可以与外部系统深度集成。但官方 API 文档分散、Token 管理繁琐、分页查询需要手动循环……  
LuBan.Qingflow 封装了完整的轻流 API 客户端，自动管理 AccessToken 缓存与刷新，支持泛型数据映射，让你用 C# 对象直接操作轻流数据。

---

## 快速预览

```csharp
// 获取轻流客户端（单例，自动从配置读取参数）
var client = OpenApiClient.Instance;

// 获取应用数据列表
var result = await client.GetAppDataListAsync(new GetAppDataListInput
{
    AppId = "your_app_id",
    PageIndex = 1,
    PageSize = 50
});

// 获取强类型数据
var orders = await client.GetPagedModelListAsync<OrderModel>(new GetAppDataListInput
{
    AppId = "your_app_id",
    PageIndex = 1,
    PageSize = 50
});
```

---

## 技术栈 & 依赖

| 类别 | 组件 | 说明 |
|------|------|------|
| 运行时 | .NET 8.0 | — |
| 框架依赖 | LuBan.Common | LuBan 基础组件（HTTP、缓存、序列化等） |

---

## 安装

```bash
dotnet add package LuBan.Qingflow
```

---

## 配置

在 `appsettings.json` 或 配置中心添加：

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

| 字段 | 说明 |
|------|------|
| `Domain` | 轻流 API 域名（公有云/私有云/钉钉等） |
| `WorkspaceId` | 工作区 ID |
| `WorkspaceSecret` | 工作区密钥（AccessToken/Secret） |
| `SuperAdminToken` | 超级管理员 Token（用于获取应用列表等管理操作） |

---

## 功能全景

### 核心类

| 类 | 说明 |
|----|------|
| `OpenApiClient` | 轻流 API 客户端（单例，~825 行），封装全部 API 调用 |
| `QingflowOptions` | 配置类：域名、工作区、密钥、应用列表 |
| `QingflowAppOptions` | 轻流应用配置（AppId + AppName） |

### API 能力矩阵

| 功能域 | 方法 | 说明 |
|--------|------|------|
| **应用管理** | `GetAppListAsync()` | 获取应用列表（需超管 Token） |
| **应用数据** | `GetAppDataListAsync(input)` | 获取应用数据列表（支持分页、过滤、全量拉取） |
| | `GetAppDataByIdAsync(appDataId)` | 获取单条数据详情 |
| | `GetTitleValueCollectionListAsync(input)` | 获取数据列表（标题-值集合格式） |
| | `GetTitleValueCollectionByIdAsync(appDataId)` | 获取单条数据（标题-值集合格式） |
| | `GetModelByIdAsync<T>(appDataId)` | 获取单条数据（强类型泛型） |
| | `GetPagedModelListAsync<T>(input)` | 获取分页数据列表（强类型泛型） |
| | `DeleteAppDataAsync(input)` | 删除应用数据 |
| **日期范围** | `GetLastestAppDataAsync(input)` | 按日期范围获取数据 |
| | `GetLastestModelAsync<T>(input)` | 按日期范围获取数据（强类型） |
| **表格数据** | `GetModelListForTabletAsync<T>(input)` | 获取表格子表数据 |
| | `GetModelListWithTableDataAsync<T1, T2>(input)` | 获取主表 + 子表关联数据 |
| **审批流程** | `GetAuditFlowListAsync(dataId)` | 获取数据审批流程列表 |
| | `GetLatestAuditFlowAsync(dataId, nodeName)` | 获取最新审批状态 |
| **用户管理** | `GetUserAsync(userId)` | 获取用户信息 |
| **报表图表** | `GetChartDataAsync(input)` | 获取报表图表数据 |
| | `GetChartDataByDatetimeAsync(input)` | 按日期范围获取图表数据 |
| **患者数据** | `GetPatientRegistInfosAsync(input)` | 获取患者注册信息 |
| | `GetAppDataByIdCardAsync(input)` | 按身份证查询数据 |
| | `GetModelByIdCardAsync<T>(input)` | 按身份证查询数据（强类型） |

---

## 代码示例

### 基本使用

```csharp
// 初始化客户端（自动从 Nacos/appsettings 读取配置）
var client = OpenApiClient.Instance;

// 或手动指定配置
var client = new OpenApiClient(new QingflowOptions
{
    Domain = "https://api.qingflow.com",
    WorkspaceId = "ws_001",
    WorkspaceSecret = "secret_xxx",
    SuperAdminToken = "admin_token_xxx"
});
```

### 获取应用数据（强类型）

```csharp
// 定义数据模型（实现 IAppData 接口）
public class OrderModel : IAppData
{
    public long AppDataId { get; set; }
    public string OrderNo { get; set; }
    public decimal Amount { get; set; }
    public string CustomerName { get; set; }
}

// 分页查询
var result = await client.GetPagedModelListAsync<OrderModel>(new GetAppDataListInput
{
    AppId = "app_001",
    PageIndex = 1,
    PageSize = 50,
    Queries = new List<AppDataQuery>
    {
        new AppDataQuery { QueId = 1, SearchKey = "关键词" }
    }
});

foreach (var order in result.Result.Result)
{
    Console.WriteLine($"{order.OrderNo}: {order.Amount}");
}
```

### 全量拉取数据

```csharp
// IsAll = true 自动翻页，拉取全部数据
var result = await client.GetAppDataListAsync(new GetAppDataListInput
{
    AppId = "app_001",
    PageIndex = 1,
    PageSize = 100,
    IsAll = true
});

Console.WriteLine($"共获取 {result.Result.Result.Count} 条数据");
```

### 按日期范围查询

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

### 查询审批流程状态

```csharp
// 获取某条数据的审批流程
var flowInfo = await client.GetAuditFlowListAsync("data_id_xxx");

// 获取最新审批节点
var latest = await client.GetLatestAuditFlowAsync("data_id_xxx", nodeName: "经理审批");
if (latest != null)
{
    Console.WriteLine($"当前节点：{latest.AuditNodeName}，状态：{latest.ApplyStatus}");
}
```

### 获取图表数据

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

### 删除数据

```csharp
await client.DeleteAppDataAsync(new DeleteAppDataInput
{
    AppId = "app_001",
    AppDataId = 12345
});
```

---

## 小贴士

1. **Token 自动管理**：`OpenApiClient` 自动缓存 AccessToken，过期前 20 分钟自动刷新，无需手动处理
2. **Token 过期重试**：当遇到错误码 `49300`（Token 失效）时，自动清除缓存并重试
3. **全量拉取**：设置 `IsAll = true` 可自动翻页，直到拉取全部数据
4. **强类型映射**：实现 `IAppData` 接口后，可直接用泛型方法获取 C# 对象
5. **单例模式**：`OpenApiClient.Instance` 全局唯一，构造函数中自动加载应用列表
6. **超管权限**：`GetAppListAsync()` 等管理接口需要 `SuperAdminToken`，普通 Token 无法调用

---

## 许可证

Copyright (c) yswenli. All Rights Reserved.
