[中文](README.md) | English

# LuBan.AIAgent

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> A complete AI Agent framework that gives large language models the ability to think, plan, use tools, and execute autonomously.

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.DI](../LuBan.DI/README.md) | [LuBan.AIFlow](../LuBan.AIFlow/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## Why Do You Need It?

- Want LLMs to call tools to complete tasks, but MCP / Function Calling implementation details are overwhelming?
- Skill management, tool registration, and session persistence each require separate implementations — high maintenance cost?
- Switching between model providers is difficult — moving from Provider A to Provider B requires rewriting large amounts of code?
- Missing middleware mechanism — logging, policy control, and permission interception are hard to extend?

LuBan.AIAgent provides complete AI Agent infrastructure from Agent runtime, multi-model routing, skill system, tool system, session storage to middleware pipeline — 85+ source files, ready to use out of the box.

## Quick Preview

```csharp
// Build Agent runtime
var runtime = new DefaultAgentRuntime(services);

// Multi-model routing (provider:model format)
var chatClient = new ChatClient("qwen:qwen-plus");

// Chat with automatic tool usage
var response = await runtime.RunAsync("Help me check the project structure and generate a report", new RunOptions
{
    Tools = new[] { "list_directory", "read_file", "write_file" },
    SessionId = "session-001"
});
```

## Tech Stack

| Component | Description |
|-----------|-------------|
| LuBan.DI | Dependency injection integration |
| IChatClient | Unified chat client abstraction |
| IChatModelProvider | Model provider interface (Qwen implemented) |
| Middleware Pipeline | LoggingChatTurn / LoggingToolExecution / ToolPolicy |

## Installation

```xml
<PackageReference Include="LuBan.AIAgent" Version="*" />
```

## Feature Overview

### Core Engine

| Component | Description |
|-----------|-------------|
| `IAgentRuntime` / `DefaultAgentRuntime` | Agent execution engine, coordinates model calls, tool execution, session management |
| `IChatClient` / `ChatClient` | Multi-provider router, unified `provider:model` format calls |
| `IChatModelProvider` | Model provider interface, `QwenChatModelProvider` implemented |

### Skill System

| Component | Description |
|-----------|-------------|
| `ISkill` | Skill interface definition |
| `ISkillRegistry` | Skill registry |
| `ISkillPlanner` / `RuleBasedSkillPlanner` | Rule-based skill planner |
| `ISkillExecutor` / `PromptSkillExecutor` | Skill executor |
| `LocalFileSkillLoader` | Load skill definitions from local files |

### Tool System

| Component | Description |
|-----------|-------------|
| `ITool` | Tool interface definition |
| `IToolRegistry` | Tool registry |
| `IToolExecutionGate` | Tool execution gate (permission/policy checks) |
| `IToolExecutionMiddleware` | Tool execution middleware |

### Built-in Tools

| Tool | Function |
|------|----------|
| `ReadFileTool` | Read file contents |
| `WriteFileTool` | Write files |
| `PatchFileTool` | Patch-style file modification |
| `MoveFileTool` | Move/rename files |
| `DeleteFileTool` | Delete files |
| `SearchFilesTool` | Search files (by pattern matching) |
| `ListDirectoryTool` | List directory contents |
| `CreateDirectoryTool` | Create directories |
| `DeleteDirectoryTool` | Delete directories |
| `ReadFilesBatchTool` | Batch read multiple files |
| `WebFetchTool` | Fetch web page content |
| `RunLocalCommandTool` | Execute local commands |

### Session Storage

| Component | Description |
|-----------|-------------|
| `ISessionStore` | Session storage interface |
| `InMemorySessionStore` | In-memory session storage (dev/testing) |
| `FileSessionStore` | File-persisted session storage (production) |

### Middleware Pipeline

| Middleware | Description |
|------------|-------------|
| `LoggingChatTurnMiddleware` | Logs input/output of each conversation turn |
| `LoggingToolExecutionMiddleware` | Logs tool call parameters and results |
| `ToolPolicyMiddleware` | Tool execution policy control (whitelist/blacklist/permissions) |
| `MiddlewarePipeline` | Middleware pipeline supporting flexible composition |

## Usage Guide

### 1. Model Routing

```csharp
// Use provider:model format to route to different models
var client = new ChatClient("qwen:qwen-plus");

// Switch providers by changing the prefix only
var gptClient = new ChatClient("openai:gpt-4");
```

### 2. Tool Registration & Usage

```csharp
// Register built-in tools
toolRegistry.Register(new ReadFileTool());
toolRegistry.Register(new WriteFileTool());
toolRegistry.Register(new ListDirectoryTool());
toolRegistry.Register(new SearchFilesTool());
toolRegistry.Register(new WebFetchTool());

// Agent runtime auto-selects and invokes tools
var result = await runtime.RunAsync(
    "List all .cs files under the src directory and count lines of code",
    new RunOptions
    {
        Tools = new[] { "list_directory", "search_files", "read_file" }
    }
);
```

### 3. Skill Management

```csharp
// Load skills from local files
var loader = new LocalFileSkillLoader(skillsDirectory);
var skills = loader.LoadAll();

// Register skills
foreach (var skill in skills)
    skillRegistry.Register(skill);

// Plan skill execution based on rules
var planner = new RuleBasedSkillPlanner(skillRegistry);
var plan = planner.Plan("Help me refactor this module's code");

// Execute skills
var executor = new PromptSkillExecutor(chatClient);
await executor.ExecuteAsync(plan);
```

### 4. Session Persistence

```csharp
// In-memory storage (suitable for dev/testing)
var sessionStore = new InMemorySessionStore();

// File storage (suitable for production)
var sessionStore = new FileSessionStore(sessionDirectory);

// Agent runtime manages sessions automatically
var result = await runtime.RunAsync("Continue the previous task", new RunOptions
{
    SessionId = "my-session-id"
});
```

### 5. Middleware Configuration

```csharp
// Build middleware pipeline
var pipeline = new MiddlewarePipeline()
    .Use(new LoggingChatTurnMiddleware())
    .Use(new ToolPolicyMiddleware(allowedTools: new[] { "read_file", "list_directory" }))
    .Use(new LoggingToolExecutionMiddleware());
```

### 6. Tool Execution Gating

```csharp
// Implement custom gate
public class ApprovalGate : IToolExecutionGate
{
    public async Task<bool> CanExecuteAsync(ITool tool, ToolContext context)
    {
        // Dangerous operations require approval
        if (tool.Name == "run_local_command")
            return await RequestApprovalAsync(context);
        return true;
    }
}
```

## Tips

- Model routing uses `provider:model` format; adding new providers only requires implementing `IChatModelProvider`
- 12 built-in file operation and network tools cover common Agent needs
- `IToolExecutionGate` implements tool-level permission control to prevent Agents from executing dangerous operations
- Middleware pipeline executes in order, allowing flexible composition of logging, policy, authentication, and other cross-cutting concerns
- `FileSessionStore` is suitable for production environments, persisting session data to disk
- Skill system supports loading from local files, making it easy for non-developers to maintain skill definitions
- Combine with LuBan.AIFlow to connect to RagFlow / Dify / Coze and other AI platforms

## License

MIT
