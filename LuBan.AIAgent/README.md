[English](README.en.md) | 中文

# LuBan.AIAgent

> **作者**: yswenli | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> 完整的 AI Agent 框架，让大模型拥有思考、规划、使用工具、自主执行的能力。

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.DI](../LuBan.DI/README.md) | [LuBan.AIFlow](../LuBan.AIFlow/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## 为什么需要它？

- 想让大模型调用工具完成任务，但 MCP / Function Calling 的实现细节太多？
- 技能（Skill）管理、工具（Tool）注册、会话持久化各写一套，维护成本高？
- 多模型提供商切换困难，Provider A 换到 Provider B 要重写大量代码？
- 缺少中间件机制，日志、策略控制、权限拦截难以扩展？

LuBan.AIAgent 提供从 Agent 运行时、多模型路由、技能系统、工具系统、会话存储到中间件管道的完整 AI Agent 基础设施，85+ 源文件，开箱即用。

## 快速预览

```csharp
// 构建 Agent 运行时
var runtime = new DefaultAgentRuntime(services);

// 多模型路由（provider:model 格式）
var chatClient = new ChatClient("qwen:qwen-plus");

// 对话并自动使用工具
var response = await runtime.RunAsync("帮我查看项目结构并生成一份报告", new RunOptions
{
    Tools = new[] { "list_directory", "read_file", "write_file" },
    SessionId = "session-001"
});
```

## 技术栈

| 组件 | 说明 |
|------|------|
| LuBan.DI | 依赖注入集成 |
| IChatClient | 统一聊天客户端抽象 |
| IChatModelProvider | 模型提供商接口（已实现 Qwen） |
| 中间件管道 | LoggingChatTurn / LoggingToolExecution / ToolPolicy |

## 安装

```xml
<PackageReference Include="LuBan.AIAgent" Version="*" />
```

## 功能总览

### 核心引擎

| 组件 | 说明 |
|------|------|
| `IAgentRuntime` / `DefaultAgentRuntime` | Agent 执行引擎，协调模型调用、工具执行、会话管理 |
| `IChatClient` / `ChatClient` | 多提供商路由器，`provider:model` 格式统一调用 |
| `IChatModelProvider` | 模型提供商接口，已实现 `QwenChatModelProvider` |

### 技能系统

| 组件 | 说明 |
|------|------|
| `ISkill` | 技能接口定义 |
| `ISkillRegistry` | 技能注册中心 |
| `ISkillPlanner` / `RuleBasedSkillPlanner` | 基于规则的技能规划器 |
| `ISkillExecutor` / `PromptSkillExecutor` | 技能执行器 |
| `LocalFileSkillLoader` | 从本地文件加载技能定义 |

### 工具系统

| 组件 | 说明 |
|------|------|
| `ITool` | 工具接口定义 |
| `IToolRegistry` | 工具注册中心 |
| `IToolExecutionGate` | 工具执行门控（权限/策略检查） |
| `IToolExecutionMiddleware` | 工具执行中间件 |

### 内置工具

| 工具 | 功能 |
|------|------|
| `ReadFileTool` | 读取文件内容 |
| `WriteFileTool` | 写入文件 |
| `PatchFileTool` | 补丁式修改文件 |
| `MoveFileTool` | 移动/重命名文件 |
| `DeleteFileTool` | 删除文件 |
| `SearchFilesTool` | 搜索文件（按模式匹配） |
| `ListDirectoryTool` | 列出目录内容 |
| `CreateDirectoryTool` | 创建目录 |
| `DeleteDirectoryTool` | 删除目录 |
| `ReadFilesBatchTool` | 批量读取多个文件 |
| `WebFetchTool` | 抓取网页内容 |
| `RunLocalCommandTool` | 执行本地命令 |

### 会话存储

| 组件 | 说明 |
|------|------|
| `ISessionStore` | 会话存储接口 |
| `InMemorySessionStore` | 内存会话存储（开发/测试） |
| `FileSessionStore` | 文件持久化会话存储（生产） |

### 中间件管道

| 中间件 | 说明 |
|------|------|
| `LoggingChatTurnMiddleware` | 记录每轮对话的输入输出 |
| `LoggingToolExecutionMiddleware` | 记录工具调用的参数与结果 |
| `ToolPolicyMiddleware` | 工具执行策略控制（白名单/黑名单/权限） |
| `MiddlewarePipeline` | 中间件管道，支持灵活组合 |

## 使用指南

### 1. 模型路由

```csharp
// 使用 provider:model 格式路由到不同模型
var client = new ChatClient("qwen:qwen-plus");

// 切换提供商只需更改前缀
var gptClient = new ChatClient("openai:gpt-4");
```

### 2. 工具注册与使用

```csharp
// 注册内置工具
toolRegistry.Register(new ReadFileTool());
toolRegistry.Register(new WriteFileTool());
toolRegistry.Register(new ListDirectoryTool());
toolRegistry.Register(new SearchFilesTool());
toolRegistry.Register(new WebFetchTool());

// Agent 运行时自动选择并调用工具
var result = await runtime.RunAsync(
    "查看 src 目录下的所有 .cs 文件并统计代码行数",
    new RunOptions
    {
        Tools = new[] { "list_directory", "search_files", "read_file" }
    }
);
```

### 3. 技能管理

```csharp
// 从本地文件加载技能
var loader = new LocalFileSkillLoader(skillsDirectory);
var skills = loader.LoadAll();

// 注册技能
foreach (var skill in skills)
    skillRegistry.Register(skill);

// 基于规则规划技能执行
var planner = new RuleBasedSkillPlanner(skillRegistry);
var plan = planner.Plan("帮我重构这个模块的代码");

// 执行技能
var executor = new PromptSkillExecutor(chatClient);
await executor.ExecuteAsync(plan);
```

### 4. 会话持久化

```csharp
// 内存存储（适合开发测试）
var sessionStore = new InMemorySessionStore();

// 文件存储（适合生产环境）
var sessionStore = new FileSessionStore(sessionDirectory);

// Agent 运行时自动管理会话
var result = await runtime.RunAsync("继续上次的任务", new RunOptions
{
    SessionId = "my-session-id"
});
```

### 5. 中间件配置

```csharp
// 构建中间件管道
var pipeline = new MiddlewarePipeline()
    .Use(new LoggingChatTurnMiddleware())
    .Use(new ToolPolicyMiddleware(allowedTools: new[] { "read_file", "list_directory" }))
    .Use(new LoggingToolExecutionMiddleware());
```

### 6. 工具执行门控

```csharp
// 实现自定义门控
public class ApprovalGate : IToolExecutionGate
{
    public async Task<bool> CanExecuteAsync(ITool tool, ToolContext context)
    {
        // 危险操作需要审批
        if (tool.Name == "run_local_command")
            return await RequestApprovalAsync(context);
        return true;
    }
}
```

## 小贴士

- 模型路由使用 `provider:model` 格式，新增提供商只需实现 `IChatModelProvider`
- 内置 12 个文件操作与网络工具，覆盖 Agent 常见需求
- `IToolExecutionGate` 实现工具级权限控制，防止 Agent 执行危险操作
- 中间件管道按顺序执行，可灵活组合日志、策略、认证等横切关注点
- `FileSessionStore` 适合生产环境，会话数据持久化到磁盘
- 技能系统支持从本地文件加载，便于非开发人员维护技能定义
- 结合 LuBan.AIFlow 可对接 RagFlow / Dify / Coze 等 AI 平台

## 许可证

MIT
