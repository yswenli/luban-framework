# JSON 紧凑格式实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 将日志和 API 返回的 JSON 格式从缩进改为紧凑格式

**Architecture:** 直接修改两处 JsonSerializerOptions 的 WriteIndented 配置，无需新增文件或类

**Tech Stack:** System.Text.Json, ASP.NET Core

---

### Task 1: 修改日志序列化配置

**Files:**
- Modify: `LuBan.Logging\Serialization\LuBanJsonSerializer.cs:15`

- [ ] **Step 1: 修改 LuBanJsonSerializer 的 WriteIndented 配置**

打开 `LuBan.Logging\Serialization\LuBanJsonSerializer.cs`，将第 15 行的：

```csharp
WriteIndented = true,
```

改为：

```csharp
WriteIndented = false,
```

同时更新第 8 行的注释，将：

```csharp
/// 输出格式与 SerializeUtil.Serialize(obj, indented:true, defalutVal:false, nullValue:true, camelCase:true) 一致。
```

改为：

```csharp
/// 输出格式与 SerializeUtil.Serialize(obj, indented:false, defalutVal:false, nullValue:true, camelCase:true) 一致。
```

---

### Task 2: 修改 API 返回配置

**Files:**
- Modify: `LuBan.Web.Core\AspNetCore\ApiConfiguration.cs:60`

- [ ] **Step 1: 修改 ApiConfiguration 的 WriteIndented 配置**

打开 `LuBan.Web.Core\AspNetCore\ApiConfiguration.cs`，将第 60 行的：

```csharp
//格式化输出内容
options.JsonSerializerOptions.WriteIndented = true;
```

改为：

```csharp
//紧凑输出内容
options.JsonSerializerOptions.WriteIndented = false;
```

---

### Task 3: 验证修改

- [ ] **Step 1: 编译项目**

```bash
dotnet build
```

Expected: Build succeeded

- [ ] **Step 2: 提交修改**

```bash
git add LuBan.Logging/Serialization/LuBanJsonSerializer.cs LuBan.Web.Core/AspNetCore/ApiConfiguration.cs
git commit -m "refactor: change JSON serialization to compact format"
```

Expected: Commit created successfully