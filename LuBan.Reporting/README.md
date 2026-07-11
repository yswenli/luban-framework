[English](README.en.md) | 中文

# LuBan.Reporting

> **作者**: yswenli | **联系邮箱**: yswenli@outlook.com | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **泛型列表一键导出 Excel/CSV，动态报表支持 SQL + Lua 脚本变换——告别重复的导出代码。**

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Orm](../LuBan.Orm/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)

---

## 为什么选择 LuBan.Reporting？

每次写数据导出都要手动拼 Excel？列顺序、日期格式、枚举翻译、布尔值显示……每次都重复造轮子？  
LuBan.Reporting 用**特性标注 + 泛型反射**自动完成列映射，还支持**动态报表**（可配置 SQL + Lua 脚本），让导出功能从"体力活"变成"配置活"。

---

## 快速预览

```csharp
// 定义数据模型
public class OrderReport
{
    [ReportDescription("订单号", 1)]
    public string OrderNo { get; set; }

    [ReportDescription("金额", 2)]
    public decimal Amount { get; set; }

    [ReportDescription("下单时间", 3, datetimeFormat: "yyyy-MM-dd")]
    public DateTime CreateTime { get; set; }

    [ReportDescription("状态", 4, enumValues: "待付款,已付款,已发货,已完成")]
    public int Status { get; set; }

    [ReportIgnore]
    public string InternalNote { get; set; }
}

// 一行导出
using var report = new Report<OrderReport>(dataList);
report.Export("orders_2026.xlsx");  // 自动识别 .xlsx / .csv
```

---

## 技术栈 & 依赖

| 类别 | 组件 | 说明 |
|------|------|------|
| 运行时 | .NET 8.0 | — |
| 框架依赖 | LuBan.Common, LuBan.Orm | LuBan 基础组件 |

---

## 安装

```bash
dotnet add package LuBan.Reporting
```

---

## 功能全景

### 静态报表（泛型导出）

| 类 | 说明 |
|----|------|
| `Report<T>` | 统一入口，根据文件后缀自动选择 Excel 或 CSV |
| `ReportBase<T>` | 基类，负责反射解析列信息（带缓存） |
| `ReportExcel<T>` | Excel 导出实现 |
| `ReportCsv<T>` | CSV 导出实现 |
| `ReportColumn` | 列信息描述（名称、标题、排序、格式化等） |

### 特性标注

| 特性 | 说明 |
|------|------|
| `[ReportDescription(title, sortNo)]` | 标记导出列，指定标题和排序 |
| `[ReportIgnore]` | 标记忽略属性，不导出 |

**ReportDescription 支持的参数：**

| 参数 | 说明 | 示例 |
|------|------|------|
| `title` | 列标题 | `"订单号"` |
| `sortNo` | 列排序 | `1` |
| `boolValues` | 布尔值翻译 | `"是,否"` |
| `enumValues` | 枚举值翻译 | `"待付款,已付款,已发货"` |
| `datetimeFormat` | 日期格式 | `"yyyy-MM-dd"` |
| `enumType` | 自动枚举（从枚举类型获取描述） | `typeof(OrderStatus)` |
| `custormConvert` | 自定义委托转换 | `Tuple<Type, string>` |

### 动态报表

| 类 | 说明 |
|----|------|
| `DynamicReportService` | 动态报表服务：可配置 SQL 模板 + 列映射 + Lua 变换 |
| `LuaScriptEngine` | Lua 脚本引擎，支持数据行变换 |
| `ReportConfigService` | 报表配置管理服务 |
| `DbReportConfig` | 报表配置实体 |
| `DbReportColumnConfig` | 报表列配置实体 |

---

## 代码示例

### 静态报表导出

```csharp
// 完整特性标注示例
public class EmployeeReport
{
    [ReportDescription("工号", 1)]
    public string EmployeeNo { get; set; }

    [ReportDescription("姓名", 2)]
    public string Name { get; set; }

    [ReportDescription("入职日期", 3, datetimeFormat: "yyyy年MM月dd日")]
    public DateTime HireDate { get; set; }

    [ReportDescription("是否在职", 4, boolValues: "在职,离职")]
    public bool IsActive { get; set; }

    [ReportDescription("职级", 5, enumType: typeof(EmployeeLevel))]
    public int Level { get; set; }

    [ReportIgnore]
    public string Password { get; set; }
}

// 导出为 Excel
var data = await GetEmployeeList();
using var report = new Report<EmployeeReport>(data);
report.Export("employees.xlsx");

// 导出为 CSV
using var report2 = new Report<EmployeeReport>(data);
report2.Export("employees.csv");
```

### 动态报表

```csharp
// 预览报表（前 N 行）
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

// 执行原生 SQL
var rawResult = await service.ExecuteRawSqlAsync(
    "SELECT * FROM orders WHERE status = @status",
    sqlParams: new Dictionary<string, object> { ["status"] = 1 },
    maxRows: 500
);
```

---

## 小贴士

1. **列缓存**：`ReportBase<T>` 使用 `ConcurrentDictionary` 缓存列信息，反射只执行一次
2. **自动识别格式**：`Report<T>.Export()` 根据文件后缀自动选择 Excel（`.xlsx`/`.xls`）或 CSV（`.csv`）
3. **布尔值翻译**：设置 `boolValues: "是,否"` 后，`true` 自动显示为"是"，`false` 显示为"否"
4. **枚举翻译**：使用 `enumType` 参数可自动从枚举的 `Description` 特性获取显示文本
5. **动态报表**：`DynamicReportService` 支持 SQL 参数化查询，防止注入攻击
6. **Lua 变换**：动态报表可配置 Lua 脚本对每行数据进行变换，实现灵活的数据处理

---

## 许可证

Copyright (c) yswenli. All Rights Reserved.
