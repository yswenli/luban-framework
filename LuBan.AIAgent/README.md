[English](README.en.md) | 中文

# LuBan.AIAgent

> **作者**: yswenli | **联系邮箱**: yswenli@outlook.com | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> 基于 Microsoft Agent Framework 的 AI Agent 库，让大模型具备思考、规划、调用工具和自主执行的能力。

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.DI](../LuBan.DI/README.md) | [LuBan.AIFlow](../LuBan.AIFlow/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## 为什么需要它？

- 想让 LLM 调用工具完成任务，但 MCP / Function Calling 的实现细节令人头疼？
- Skill 管理、工具注册、会话持久化各自需要单独实现，维护成本高？
- 模型 Provider 切换困难——从 Provider A 换到 Provider B 需要重写大量代码？
- 缺少中间件机制——日志、策略控制、权限拦截难以扩展？

LuBan.AIAgent 提供完整的 AI Agent 基础设施，从 Agent 运行时、多模型路由、技能系统、工具系统、会话存储到中间件管道——开箱即用。

## 快速预览

```csharp
// 注册服务
services.AddSingleton<IChatClient>(sp => CreateChatClient());
services.AddLuBanAgent(configuration);

// 创建 Agent
var factory = serviceProvider.GetRequiredService<ILuBanAgentFactory>();
var agent = await factory.CreateAsync(
    systemPrompt: "你是一个浏览器自动化助手",
    toolGroups: new[] { "browser" });

// 执行任务
var response = await agent.RunAsync("打开百度并搜索 LuBan Framework");
Console.WriteLine(response.Text);
```

## 技术栈

| 组件 | 说明 |
|------|------|
| Microsoft.Agents.AI.Foundry | Agent 运行时框架 |
| Microsoft.Extensions.AI | 统一聊天客户端抽象 |
| Microsoft.Playwright | 浏览器自动化引擎 |
| LuBan.DI | 依赖注入集成 |
| LuBan.Common | 基础接口与工具定义 |

## 安装

```bash
dotnet add package LuBan.AIAgent
```

安装 Playwright 浏览器（使用浏览器工具时需要）：

```powershell
npx playwright@1.61.0 install chromium
```

## 功能总览

### 核心引擎

| 组件 | 说明 |
|------|------|
| `LuBanAgent` | Agent 实例，封装 ChatClientAgent，支持同步/流式运行 |
| `ILuBanAgentFactory` / `LuBanAgentFactory` | Agent 工厂，按配置创建 Agent 并注入工具 |
| `IAppConfigReader` | 应用配置只读接口 |
| `IProviderRouter` | Provider 路由接口 |
| `TextUtils` | 文本处理工具 |
| `WildcardMatcher` | 通配符匹配 |
| `SkillMdParser` | SKILL.md 解析器 |

### 组件注册表架构

Skills、MCPs、Rules 采用统一的三级优先级注册表模式：

| 优先级 | 来源 | 行为 |
|--------|------|------|
| 最高 | 硬编码（DI 注册） | 始终存在，可通过 `DisabledBuiltin` 配置禁用 |
| 中 | 工作区文件 | 添加新项，同名项被忽略 |
| 最低 | config.json | 添加新项，同名项被忽略 |

**加载时机**：
- 启动时：加载硬编码组件 + config.json 全局配置
- 工作区切换时：加载工作区文件，自动合并

**工作区目录结构**：
```
.luban-agent/
├── skills/          # 工作区级 Skill
│   └── my-skill/
│       └── SKILL.md
├── mcps/            # 工作区级 MCP 服务器
│   └── my-mcp.json
└── rules/           # 工作区级规则
    └── my-rule.json
```

### 工具系统

| 组件 | 说明 |
|------|------|
| `ILuBanToolPlugin` | 工具插件接口，定义工具分组和提供工具函数 |
| `ToolPluginRegistry` | 工具插件注册表，管理插件的启用/禁用和分组筛选 |
| `ToolAttribute` | 工具标注特性 |

### 内置工具

| 工具组 | 分组名 | 核心能力 |
|--------|--------|----------|
| **浏览器工具** | `browser` | 导航、点击、输入、截图、获取内容、等待元素、获取 URL（基于 Playwright） |
| **文件系统工具** | `filesystem` | 读取文件、写入文件、列出目录、删除文件、删除目录、搜索文件（glob）、内容搜索（regex）、创建目录、复制文件、移动文件、获取文件信息，支持 AllowedRoots 安全限制 |
| **脚本执行工具** | `script` | 执行 Shell、Lua、Python 脚本 |
| **Web 工具** | `web` | 发送 HTTP 请求获取网页内容 |
| **语义检索工具** | `retrieval` | 索引本地代码/文档，按语义搜索相关片段 |
| **上下文压缩工具** | `context` | 压缩当前会话的对话历史，释放 token 预算（LLM 可见、可主动调用） |
| **本地记忆工具** | `localmemory` | 长期记忆的存储、查询和管理 |

### Skill 系统

| 组件 | 说明 |
|------|------|
| `ISkill` | Skill 接口定义，包含 `PromptTemplate` 属性用于对话内激活 |
| `SkillBase` | Skill 基类，提供日志、状态更新、Agent 调用等通用功能 |
| `SkillRegistry` | Skill 注册表，管理内置、文件级和自定义 Skill |
| `SkillLoader` | Skill 文件加载器，从 SKILL.md 文件加载 Skill 定义 |
| `FileSkill` | 文件级 Skill 适配器，将 SKILL.md 文件包装为 ISkill |
| `CustomSkill` | 自定义 Skill 适配器，将 CustomSkillConfig 包装为 ISkill |

### 内置 Skill

| Skill ID | 名称 | 分类 | 说明 |
|----------|------|------|------|
| `brainstorming` | 头脑风暴 | creative | 实现功能前探索需求和设计 |
| `code-review` | 代码审查 | development | 审查代码、发现问题、提供改进建议 |
| `documentation` | 文档生成 | productivity | 生成代码注释、README、API 文档等 |
| `code-refactor` | 代码重构 | development | 重构代码，提升代码质量 |
| `test-generation` | 测试生成 | development | 自动生成单元测试 |
| `code-explain` | 代码解释 | development | 解释复杂代码逻辑 |
| `debug-assistant` | 调试助手 | development | 辅助调试问题 |
| `git-commit` | Git 提交 | productivity | 生成规范的 Git 提交信息 |
| `find-skills` | 技能发现 | meta | 自动发现和推荐合适的技能 |
| `agents-md-generator` | Agents.md 规约生成器 | productivity | 生成工作区 AGENTS.md 元描述文档 |

### 文件化 Skill

支持通过 SKILL.md 文件定义自定义 Skill，兼容 OpenCode 格式：

```markdown
---
name: my-skill
description: "技能描述"
category: custom
---

# Skill 指令内容

这里是 Skill 的提示词模板...
```

**存储位置**（按优先级）：
- 项目级: `<workspace>/.luban-agent/skills/<skill-id>/SKILL.md`
- 用户级: `%LocalAppData%/LuBan/AIAgent/skills/<skill-id>/SKILL.md`

**优先级**：硬编码（DI）> 工作区文件 > config.json

### Rule 系统

| 组件 | 说明 |
|------|------|
| `IRule` | 规则接口，定义执行条件和行为 |
| `RuleBase` | 规则基类 |
| `RuleEngine` | 规则引擎，按优先级评估规则 |
| `ContextInjectBuilder` | 上下文注入构建器，把规则引擎产出的 `Inject` 文本装配为可注入对话/系统提示词的上下文 |
| `PathAccessRule` | 内置路径访问规则，限制文件系统访问范围 |

### MCP 系统

| 组件 | 说明 |
|------|------|
| `IMCPClient` | MCP 客户端接口，与 MCP 服务器交互 |
| `StdioMCPClient` | 基于 stdio JSON-RPC 的外部 MCP 客户端 |
| `MCPRegistry` | MCP 注册表，管理内置和外部客户端 |
| `MCPToolPlugin` | MCP 工具插件，将 MCP 工具暴露给 Agent |
| `FileSystemMCPClient` | 内置文件系统 MCP 客户端 |

### 会话系统

| 组件 | 说明 |
|------|------|
| `ISessionManager` | 会话管理接口，支持创建、切换、清除会话 |
| `SessionChatHistoryProvider` | 会话历史提供者，自动持久化对话历史 |
| `SessionOptions` | 会话配置，支持压缩阈值设置 |

### 规则拦截

| 组件 | 说明 |
|------|------|
| `RuleCheckedAIFunction` | 规则检查装饰器，工具执行前自动拦截检查 |
| `CustomRule` | 自定义规则适配器，支持通配符匹配 |

### 安全与确认

| 组件 | 说明 |
|------|------|
| `ToolConfirmationService` | 工具执行确认服务，危险操作前要求用户确认 |
| `PathGuard` | 路径安全守卫，防止越权访问 |
| `RuleEngine` | 规则引擎，工具执行前进行权限检查和参数修改 |

### 多 Agent 编排系统

主 Agent 解析复合任务 → 拆解 DAG 任务图谱 → 分发 SubAgent 执行（串行 / 并行混合编排）。

| 组件 | 说明 |
|------|------|
| `IOrchestrator` / `Orchestrator` | 编排器入口，串联规划、调度与结果聚合 |
| `ITaskPlanner` | 任务规划器接口，将自然语言任务转换为 TaskGraph |
| `LlmTaskPlanner` | 基于 LLM 的规划器，通过提示词引导模型生成 DAG |
| `TemplateTaskPlanner` | 基于模板匹配的规划器，命中预定义模板时快速生成图谱 |
| `CompositeTaskPlanner` | 组合式规划器，模板优先匹配，未命中回退到 LLM |
| `DagScheduler` | DAG 调度器，基于拓扑分层实现同层并行、跨层串行 |
| `SubAgentFactory` | SubAgent 工厂，封装 LuBanAgentFactory 的子 Agent 创建 |
| `SubAgentRoleRegistry` | SubAgent 角色注册表，管理内置角色与自定义角色 |
| `SubAgentRole` | SubAgent 角色定义，包含名称、系统提示词模板、默认工具组 |
| `ContextStore` | 跨节点上下文存储，按图谱 ID 隔离，线程安全 |
| `TaskGraph` / `TaskNode` | DAG 数据模型，支持依赖声明、占位符引用、关键节点、角色指定；`TaskGraph.SharedContext` 承载工作区记忆/规则上下文 |
| `SubAgentSpec` | SubAgent 规格（提示词、工具组、工作区根等），其 `SharedContext` 会追加到子代理系统提示词 |
| `OrchestrationToolPlugin` | 工具插件，将编排能力暴露给主 Agent 自动调用 |
| `OrchestrationProgress` / `OrchestrationProgressContent` | 编排进度事件与流式内容载体（`AIContent`），供 UI 在规划/节点执行期间实时渲染 |
| `ReflectionResult` / `ReplanContext` | 动态重规划数据模型，关键节点失败后 LLM 分析并生成修正图谱 |

## 使用指南

### 1. 配置与注册

```json
{
  "LuBanAgent": {
    "DefaultModel": "openai:gpt-4o",
    "SystemPrompt": "你是一个智能助手。",
    "MaxToolLoopIterations": 10,
    "Session": {
      "CompactTargetMessages": 20,
      "CompactThreshold": 10
    }
  }
}
```

```csharp
// 注册服务
services.AddSingleton<IAppConfigReader>(myConfigManager);
services.AddSingleton<IProviderRouter>(myProviderRouter);
services.AddLuBanAgent(configuration);
```

### 2. 多模型路由

```csharp
// 使用 provider:model 格式路由到不同模型
// IProviderRouter 根据 ModelId 中的 provider 前缀自动分发
var agent = await factory.CreateAsync(modelName: "qwen:qwen-plus");

// 切换 Provider 只需更改前缀
var agent2 = await factory.CreateAsync(modelName: "openai:gpt-4o");
```

### 3. 工具注册与使用

```csharp
// 创建 Agent 时指定工具组
var agent = await factory.CreateAsync(
    toolGroups: new[] { "browser", "filesystem" });

// Agent 自动选择并调用工具
var response = await agent.RunAsync("列出 src 目录下所有 .cs 文件并统计代码行数");

// 流式运行
await foreach (var update in agent.RunStreamingAsync("帮我分析这段代码"))
{
    Console.Write(update.Text);
}
```

### 4. Skill 管理

```csharp
// 获取 Skill 注册表
var skillRegistry = serviceProvider.GetRequiredService<SkillRegistry>();

// 列出所有 Skill
var skills = skillRegistry.GetAll();

// 执行 Skill
var context = new SkillContext
{
    Agent = agent,
    UpdateStatus = status => Console.WriteLine($"状态: {status}")
};
var result = await skillRegistry.Get("brainstorming")
    .ExecuteAsync(context, "我想实现一个用户登录功能");
```

### 5. 自定义工具插件

```csharp
public class MyToolPlugin : ILuBanToolPlugin
{
    public string GroupName => "my-tools";
    public string? Description => "自定义工具集";

    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp, ToolGroupOptions? toolsOptions = null)
    {
        // 返回自定义工具函数
        return new List<AIFunction> { /* ... */ };
    }

    public bool IsEnabled(LuBanAgentOptions options) => true;
}

// 注册
services.AddSingleton<ILuBanToolPlugin, MyToolPlugin>();
```

### 6. 自定义 Skill

**方式一：文件化 Skill（推荐）**

在项目级或用户级目录创建 `SKILL.md` 文件：

```bash
# 项目级目录
<workspace>/.luban-agent/skills/my-skill/SKILL.md

# 用户级目录
%LocalAppData%/LuBan/AIAgent/skills/my-skill/SKILL.md
```

SKILL.md 格式：

```markdown
---
name: my-translator
description: "将文本翻译成英文"
category: custom
---

# 翻译助手

请将用户提供的内容翻译成英文。

## 要求
- 保持原文的语气和风格
- 使用地道的英文表达
```

**方式二：代码定义 Skill**

```csharp
public class MyCustomSkill : SkillBase
{
    public override string Id => "my-custom-skill";
    public override string Name => "我的自定义 Skill";
    public override string Description => "自定义 Skill 示例";
    public override string Category => "custom";
    public override string? PromptTemplate => "自定义提示词模板...";

    public override async Task<SkillResult> ExecuteAsync(SkillContext context, string input)
    {
        UpdateStatus(context, "正在处理...");
        var result = await CallAgentAsync(context, input);
        return SkillResult.Ok(result ?? "");
    }
}

// 注册
services.AddSingleton<ISkill, MyCustomSkill>();
```

### 7. 自定义规则

```csharp
public class MyRule : RuleBase
{
    public override string Id => "my-rule";
    public override string Name => "我的规则";
    public override int Priority => 50;

    public override bool IsApplicable(RuleContext context)
        => context.ActionType == "file-write";

    public override Task<RuleResult> ExecuteAsync(RuleContext context)
    {
        var path = context.Arguments.GetValueOrDefault("path")?.ToString();
        if (path?.Contains("secret") == true)
            return Task.FromResult(RuleResult.DenyResult("禁止访问包含 secret 的路径"));
        return Task.FromResult(RuleResult.AllowResult());
    }
}

// 注册
services.AddSingleton<IRule, MyRule>();
```

### 8. 外部插件加载

```json
{
  "LuBanAgent": {
    "ExternalPlugins": ["MyCompany.AgentPlugins", "ThirdParty.Tools"]
  }
}
```

通过配置 `ExternalPlugins` 指定程序集名称，框架会自动扫描并注册其中实现了 `ILuBanToolPlugin` 的类型。

### 9. 多 Agent 任务编排

```json
{
  "LuBanAgent": {
    "Orchestration": {
      "Enabled": true,
      "PlannerType": "composite",
      "AutoDetect": true,
      "MaxNodes": 10,
      "MaxParallelism": 4,
      "DefaultNodeTimeoutSeconds": 300,
      "MaxReplanAttempts": 3,
      "ReflectionTimeoutSeconds": 180,
      "ExposeAsTool": false,
      "HeuristicFilter": {
        "Enabled": true,
        "MinLength": 8,
        "MaxLength": 200,
        "RequireKeyword": true,
        "Keywords": [ "和", "同时", "然后", "并且", "另外", "还有", "分析并", "搜索并" ]
      }
    }
  }
}
```

- `AutoDetect`：启用后，每轮用户输入先由 planner 判定是否为复合任务（≥2 节点），是则自动走编排，否则走普通对话。
- `ExposeAsTool`：设为 `false` 时不再将编排暴露为显式工具，由自动判定取代。

**SubAgent 角色系统**：规划器可为每个节点指定角色（`analyst`/`researcher`/`coder`/`writer`），角色提供专业系统提示词和默认工具组。内置 4 个角色，支持通过工作区扩展自定义角色。

#### 工作区编排扩展

进入 `/agi` 工作区时自动加载以下目录：

- `.luban-agent/plans/*.json`：任务模板，命中关键词时由 TemplateTaskPlanner 直接生成图谱（不消耗 LLM 调用）。格式：`{ "name": "...", "keywords": [...], "graph": { "nodes": [...] } }`。
- `.luban-agent/roles/*.json`：自定义 SubAgent 角色，同名覆盖内置角色。格式：`{ "name": "...", "systemPromptTemplate": "... {prompt} ...", "defaultToolGroups": [...] }`。

#### 多模型路由

注册 `IProviderRouter` 后，`TaskNode.ModelName`（格式 `provider:model`）与 `OrchestrationOptions.PlannerModel` 会路由到对应 Provider；路由失败自动回退默认模型并记录警告。未注册路由时行为不变。

#### 启发式预过滤

`Orchestration:HeuristicFilter`（Enabled / MinLength / MaxLength / RequireKeyword / Keywords）：仅当输入长度落在 `[MinLength, MaxLength]` 且命中关键词时才进入 planner；空白、过短、超长或未命中关键词均跳过 planner，直接走主 Agent 对话（保留记忆召回），节省一次 LLM 调用并避免普通长问题被误判为复合任务。

**动态重规划**：当关键节点失败导致整体状态为 `failed` 时，编排器自动触发反思阶段：
1. **反思**：LLM 分析失败节点及其直接依赖的输出，判断是否可修复
2. **重规划**：LLM 生成修正节点（`fix_{attempt}_` 前缀）；指向已成功节点的依赖会被解析并内联为 prompt 文本，避免引用不在修正图谱中的节点
3. **重试**：执行修正图谱，最多尝试 `MaxReplanAttempts` 次（默认 3）

```csharp
// 直接调用编排器
var orchestrator = serviceProvider.GetRequiredService<IOrchestrator>();
var result = await orchestrator.RunAsync("调研 LuBan 框架并生成对比报告");

Console.WriteLine($"整体状态: {result.OverallStatus}");
Console.WriteLine($"重规划次数: {result.ReplanningAttempts}");
Console.WriteLine($"最终输出:\n{result.FinalOutput}");

// 带进度回调执行（不丢最终结果）：规划/节点级事件实时回调，返回值仍是完整编排结果
var result2 = await orchestrator.RunAsync(
    graph,
    onProgress: p => Console.WriteLine($"{p.EventType}: {p.Message}"),
    cancellationToken: default);
```

`LuBanAgent.RunStreamingAsync` 命中自动编排时会把进度包装为 `OrchestrationProgressContent`
（`EventType` / `NodeId` / `Message` / `NodeResult`）随流产出，最后再产出 `TextContent` 承载
`OrchestrationResult.FinalOutput`，上层 UI 因此可在规划与节点执行期间逐条渲染进度。
进度事件类型包含：`PlanningStarted`、`PlanningCompleted`、`NodeStarted`、`NodeCompleted`、
`NodeFailed`、`NodeSkipped`（关键前驱失败导致后继被跳过，逐节点上报）、`ReflectionStarted`、
`NodeActivity`（节点内部思考/工具调用明细）。

**编排执行流程**：

1. **规划阶段**：`ITaskPlanner` 将自然语言任务拆解为 DAG 任务图谱（模板优先，LLM 回退）
2. **校验阶段**：`TaskGraph.Validate` 检查无环、依赖存在、无重复 ID
3. **调度阶段**：`DagScheduler` 基于 Kahn 拓扑排序分层执行，同层节点并行
4. **上下文传递**：节点 prompt 中的 `{dep:xxx}` 占位符由 `ContextStore` 替换为前驱输出
5. **错误处理**：关键节点失败时跳过后继节点；非关键节点失败时继续执行
6. **结果聚合**：终点节点（无后继）的输出聚合为 `FinalOutput`

**记忆上下文注入**：编排入口（`Orchestrator.ExecuteGraphAsync`）会通过 `ContextInjectBuilder` 构建工作区长期记忆与规则上下文，
一次性写入 `TaskGraph.SharedContext`（仅填充一次，重规划生成的修正图谱继承同一份），再由 `SubAgentFactory` 追加到每个
SubAgent 的系统提示词，使子代理与主 Agent 对话共享同一份工作区记忆。常规对话路径则由 `SessionChatHistoryProvider`
直接把召回结果作为 System 消息注入，两条路径共用同一个 `ContextInjectBuilder`。

**关键概念**：

- **关键节点**（`IsCritical = true`）：失败时阻止后继节点执行，整体状态为 `failed`
- **非关键节点**：失败时后继节点继续执行，整体状态为 `partial`
- **占位符**：`{dep:节点id}` 引用前驱节点输出，运行时自动替换
- **并行度**：`MaxParallelism` 限制同层最大并行节点数，0 表示不限制
- **共享上下文**（`SharedContext`）：工作区记忆/规则上下文，由编排入口构建并注入所有子代理

### 10. 错误处理与自动重试

框架对所有 LLM/API 调用做统一错误分类（结构化优先：HTTP 状态码 + `error.code`），最终失败抛出 `AgentApiException`，宿主渲染 `Error.FriendlyMessage` 即可：

| 类别 | 典型错误 | 是否自动重试 |
|---|---|---|
| 认证/权限/余额 | 401 / 402 / 403 | 否，终止并提示 |
| 模型不存在 | 404 / 410 | 否，终止并给出排查清单 |
| 参数错误 / 上下文超长 / 能力不支持 | 400 / 413 / 422 | 否，终止并提示 |
| 内容风控 | `content_filter` | 否，终止并建议改措辞 |
| 限流 | 429 `rate_limit_exceeded` | 是（默认 3 次尝试，1s/2s 指数退避 + 抖动，遵循 `Retry-After`，单次等待上限 30s） |
| 服务端错误 / 网络错误 / 超时 | 5xx / DNS/TLS / 超时 | 是（同上） |
| 配额耗尽 | 429 `insufficient_quota` | 否，终止并在文案中带上服务端给出的重置时间 |
| 流式中断 | 已输出内容后连接断开 | 否，保留已输出并提示响应中断 |

配置（`appsettings.json` 的 `LuBanAgent` 节）：

```json
"ApiRetry": {
  "Enabled": true,
  "MaxAttempts": 3,
  "BaseDelayMs": 1000,
  "BackoffFactor": 2,
  "MaxDelayMs": 30000
}
```

重试进度回调（由宿主在启动时设置，回调在调用线程执行）：

```csharp
options.Value.OnApiRetry = notice =>
    Console.WriteLine($"{notice.Category} 将在 {notice.Delay.TotalSeconds:F0}s 后重试（第 {notice.Attempt} 次）");
```

说明：

- 流式调用仅在**首个 token 产出前**重试；已产出内容后中断不会重试，避免内容重复。
- 容错中间件包裹在 `FunctionInvokingChatClient` 之内，每个工具轮次的 API 调用独立重试。
- OpenAI SDK 的内置重试（`ClientRetryPolicy.Default`，默认再重试 3 次）已在宿主侧禁用，重试次数与进度提示以本配置为准。
- 编排规划期的可识别 API 故障不再静默降级为常规对话，而是按分类结果透出。
- 会话摘要压缩失败会降级为「跳过压缩」继续对话，不再打断主对话。

## 支持的 AI Provider

| Provider | 显示名称 | 支持的模型 |
|----------|---------|-----------|
| openai | OpenAI | gpt-4.1, gpt-4o, gpt-4-turbo, o1, o3-mini 等 |
| azure | Azure OpenAI | gpt-4o, gpt-4-turbo, gpt-35-turbo 等 |
| deepseek | DeepSeek | deepseek-chat, deepseek-coder, deepseek-reasoner |
| kimi | Kimi | k3, k3-256k, kimi-for-coding, kimi-for-coding-highspeed |
| glm | 智谱 GLM | glm-4-plus, glm-4-air, glm-4-flash 等 |
| qwen | 通义千问 | qwen-turbo, qwen-plus, qwen-max 等 |
| doubao | 豆包 | doubao-pro-4k, doubao-pro-32k, doubao-lite-4k 等 |
| claude | Claude | claude-3-5-sonnet, claude-3-5-haiku, claude-3-opus 等 |
| gemini | Google Gemini | gemini-2.0-flash, gemini-1.5-pro, gemini-1.5-flash 等 |
| ollama | Ollama (本地) | llama3.1, llama3.2, qwen2.5, deepseek-coder-v2 等 |
| ernie | 百度文心一言 | ernie-4.0-turbo-8k, ernie-4.0-8k 等 |
| minimax | MiniMax | abab6.5s-chat, abab6.5-chat 等 |
| hunyuan | 腾讯混元 | hunyuan-pro, hunyuan-standard 等 |
| mimo | 小米 MiMo | mimo-v1, mimo-v1-32k 等 |
| xai | xAI Grok | grok-2, grok-2-mini, grok-beta |
| qianfan | 百度智能云千帆 | ernie-4.0-8k, ernie-speed-128k 等 |
| tencent-ti | 腾讯云 TI 平台 | hunyuan-pro, hunyuan-standard 等 |
| huawei-pangu | 华为云盘古 | pangu-7b, pangu-13b, pangu-52b |
| bedrock | AWS Bedrock | anthropic.claude-3-sonnet 等 |
| openrouter | OpenRouter | openai/gpt-4o, anthropic/claude-3.5-sonnet 等 |

## 项目结构

```
LuBan.AIAgent/
├── Abstractions/
│   ├── ILuBanToolPlugin.cs            # 工具插件接口
│   ├── ToolPluginRegistry.cs          # 插件注册表
│   ├── ToolConfirmationService.cs     # 工具执行确认服务
│   ├── ToolAttribute.cs               # 工具标注特性
│   ├── ToolResult.cs                  # 工具结果
│   └── IIdentifiable.cs               # 可标识组件接口
├── Configuration/
│   ├── IAppConfigReader.cs            # 应用配置只读接口
│   ├── IProviderRouter.cs             # Provider 路由接口
│   ├── CustomSkillConfig.cs           # 自定义 Skill 配置
│   ├── CustomRuleConfig.cs            # 自定义规则配置
│   ├── McpServerConfig.cs             # 外部 MCP 服务器配置
│   ├── LuBanAgentOptions.cs           # Agent 配置选项
│   ├── BrowserToolOptions.cs          # 浏览器工具选项
│   ├── FileSystemToolOptions.cs       # 文件系统工具选项
│   ├── ScriptToolOptions.cs           # 脚本工具选项
│   ├── WebToolOptions.cs              # Web 工具选项
│   ├── RetrievalToolOptions.cs        # 检索工具选项
│   ├── LocalMemoryOptions.cs          # 本地记忆选项
│   ├── ModelEndpointOptions.cs        # 模型端点选项
│   ├── OrchestrationOptions.cs        # 编排选项
│   └── ToolGroupOptions.cs            # 工具组配置
├── Infrastructure/
│   ├── PlaywrightSession.cs           # Playwright 会话管理
│   ├── ProcessRunner.cs               # 进程执行器
│   └── PathGuard.cs                   # 路径安全守卫
├── Tools/
│   ├── Browser/BrowserToolPlugin.cs   # 浏览器工具
│   ├── FileSystem/FileSystemToolPlugin.cs  # 文件系统工具
│   ├── Script/ScriptToolPlugin.cs     # 脚本执行工具
│   ├── Web/WebToolPlugin.cs           # Web 工具
│   ├── Context/
│   │   └── CompactContextToolPlugin.cs # 上下文压缩工具
│   ├── LocalMemory/LocalMemoryToolPlugin.cs  # 本地记忆工具
│   ├── Retrieval/RetrievalToolPlugin.cs # 语义检索工具
│   └── Orchestration/                 # 编排工具
│       ├── OrchestrationToolPlugin.cs # 编排工具插件
│       └── OrchestrationToolGroup.cs  # 编排工具组
├── Skills/
│   ├── ISkill.cs                      # Skill 接口（含 PromptTemplate）
│   ├── SkillBase.cs                   # Skill 基类
│   ├── SkillRegistry.cs               # Skill 注册表（合并多来源）
│   ├── SkillLoader.cs                 # SKILL.md 文件加载器
│   ├── SkillMdParser.cs               # SKILL.md 解析器
│   ├── FileSkill.cs                   # 文件级 Skill 适配器
│   ├── CustomSkill.cs                 # 自定义 Skill 适配器
│   └── BuiltIn/
│       ├── BrainstormingSkill.cs      # 头脑风暴
│       ├── CodeReviewSkill.cs         # 代码审查
│       ├── DocumentationSkill.cs      # 文档生成
│       ├── CodeRefactorSkill.cs       # 代码重构
│       ├── TestGenerationSkill.cs     # 测试生成
│       ├── CodeExplainSkill.cs        # 代码解释
│       ├── DebugAssistantSkill.cs     # 调试助手
│       ├── GitCommitSkill.cs          # Git 提交
│       └── FindSkillsSkill.cs         # 技能发现
├── Rules/
│   ├── IRule.cs                       # 规则接口
│   ├── RuleBase.cs                    # 规则基类
│   ├── RuleEngine.cs                  # 规则引擎
│   ├── RuleCheckedAIFunction.cs       # 规则检查装饰器
│   ├── CustomRule.cs                  # 自定义规则适配器
│   └── BuiltIn/
│       └── PathAccessRule.cs          # 路径访问规则
├── MCP/
│   ├── IMCPClient.cs                  # MCP 客户端接口
│   ├── StdioMCPClient.cs              # stdio JSON-RPC 客户端
│   ├── MCPRegistry.cs                 # MCP 注册表
│   ├── MCPToolPlugin.cs               # MCP 工具插件
│   └── BuiltIn/
│       └── FileSystemMCPClient.cs     # 文件系统 MCP 客户端
├── Sessions/
│   ├── ISessionManager.cs             # 会话管理接口
│   └── SessionChatHistoryProvider.cs  # 会话历史提供者
├── Retrieval/
│   ├── IRetrievalService.cs           # 语义检索接口
│   ├── RetrievalService.cs            # 检索服务实现
│   └── Chunkers/                     # 代码切块器
├── Utils/Text/
│   ├── TextUtils.cs                   # 文本处理工具
│   ├── NGramExtractor.cs              # N-Gram 提取器
│   └── WildcardMatcher.cs             # 通配符匹配
├── LocalMemory/
│   ├── ILocalMemoryService.cs         # 本地记忆服务接口
│   ├── ILocalMemoryStore.cs           # 本地记忆存储接口
│   ├── IWorkspaceContextProvider.cs   # 工作区上下文提供者接口
│   ├── LocalMemoryService.cs          # 本地记忆服务
│   ├── MemoryCategories.cs            # 记忆分类
│   └── MemoryEntry.cs                 # 记忆条目模型
├── Orchestration/                     # 多 Agent 编排子系统
│   ├── IOrchestrator.cs               # 编排器接口
│   ├── Orchestrator.cs                # 编排器默认实现
│   ├── DagScheduler.cs                # DAG 调度器（拓扑分层并行）
│   ├── SubAgentFactory.cs             # SubAgent 工厂
│   ├── SubAgentRoleRegistry.cs        # SubAgent 角色注册表
│   ├── ContextStore.cs                # 跨节点上下文存储
│   ├── Models/                        # 数据模型
│   │   ├── TaskGraph.cs               # 任务图谱
│   │   ├── TaskNode.cs                # 任务节点
│   │   ├── TaskNodeStatus.cs          # 节点状态枚举
│   │   ├── SubAgentSpec.cs            # SubAgent 规格
│   │   ├── SubAgentRole.cs            # SubAgent 角色定义
│   │   ├── NodeResult.cs              # 节点结果
│   │   ├── OrchestrationResult.cs     # 编排结果
│   │   ├── OrchestrationProgress.cs   # 进度事件
│   │   ├── ProgressEventType.cs       # 进度事件类型
│   │   └── ReflectionResult.cs        # 反思结果与重规划上下文
│   ├── Planner/                       # 任务规划器
│   │   ├── ITaskPlanner.cs            # 规划器接口
│   │   ├── LlmTaskPlanner.cs          # LLM 规划器
│   │   ├── TemplateTaskPlanner.cs     # 模板规划器
│   │   ├── CompositeTaskPlanner.cs    # 组合式规划器
│   │   └── TaskGraphTemplate.cs       # 图谱模板
│   └── Exceptions/                    # 异常定义
│       ├── TaskPlanningException.cs   # 规划异常
│       └── NodeExecutionException.cs  # 节点执行异常
├── LuBanAgent.cs                      # Agent 实例
├── LuBanAgentFactory.cs               # Agent 工厂
├── LuBanAgentExtensions.cs            # DI 扩展方法
├── SanitizingChatClient.cs            # 聊天客户端消毒器
├── AIFunctionFactoryHelper.cs         # AI 函数工厂辅助类
└── ILuBanAgentFactory.cs              # Agent 工厂接口
```

## 小贴士

- 模型路由使用 `provider:model` 格式，新增 Provider 只需通过 `IAppConfigReader` / 宿主实现添加
- **9 大内置工具组**覆盖浏览器自动化、文件操作、脚本执行、Web 请求、语义检索、上下文压缩、本地记忆、MCP、多 Agent 编排
- `ToolConfirmationService` 对写入、删除、执行等危险操作自动要求用户确认
- `FileSystemToolOptions.AllowedRoots` 限制文件访问范围，防止 Agent 越权操作
- **会话历史自动持久化**，支持长对话压缩（SummarizingChatReducer），上下文永不丢失；启动对话时显示最近历史，快速了解上下文
- **自定义 Skill/Rule/MCP 持久化**，配置保存到本地文件，重启后自动加载
- **文件化 Skill**：支持通过 SKILL.md 文件定义 Skill，兼容 OpenCode 格式，项目级/用户级目录自动加载
- **规则拦截**在工具执行前自动检查，支持 deny/allow/modify
- **MCP 工具集成**，外部 MCP 服务器工具自动暴露给 Agent
- 通过 `ExternalPlugins` 配置可热加载外部工具插件程序集
- 结合 LuBan.AIFlow 可对接 RagFlow / Dify / Coze 等 AI 平台
- **多 Agent 编排**：复合任务自动拆解为 DAG，SubAgent 串行/并行混合执行，支持关键节点失败跳过、超时控制、上下文传递

## 许可证

MIT
