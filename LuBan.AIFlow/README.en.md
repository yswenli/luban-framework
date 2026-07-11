[中文](README.md) | English

# LuBan.AIFlow

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> One interface to connect multiple AI platforms — RagFlow / Dify / Coze, plug and play.

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.AIAgent](../LuBan.AIAgent/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## Why Do You Need It?

- Writing a completely different SDK integration for every AI platform?
- Switching AI providers requires massive changes to business code?
- RAG knowledge base management, conversational sessions, and Agent orchestration scattered across different modules?

LuBan.AIFlow wraps RagFlow, Dify, and Coze into a consistent API experience through a unified `IAIClient` interface and Builder pattern. One codebase, multi-platform switching, zero business code changes.

## Quick Preview

```csharp
// Build AI client
var client = new AIClientBuilder()
    .Configure(config)
    .Build();

// Chat
var response = await client.ChatAsync("Help me analyze the performance issues in this code");

// Knowledge base management (RagFlow)
var datasets = await client.GetDatasetsAsync();
var chunks = await client.GetChunksAsync(datasetId, fileId);
```

## Tech Stack

| Component | Description |
|-----------|-------------|
| ASP.NET Core | Web framework foundation |
| LuBan.Common | Base interfaces and enum definitions (EnumAIType) |
| Builder Pattern | Configuration-driven client construction |
| Multi-Platform API | RagFlow / Dify / Coze REST API wrappers |

## Installation

```xml
<PackageReference Include="LuBan.AIFlow" Version="*" />
```

## Feature Overview

### Core Architecture

| Component | Description |
|-----------|-------------|
| `IAIClient` | Unified AI client interface, exposes `AIOptions` configuration |
| `AIClientBuilder` | Builder pattern constructor, reads configuration and dispatches by `EnumAIType` |
| `EnumAIType` | AI platform enum (RagFlow / Dify / Coze) |

### Platform Support

| Platform | Client | Core Capabilities |
|----------|--------|-------------------|
| **RagFlow** | `RagFlowAIClient` | Knowledge base (Datasets), file management, chunks, sessions, chat, Agent, Chat Assistant, Bearer Token authentication |
| **Dify** | `DifyAIClient` | Chat completion, message management |
| **Coze** | `CozeAIClient` | Workflow execution, execution history |

## Usage Guide

### 1. Configuration & Construction

```csharp
// Build client via AIClientBuilder
var client = new AIClientBuilder()
    .Configure(configuration)  // Pass in IConfiguration
    .Build();                  // Auto-selects implementation based on EnumAIType in config
```

### 2. RagFlow Knowledge Base & Chat

```csharp
// Knowledge base management
var datasets = await client.GetDatasetsAsync();
var files = await client.GetFilesAsync(datasetId);

// Document chunks
var chunks = await client.GetChunksAsync(datasetId, fileId);

// Sessions and chat
var session = await client.CreateSessionAsync(sessionName);
var chatResponse = await client.ChatAsync(sessionId, "Please analyze this report");

// Agent and Assistant
var agents = await client.GetAgentsAsync();
var assistant = await client.CreateChatAssistantAsync(config);
```

### 3. Dify Chat

```csharp
// Chat completion
var response = await client.ChatCompletionAsync(new ChatRequest
{
    Query = "Explain the pros and cons of microservices architecture",
    ConversationId = conversationId
});

// Message management
var messages = await client.GetMessagesAsync(conversationId);
```

### 4. Coze Workflows

```csharp
// Execute workflow
var result = await client.ExecuteWorkflowAsync(new WorkflowRequest
{
    WorkflowId = "your-workflow-id",
    Parameters = new { input = "Analyze data" }
});

// View execution history
var history = await client.GetWorkflowHistoryAsync(workflowId);
```

## Tips

- Use `AIClientBuilder` to construct clients; switching AI platforms only requires config changes, no business code modifications
- RagFlow client has the richest features, covering the full knowledge base lifecycle (datasets → files → chunks → sessions → chat)
- All platform clients implement the `IAIClient` interface, allowing flexible replacement via dependency injection
- RagFlow uses Bearer Token authentication; ensure the API Key is correctly set in configuration
- Combine with LuBan.AIAgent to build complete AI Agent applications

## License

MIT
