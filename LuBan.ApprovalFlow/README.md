[English](README.en.md) | 中文

# LuBan.ApprovalFlow

> **作者**: yswenli | **联系邮箱**: yswenli@outlook.com | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **JSON 定义流程、代码驱动审批——一个轻量级审批流引擎，支持会签、网关路由和 HTTP 回调。**

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.DI](../LuBan.DI/README.md) | [LuBan.EventBus](../LuBan.EventBus/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)

---

## 为什么选择 LuBan.ApprovalFlow？

市面上的审批流引擎要么太重（Activiti/Flowable 需要独立部署），要么太封闭（SaaS 平台无法定制）。  
LuBan.ApprovalFlow 是一个**嵌入式**审批流引擎：用 JSON 定义流程结构，用 C# 扩展节点行为，无需额外基础设施，直接跑在你的 ASP.NET Core 进程里。

---

## 快速预览

```csharp
// 1. 注册服务
services.AddApprovalFlow();

// 2. 启动流程
var engine = DiContainer.Instance.GetRequiredService<ApprovalFlowEngine>();
var response = await engine.StartFlowAsync(new StartFlowRequest
{
    FormPayload = new Dictionary<string, object> { ["amount"] = 50000 },
    Variables = new Dictionary<string, object> { ["deptId"] = 101 }
}, definition, repository);

// 3. 处理审批
await engine.ProcessApprovalAsync(new ProcessApprovalRequest
{
    RecordId = response.RecordId,
    ApproverId = "user_001",
    Approved = true,
    Comment = "同意"
});
```

---

## 技术栈 & 依赖

| 类别 | 组件 | 说明 |
|------|------|------|
| 运行时 | .NET 10.0 | ASP.NET Core FrameworkReference |
| 框架依赖 | LuBan.Common, LuBan.DI, LuBan.EventBus, LuBan.Orm | LuBan 基础组件 |
| 序列化 | System.Text.Json | 流程定义序列化 |
| 线程池 | SimpleThreadPool | 内置轻量线程池 |

---

## 安装

```bash
dotnet add package LuBan.ApprovalFlow
```

---

## 功能全景

### 核心架构

```
FlowEngine (单例，管理线程池)
  └── ApprovalFlowEngine (核心引擎，~700行)
        ├── FlowBuilder — 从 JSON 加载流程定义，缓存执行器
        ├── FlowExecutor — 执行流程步骤
        ├── RuleEngine — 条件网关路由
        ├── AggregationEvaluator — 会签聚合（AND/OR/比例）
        ├── HttpCallbackExecutor — HTTP 回调通知
        └── Node Handlers
              ├── StartNodeHandler
              ├── EndNodeHandler
              ├── TaskNodeHandler
              ├── UserNodeHandler
              ├── GatewayNodeHandler
              └── HttpNodeHandler
```

### 流程引擎

| 组件 | 说明 |
|------|------|
| `FlowEngine` | 单例引擎管理器，维护线程池 |
| `ApprovalFlowEngine` | 核心引擎：启动流程、处理审批、网关路由、事件触发 |
| `FlowBuilder` | 从 JSON 文件/字符串加载流程定义，缓存到内存 |
| `FlowExecutor` | 流程执行器，绑定流程定义并管理运行时状态 |
| `FlowContextAccessor` | 流程上下文访问器 |

### 节点处理器

| 处理器 | 说明 |
|--------|------|
| `StartNodeHandler` | 开始节点：初始化流程 |
| `EndNodeHandler` | 结束节点：完成/拒绝流程 |
| `TaskNodeHandler` | 任务节点：创建审批任务 |
| `UserNodeHandler` | 用户节点：指定审批人 |
| `GatewayNodeHandler` | 网关节点：条件分支路由 |
| `HttpNodeHandler` | HTTP 节点：调用外部接口 |

### 规则 & 聚合

| 组件 | 说明 |
|------|------|
| `RuleEngine` | 条件网关规则引擎，支持变量表达式 |
| `AggregationEvaluator` | 会签聚合评估器，支持 AND（全部通过）、OR（任一通过）、比例通过 |
| `PlaceholderResolver` | 占位符解析，支持模板变量替换 |

### 持久化实体

| 实体 | 说明 |
|------|------|
| `DbApprovalFlow` | 审批流定义 |
| `DbApprovalFlowRecord` | 流程实例记录 |
| `DbApprovalTask` | 审批任务 |
| `DbApprovalStep` | 审批步骤 |
| `DbApprovalNodeRecord` | 节点执行记录 |
| `DbApprovalStatistics` | 流程统计 |
| `DbApprovalDelegation` | 审批委托（代审） |

### 扩展接口

| 接口 | 说明 |
|------|------|
| `IApprovalRepository` | 持久化接口，自行实现数据库存储 |
| `IFlowEventListener` | 流程事件监听器 |
| `IFlowEventHandler` | 流程事件处理器 |
| `IUserService` | 用户服务，提供审批人信息 |
| `INotificationService` | 通知服务，审批消息推送 |
| `IStatisticsUpdater` | 统计更新器 |

---

## DI 注册

```csharp
// 注册审批流核心服务
services.AddApprovalFlow(options);

// 或从配置自动加载 JSON 流程定义
services.AddApprovalFlow(configuration);

// 注册自定义持久化
services.AddApprovalRepository<MyApprovalRepository>();

// 注册事件监听器
services.AddApprovalFlowListener<MyFlowEventListener>();

// 注册用户服务
services.AddApprovalUserService<MyUserService>();

// 注册通知服务
services.AddApprovalNotificationService<MyNotificationService>();

// 注册统计更新器
services.AddStatisticsUpdater<MyStatisticsUpdater>();
```

---

## 配置

在 `appsettings.json`  中配置：

```json
{
  "ApprovalFlowOptions": {
    "DataDir": "data",
    "ThreadPoolSize": 10
  }
}
```

| 字段 | 说明 | 默认值 |
|------|------|--------|
| `DataDir` | 流程定义 JSON 文件目录 | `"data"` |
| `AutoApproval` | 预留字段：当前代码中无读取点，设置后不会产生自动审批行为 | `false` |
| `ThreadPoolSize` | 引擎线程池大小 | `10` |

---

## 代码示例

### 定义流程（JSON）

```json
{
  "key": "leave_approval",
  "name": "请假审批",
  "nodes": [
    { "id": "start", "type": "start-node", "x": 0, "y": 0, "properties": {}, "text": { "value": "开始" } },
    {
      "id": "manager",
      "type": "user-node",
      "x": 0,
      "y": 120,
      "properties": {
        "role": "部门经理",
        "stage": "初审",
        "multiApproval": false,
        "aggregationType": "any_approve",
        "approvePercentage": 100,
        "assignees": [
          { "userId": 101, "userName": "张经理", "role": "部门经理" }
        ]
      },
      "text": { "value": "经理审批" }
    },
    {
      "id": "gateway1",
      "type": "gateway-node",
      "x": 0,
      "y": 240,
      "properties": {
        "rules": [
          {
            "edgeId": "gw-to-director",
            "edgeText": "金额大于 1 万",
            "conditions": [
              { "field": "amount", "operator": ">", "value": "10000", "valueType": "number" }
            ],
            "logic": "and"
          }
        ],
        "defaultEdgeId": "gw-to-end",
        "defaultEdgeText": "金额不超过 1 万"
      },
      "text": { "value": "金额判断" }
    },
    {
      "id": "director",
      "type": "user-node",
      "x": 200,
      "y": 360,
      "properties": {
        "role": "总监",
        "stage": "复审",
        "multiApproval": true,
        "aggregationType": "all_approve",
        "approvePercentage": 100,
        "assignees": [
          { "userId": 201, "userName": "李总监", "role": "总监" }
        ]
      },
      "text": { "value": "总监审批" }
    },
    { "id": "end", "type": "end-node", "x": 0, "y": 480, "properties": {}, "text": { "value": "结束" } }
  ],
  "edges": [
    { "id": "e-start-manager", "type": "polyline", "sourceNodeId": "start", "targetNodeId": "manager" },
    { "id": "e-manager-gw", "type": "polyline", "sourceNodeId": "manager", "targetNodeId": "gateway1" },
    { "id": "gw-to-director", "type": "polyline", "sourceNodeId": "gateway1", "targetNodeId": "director", "text": { "value": "通过" } },
    { "id": "gw-to-end", "type": "polyline", "sourceNodeId": "gateway1", "targetNodeId": "end", "text": { "value": "通过" } },
    { "id": "e-director-end", "type": "polyline", "sourceNodeId": "director", "targetNodeId": "end" }
  ]
}
```

节点类型取 `start-node` / `user-node` / `gateway-node` / `end-node`（另有 `service-node`、`service-gateway-node`）；审批人写在 `properties.assignees`，网关条件写在 `properties.rules`，连线用 `sourceNodeId` / `targetNodeId` 表达。

### 启动流程

```csharp
var engine = DiContainer.Instance.GetRequiredService<ApprovalFlowEngine>();
var repository = DiContainer.Instance.GetRequiredService<IApprovalRepository>();

// 加载流程定义
var builder = DiContainer.Instance.GetRequiredService<FlowBuilder>();
var executor = await builder.CreateExecutorFromJsonFileAsync("data/leave_approval.json");
var definition = executor.Definition;

// 启动流程
var response = await engine.StartFlowAsync(new StartFlowRequest
{
    FormPayload = new Dictionary<string, object>
    {
        ["applicant"] = "张三",
        ["days"] = 3,
        ["reason"] = "年假"
    },
    Variables = new Dictionary<string, object>
    {
        ["amount"] = 5000,
        ["managerId"] = "user_002"
    }
}, definition, repository);

Console.WriteLine($"流程已启动，RecordId: {response.RecordId}");
```

### 处理审批

```csharp
var result = await engine.ProcessApprovalAsync(new ProcessApprovalRequest
{
    RecordId = 1001,
    ApproverId = "user_002",
    Approved = true,
    Comment = "同意请假"
}, repository);
```

---

## 小贴士

1. **自动推进（未启用）**：`AutoApproval` 是预留字段，当前无读取点；`FlowEngine` 的监控列表不会被写入，因此进行中的流程不会被定时推进
2. **JSON 批量装载**：`ApprovalFlowWarmupService` 在应用启动时扫描 `DataDir` 下的 `*.json` 一次性装载；没有文件监听，改动 JSON 后需重启或再次调用 `CreateExecutorFromJsonFileAsync` 才会刷新缓存
3. **会签模式**：`AggregationEvaluator` 支持 AND（所有人通过）、OR（任一人通过）、比例（如 2/3 通过）
4. **HTTP 回调**：`HttpNodeHandler` 可在流程中调用外部 API，实现自动化集成
5. **审批委托**：通过 `DbApprovalDelegation` 实体支持代审功能
6. **事件驱动**：实现 `IFlowEventListener` 可监听流程初始化、节点进入/离开、完成等事件

---

## 许可证

Copyright (c) yswenli.

Licensed under the [Apache License 2.0](../LICENSE).
