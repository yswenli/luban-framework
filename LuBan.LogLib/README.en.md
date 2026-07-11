[中文](README.md) | English

# LuBan.LogLib

> **Author**: yswenli | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> Automatic database log collection — batch insert error logs and API call logs into the database, with automatic cleanup, ready to use out of the box.

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## Why LuBan.LogLib?

Only writing application logs to files? Searching through log files to troubleshoot production issues is extremely inefficient. Storing logs in a database enables easy querying, statistics, and alerting, but manual writes impact performance.

**LuBan.LogLib** provides a high-performance database log collection solution:
- Subscribe to `Logger.OnCalled` / `Logger.OnError` events for automatic log collection
- Uses `Batcher<T>` for batch database writes, avoiding per-row insert performance overhead
- Error logs written to `db_log_error` table, API call logs written to `db_log_api` table
- Automatically parses device info, OS, and browser from User-Agent
- Built-in `DbLogCleaner` for scheduled cleanup by max count and expiration time
- Singleton pattern, start with one line of code

## Quick Preview

```csharp
// Start log collection (subscribe to Logger events)
LoggerCollector.Instance.Start();

// All Logger.Info/Error/Warn calls will be automatically collected afterwards
Logger.Error("Database connection failed", exception);
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

// Stop collection
LoggerCollector.Instance.Stop();
```

## Tech Stack

| Component | Description |
|-----------|-------------|
| LuBan.Common | Base utility library (Logger, Batcher, etc.) |
| LuBan.Orm | Data persistence |
| .NET | 8.0 |

## Installation

```bash
dotnet add package LuBan.LogLib
```

## Feature Overview

| Module | Core Class | Description |
|--------|------------|-------------|
| Log Collector | `LoggerCollector` | Singleton, subscribes to Logger events, batch inserts to DB |
| Log Cleaner | `DbLogCleaner` | Runs hourly, cleans by count + expiration time |
| Error Log Entity | `DbLogError` | Service name, description, parameters, exception info |
| API Log Entity | `DbLogApi` | Service name, IP, URL, request method, input/output, duration, device info |

## Detailed Usage

### Database Table Structure

#### db_log_error (Error Log Table)

| Field | Type | Description |
|-------|------|-------------|
| ServiceName | string(256) | Service name |
| Description | string(1024) | Log description |
| Parmas | BigString | Parameters (JSON) |
| Exception | BigString | Exception info (JSON) |

#### db_log_api (API Call Log Table)

| Field | Type | Description |
|-------|------|-------------|
| ServiceName | string(256) | Service name |
| CallIp | string(100) | Caller IP |
| Url | string(2048) | Request URL |
| RequestMethod | string(50) | Request method |
| Header | string(2048) | Request headers |
| UserAgent | string(1024) | User agent |
| Input | BigString | Input value |
| Output | BigString | Output value |
| StatusCode | int | Response status code |
| UserId | string(64) | User ID |
| Cost | long | Duration (ms) |
| Exception | BigString | Exception info |
| Device | string(128) | Device info |
| Os | string(128) | Operating system |
| Ua | string(128) | Browser |

### Log Cleanup Configuration

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

| Config Item | Description |
|-------------|-------------|
| ApiLogMaxSize | Max API log entries to retain (0 = no cleanup) |
| ApiLogExpiredSeconds | API log expiration in seconds (0 = no expiration) |
| ErrorLogMaxSize | Max error log entries to retain (0 = no cleanup) |
| ErrorLogExpiredSeconds | Error log expiration in seconds (0 = no expiration) |

### Workflow

```
Logger.Error/OnCalled
       |
       v
LoggerCollector (subscribes to events)
       |
       +---> LogInfo --> Batcher<LogInfo> --> db_log_error
       |
       +---> ApiLogInfo --> Batcher<ApiLogInfo> --> db_log_api
       
DbLogCleaner (runs hourly)
       |
       +---> Clean by max count (delete oldest records)
       +---> Clean by expiration (delete expired records)
```

## Usage Tips

- A database connection string named `LogsDB` must be configured in the config file
- `LoggerCollector` depends on `DbConnectionOptions.EnableDbLogs` switch; logs are not collected when set to `false`
- `Batcher<T>` accumulates a certain number of logs before batch writing to avoid frequent database operations
- `DbLogCleaner` automatically runs cleanup every hour, supporting both count-based and expiration-based strategies
- `Header` and `Url` fields in API logs are automatically truncated when exceeding 2048 characters
- User-Agent parsing automatically extracts device type (Device), operating system (Os), and browser (Ua) fields
- `ServiceName` is automatically obtained from `ConfigUtil.GetServiceName()` for multi-service log differentiation
- Calling `Stop()` unsubscribes from events and stops the cleaner, suitable for graceful shutdown scenarios

## License

Copyright (c) yswenli. All rights reserved.
