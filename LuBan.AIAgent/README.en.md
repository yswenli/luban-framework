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
| `LuBanChatClient` | Multi-provider router, unified `provider:model` format calls |
| `ConfigManager` | Configuration manager for loading, saving, and managing Provider configs |

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
| **File System Tools** | `filesystem` | Read files, write files, list directories, with AllowedRoots security restrictions |
| **Script Tools** | `script` | Execute Shell, Lua, Python scripts |
| **Database Tools** | `database` | Execute SQL statements via sqlcmd |
| **Redis Tools** | `redis` | Execute Redis commands via redis-cli |
| **Web Tools** | `web` | Send HTTP requests to fetch web content |

### Skill System

| Component | Description |
|-----------|-------------|
| `ISkill` | Skill interface definition |
| `SkillBase` | Skill base class, provides logging, status updates, Agent invocation |
| `SkillRegistry` | Skill registry |

### Built-in Skills

| Skill ID | Name | Category | Description |
|----------|------|----------|-------------|
| `brainstorming` | Brainstorming | creative | Explore requirements and design before implementing features |
| `code-review` | Code Review | development | Review code, find issues, provide improvement suggestions |
| `documentation` | Documentation | productivity | Generate code comments, README, API docs, etc. |

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
| `MCPClientBase` | MCP client base class |
| `MCPRegistry` | MCP registry |
| `FileSystemMCPClient` | Built-in file system MCP client |

### Security & Confirmation

| Component | Description |
|-----------|-------------|
| `ToolConfirmationService` | Tool execution confirmation service, requires user confirmation for dangerous operations |
| `PathGuard` | Path security guard, prevents unauthorized access |
| `RuleEngine` | Rule engine, performs permission checks and parameter modification before tool execution |

## Usage Guide

### 1. Configuration & Registration

```json
{
  "LuBanAgent": {
    "DefaultModel": "openai:gpt-4o",
    "SystemPrompt": "You are a helpful assistant.",
    "MaxToolLoopIterations": 10,
    "Models": {
      "openai": { "BaseUrl": "https://api.openai.com/v1", "ApiKey": "sk-xxx" },
      "qwen": { "BaseUrl": "https://dashscope.aliyuncs.com/compatible-mode/v1", "ApiKey": "sk-xxx" }
    },
    "Tools": {
      "Browser": { "Enabled": true, "Headless": false },
      "FileSystem": { "Enabled": true, "AllowedRoots": ["C:\\Work"] }
    }
  }
}
```

```csharp
// Register services (with custom ChatClient)
services.AddSingleton<IChatClient>(sp => CreateChatClient());
services.AddLuBanAgent(configuration);

// Or register with factory method
services.AddLuBanAgent(configuration, sp => new LuBanChatClient(providers, "openai"));
```

### 2. Multi-Model Routing

```csharp
// Use provider:model format to route to different models
// LuBanChatClient auto-dispatches based on the provider prefix in ModelId
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

```csharp
public class MyCustomSkill : SkillBase
{
    public override string Id => "my-custom-skill";
    public override string Name => "My Custom Skill";
    public override string Description => "Custom Skill example";
    public override string Category => "custom";

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

## Project Structure

```
LuBan.AIAgent/
├── Configuration/
│   ├── Storage/
│   │   ├── ProviderConfig.cs          # Provider configuration
│   │   ├── AppConfig.cs               # Application configuration
│   │   ├── ConfigManager.cs           # Configuration manager
│   │   └── ProviderModels.cs          # Predefined model list
│   ├── LuBanAgentOptions.cs           # Agent configuration options
│   ├── ModelEndpointOptions.cs        # Model endpoint configuration
│   ├── ToolGroupOptions.cs            # Tool group configuration
│   ├── BrowserToolOptions.cs          # Browser tool configuration
│   ├── FileSystemToolOptions.cs       # File system tool configuration
│   ├── ScriptToolOptions.cs           # Script tool configuration
│   ├── DatabaseToolOptions.cs         # Database tool configuration
│   ├── RedisToolOptions.cs            # Redis tool configuration
│   └── WebToolOptions.cs              # Web tool configuration
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
│   └── Web/WebToolPlugin.cs          # Web tools
├── Skills/
│   ├── ISkill.cs                      # Skill interface
│   ├── SkillBase.cs                   # Skill base class
│   ├── SkillRegistry.cs               # Skill registry
│   └── BuiltIn/
│       ├── BrainstormingSkill.cs      # Brainstorming
│       ├── CodeReviewSkill.cs         # Code review
│       └── DocumentationSkill.cs      # Documentation generation
├── Rules/
│   ├── IRule.cs                       # Rule interface
│   ├── RuleBase.cs                    # Rule base class
│   ├── RuleEngine.cs                  # Rule engine
│   └── BuiltIn/
│       └── PathAccessRule.cs          # Path access rule
├── MCP/
│   ├── IMCPClient.cs                  # MCP client interface
│   ├── MCPClientBase.cs               # MCP client base class
│   ├── MCPRegistry.cs                 # MCP registry
│   └── BuiltIn/
│       └── FileSystemMCPClient.cs     # File system MCP client
├── Providers/
│   └── LuBanChatClient.cs             # Provider router
├── Abstractions/
│   ├── ILuBanToolPlugin.cs            # Tool plugin interface
│   └── ToolAttribute.cs              # Tool annotation attribute
├── Plugins/
│   └── ToolPluginRegistry.cs          # Plugin registry
├── Services/
│   └── ToolConfirmationService.cs     # Tool execution confirmation service
├── LuBanAgent.cs                      # Agent instance
├── LuBanAgentFactory.cs               # Agent factory
└── LuBanAgentExtensions.cs            # DI extension methods
```

## Tips

- Model routing uses `provider:model` format; adding new providers only requires implementing `IChatModelProvider` or adding endpoints in configuration
- 6 built-in tool groups cover browser automation, file operations, script execution, database, Redis, and web requests
- `ToolConfirmationService` automatically requires user confirmation for dangerous operations (write, delete, execute)
- `FileSystemToolOptions.AllowedRoots` restricts file access scope to prevent Agent overreach
- Skill system supports custom extensions — inherit `SkillBase` for quick implementation
- Rule system supports priority ordering, enabling permission checks and parameter modification before tool execution
- External tool plugin assemblies can be hot-loaded via `ExternalPlugins` configuration
- Combine with LuBan.AIFlow to connect to RagFlow / Dify / Coze and other AI platforms

## License

MIT
