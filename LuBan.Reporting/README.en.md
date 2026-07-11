[中文](README.md) | English

# LuBan.Reporting

> **Author**: yswenli | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **One-click Excel/CSV export for generic lists, dynamic reports with SQL + Lua script transforms — say goodbye to repetitive export code.**

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)

---

## Why Choose LuBan.Reporting?

Manually assembling Excel for every data export? Column order, date formats, enum translations, boolean display... reinventing the wheel every time?  
LuBan.Reporting uses **attribute annotations + generic reflection** to automatically handle column mapping, and also supports **dynamic reports** (configurable SQL + Lua scripts), turning export from "manual labor" into "configuration work".

---

## Quick Preview

```csharp
// Define data model
public class OrderReport
{
    [ReportDescription("Order No.", 1)]
    public string OrderNo { get; set; }

    [ReportDescription("Amount", 2)]
    public decimal Amount { get; set; }

    [ReportDescription("Order Time", 3, datetimeFormat: "yyyy-MM-dd")]
    public DateTime CreateTime { get; set; }

    [ReportDescription("Status", 4, enumValues: "Pending,Paid,Shipped,Completed")]
    public int Status { get; set; }

    [ReportIgnore]
    public string InternalNote { get; set; }
}

// One-line export
using var report = new Report<OrderReport>(dataList);
report.Export("orders_2026.xlsx");  // Auto-detects .xlsx / .csv
```

---

## Tech Stack & Dependencies

| Category | Component | Description |
|----------|-----------|-------------|
| Runtime | .NET 8.0 | — |
| Framework Dependencies | LuBan.Common, LuBan.Orm | LuBan base components |

---

## Installation

```bash
dotnet add package LuBan.Reporting
```

---

## Feature Overview

### Static Reports (Generic Export)

| Class | Description |
|-------|-------------|
| `Report<T>` | Unified entry point, auto-selects Excel or CSV based on file extension |
| `ReportBase<T>` | Base class, handles reflective column parsing (with caching) |
| `ReportExcel<T>` | Excel export implementation |
| `ReportCsv<T>` | CSV export implementation |
| `ReportColumn` | Column information descriptor (name, title, sorting, formatting, etc.) |

### Attribute Annotations

| Attribute | Description |
|-----------|-------------|
| `[ReportDescription(title, sortNo)]` | Mark export column, specify title and sort order |
| `[ReportIgnore]` | Mark property to be ignored in export |

**ReportDescription supported parameters:**

| Parameter | Description | Example |
|-----------|-------------|---------|
| `title` | Column title | `"Order No."` |
| `sortNo` | Column sort order | `1` |
| `boolValues` | Boolean value translation | `"Yes,No"` |
| `enumValues` | Enum value translation | `"Pending,Paid,Shipped"` |
| `datetimeFormat` | Date format | `"yyyy-MM-dd"` |
| `enumType` | Auto-enum (get descriptions from enum type) | `typeof(OrderStatus)` |
| `custormConvert` | Custom delegate conversion | `Tuple<Type, string>` |

### Dynamic Reports

| Class | Description |
|-------|-------------|
| `DynamicReportService` | Dynamic report service: configurable SQL templates + column mapping + Lua transforms |
| `LuaScriptEngine` | Lua script engine, supports data row transforms |
| `ReportConfigService` | Report configuration management service |
| `DbReportConfig` | Report configuration entity |
| `DbReportColumnConfig` | Report column configuration entity |

---

## Code Examples

### Static Report Export

```csharp
// Full attribute annotation example
public class EmployeeReport
{
    [ReportDescription("Employee No.", 1)]
    public string EmployeeNo { get; set; }

    [ReportDescription("Name", 2)]
    public string Name { get; set; }

    [ReportDescription("Hire Date", 3, datetimeFormat: "yyyy-MM-dd")]
    public DateTime HireDate { get; set; }

    [ReportDescription("Active", 4, boolValues: "Active,Inactive")]
    public bool IsActive { get; set; }

    [ReportDescription("Level", 5, enumType: typeof(EmployeeLevel))]
    public int Level { get; set; }

    [ReportIgnore]
    public string Password { get; set; }
}

// Export to Excel
var data = await GetEmployeeList();
using var report = new Report<EmployeeReport>(data);
report.Export("employees.xlsx");

// Export to CSV
using var report2 = new Report<EmployeeReport>(data);
report2.Export("employees.csv");
```

### Dynamic Reports

```csharp
// Preview report (first N rows)
var service = new DynamicReportService(repository, luaEngine);
var dataTable = await service.PreviewAsync(
    reportConfigId: 1,
    sqlParams: new Dictionary<string, object>
    {
        ["startDate"] = "2026-01-01",
        ["endDate"] = "2026-07-01"
    },
    previewRows: 100
);

// Execute raw SQL
var rawResult = await service.ExecuteRawSqlAsync(
    "SELECT * FROM orders WHERE status = @status",
    sqlParams: new Dictionary<string, object> { ["status"] = 1 },
    maxRows: 500
);
```

---

## Tips

1. **Column Caching**: `ReportBase<T>` uses `ConcurrentDictionary` to cache column info; reflection runs only once
2. **Auto-detect Format**: `Report<T>.Export()` auto-selects Excel (`.xlsx`/`.xls`) or CSV (`.csv`) based on file extension
3. **Boolean Translation**: Set `boolValues: "Yes,No"` and `true` displays as "Yes", `false` as "No"
4. **Enum Translation**: Use `enumType` parameter to auto-get display text from enum's `Description` attribute
5. **Dynamic Reports**: `DynamicReportService` supports parameterized SQL queries to prevent injection attacks
6. **Lua Transforms**: Dynamic reports can configure Lua scripts to transform each data row for flexible data processing

---

## License

Copyright (c) yswenli. All Rights Reserved.
