[English](README.en.md) | 中文

# LuBan.Orm

> **作者**: yswenli | **联系邮箱**: yswenli@outlook.com | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> 一站式 ORM 解决方案，从建表到查询，从单库到多租户，一个库全搞定。

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.DI](../LuBan.DI/README.md) | [LuBan.Linq](../LuBan.Linq/README.md) | [LuBan.Service](../LuBan.Service/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md) | [LuBan.Redis](../LuBan.Redis/README.md) | [LuBan.Logging](../LuBan.Logging/README.md)
---

## 为什么需要它？

- 每次新项目都要重复？
- 多租户表路由、软删除、审计字段填充写到手软？
- CodeFirst / DBFirst 切换麻烦，实体生成全靠手写？
- 跨库事务、分页查询、树形结构查询写一遍忘一遍？

LuBan.Orm 深度封装，提供开箱即用的多数据库支持、多租户隔离、CodeFirst 自动建表、雪花 ID 与审计字段自动填充、泛型仓储、工作单元、对象映射、代码生成等完整 ORM 能力。

## 快速预览

```csharp
// 定义实体
[SysTable]
public class Product : EntityBase
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// 使用仓储（BaseRepository 来自 LuBan.Orm；BaseService<T> 在 LuBan.Service，不提供 Repository 属性）
public class ProductService
{
    private readonly BaseRepository<Product> _repo;

    public ProductService(BaseRepository<Product> repo) => _repo = repo;

    public async Task<ProductDto> GetByIdAsync(long id)
    {
        var product = await _repo.GetByIdAsync(id);
        return product.ConvertTo<ProductDto>();
    }

    public async Task<PagedList<ProductDto>> GetPagedAsync(int page, int size)
    {
        return await _repo.AsQueryable()
            .Where(p => !p.IsDelete)
            .OrderByDescending(p => p.CreateTime)
            .ToPagedListAsync<Product, ProductDto>(page, size,
                p => new ProductDto { Id = p.Id, Name = p.Name });
    }
}
```

## 技术栈

| 组件 | 说明 |
|------|------|
| LuBan.Common | 基础接口与实体定义 |
| LuBan.DI | 依赖注入扩展 |
| LuBan.Linq | LINQ 增强工具 |

## 安装

```bash
dotnet add package LuBan.Orm
```

## 功能总览

### 数据库支持

SqlServer、MySQL、Oracle、PostgreSQL、SQLite、DM（达梦）、Kdbndp（人大金仓）、Oscar、OpenGauss、QuestDB、HG、ClickHouse、GBase 共 13 种。

> 具体数据库支持取决于 SqlSugar 驱动；`LuBanOrm` 只对 Oracle / DM / SqlServer（不支持建库）、MySql（排序规则）、Kdbndp（PG 模式）做显式分支处理。

### 核心能力

| 分类 | 功能 |
|------|------|
| 初始化 | `LuBanOrm.Init()` 统一入口：建库、CodeFirst 自动建表、视图初始化、种子数据填充（按 `DbSettings`/`TableSettings`/`SeedSettings` 开关执行） |
| 多租户 | 主库 / 日志库 / 租户库三级隔离，`[SysTable]`、`[LogTable]` 路由（`[Tenant]` 为 SqlSugar 提供），按 configId 连接切换 |
| 全局过滤 | 软删除过滤、租户隔离过滤、组织机构过滤，全局自动生效 |
| 自动填充 | 雪花 ID 自动生成、创建/修改时间、创建/修改人等审计字段自动赋值 |
| SQL 日志 | 彩色 SQL 输出、差异日志记录 |

### 仓储层

| 功能 | 说明 |
|------|------|
| CRUD | 增删改查、批量操作 |
| 查询映射 | 自动 DTO 映射，`ConvertTo<T>()`、`FillFrom<S,D>()` |
| 软删除 | `LogicDeleteAsync()` 逻辑删除，全局过滤自动生效 |
| 联表查询 | Join、导航属性、子查询 |
| 树形结构 | `ToPagedTreeList` 树形分页 |
| 原生 SQL | 参数化原生 SQL、存储过程调用 |
| 事务 | 单库事务、跨库事务（`UnitOfWork` / `DbTran`） |
| 分页 | `ToPagedList` 通用分页、`ToPagedTreeList` 树形分页 |

### 实体体系

```
EntityBaseId          → 仅含主键
  └─ EntityBase       → 主键 + 审计字段（创建时间/修改时间/创建人/修改人/软删除）
       ├─ EntityTenant       → + 租户 ID
       └─ EntityDataScoreBase → + 数据评分字段

EntityTenantId        → 独立租户标识实体
```

### 内置资源

- **27 个内置实体** — DbUser、DbRole、DbMenu、DbOrg、DbConfig、DbTenant 等系统基础实体
- **10 个自定义特性** — 表路由（`[SysTable]`/`[LogTable]`）、增量建表/种子、忽略表、数据归属、`[UnitOfWork]` 等（租户标记 `[Tenant]` 由 SqlSugar 提供，不在此列）
- **7 个接口** — 仓储、工作单元、映射等核心契约
- **20 个枚举** — 业务状态、数据类型等常用枚举

### 日志收集

| 组件 | 说明 |
|------|------|
| `LoggerCollector` | 订阅 `Logger.OnCalled`/`OnError` 事件，将日志批量写入数据库 |
| `DbLogCleaner` | 定时清理过期的数据库日志记录 |
| `DbLogError` | 错误日志实体（异常类型、堆栈、请求信息） |
| `DbLogApi` | API 调用日志实体（TraceId、URL、请求/响应、耗时） |

> 日志收集器通过订阅 `LuBan.Common.Logger` 的静态事件实现，启动时调用 `LoggerCollector.Instance.Start()` 即可自动收集日志到数据库；`LuBanLoggingOptions.EnableDb` 为 `false`（默认）时 `Start()` 直接返回，不收集。

## 使用指南

### 1. 初始化 ORM

```csharp
// 初始化 ORM（建库 + CodeFirst 建表 + 视图 + 种子数据，均受配置开关控制）
LuBanOrm.Init();

// 需要自定义 SqlSugarClient 时传入配置回调
LuBanOrm.Init(client => { /* Aop、过滤器等自定义配置 */ });
```

### 2. 定义实体

```csharp
// 系统表（主库）
[SysTable]
public class Product : EntityBase
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}

// 租户表（租户库）—— [Tenant] 由 SqlSugar 提供
[Tenant]
public class Order : EntityTenant
{
    public string OrderNo { get; set; }
    public decimal Amount { get; set; }
}

// 日志表（日志库）
[LogTable]
public class OperationLog : EntityBase
{
    public string Action { get; set; }
    public string Detail { get; set; }
}
```

### 3. 使用仓储

```csharp
public class ProductService
{
    private readonly BaseRepository<Product> _repo;

    public ProductService(BaseRepository<Product> repo)
    {
        _repo = repo;
    }

    // 新增（自动填充雪花 ID、创建时间等）
    public async Task<long> CreateAsync(Product product)
    {
        await _repo.InsertAsync(product);
        return product.Id;
    }

    // 查询并映射为 DTO
    public async Task<ProductDto> GetDtoAsync(long id)
    {
        var product = await _repo.GetByIdAsync(id);
        return product.ConvertTo<ProductDto>();
    }

    // 软删除（仓储的 LogicDeleteAsync 接收实体、条件表达式或实体集合，没有 id 重载）
    public async Task DeleteAsync(long id)
    {
        await _repo.LogicDeleteAsync(p => p.Id == id);
    }

    // 分页查询
    public async Task<PagedList<ProductDto>> GetPagedAsync(int page, int size, string? keyword)
    {
        var query = _repo.AsQueryable();
        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(p => p.Name.Contains(keyword));

        return await query
            .OrderByDescending(p => p.CreateTime)
            .ToPagedListAsync<Product, ProductDto>(page, size,
                p => new ProductDto { Id = p.Id, Name = p.Name });
    }

    // 条件查询后转换
    public async Task<List<ProductDto>> SearchAsync(string keyword)
    {
        var list = await _repo.AsQueryable()
            .Where(p => p.Name.Contains(keyword))
            .ToListAsync();
        return list.ConvertTo<List<ProductDto>>();
    }
}
```

### 4. 事务管理

```csharp
// 方式一：UnitOfWork 特性（Action Filter）
[UnitOfWork]
public async Task TransferAsync(decimal amount, long fromId, long toId)
{
    await _fromRepo.UpdateAsync(...);
    await _toRepo.UpdateAsync(...);
    // 方法结束自动提交，异常自动回滚
}

// 方式二：DbTran 手动控制（构造需传 ITenant 或 ISqlSugarClient）
var tenant = LuBanOrm.SqlSugarScope.AsTenant();
using (var tran = new DbTran(tenant))
{
    await repo1.InsertAsync(entity1);
    await repo2.InsertAsync(entity2);
    tran.Commit();
}
// 中途异常无需手动回滚：未 Commit 就 Dispose 时 DbTran 自动回滚，异常照常向外抛
```

### 5. 对象映射

```csharp
// 注册映射
services.AddObjectMapper();

// 转换
ProductDto dto = product.ConvertTo<ProductDto>();

// 填充（SModel 为填充落点、DModel 为数据来源，返回 SModel）
product.FillFrom<Product, ProductDto>(dto);
```

### 6. 代码生成

```csharp
// DBFirst：把库中所有表生成为实体类文件，返回输出目录
var dir = CodeGenerator.GetEntityCSCodeFilePath();

// 只取单张表的实体代码（返回字符串，不落盘）
var code = CodeGenerator.GetEntityCSCode("DbUser");

// 生成种子数据代码（也可传条件表达式或 ISugarQueryable）
var seed = CodeGenerator.GetSeedDataCSCode(new List<DbUser> { /* ... */ });
```

## 小贴士

- 使用 `[SysTable]`、`[LogTable]` 控制表的路由目标库（`[Tenant]` 由 SqlSugar 提供，用于租户分库），ORM 自动处理连接切换
- 继承 `EntityBase` 自动获得雪花 ID、审计字段、软删除能力，无需手动维护
- 全局查询过滤器自动生效，软删除数据、非当前租户数据自动隔离
- `ConvertTo<T>()` 基于 Mapster，一行完成实体与 DTO 的相互转换
- 跨库事务使用 `UnitOfWork` 特性最简洁，复杂场景可用 `DbTran` 精细控制
- 内置 27 个系统实体可直接使用或继承扩展

## 许可证

Copyright (c) yswenli.

Licensed under the [Apache License 2.0](../LICENSE).
