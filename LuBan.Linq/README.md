[English](README.en.md) | 中文

# LuBan.Linq

> **作者**: yswenli | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **"动态查询、表达式组合、树形转换——LINQ 的终极增强包。"**

你是否还在为这些查询问题头疼？

- 前端传来动态字段名和值，还要手写一堆 `if-else` 拼表达式？
- 多个筛选条件要组合，`And`/`Or` 表达式写到手软？
- 非泛型 `IQueryable`/`IEnumerable` 没法直接 `Where("Name", "张三")`？
- 扁平列表转树形结构每次都要写递归？
- 分页查询总是重复写 `Skip/Take/Count`？

如果是，那 **LuBan.Linq** 就是为你准备的。

它是 **.NET 8** 下的 LINQ 动态增强库，提供表达式组合、动态字段查询、非泛型集合扩展、树形列表转换、分页等一套完整的查询解决方案。无论是 EF Core、内存集合还是动态场景，都能让你少写一半代码。

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Orm](../LuBan.Orm/README.md)

---

## 为什么选择 LuBan.Linq？

| 痛点 | LuBan.Linq 的解法 |
|---|---|
| 动态条件拼接表达式复杂 | `And()`、`Or()`、`AndIf()`、`OrIf()` 链式组合 |
| 前端传字段名无法直接查询 | `Where("Name", "张三")` 动态字段查询 |
| 非泛型集合操作受限 | `QueryableExtensions` / `EnumerableExtensions` 非泛型扩展 |
| 列表转树形结构要写递归 | `ToTreeList()` 一行转换 |
| 分页代码重复写 | `ToPagedList(page, pageSize)` 一行分页 |
| 条件不确定是否要加 | `Where(condition, expr)` 条件成立才拼接 |

---

## 一分钟预览

```csharp
using System.Linq.Dynamic;

// 表达式动态组合
Expression<Func<User, bool>> filter = u => u.Status == 1;
filter = filter.AndIf(!string.IsNullOrEmpty(keyword), u => u.Name.Contains(keyword));
filter = filter.OrIf(roleId > 0, u => u.RoleId == roleId);

var users = dbContext.Users.Where(filter).ToList();

// 动态字段查询（字段名来自前端）
var result = dbContext.Users
    .AsQueryable()
    .Where("Name", "张三")
    .OrderBy("CreateTime", isAscending: false)
    .ToList();

// 树形转换
var tree = flatList.ToTreeList(
    idName: "Id",
    childListName: "Children",
    parentIdName: "ParentId",
    rootValue: 0);

// 分页
var paged = dbContext.Users.ToPagedList(page: 1, pageSize: 20);
```

> 没有废话，只有生产力。

---

## 技术栈

- **目标框架**：.NET 8.0
- **项目类型**：类库（Class Library）
- **NuGet 包**：`LuBan.Linq`

### 核心依赖

| 包名 | 来源 | 用途 |
|---|---|---|
| LuBan.Common（项目引用） | 项目引用 | 序列化、反射、分页模型等基础能力 |

---

## 安装

```bash
dotnet add package LuBan.Linq
```

---

## 工具箱全览

### 表达式组合 — `ExpressionExtensions`

> 命名空间：`System.Linq.Dynamic`

| 方法 | 说明 |
|---|---|
| `And(expr1, expr2)` | 与操作合并两个表达式 |
| `Or(expr1, expr2)` | 或操作合并两个表达式 |
| `AndIf(condition, expr)` | 条件成立才做 And 合并 |
| `OrIf(condition, expr)` | 条件成立才做 Or 合并 |
| `Compose(expr1, expr2, mergeFunc)` | 自定义方式组合两个表达式 |
| `GetMemberName()` | 从表达式中提取成员名称 |
| `GetExpressionPropertyName()` | 从 Lambda 表达式获取属性名 |
| `BuildFilterExpression(Type, PropertyInfo, object)` | 动态构建 `element => element.Property == value` 表达式 |
| `IsNullOrEmpty<T>()` | 判断集合是否为空 |
| `ToDynamic<T>(Expression)` | 按表达式提取对象属性为动态对象 |

### 泛型查询扩展 — `QueryableTExtensions`

> 命名空间：`System.Linq.Dynamic`

| 方法 | 说明 |
|---|---|
| `Where(condition, expression)` | 条件成立才拼接 Where |
| `Where(params expression[])` | 多表达式 OR 合并 |
| `Where(params (bool, expression)[])` | 条件元组数组，成立才拼接 |
| `Where(string field, object value)` | 按字段名动态筛选 |
| `OrderBy(string field, bool isDesc)` | 按字段名动态排序 |
| `Select(string field)` | 按字段名动态投影 |
| `Select<TVal>(string field)` | 按字段名投影为指定类型 |
| `Any(string field, object value)` | 动态字段存在判断 |
| `NotAny(string field, object value)` | 动态字段不存在判断 |
| `ToPagedList(page, pageSize)` | 分页查询，返回 `PagedList<T>` |

### 非泛型查询扩展 — `QueryableExtensions`

> 命名空间：`System.Linq.Dynamic`

为非泛型 `IQueryable` 提供动态查询能力，字段名和值在运行时确定。

| 方法 | 说明 |
|---|---|
| `Where(string field, object value)` | 动态字段筛选 |
| `OrderBy(string field, bool isAscending)` | 动态排序 |
| `Select(string field)` | 动态投影 |
| `Any()` | 是否有元素 |
| `Any(string field, object value)` | 动态字段存在判断 |
| `NotAny(string field, object value)` | 动态字段不存在判断 |
| `First()` / `FirstOrDefault()` | 获取首元素（dynamic 返回） |
| `ToDynamicList()` | 转为 `List<object>`（JSON 序列化中间态） |

### IEnumerable 扩展

> 命名空间：`System.Linq.Dynamic`

`EnumerableExtensions`（非泛型）和 `EnumerableTExtensions`（泛型）提供与 Queryable 对应的内存集合版本，内部通过 `AsQueryable()` 桥接。

| 方法 | 说明 |
|---|---|
| `Add(dynamic)` / `Remove(dynamic)` | 非泛型集合动态增删 |
| `Where/OrderBy/Select/Any/First/FirstOrDefault` | 同 Queryable 版本的内存集合等价物 |
| `ToPagedList(page, pageSize)` | 内存集合分页 |

### 树形列表 — `TreeListExtentions`

> 命名空间：`System.Linq.Dynamic`

| 方法 | 说明 |
|---|---|
| `ToTreeList(idName, childListName, parentIdName, rootValue, maxLevel)` | 字符串字段名版本 |
| `ToTreeList(idExpr, childListExpr, parentIdExpr, rootValue, maxLevel)` | Lambda 表达式版本 |
| `ToTreeList(childListExpr, parentIdExpr, rootValue, maxLevel)` | 省略 id（默认 "Id"） |

### 名称对扩展 — `NamePairExtensions`

> 命名空间：`System.Linq.Dynamic`

| 方法 | 说明 |
|---|---|
| `GetNamePairsByArr(params NamePair[])` | 按名称对数组映射属性 |
| `GetNamePairs(params (string, string)[])` | 元组方式映射：`("Id", "编号")` |
| `GetNamePairs(params (Expression, string)[])` | 表达式方式映射：`(q => q.Id, "编号")` |

---

## 实战示例

### 动态条件组合

```csharp
using System.Linq.Dynamic;

// 基础条件
Expression<Func<Order, bool>> filter = o => o.Status != -1;

// 条件成立才拼接
filter = filter.AndIf(!string.IsNullOrEmpty(orderNo), o => o.OrderNo.Contains(orderNo));
filter = filter.AndIf(startDate.HasValue, o => o.CreateTime >= startDate.Value);
filter = filter.AndIf(endDate.HasValue, o => o.CreateTime <= endDate.Value);
filter = filter.OrIf(userIds != null && userIds.Length > 0, o => userIds.Contains(o.UserId));

var orders = dbContext.Orders.Where(filter).ToList();
```

### 动态字段查询（字段名来自前端/配置）

```csharp
using System.Linq.Dynamic;

// 前端传来排序字段和方向
string sortField = "CreateTime";
bool isDesc = true;

var result = dbContext.Products
    .AsQueryable()
    .Where("Status", 1)
    .OrderBy(sortField, isDesc)
    .ToList();

// 动态投影
var names = dbContext.Users
    .AsQueryable()
    .Select("Name")
    .ToList();
```

### 多条件 OR 查询

```csharp
using System.Linq.Dynamic;

// 方式一：多表达式 OR
var result = dbContext.Users.Where(
    u => u.Role == "Admin",
    u => u.Role == "SuperAdmin",
    u => u.IsVip
);

// 方式二：条件元组
var result2 = dbContext.Users.Where(
    (hasKeyword, u => u.Name.Contains(keyword)),
    (hasRole, u => u.Role == role),
    (hasStatus, u => u.Status == status)
);
```

### 树形列表转换

```csharp
using System.Linq.Dynamic;

// 方式一：字符串字段名
var tree = menuList.ToTreeList(
    idName: "Id",
    childListName: "Children",
    parentIdName: "ParentId",
    rootValue: 0,
    maxLevel: 5);

// 方式二：Lambda 表达式（编译时安全）
var tree2 = menuList.ToTreeList(
    idExpression: m => m.Id,
    childListExpression: m => m.Children,
    parentIdExpression: m => m.ParentId,
    rootValue: 0);
```

### 分页查询

```csharp
using System.Linq.Dynamic;

// IQueryable 分页
var paged = dbContext.Orders
    .Where(o => o.Status == 1)
    .OrderBy(o => o.CreateTime, isDesc: true)
    .ToPagedList(page: 1, pageSize: 20);

Console.WriteLine($"总计: {paged.Total}, 当前页: {paged.Page}");
foreach (var item in paged.Items)
{
    Console.WriteLine(item.OrderNo);
}

// IEnumerable 同样支持
var memPaged = memoryList.ToPagedList(page: 2, pageSize: 10);
```

### 名称对映射

```csharp
using System.Linq.Dynamic;

var log = new ApiLogInfo { Id = 1, Url = "/api/test", Method = "GET" };

// 元组方式
var pairs = log.GetNamePairs(("Id", "编号"), ("Url", "地址"), ("Method", "请求方式"));

// 表达式方式（编译时安全）
var pairs2 = log.GetNamePairs(
    (q => q.Id, "编号"),
    (q => q.Url, "地址"),
    (q => q.Method, "请求方式"));
```

---

## 使用小贴士

1. **命名空间统一为 `System.Linq.Dynamic`**：所有扩展类都在这一个命名空间下，`using System.Linq.Dynamic;` 一行搞定。
2. **动态字段名大小写**：非泛型 `Where("name", ...)` 默认忽略大小写（`BindingFlags.IgnoreCase`）；泛型版本区分大小写。
3. **`AndIf`/`OrIf` 是懒组合**：条件不成立时直接返回原表达式，不会产生多余的 `&& true`。
4. **`ToTreeList` 默认最大 3 层**：如果树结构超过 3 层，记得传 `maxLevel` 参数。
5. **`ToDynamicList` 走 JSON 序列化**：性能敏感场景慎用，适合做 API 返回的动态数据转换。
6. **`ToPagedList` 会执行两次查询**：一次 Count，一次分页取值，大数据量时注意索引优化。

---

## 许可证

MIT License

---

**LuBan.Linq** —— 让 .NET 查询更动态、更灵活、更爽快。

> 如果你也受够了手写动态查询的痛苦，那就把它加进你的工具箱吧。
