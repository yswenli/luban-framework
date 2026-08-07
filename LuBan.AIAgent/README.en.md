[中文](README.md) | English

# LuBan.AIAgent

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> An AI Agent library built on Microsoft Agent Framework — giving LLMs the ability to think, plan, use tools, and execute autonomously.

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.DI](../LuBan.DI/README.md) | [LuBan.AIFlow](../LuBan.AIFlow/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## Why Do You Need It?

- Want LLMs to call tools to complete tasks, but MCP / Function Calling implementation details are overwhelming?
- Skill management, tool registration, and session persistence each require separate implementations — high maintenance cost?
- Switching between model providers is difficult — moving from Provider A to Provider B requires rewriting large amounts of code?
- Missing middleware mechanism — logging, policy control, and permission interception are hard to extend?

LuBan.AIAgent provides complete AI Agent infrastructure — from Agent runtime, multi-model routing, skill system, tool system, session storage to middleware pipeline — ready to use out of the box.

## Quick Preview

```csharp
// Register services
services.AddSingleton<IChatClient>(sp => CreateChatClient());
services.AddLuBanAgent(configuration);

// Create Agent
var factory = serviceProvider.GetRequiredService<ILuBanAgentFactory>();
var agent = await factory.CreateAsync(
    systemPrompt: "You are a browser automation assistant",
    toolGroups: new[] { "browser" });

// Execute task
var response = await agent.RunAsync("Open Baidu and search for LuBan Framework");
Console.WriteLine(response.Text);
```

## Tech Stack

| Component | Description |
|-----------|-------------|
| Microsoft.Agents.AI.Foundry | Agent runtime framework |
| Microsoft.Extensions.AI | Unified chat client abstraction |
| Microsoft.Playwright | Browser automation engine |
| LuBan.DI | Dependency injection integration |
| LuBan.Common | Base interfaces and utility definitions |

## Installation

```bash
dotnet add package LuBan.AIAgent
```

Install Playwright browsers (required for browser tools):

```powershell
npx playwright@1.61.0 install chromium
```

## Feature Overview

### Core Engine

| Component | Description |
|-----------|-------------|
| `LuBanAgent` | Agent instance, wraps ChatClientAgent, supports sync/streaming execution |
| `ILuBanAgentFactory` / `LuBanAgentFactory` | Agent factory, creates agents with configured tools |
| `IAppConfigReader` | Application config read-only interface |
| `IProviderRouter` | Provider routing interface |
| `TextUtils` | Text processing utilities |
| `WildcardMatcher` | Wildcard matching |
| `SkillMdParser` | SKILL.md parser |

### Component Registry Architecture

Skills, MCPs, and Rules use a unified three-tier priority registry pattern:

| Priority | Source | Behavior |
|----------|--------|----------|
| Highest | Hardcoded (DI) | Always present, can be disabled via `DisabledBuiltin` config |
| Medium | Workspace files | Add new items, same-name items are ignored |
| Lowest | config.json | Add new items, same-name items are ignored |

**Loading timing**:
- At startup: Load hardcoded components + config.json global config
- On workspace switch: Load workspace files, auto-merge

**Workspace directory structure**:
```
.luban-agent/
├── skills/          # Workspace-level Skills
│   └── my-skill/
│       └── SKILL.md
├── mcps/            # Workspace-level MCP servers
│   └── my-mcp.json
└── rules/           # Workspace-level rules
    └── my-rule.json
```

### Tool System

| Component | Description |
|-----------|-------------|
| `ILuBanToolPlugin` | Tool plugin interface, defines tool groups and provides tool functions |
| `ToolPluginRegistry` | Tool plugin registry, manages enable/disable and group filtering |
| `ToolAttribute` | Tool annotation attribute |

### Built-in Tools

| Tool Group | Group Name | Core Capabilities |
|------------|------------|-------------------|
| **Browser Tools** | `browser` | Navigate, click, type, screenshot, get content, wait for selector, get URL (Playwright-based) |
| **File System Tools** | `filesystem` | Read files, write files, list directories, delete files, delete directories, search files (glob), content search (regex), create directories, copy files, move files, get file info, with AllowedRoots security restrictions |
| **Script Tools** | `script` | Execute Shell, Lua, Python scripts |
| **Database Tools** | `database` | Execute SQL via ADO.NET direct connections (MySQL/PostgreSQL/SQL Server/SQLite), read-only query whitelist validation, write operations require user confirmation, supports dynamic connection strings |
| **Redis Tools** | `redis` | Execute Redis commands via redis-cli |
| **Web Tools** | `web` | Send HTTP requests to fetch web content |
| **Retrieval Tools** | `retrieval` | Index local code/documents, semantic search |

### Skill System

| Component | Description |
|-----------|-------------|
| `ISkill` | Skill interface definition, includes `PromptTemplate` property for in-conversation activation |
| `SkillBase` | Skill base class, provides logging, status updates, Agent invocation |
| `SkillRegistry` | Skill registry, manages built-in, file-based, and custom Skills |
| `SkillLoader` | Skill file loader, loads Skill definitions from SKILL.md files |
| `FileSkill` | File-based Skill adapter, wraps SKILL.md files as ISkill |
| `CustomSkill` | Custom Skill adapter, wraps CustomSkillConfig as ISkill |

### Built-in Skills

| Skill ID | Name | Category | Description |
|----------|------|----------|-------------|
| `brainstorming` | Brainstorming | creative | Explore requirements and design before implementing features |
| `code-review` | Code Review | development | Review code, find issues, provide improvement suggestions |
| `documentation` | Documentation | productivity | Generate code comments, README, API docs, etc. |
| `code-refactor` | Code Refactoring | development | Refactor code to improve quality |
| `test-generation` | Test Generation | development | Automatically generate unit tests |
| `code-explain` | Code Explanation | development | Explain complex code logic |
| `debug-assistant` | Debug Assistant | development | Assist with debugging issues |
| `git-commit` | Git Commit | productivity | Generate standardized Git commit messages |
| `find-skills` | Skill Discovery | meta | Automatically discover and recommend suitable skills |

### File-based Skills

Support defining custom Skills via SKILL.md files, compatible with OpenCode format:

```markdown
---
name: my-skill
description: "Skill description"
category: custom
---

# Skill Instructions

Prompt template content here...
```

**Storage Locations** (by priority):
- Project-level: `<workspace>/.luban-agent/skills/<skill-id>/SKILL.md`
- User-level: `%LocalAppData%/LuBan/AIAgent/skills/<skill-id>/SKILL.md`

**Priority**: Hardcoded (DI) > Workspace files > config.json

### Rule System

| Component | Description |
|-----------|-------------|
| `IRule` | Rule interface, defines execution conditions and behavior |
| `RuleBase` | Rule base class |
| `RuleEngine` | Rule engine, evaluates rules by priority |
| `PathAccessRule` | Built-in path access rule, restricts file system access scope |

### MCP System

| Component | Description |
|-----------|-------------|
| `IMCPClient` | MCP client interface, interacts with MCP servers |
| `StdioMCPClient` | stdio JSON-RPC based external MCP client |
| `MCPRegistry` | MCP registry, manages built-in and external clients |
| `MCPToolPlugin` | MCP tool plugin, exposes MCP tools to Agent |
| `FileSystemMCPClient` | Built-in file system MCP client |

### Session System

| Component | Description |
|-----------|-------------|
| `ISessionManager` | Session manager interface, supports create/switch/clear |
| `SessionChatHistoryProvider` | Session history provider, auto-persists conversation history |
| `SessionOptions` | Session config, supports compression thresholds |

### Rule Interception

| Component | Description |
|-----------|-------------|
| `RuleCheckedAIFunction` | Rule-checking decorator, intercepts tool calls |
| `CustomRule` | Custom rule adapter, supports wildcard matching |

### Security & Confirmation

| Component | Description |
|-----------|-------------|
| `ToolConfirmationService` | Tool execution confirmation service, requires user confirmation for dangerous operations |
| `PathGuard` | Path security guard, prevents unauthorized access |
| `RuleEngine` | Rule engine, performs permission checks and parameter modification before tool execution |

### Multi-Agent Orchestration

Main Agent parses composite tasks → decomposes into DAG task graph → dispatches SubAgents for serial/parallel execution.

| Component | Description |
|-----------|-------------|
| `IOrchestrator` / `Orchestrator` | Orchestrator entry, chains planning, scheduling, and result aggregation |
| `ITaskPlanner` | Task planner interface, converts natural language tasks to TaskGraph |
| `LlmTaskPlanner` | LLM-based planner, generates DAG via prompt engineering |
| `TemplateTaskPlanner` | Template-based planner, fast generation when matching predefined templates |
| `CompositeTaskPlanner` | Composite planner, template-first with LLM fallback |
| `DagScheduler` | DAG scheduler, layer-based parallel execution via topological sort |
| `SubAgentFactory` | SubAgent factory, wraps LuBanAgentFactory for child agent creation |
| `ContextStore` | Cross-node context store, isolated by graph ID, thread-safe |
| `TaskGraph` / `TaskNode` | DAG data models, support dependency declaration, placeholder references, critical nodes |
| `OrchestrationToolPlugin` | Tool plugin, exposes orchestration capability to main Agent |
| `ReflectionResult` / `ReplanContext` | Dynamic replanning models, LLM analyzes failures and generates fix graph after critical node failure |

## Usage Guide

### 1. Configuration & Registration

```json
{
  "LuBanAgent": {
    "DefaultModel": "openai:gpt-4o",
    "SystemPrompt": "You are a helpful assistant.",
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
// Register services
services.AddSingleton<IAppConfigReader>(myConfigManager);
services.AddSingleton<IProviderRouter>(myProviderRouter);
services.AddLuBanAgent(configuration);
```

### 2. Multi-Model Routing

```csharp
// Use provider:model format to route to different models
// IProviderRouter auto-dispatches based on the provider prefix in ModelId
var agent = await factory.CreateAsync(modelName: "qwen:qwen-plus");

// Switch providers by changing the prefix only
var agent2 = await factory.CreateAsync(modelName: "openai:gpt-4o");
```

### 3. Tool Registration & Usage

```csharp
// Specify tool groups when creating Agent
var agent = await factory.CreateAsync(
    toolGroups: new[] { "browser", "filesystem" });

// Agent auto-selects and invokes tools
var response = await agent.RunAsync("List all .cs files under src directory and count lines of code");

// Streaming execution
await foreach (var update in agent.RunStreamingAsync("Help me analyze this code"))
{
    Console.Write(update.Text);
}
```

### 4. Skill Management

```csharp
// Get Skill registry
var skillRegistry = serviceProvider.GetRequiredService<SkillRegistry>();

// List all Skills
var skills = skillRegistry.GetAll();

// Execute a Skill
var context = new SkillContext
{
    Agent = agent,
    UpdateStatus = status => Console.WriteLine($"Status: {status}")
};
var result = await skillRegistry.Get("brainstorming")
    .ExecuteAsync(context, "I want to implement a user login feature");
```

### 5. Custom Tool Plugin

```csharp
public class MyToolPlugin : ILuBanToolPlugin
{
    public string GroupName => "my-tools";
    public string? Description => "Custom toolset";

    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp)
    {
        return new List<AIFunction> { /* ... */ };
    }

    public bool IsEnabled(LuBanAgentOptions options) => true;
}

// Register
services.AddSingleton<ILuBanToolPlugin, MyToolPlugin>();
```

### 6. Custom Skill

**Method 1: File-based Skill (Recommended)**

Create `SKILL.md` files in project-level or user-level directories:

```bash
# Project-level directory
<workspace>/.luban-agent/skills/my-skill/SKILL.md

# User-level directory
%LocalAppData%/LuBan/AIAgent/skills/my-skill/SKILL.md
```

SKILL.md format:

```markdown
---
name: my-translator
description: "Translate text to English"
category: custom
---

# Translation Assistant

Please translate the user's content to English.

## Requirements
- Maintain the tone and style of the original text
- Use idiomatic English expressions
```

**Method 2: Code-defined Skill**

```csharp
public class MyCustomSkill : SkillBase
{
    public override string Id => "my-custom-skill";
    public override string Name => "My Custom Skill";
    public override string Description => "Custom Skill example";
    public override string Category => "custom";
    public override string? PromptTemplate => "Custom prompt template...";

    public override async Task<SkillResult> ExecuteAsync(SkillContext context, string input)
    {
        UpdateStatus(context, "Processing...");
        var result = await CallAgentAsync(context, input);
        return SkillResult.Ok(result ?? "");
    }
}

// Register
services.AddSingleton<ISkill, MyCustomSkill>();
```

### 7. Custom Rule

```csharp
public class MyRule : RuleBase
{
    public override string Id => "my-rule";
    public override string Name => "My Rule";
    public override int Priority => 50;

    public override bool IsApplicable(RuleContext context)
        => context.ActionType == "file-write";

    public override Task<RuleResult> ExecuteAsync(RuleContext context)
    {
        var path = context.Arguments.GetValueOrDefault("path")?.ToString();
        if (path?.Contains("secret") == true)
            return Task.FromResult(RuleResult.DenyResult("Access to paths containing 'secret' is forbidden"));
        return Task.FromResult(RuleResult.AllowResult());
    }
}

// Register
services.AddSingleton<IRule, MyRule>();
```

### 8. External Plugin Loading

```json
{
  "LuBanAgent": {
    "ExternalPlugins": ["MyCompany.AgentPlugins", "ThirdParty.Tools"]
  }
}
```

Specify assembly names via `ExternalPlugins` configuration — the framework auto-scans and registers types implementing `ILuBanToolPlugin`.

### 9. Multi-Agent Task Orchestration

```json
{
  "LuBanAgent": {
    "Orchestration": {
      "Enabled": true,
      "PlannerType": "composite",
      "MaxNodes": 10,
      "MaxParallelism": 4,
      "DefaultNodeTimeoutSeconds": 120,
      "MaxReplanAttempts": 3,
      "ReflectionTimeoutSeconds": 60,
      "ExposeAsTool": true
    }
  }
}
```

**Dynamic Replanning**: When critical node failures cause overall status `failed`, the orchestrator automatically triggers reflection:
1. **Reflect**: LLM analyzes failed nodes and their direct dependencies' outputs to determine if fixable
2. **Replan**: LLM generates fix nodes (with `fix_{attempt}_` prefix), reusing succeeded nodes
3. **Retry**: Executes fix graph, up to `MaxReplanAttempts` times (default: 3)

```csharp
// Invoke orchestrator directly
var orchestrator = serviceProvider.GetRequiredService<IOrchestrator>();
var result = await orchestrator.RunAsync("Research LuBan framework and generate a comparison report");

Console.WriteLine($"Overall status: {result.OverallStatus}");
Console.WriteLine($"Replanning attempts: {result.ReplanningAttempts}");
Console.WriteLine($"Final output:\n{result.FinalOutput}");

// Subscribe to streaming progress events
await foreach (var progress in orchestrator.RunStreamingAsync("..."))
{
    Console.WriteLine($"{progress.EventType}: {progress.Message}");
}
```

**Orchestration Flow**:

1. **Planning**: `ITaskPlanner` decomposes natural language task into DAG (template-first, LLM fallback)
2. **Validation**: `TaskGraph.Validate` checks for acyclicity, dependency existence, no duplicate IDs
3. **Scheduling**: `DagScheduler` executes layers via Kahn topological sort, parallel within same layer
4. **Context Passing**: `{dep:xxx}` placeholders in node prompts are replaced with predecessor outputs by `ContextStore`
5. **Error Handling**: Critical node failure skips successors; non-critical failure continues execution
6. **Result Aggregation**: Terminal nodes (no successors) outputs are aggregated into `FinalOutput`

**Key Concepts**:

- **Critical Node** (`IsCritical = true`): Failure blocks successor execution, overall status is `failed`
- **Non-Critical Node**: Failure allows successors to continue, overall status is `partial`
- **Placeholder**: `{dep:node-id}` references predecessor output, auto-replaced at runtime
- **Parallelism**: `MaxParallelism` limits max parallel nodes per layer, 0 means unlimited

## Supported AI Providers

| Provider | Display Name | Supported Models |
|----------|-------------|-----------------|
| openai | OpenAI | gpt-4.1, gpt-4o, gpt-4-turbo, o1, o3-mini, etc. |
| azure | Azure OpenAI | gpt-4o, gpt-4-turbo, gpt-35-turbo, etc. |
| deepseek | DeepSeek | deepseek-chat, deepseek-coder, deepseek-reasoner |
| kimi | Kimi | k3, k3-256k, kimi-for-coding, kimi-for-coding-highspeed |
| glm | Zhipu GLM | glm-4-plus, glm-4-air, glm-4-flash, etc. |
| qwen | Qwen | qwen-turbo, qwen-plus, qwen-max, etc. |
| doubao | Doubao | doubao-pro-4k, doubao-pro-32k, doubao-lite-4k, etc. |
| claude | Claude | claude-3-5-sonnet, claude-3-5-haiku, claude-3-opus, etc. |
| gemini | Google Gemini | gemini-2.0-flash, gemini-1.5-pro, gemini-1.5-flash, etc. |
| ollama | Ollama (Local) | llama3.1, llama3.2, qwen2.5, deepseek-coder-v2, etc. |
| ernie | Baidu ERNIE | ernie-4.0-turbo-8k, ernie-4.0-8k, etc. |
| minimax | MiniMax | abab6.5s-chat, abab6.5-chat, etc. |
| hunyuan | Tencent Hunyuan | hunyuan-pro, hunyuan-standard, etc. |
| mimo | Xiaomi MiMo | mimo-v1, mimo-v1-32k, etc. |
| xai | xAI Grok | grok-2, grok-2-mini, grok-beta |
| qianfan | Baidu Qianfan | ernie-4.0-8k, ernie-speed-128k, etc. |
| tencent-ti | Tencent TI Platform | hunyuan-pro, hunyuan-standard, etc. |
| huawei-pangu | Huawei Pangu | pangu-7b, pangu-13b, pangu-52b |
| bedrock | AWS Bedrock | anthropic.claude-3-sonnet, etc. |
| openrouter | OpenRouter | openai/gpt-4o, anthropic/claude-3.5-sonnet, etc. |

## Project Structure

```
LuBan.AIAgent/
├── Configuration/
│   ├── IAppConfigReader.cs            # Application config read-only interface
│   ├── Storage/
│   │   ├── CustomSkillConfig.cs       # Custom skill configuration
│   │   ├── CustomRuleConfig.cs        # Custom rule configuration
│   │   └── McpServerConfig.cs         # External MCP server config
│   ├── LuBanAgentOptions.cs           # Agent configuration options
│   ├── SessionOptions.cs              # Session configuration options
│   └── ToolGroupOptions.cs            # Tool group configuration
├── Infrastructure/
│   ├── PlaywrightSession.cs           # Playwright session management
│   ├── ProcessRunner.cs               # Process executor
│   └── PathGuard.cs                   # Path security guard
├── Tools/
│   ├── Browser/BrowserToolPlugin.cs   # Browser tools
│   ├── FileSystem/FileSystemToolPlugin.cs  # File system tools
│   ├── Script/ScriptToolPlugin.cs     # Script execution tools
│   ├── Database/DatabaseToolPlugin.cs # Database tools
│   ├── Redis/RedisToolPlugin.cs       # Redis tools
│   ├── Web/WebToolPlugin.cs           # Web tools
│   └── Retrieval/RetrievalToolPlugin.cs # Semantic retrieval tools
├── Skills/
│   ├── ISkill.cs                      # Skill interface (with PromptTemplate)
│   ├── SkillBase.cs                   # Skill base class
│   ├── SkillRegistry.cs               # Skill registry (merges multiple sources)
│   ├── SkillLoader.cs                 # SKILL.md file loader
│   ├── SkillMdParser.cs               # SKILL.md parser
│   ├── FileSkill.cs                   # File-based Skill adapter
│   ├── CustomSkill.cs                 # Custom Skill adapter
│   └── BuiltIn/
│       ├── BrainstormingSkill.cs      # Brainstorming
│       ├── CodeReviewSkill.cs         # Code review
│       ├── DocumentationSkill.cs      # Documentation generation
│       ├── CodeRefactorSkill.cs       # Code refactoring
│       ├── TestGenerationSkill.cs     # Test generation
│       ├── CodeExplainSkill.cs        # Code explanation
│       ├── DebugAssistantSkill.cs     # Debug assistant
│       ├── GitCommitSkill.cs          # Git commit
│       └── FindSkillsSkill.cs         # Skill discovery
├── Rules/
│   ├── IRule.cs                       # Rule interface
│   ├── RuleBase.cs                    # Rule base class
│   ├── RuleEngine.cs                  # Rule engine
│   ├── RuleCheckedAIFunction.cs       # Rule-checking decorator
│   ├── CustomRule.cs                  # Custom rule adapter
│   └── BuiltIn/
│       └── PathAccessRule.cs          # Path access rule
├── MCP/
│   ├── IMCPClient.cs                  # MCP client interface
│   ├── StdioMCPClient.cs              # stdio JSON-RPC client
│   ├── MCPRegistry.cs                 # MCP registry
│   ├── MCPToolPlugin.cs               # MCP tool plugin
│   └── BuiltIn/
│       └── FileSystemMCPClient.cs     # File system MCP client
├── Sessions/
│   ├── ISessionManager.cs             # Session manager interface
│   └── SessionChatHistoryProvider.cs  # Session history provider
├── Retrieval/
│   ├── IRetrievalService.cs           # Semantic retrieval interface
│   ├── RetrievalService.cs            # Retrieval service
│   └── Chunkers/                     # Code chunkers
├── Providers/
│   └── IProviderRouter.cs             # Provider routing interface
├── Abstractions/
│   └── ILuBanToolPlugin.cs            # Tool plugin interface
├── Plugins/
│   └── ToolPluginRegistry.cs          # Plugin registry
├── Services/
│   └── ToolConfirmationService.cs     # Tool execution confirmation service
├── Utils/
│   └── Text/
│       ├── TextUtils.cs               # Text processing utilities
│       ├── NGramExtractor.cs          # N-gram extractor
│       └── WildcardMatcher.cs         # Wildcard matching
├── Orchestration/                     # Multi-Agent orchestration subsystem
│   ├── IOrchestrator.cs               # Orchestrator interface
│   ├── Orchestrator.cs                # Orchestrator default implementation
│   ├── DagScheduler.cs                # DAG scheduler (topological layer parallel)
│   ├── SubAgentFactory.cs             # SubAgent factory
│   ├── ContextStore.cs                # Cross-node context store
│   ├── Models/                        # Data models
│   │   ├── TaskGraph.cs               # Task graph
│   │   ├── TaskNode.cs                # Task node
│   │   ├── TaskNodeStatus.cs          # Node status enum
│   │   ├── SubAgentSpec.cs            # SubAgent specification
│   │   ├── NodeResult.cs              # Node result
│   │   ├── OrchestrationResult.cs     # Orchestration result
│   │   ├── OrchestrationProgress.cs   # Progress event
│   │   ├── ProgressEventType.cs       # Progress event type
│   │   └── ReflectionResult.cs        # Reflection result and replan context
│   ├── Planner/                       # Task planners
│   │   ├── ITaskPlanner.cs            # Planner interface
│   │   ├── LlmTaskPlanner.cs          # LLM planner
│   │   ├── TemplateTaskPlanner.cs     # Template planner
│   │   ├── CompositeTaskPlanner.cs    # Composite planner
│   │   └── TaskGraphTemplate.cs       # Graph template
│   └── Exceptions/                    # Exception definitions
│       ├── TaskPlanningException.cs   # Planning exception
│       └── NodeExecutionException.cs  # Node execution exception
├── Tools/Orchestration/               # Orchestration tool plugin
│   ├── OrchestrationToolPlugin.cs     # Tool plugin
│   └── OrchestrationToolGroup.cs      # Tool group
├── LuBanAgent.cs                      # Agent instance
├── LuBanAgentFactory.cs               # Agent factory
└── LuBanAgentExtensions.cs            # DI extension methods
```

## Tips

- Model routing uses `provider:model` format; configure providers via `IAppConfigReader` / host implementation
- **7 built-in tool groups** cover browser automation, file operations, script execution, database, Redis, web requests, and semantic retrieval
- `ToolConfirmationService` automatically requires user confirmation for dangerous operations (write, delete, execute)
- `FileSystemToolOptions.AllowedRoots` restricts file access scope to prevent Agent overreach
- **Session history auto-persistence** with compression (SummarizingChatReducer), context never lost
- **Custom Skill/Rule/MCP persistence**, configs saved locally and auto-loaded on restart
- **File-based Skills**: define custom Skills via SKILL.md files, compatible with OpenCode format, automatically loaded from project-level/user-level directories
- **Rule interception** checks before tool execution, supports deny/allow/modify
- **MCP tool integration**, external MCP server tools exposed to Agent
- External tool plugin assemblies can be hot-loaded via `ExternalPlugins` configuration
- Combine with LuBan.AIFlow to connect to RagFlow / Dify / Coze and other AI platforms
- **Multi-Agent Orchestration**: composite tasks decomposed into DAG, SubAgents execute serially/parallelly with skip-on-failure, timeout, and context passing

## License

MIT
