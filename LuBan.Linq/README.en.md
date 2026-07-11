[中文](README.md) | English

# LuBan.Linq

> **Author**: yswenli | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> **"Dynamic queries, expression composition, tree conversion — the ultimate LINQ enhancement package."**

Are you still struggling with these query issues?

- Frontend sends dynamic field names and values, and you have to write tons of `if-else` to build expressions?
- Multiple filter conditions to combine, writing `And`/`Or` expressions until your hands hurt?
- Non-generic `IQueryable`/`IEnumerable` can't directly do `Where("Name", "Zhang San")`?
- Converting flat lists to tree structures requires writing recursion every time?
- Pagination queries always repeat `Skip/Take/Count`?

If so, **LuBan.Linq** is built for you.

It's a LINQ dynamic enhancement library for **.NET 8**, providing expression composition, dynamic field queries, non-generic collection extensions, tree list conversion, pagination, and a complete query solution. Whether it's EF Core, in-memory collections, or dynamic scenarios, it lets you write half the code.

---

**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.Orm](../LuBan.Orm/README.md)

---

## Why Choose LuBan.Linq?

| Pain Point | LuBan.Linq's Solution |
|---|---|
| Dynamic condition expression building is complex | `And()`, `Or()`, `AndIf()`, `OrIf()` chain composition |
| Frontend field names can't be queried directly | `Where("Name", "Zhang San")` dynamic field query |
| Non-generic collection operations are limited | `QueryableExtensions` / `EnumerableExtensions` non-generic extensions |
| Converting lists to tree structures requires recursion | `ToTreeList()` one-line conversion |
| Pagination code is repetitive | `ToPagedList(page, pageSize)` one-line pagination |
| Uncertain whether to add a condition | `Where(condition, expr)` only composes when condition is true |

---

## One-Minute Preview

```csharp
using System.Linq.Dynamic;

// Dynamic expression composition
Expression<Func<User, bool>> filter = u => u.Status == 1;
filter = filter.AndIf(!string.IsNullOrEmpty(keyword), u => u.Name.Contains(keyword));
filter = filter.OrIf(roleId > 0, u => u.RoleId == roleId);

var users = dbContext.Users.Where(filter).ToList();

// Dynamic field query (field name from frontend)
var result = dbContext.Users
    .AsQueryable()
    .Where("Name", "Zhang San")
    .OrderBy("CreateTime", isAscending: false)
    .ToList();

// Tree conversion
var tree = flatList.ToTreeList(
    idName: "Id",
    childListName: "Children",
    parentIdName: "ParentId",
    rootValue: 0);

// Pagination
var paged = dbContext.Users.ToPagedList(page: 1, pageSize: 20);
```

> No fluff, just productivity.

---

## Tech Stack

- **Target Framework**: .NET 8.0
- **Project Type**: Class Library
- **NuGet Package**: `LuBan.Linq`

### Core Dependencies

| Package | Source | Purpose |
|---|---|---|
| LuBan.Common (project reference) | Project reference | Serialization, reflection, pagination model, and other foundational capabilities |

---

## Installation

```bash
dotnet add package LuBan.Linq
```

---

## Toolbox Overview

### Expression Composition — `ExpressionExtensions`

> Namespace: `System.Linq.Dynamic`

| Method | Description |
|---|---|
| `And(expr1, expr2)` | Combine two expressions with AND |
| `Or(expr1, expr2)` | Combine two expressions with OR |
| `AndIf(condition, expr)` | AND combine only if condition is true |
| `OrIf(condition, expr)` | OR combine only if condition is true |
| `Compose(expr1, expr2, mergeFunc)` | Combine two expressions with custom merge function |
| `GetMemberName()` | Extract member name from expression |
| `GetExpressionPropertyName()` | Get property name from lambda expression |
| `BuildFilterExpression(Type, PropertyInfo, object)` | Dynamically build `element => element.Property == value` expression |
| `IsNullOrEmpty<T>()` | Check if collection is empty |
| `ToDynamic<T>(Expression)` | Extract object properties as dynamic object by expression |

### Generic Query Extensions — `QueryableTExtensions`

> Namespace: `System.Linq.Dynamic`

| Method | Description |
|---|---|
| `Where(condition, expression)` | Compose Where only if condition is true |
| `Where(params expression[])` | Multiple expressions OR-combined |
| `Where(params (bool, expression)[])` | Condition tuple array, compose only if true |
| `Where(string field, object value)` | Dynamic filter by field name |
| `OrderBy(string field, bool isDesc)` | Dynamic sort by field name |
| `Select(string field)` | Dynamic projection by field name |
| `Select<TVal>(string field)` | Project to specified type by field name |
| `Any(string field, object value)` | Dynamic field existence check |
| `NotAny(string field, object value)` | Dynamic field non-existence check |
| `ToPagedList(page, pageSize)` | Paginated query, returns `PagedList<T>` |

### Non-Generic Query Extensions — `QueryableExtensions`

> Namespace: `System.Linq.Dynamic`

Provides dynamic query capabilities for non-generic `IQueryable`, with field names and values determined at runtime.

| Method | Description |
|---|---|
| `Where(string field, object value)` | Dynamic field filter |
| `OrderBy(string field, bool isAscending)` | Dynamic sort |
| `Select(string field)` | Dynamic projection |
| `Any()` | Whether any elements exist |
| `Any(string field, object value)` | Dynamic field existence check |
| `NotAny(string field, object value)` | Dynamic field non-existence check |
| `First()` / `FirstOrDefault()` | Get first element (dynamic return) |
| `ToDynamicList()` | Convert to `List<object>` (JSON serialization intermediate state) |

### IEnumerable Extensions

> Namespace: `System.Linq.Dynamic`

`EnumerableExtensions` (non-generic) and `EnumerableTExtensions` (generic) provide in-memory collection equivalents corresponding to Queryable, internally bridged via `AsQueryable()`.

| Method | Description |
|---|---|
| `Add(dynamic)` / `Remove(dynamic)` | Non-generic collection dynamic add/remove |
| `Where/OrderBy/Select/Any/First/FirstOrDefault` | In-memory collection equivalents of Queryable versions |
| `ToPagedList(page, pageSize)` | In-memory collection pagination |

### Tree Lists — `TreeListExtentions`

> Namespace: `System.Linq.Dynamic`

| Method | Description |
|---|---|
| `ToTreeList(idName, childListName, parentIdName, rootValue, maxLevel)` | String field name version |
| `ToTreeList(idExpr, childListExpr, parentIdExpr, rootValue, maxLevel)` | Lambda expression version |
| `ToTreeList(childListExpr, parentIdExpr, rootValue, maxLevel)` | Omit id (defaults to "Id") |

### Name Pair Extensions — `NamePairExtensions`

> Namespace: `System.Linq.Dynamic`

| Method | Description |
|---|---|
| `GetNamePairsByArr(params NamePair[])` | Map by name pair array |
| `GetNamePairs(params (string, string)[])` | Tuple mapping: `("Id", "Number")` |
| `GetNamePairs(params (Expression, string)[])` | Expression mapping: `(q => q.Id, "Number")` |

---

## Practical Examples

### Dynamic Condition Composition

```csharp
using System.Linq.Dynamic;

// Base condition
Expression<Func<Order, bool>> filter = o => o.Status != -1;

// Compose only if condition is true
filter = filter.AndIf(!string.IsNullOrEmpty(orderNo), o => o.OrderNo.Contains(orderNo));
filter = filter.AndIf(startDate.HasValue, o => o.CreateTime >= startDate.Value);
filter = filter.AndIf(endDate.HasValue, o => o.CreateTime <= endDate.Value);
filter = filter.OrIf(userIds != null && userIds.Length > 0, o => userIds.Contains(o.UserId));

var orders = dbContext.Orders.Where(filter).ToList();
```

### Dynamic Field Query (Field Names from Frontend/Configuration)

```csharp
using System.Linq.Dynamic;

// Frontend sends sort field and direction
string sortField = "CreateTime";
bool isDesc = true;

var result = dbContext.Products
    .AsQueryable()
    .Where("Status", 1)
    .OrderBy(sortField, isDesc)
    .ToList();

// Dynamic projection
var names = dbContext.Users
    .AsQueryable()
    .Select("Name")
    .ToList();
```

### Multi-Condition OR Query

```csharp
using System.Linq.Dynamic;

// Method 1: Multiple expressions OR
var result = dbContext.Users.Where(
    u => u.Role == "Admin",
    u => u.Role == "SuperAdmin",
    u => u.IsVip
);

// Method 2: Condition tuples
var result2 = dbContext.Users.Where(
    (hasKeyword, u => u.Name.Contains(keyword)),
    (hasRole, u => u.Role == role),
    (hasStatus, u => u.Status == status)
);
```

### Tree List Conversion

```csharp
using System.Linq.Dynamic;

// Method 1: String field names
var tree = menuList.ToTreeList(
    idName: "Id",
    childListName: "Children",
    parentIdName: "ParentId",
    rootValue: 0,
    maxLevel: 5);

// Method 2: Lambda expressions (compile-time safe)
var tree2 = menuList.ToTreeList(
    idExpression: m => m.Id,
    childListExpression: m => m.Children,
    parentIdExpression: m => m.ParentId,
    rootValue: 0);
```

### Paginated Query

```csharp
using System.Linq.Dynamic;

// IQueryable pagination
var paged = dbContext.Orders
    .Where(o => o.Status == 1)
    .OrderBy(o => o.CreateTime, isDesc: true)
    .ToPagedList(page: 1, pageSize: 20);

Console.WriteLine($"Total: {paged.Total}, Current Page: {paged.Page}");
foreach (var item in paged.Items)
{
    Console.WriteLine(item.OrderNo);
}

// IEnumerable also supported
var memPaged = memoryList.ToPagedList(page: 2, pageSize: 10);
```

### Name Pair Mapping

```csharp
using System.Linq.Dynamic;

var log = new ApiLogInfo { Id = 1, Url = "/api/test", Method = "GET" };

// Tuple method
var pairs = log.GetNamePairs(("Id", "Number"), ("Url", "Address"), ("Method", "Request Method"));

// Expression method (compile-time safe)
var pairs2 = log.GetNamePairs(
    (q => q.Id, "Number"),
    (q => q.Url, "Address"),
    (q => q.Method, "Request Method"));
```

---

## Usage Tips

1. **Unified namespace `System.Linq.Dynamic`**: All extension classes are under this single namespace, `using System.Linq.Dynamic;` one line does it all.
2. **Dynamic field name case sensitivity**: Non-generic `Where("name", ...)` is case-insensitive by default (`BindingFlags.IgnoreCase`); generic version is case-sensitive.
3. **`AndIf`/`OrIf` are lazy composition**: Returns the original expression directly when condition is false, no extra `&& true` generated.
4. **`ToTreeList` defaults to max 3 levels**: If tree structure exceeds 3 levels, remember to pass the `maxLevel` parameter.
5. **`ToDynamicList` uses JSON serialization**: Use cautiously in performance-sensitive scenarios, suitable for API dynamic data conversion.
6. **`ToPagedList` executes two queries**: One Count, one paginated fetch — pay attention to index optimization for large datasets.

---

## License

MIT License

---

**LuBan.Linq** — Making .NET queries more dynamic, flexible, and enjoyable.

> If you're tired of hand-writing dynamic queries, add it to your toolbox.
