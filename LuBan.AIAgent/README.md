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
| `LuBanChatClient` | 多 Provider 路由器，统一 `provider:model` 格式调用 |
| `ConfigManager` | 配置管理器，负责 Provider 配置的加载、保存和管理 |

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
| **数据库工具** | `database` | 通过 sqlcmd 执行 SQL 语句 |
| **Redis 工具** | `redis` | 通过 redis-cli 执行 Redis 命令 |
| **Web 工具** | `web` | 发送 HTTP 请求获取网页内容 |
| **语义检索工具** | `retrieval` | 索引本地代码/文档，按语义搜索相关片段 |

### Skill 系统

| 组件 | 说明 |
|------|------|
| `ISkill` | Skill 接口定义 |
| `SkillBase` | Skill 基类，提供日志、状态更新、Agent 调用等通用功能 |
| `SkillRegistry` | Skill 注册表 |

### 内置 Skill

| Skill ID | 名称 | 分类 | 说明 |
|----------|------|------|------|
| `brainstorming` | 头脑风暴 | creative | 实现功能前探索需求和设计 |
| `code-review` | 代码审查 | development | 审查代码、发现问题、提供改进建议 |
| `documentation` | 文档生成 | productivity | 生成代码注释、README、API 文档等 |

### Rule 系统

| 组件 | 说明 |
|------|------|
| `IRule` | 规则接口，定义执行条件和行为 |
| `RuleBase` | 规则基类 |
| `RuleEngine` | 规则引擎，按优先级评估规则 |
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
| `ContextStore` | 跨节点上下文存储，按图谱 ID 隔离，线程安全 |
| `TaskGraph` / `TaskNode` | DAG 数据模型，支持依赖声明、占位符引用、关键节点 |
| `OrchestrationToolPlugin` | 工具插件，将编排能力暴露给主 Agent 自动调用 |

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
    },
    "Tools": {
      "Browser": { "Enabled": true, "Headless": false },
      "FileSystem": { "Enabled": true, "AllowedRoots": ["C:\\Work"] },
      "Retrieval": { "Enabled": true, "ModelId": "bge-small-zh-v1.5" }
    }
  }
}
```

```csharp
// 注册服务（使用自定义 ChatClient）
services.AddSingleton<IChatClient>(sp => CreateChatClient());
services.AddLuBanAgent(configuration);

// 配置 Provider（通过 ConfigManager）
var configManager = serviceProvider.GetRequiredService<ConfigManager>();
configManager.AddProvider("openai", "sk-xxx");
configManager.Save();
```

### 2. 多模型路由

```csharp
// 使用 provider:model 格式路由到不同模型
// LuBanChatClient 根据 ModelId 中的 provider 前缀自动分发
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

    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp)
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

```csharp
public class MyCustomSkill : SkillBase
{
    public override string Id => "my-custom-skill";
    public override string Name => "我的自定义 Skill";
    public override string Description => "自定义 Skill 示例";
    public override string Category => "custom";

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
      "MaxNodes": 10,
      "MaxParallelism": 4,
      "DefaultNodeTimeoutSeconds": 120,
      "ExposeAsTool": true
    }
  }
}
```

```csharp
// 直接调用编排器
var orchestrator = serviceProvider.GetRequiredService<IOrchestrator>();
var result = await orchestrator.RunAsync("调研 LuBan 框架并生成对比报告");

Console.WriteLine($"整体状态: {result.OverallStatus}");
Console.WriteLine($"最终输出:\n{result.FinalOutput}");

// 流式订阅进度事件
await foreach (var progress in orchestrator.RunStreamingAsync("..."))
{
    Console.WriteLine($"{progress.EventType}: {progress.Message}");
}
```

**编排执行流程**：

1. **规划阶段**：`ITaskPlanner` 将自然语言任务拆解为 DAG 任务图谱（模板优先，LLM 回退）
2. **校验阶段**：`TaskGraph.Validate` 检查无环、依赖存在、无重复 ID
3. **调度阶段**：`DagScheduler` 基于 Kahn 拓扑排序分层执行，同层节点并行
4. **上下文传递**：节点 prompt 中的 `{dep:xxx}` 占位符由 `ContextStore` 替换为前驱输出
5. **错误处理**：关键节点失败时跳过后继节点；非关键节点失败时继续执行
6. **结果聚合**：终点节点（无后继）的输出聚合为 `FinalOutput`

**关键概念**：

- **关键节点**（`IsCritical = true`）：失败时阻止后继节点执行，整体状态为 `failed`
- **非关键节点**：失败时后继节点继续执行，整体状态为 `partial`
- **占位符**：`{dep:节点id}` 引用前驱节点输出，运行时自动替换
- **并行度**：`MaxParallelism` 限制同层最大并行节点数，0 表示不限制

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

## 项目结构

```
LuBan.AIAgent/
├── Configuration/
│   ├── Storage/
│   │   ├── ProviderConfig.cs          # Provider 配置
│   │   ├── AppConfig.cs               # 应用配置
│   │   ├── ConfigManager.cs           # 配置管理器（含 CRUD）
│   │   ├── CustomSkillConfig.cs       # 自定义 Skill 配置
│   │   ├── CustomRuleConfig.cs        # 自定义规则配置
│   │   └── McpServerConfig.cs         # 外部 MCP 服务器配置
│   ├── LuBanAgentOptions.cs           # Agent 配置选项
│   ├── SessionOptions.cs              # 会话配置选项
│   └── ToolGroupOptions.cs            # 工具组配置
├── Infrastructure/
│   ├── PlaywrightSession.cs           # Playwright 会话管理
│   ├── ProcessRunner.cs               # 进程执行器
│   └── PathGuard.cs                   # 路径安全守卫
├── Tools/
│   ├── Browser/BrowserToolPlugin.cs   # 浏览器工具
│   ├── FileSystem/FileSystemToolPlugin.cs  # 文件系统工具
│   ├── Script/ScriptToolPlugin.cs     # 脚本执行工具
│   ├── Database/DatabaseToolPlugin.cs # 数据库工具
│   ├── Redis/RedisToolPlugin.cs       # Redis 工具
│   ├── Web/WebToolPlugin.cs           # Web 工具
│   └── Retrieval/RetrievalToolPlugin.cs # 语义检索工具
├── Skills/
│   ├── ISkill.cs                      # Skill 接口
│   ├── SkillBase.cs                   # Skill 基类
│   ├── SkillRegistry.cs               # Skill 注册表
│   ├── CustomSkill.cs                 # 自定义 Skill 适配器
│   └── BuiltIn/
│       ├── BrainstormingSkill.cs      # 头脑风暴
│       ├── CodeReviewSkill.cs         # 代码审查
│       └── DocumentationSkill.cs      # 文档生成
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
├── Providers/
│   └── LuBanChatClient.cs             # Provider 路由器
├── Abstractions/
│   └── ILuBanToolPlugin.cs            # 工具插件接口
├── Plugins/
│   └── ToolPluginRegistry.cs          # 插件注册表
├── Services/
│   └── ToolConfirmationService.cs     # 工具执行确认服务
├── Orchestration/                     # 多 Agent 编排子系统
│   ├── IOrchestrator.cs               # 编排器接口
│   ├── Orchestrator.cs                # 编排器默认实现
│   ├── DagScheduler.cs                # DAG 调度器（拓扑分层并行）
│   ├── SubAgentFactory.cs             # SubAgent 工厂
│   ├── ContextStore.cs                # 跨节点上下文存储
│   ├── Models/                        # 数据模型
│   │   ├── TaskGraph.cs               # 任务图谱
│   │   ├── TaskNode.cs                # 任务节点
│   │   ├── TaskNodeStatus.cs          # 节点状态枚举
│   │   ├── SubAgentSpec.cs            # SubAgent 规格
│   │   ├── NodeResult.cs              # 节点结果
│   │   ├── OrchestrationResult.cs     # 编排结果
│   │   ├── OrchestrationProgress.cs   # 进度事件
│   │   └── ProgressEventType.cs       # 进度事件类型
│   ├── Planner/                       # 任务规划器
│   │   ├── ITaskPlanner.cs            # 规划器接口
│   │   ├── LlmTaskPlanner.cs          # LLM 规划器
│   │   ├── TemplateTaskPlanner.cs     # 模板规划器
│   │   ├── CompositeTaskPlanner.cs    # 组合式规划器
│   │   └── TaskGraphTemplate.cs       # 图谱模板
│   └── Exceptions/                    # 异常定义
│       ├── TaskPlanningException.cs   # 规划异常
│       └── NodeExecutionException.cs  # 节点执行异常
├── Tools/Orchestration/               # 编排工具插件
│   ├── OrchestrationToolPlugin.cs     # 工具插件
│   └── OrchestrationToolGroup.cs      # 工具组
├── LuBanAgent.cs                      # Agent 实例
├── LuBanAgentFactory.cs               # Agent 工厂
└── LuBanAgentExtensions.cs            # DI 扩展方法
```

## 小贴士

- 模型路由使用 `provider:model` 格式，新增 Provider 只需通过 `ConfigManager.AddProvider()` 添加
- **7 大内置工具组**覆盖浏览器自动化、文件操作、脚本执行、数据库、Redis、Web 请求、语义检索等场景
- `ToolConfirmationService` 对写入、删除、执行等危险操作自动要求用户确认
- `FileSystemToolOptions.AllowedRoots` 限制文件访问范围，防止 Agent 越权操作
- **会话历史自动持久化**，支持长对话压缩（SummarizingChatReducer），上下文永不丢失
- **自定义 Skill/Rule/MCP 持久化**，配置保存到本地文件，重启后自动加载
- **规则拦截**在工具执行前自动检查，支持 deny/allow/modify
- **MCP 工具集成**，外部 MCP 服务器工具自动暴露给 Agent
- 通过 `ExternalPlugins` 配置可热加载外部工具插件程序集
- 结合 LuBan.AIFlow 可对接 RagFlow / Dify / Coze 等 AI 平台
- **多 Agent 编排**：复合任务自动拆解为 DAG，SubAgent 串行/并行混合执行，支持关键节点失败跳过、超时控制、上下文传递

## 许可证

MIT
