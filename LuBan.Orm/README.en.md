[中文](README.md) | English

# LuBan.Orm

> **Author**: yswenli | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> An all-in-one ORM solution — from table creation to queries, from single database to multi-tenancy, all in one package.

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.DI](../LuBan.DI/README.md) | [LuBan.Linq](../LuBan.Linq/README.md) | [LuBan.Service](../LuBan.Service/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md) | [LuBan.Redis](../LuBan.Redis/README.md)
---

## Why Do You Need It?

- Repeatedly providing ORM for every new project?
- Tired of manually implementing multi-tenant table routing, soft delete, and audit field population?
- Switching between CodeFirst / DBFirst is a hassle, entity generation is all manual?
- Cross-database transactions, paged queries, tree structure queries — write once, forget immediately?

LuBan.Orm is a deep wrapper around ORM, providing out-of-the-box multi-database support, multi-tenant isolation, CodeFirst auto table creation, snowflake ID and audit field auto-population, generic repositories, unit of work, object mapping, code generation, and complete ORM capabilities.

## Quick Preview

```csharp
// Define entity
[SysTable]
public class Product : EntityBase
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Use repository
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

## Tech Stack

| Component | Description |
|-----------|-------------|
| LuBan.Common | Base interfaces and entity definitions |
| LuBan.DI | Dependency injection extensions |
| LuBan.Linq | LINQ enhancement utilities |

## Installation

```xml
<PackageReference Include="LuBan.Orm" Version="*" />
```

## Feature Overview

### Database Support

SqlServer, MySQL, Oracle, PostgreSQL, SQLite, DM (Dameng), Kdbndp (KingbaseES), Oscar, OpenGauss, QuestDB, HG, ClickHouse, GBase — **13 databases, plug and play**.

### Core Capabilities

| Category | Features |
|----------|----------|
| Initialization | `InitDb` database creation, `InitTables` CodeFirst auto table creation, `InitViews` view initialization, `InitSeeds` seed data population |
| Multi-Tenancy | Primary DB / Log DB / Tenant DB three-tier isolation, `[SysTable]`, `[LogTable]`, `[Tenant]` attribute routing |
| Global Filters | Soft delete filter, tenant isolation filter, organization filter — all globally effective |
| Auto Population | Snowflake ID auto-generation, create/modify time, create/modify user audit fields auto-populated |
| SQL Logging | Colorized SQL output, differential logging |

### Repository Layer

| Feature | Description |
|---------|-------------|
| CRUD | Insert, delete, update, query, batch operations |
| Query Mapping | Auto DTO mapping, `ConvertTo<T>()`, `FillFrom<S,D>()` |
| Soft Delete | `LogicDeleteAsync()` logical delete, global filter auto-effective |
| Join Queries | Join, navigation properties, subqueries |
| Tree Structure | `ToPagedTreeList` tree-structured pagination |
| Raw SQL | Parameterized raw SQL, stored procedure calls |
| Transactions | Single-DB transactions, cross-DB transactions (`UnitOfWork` / `DbTran`) |
| Pagination | `ToPagedList` general pagination, `ToPagedTreeList` tree pagination |

### Entity Hierarchy

```
EntityBaseId          → Primary key only
  └─ EntityBase       → Primary key + audit fields (create time/modify time/creator/modifier/soft delete)
        ├─ EntityTenant       → + Tenant ID
        └─ EntityDataScoreBase → + Data scoring fields

EntityTenantId        → Standalone tenant identification entity
```

### Built-in Resources

- **27 built-in entities** — DbUser, DbRole, DbMenu, DbOrg, DbConfig, DbTenant and other system base entities
- **10 custom attributes** — Table routing, log marking, tenant marking, etc.
- **7 interfaces** — Repository, unit of work, mapping and other core contracts
- **17 enums** — Business status, data types and other commonly used enums

## Usage Guide

### 1. Initialize ORM

```csharp
// Initialize database and table structure
LuBanOrm.InitDb();
LuBanOrm.InitTables();   // CodeFirst auto table creation
LuBanOrm.InitViews();    // Initialize views
LuBanOrm.InitSeeds();    // Populate seed data
```

### 2. Define Entities

```csharp
// System table (primary DB)
[SysTable]
public class Product : EntityBase
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}

// Tenant table (tenant DB)
[Tenant]
public class Order : EntityTenant
{
    public string OrderNo { get; set; }
    public decimal Amount { get; set; }
}

// Log table (log DB)
[LogTable]
public class OperationLog : EntityBase
{
    public string Action { get; set; }
    public string Detail { get; set; }
}
```

### 3. Use Repository

```csharp
public class ProductService
{
    private readonly BaseRepository<Product> _repo;

    public ProductService(BaseRepository<Product> repo)
    {
        _repo = repo;
    }

    // Insert (auto-populates snowflake ID, create time, etc.)
    public async Task<long> CreateAsync(Product product)
    {
        await _repo.InsertAsync(product);
        return product.Id;
    }

    // Query and map to DTO
    public async Task<ProductDto> GetDtoAsync(long id)
    {
        return await _repo.GetByIdAsync(id)
            .ConvertTo<ProductDto>();
    }

    // Soft delete
    public async Task DeleteAsync(long id)
    {
        await _repo.LogicDeleteAsync(id);
    }

    // Paged query
    public async Task<PageResult<ProductDto>> GetPagedAsync(int page, int size, string? keyword)
    {
        var query = _repo.AsQueryable();
        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(p => p.Name.Contains(keyword));

        return await query
            .OrderByDescending(p => p.CreateTime)
            .ToPagedList<ProductDto>(page, size);
    }

    // Raw SQL
    public async Task<List<ProductDto>> SearchAsync(string keyword)
    {
        return await _repo.AsQueryable()
            .Where(p => p.Name.Contains(keyword))
            .ConvertTo<List<ProductDto>>();
    }
}
```

### 4. Transaction Management

```csharp
// Approach 1: UnitOfWork attribute (Action Filter)
[UnitOfWork]
public async Task TransferAsync(decimal amount, long fromId, long toId)
{
    await _fromRepo.UpdateAsync(...);
    await _toRepo.UpdateAsync(...);
    // Auto-commits when method completes, auto-rolls back on exception
}

// Approach 2: DbTran manual control
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

### 5. Object Mapping

```csharp
// Register mapper
services.AddObjectMapper();

// Convert
ProductDto dto = product.ConvertTo<ProductDto>();

// Fill (populate target object properties from source)
dto.FillFrom<Product, ProductDto>(product);
```

### 6. Code Generation

```csharp
// DBFirst: Generate entity classes from database tables
CodeGenerator.GenerateEntities(connectionString);

// Generate seed data
CodeGenerator.GenerateSeeds<Product>();
```

## Tips

- Use `[SysTable]`, `[LogTable]`, `[Tenant]` attributes to control table routing target databases; ORM handles connection switching automatically
- Inheriting `EntityBase` automatically provides snowflake ID, audit fields, and soft delete capabilities — no manual maintenance needed
- Global query filters take effect automatically; soft-deleted data and non-current tenant data are isolated automatically
- `ConvertTo<T>()` provides high-performance object mapping, performing far better than AutoMapper
- Cross-DB transactions are simplest with the `UnitOfWork` attribute; use `DbTran` for fine-grained control in complex scenarios
- 27 built-in system entities can be used directly or extended through inheritance

## License

MIT
