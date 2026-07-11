[English](README.en.md) | 中文

# LuBan.Orm

> **作者**: yswenli | **联系邮箱**: yswenli@outlook.com | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> 一站式 ORM 解决方案，从建表到查询，从单库到多租户，一个库全搞定。

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.DI](../LuBan.DI/README.md) | [LuBan.Linq](../LuBan.Linq/README.md) | [LuBan.Service](../LuBan.Service/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md) | [LuBan.Redis](../LuBan.Redis/README.md)
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

// 使用仓储
public class ProductService : BaseService<Product>
{
    public async Task<ProductDto> GetByIdAsync(long id)
    {
        return await Repository.GetByIdAsync(id)
            .ConvertTo<ProductDto>();
    }

    public async Task<PageResult<ProductDto>> GetPagedAsync(int page, int size)
    {
        return await Repository.AsQueryable()
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreateTime)
            .ToPagedList<ProductDto>(page, size);
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

```xml
<PackageReference Include="LuBan.Orm" Version="*" />
```

## 功能总览

### 数据库支持

SqlServer、MySQL、Oracle、PostgreSQL、SQLite、DM（达梦）、Kdbndp（人大金仓）、Oscar、OpenGauss、QuestDB、HG、ClickHouse、GBase — **13 种数据库即插即用**。

### 核心能力

| 分类 | 功能 |
|------|------|
| 初始化 | `InitDb` 数据库创建、`InitTables` CodeFirst 自动建表、`InitViews` 视图初始化、`InitSeeds` 种子数据填充 |
| 多租户 | 主库 / 日志库 / 租户库三级隔离，`[SysTable]`、`[LogTable]`、`[Tenant]` 属性路由 |
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
- **10 个自定义属性** — 表路由、日志标记、租户标记等
- **7 个接口** — 仓储、工作单元、映射等核心契约
- **17 个枚举** — 业务状态、数据类型等常用枚举

## 使用指南

### 1. 初始化 ORM

```csharp
// 初始化数据库与表结构
LuBanOrm.InitDb();
LuBanOrm.InitTables();   // CodeFirst 自动建表
LuBanOrm.InitViews();    // 初始化视图
LuBanOrm.InitSeeds();    // 填充种子数据
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

// 租户表（租户库）
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
        return await _repo.GetByIdAsync(id)
            .ConvertTo<ProductDto>();
    }

    // 软删除
    public async Task DeleteAsync(long id)
    {
        await _repo.LogicDeleteAsync(id);
    }

    // 分页查询
    public async Task<PageResult<ProductDto>> GetPagedAsync(int page, int size, string? keyword)
    {
        var query = _repo.AsQueryable();
        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(p => p.Name.Contains(keyword));

        return await query
            .OrderByDescending(p => p.CreateTime)
            .ToPagedList<ProductDto>(page, size);
    }

    // 原生 SQL
    public async Task<List<ProductDto>> SearchAsync(string keyword)
    {
        return await _repo.AsQueryable()
            .Where(p => p.Name.Contains(keyword))
            .ConvertTo<List<ProductDto>>();
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

// 方式二：DbTran 手动控制
using var tran = new DbTran();
try
{
    await repo1.InsertAsync(entity1);
    await repo2.InsertAsync(entity2);
    tran.Commit();
}
catch
{
    tran.Rollback();
    throw;
}
```

### 5. 对象映射

```csharp
// 注册映射
services.AddObjectMapper();

// 转换
ProductDto dto = product.ConvertTo<ProductDto>();

// 填充（从源对象填充目标对象属性）
dto.FillFrom<Product, ProductDto>(product);
```

### 6. 代码生成

```csharp
// DBFirst：从数据库表生成实体类
CodeGenerator.GenerateEntities(connectionString);

// 生成种子数据
CodeGenerator.GenerateSeeds<Product>();
```

## 小贴士

- 使用 `[SysTable]`、`[LogTable]`、`[Tenant]` 属性控制表的路由目标库，ORM 自动处理连接切换
- 继承 `EntityBase` 自动获得雪花 ID、审计字段、软删除能力，无需手动维护
- 全局查询过滤器自动生效，软删除数据、非当前租户数据自动隔离
- `ConvertTo<T>()` 高性能对象映射，性能远超 AutoMapper
- 跨库事务使用 `UnitOfWork` 特性最简洁，复杂场景可用 `DbTran` 精细控制
- 内置 27 个系统实体可直接使用或继承扩展

## 许可证

MIT
