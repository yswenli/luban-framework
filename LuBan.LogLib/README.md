[English](README.en.md) | 中文

# LuBan.LogLib

> **作者**: yswenli | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> 数据库日志自动收集 —— 异常日志、API 调用日志批量入库，自动清理，开箱即用。

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## 为什么选择 LuBan.LogLib？

应用日志只写到文件？排查线上问题时翻日志文件效率极低。把日志存到数据库可以方便查询、统计、告警，但手动写入又影响性能。

**LuBan.LogLib** 提供了高性能的数据库日志收集方案：
- 订阅 `Logger.OnCalled` / `Logger.OnError` 事件，自动收集日志
- 使用 `Batcher<T>` 批量写入数据库，避免逐条插入的性能损耗
- 异常日志写入 `db_log_error` 表，API 调用日志写入 `db_log_api` 表
- 自动从 User-Agent 解析设备信息、操作系统、浏览器
- 内置 `DbLogCleaner` 定时清理器，按最大条数和过期时间自动清理
- 单例模式，一行代码启动

## 快速预览

```csharp
// 启动日志收集（订阅 Logger 事件）
LoggerCollector.Instance.Start();

// 之后所有 Logger.Info/Error/Warn 调用都会被自动收集
Logger.Error("数据库连接失败", exception);
Logger.OnCalled?.Invoke(new ApiLogInfo
{
    Url = "/api/users",
    RequestMethod = "GET",
    StatusCode = 200,
    Cost = 15,
    CallIp = "192.168.1.100",
    UserAgent = "Mozilla/5.0 ...",
    Input = "",
    Output = "{...}"
});

// 停止收集
LoggerCollector.Instance.Stop();
```

## 技术栈

| 组件 | 说明 |
|------|------|
| LuBan.Common | 基础工具库（Logger、Batcher 等） |
| LuBan.Orm | 数据持久化 |
| .NET | 8.0 |

## 安装

```bash
dotnet add package LuBan.LogLib
```

## 功能概览

| 功能模块 | 核心类 | 说明 |
|---------|--------|------|
| 日志收集器 | `LoggerCollector` | 单例，订阅 Logger 事件，批量入库 |
| 日志清理器 | `DbLogCleaner` | 每小时执行，按条数+过期时间清理 |
| 异常日志实体 | `DbLogError` | 服务名、描述、参数、异常信息 |
| API 日志实体 | `DbLogApi` | 服务名、IP、URL、请求方式、输入输出、耗时、设备信息 |

## 详细用法

### 数据库表结构

#### db_log_error（异常日志表）

| 字段 | 类型 | 说明 |
|------|------|------|
| ServiceName | string(256) | 服务名称 |
| Description | string(1024) | 日志描述 |
| Parmas | BigString | 参数（JSON） |
| Exception | BigString | 异常信息（JSON） |

#### db_log_api（API 调用日志表）

| 字段 | 类型 | 说明 |
|------|------|------|
| ServiceName | string(256) | 服务名称 |
| CallIp | string(100) | 调用方 IP |
| Url | string(2048) | 请求地址 |
| RequestMethod | string(50) | 请求方式 |
| Header | string(2048) | 请求头 |
| UserAgent | string(1024) | 用户代理 |
| Input | BigString | 输入值 |
| Output | BigString | 输出值 |
| StatusCode | int | 响应码 |
| UserId | string(64) | 用户 ID |
| Cost | long | 耗时（ms） |
| Exception | BigString | 异常信息 |
| Device | string(128) | 设备信息 |
| Os | string(128) | 操作系统 |
| Ua | string(128) | 浏览器 |

### 日志清理配置

```json
{
  "DbLogOptions": {
    "ApiLogMaxSize": 100000,
    "ApiLogExpiredSeconds": 2592000,
    "ErrorLogMaxSize": 50000,
    "ErrorLogExpiredSeconds": 2592000
  }
}
```

| 配置项 | 说明 |
|--------|------|
| ApiLogMaxSize | API 日志最大保留条数（0 = 不清理） |
| ApiLogExpiredSeconds | API 日志过期秒数（0 = 不过期） |
| ErrorLogMaxSize | 异常日志最大保留条数（0 = 不清理） |
| ErrorLogExpiredSeconds | 异常日志过期秒数（0 = 不过期） |

### 工作流程

```
Logger.Error/OnCalled
       |
       v
LoggerCollector (订阅事件)
       |
       +---> LogInfo --> Batcher<LogInfo> --> db_log_error
       |
       +---> ApiLogInfo --> Batcher<ApiLogInfo> --> db_log_api
       
DbLogCleaner (每小时执行)
       |
       +---> 按最大条数清理（删除最早的记录）
       +---> 按过期时间清理（删除过期的记录）
```

## 使用提示

- 需要在配置文件中配置名为 `LogsDB` 的数据库连接字符串
- `LoggerCollector` 依赖 `DbConnectionOptions.EnableDbLogs` 开关，设为 `false` 时不收集日志
- `Batcher<T>` 会累积一定数量的日志后批量写入，避免频繁数据库操作
- `DbLogCleaner` 每小时自动执行一次清理，同时支持按条数和过期时间两种策略
- API 日志中的 `Header` 和 `Url` 字段超过 2048 字符时会自动截断
- User-Agent 解析自动提取设备类型（Device）、操作系统（Os）、浏览器（Ua）三个字段
- `ServiceName` 自动从 `ConfigUtil.GetServiceName()` 获取，用于多服务日志区分
- 调用 `Stop()` 会取消事件订阅并停止清理器，适用于优雅关闭场景

## 许可证

Copyright (c) yswenli. All rights reserved.
