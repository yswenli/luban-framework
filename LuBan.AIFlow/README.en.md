[中文](README.md) | English

# LuBan.AIFlow

> **Author**: yswenli | **Contact**: yswenli@outlook.com | **Repository**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> One construction entry point for multiple AI platforms — pick RagFlow / Dify / Coze as needed (the other `EnumAIType` values are not implemented in `Build()` yet).

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.AIAgent](../LuBan.AIAgent/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## Why Do You Need It?

- Writing a completely different SDK integration for every AI platform?
- Switching AI providers requires massive changes to business code?
- RAG knowledge base management, conversational sessions, and Agent orchestration scattered across different modules?

LuBan.AIFlow provides a unified construction entry point plus platform-specific clients: `IAIClient` exposes only the `AIOptions` configuration (no send/receive methods), while RagFlow, Dify, and Coze each expose methods matching their own APIs. Switching platforms only changes configuration; calls are made against the platform client.

## Quick Preview

```csharp
// Build the AI client (the parameterless constructor reads the AIOptions section automatically)
var client = new AIClientBuilder()
    .Configure(options =>
    {
        options.AIType = EnumAIType.RagFlow;
        options.BaseUrl = "https://ragflow.example.com";
        options.ApiKey = "your-api-key";
    })
    .Build();

// IAIClient exposes configuration only
var aiOptions = client.Options;

// Send/receive methods live on the platform client: cast first
var ragflow = (RagFlowAIClient)client;
var datasets = await ragflow.ListDatasetsAsync(new ListDatasetsQuery());
var chunks = await ragflow.ListChunksAsync(datasetId, new ListChunksQuery());
```

## Tech Stack

| Component | Description |
|-----------|-------------|
| ASP.NET Core | Web framework foundation |
| Model layer (LuBan.AIFlow.Models) | `EnumAIType` and the per-platform request/response models |
| Builder Pattern | Configuration-driven client construction |
| Multi-Platform API | RagFlow / Dify / Coze REST API wrappers |

## Installation

```bash
dotnet add package LuBan.AIFlow
```

## Feature Overview

### Core Architecture

| Component | Description |
|-----------|-------------|
| `IAIClient` | Unified AI client entry point, exposes only the `AIOptions` configuration, no send/receive methods |
| `AIClientBuilder` | Builder that dispatches by `EnumAIType`; `Build()` supports only RagFlow / Dify / Coze |
| `EnumAIType` | AI platform enum with 8 values: `OpenAI`/`AzureOpenAI`/`AliyunBaiLian`/`Baidu`/`TengXun`/`RagFlow`/`Dify`/`Coze` |

### Platform Support

| Platform | Client | Core Capabilities |
|----------|--------|-------------------|
| **RagFlow** | `RagFlowAIClient` | Knowledge base (Datasets), file management, chunks, sessions, chat (Chat Completion / Session Chat), Agent, Chat Assistant, Bearer Token authentication |
| **Dify** | `DifyAIClient` | Send chat messages (`SendChatMsgAsync`, blocking mode) |
| **Coze** | `CozeAIClient` | Workflow execution, execution history |

## Usage Guide

### 1. Configuration & Construction

```csharp
// Build the client via AIClientBuilder: Configure takes an Action<AIOptions>
var client = new AIClientBuilder()
    .Configure(options =>
    {
        options.AIType = EnumAIType.Dify;          // decides which client Build() produces
        options.BaseUrl = "https://api.dify.ai";
        options.ApiKey = "your-api-key";
    })
    .Build();

// Or use the parameterless constructor: it reads the AIOptions section (e.g. appsettings.json), then override as needed
var fromConfig = new AIClientBuilder().Build();
```

### 2. RagFlow Knowledge Base & Chat

```csharp
var ragflow = (RagFlowAIClient)client;

// Knowledge base management
var datasets = await ragflow.ListDatasetsAsync(new ListDatasetsQuery { Page = 1, PageSize = 30 });
var files = await ragflow.ListFilesAsync(datasetId, new ListFilesQuery());

// Document chunks
var chunks = await ragflow.ListChunksAsync(datasetId, new ListChunksQuery());

// Sessions and chat (entered via chatId)
var session = await ragflow.CreateSessionAsync(chatId, new CreateSessionRequest { Name = "Quarterly report analysis" });
var chatResponse = await ragflow.SessionChatAsync(chatId, session.Id, new SessionChatRequest { Message = "Please analyze this report" });
var completion = await ragflow.CreateChatCompletionAsync(chatId, new ChatCompletionRequest
{
    Messages = new List<ChatMessage> { new() { Role = "user", Content = "Please analyze this report" } }
});

// Agent and Assistant
var agents = await ragflow.ListAgentsAsync(new ListAgentsQuery());
var assistant = await ragflow.CreateChatAssistantAsync(new CreateChatAssistantRequest { Name = "Report assistant", DataSetIds = new List<string> { datasetId } });
```

### 3. Dify Chat

```csharp
var dify = (DifyAIClient)client;

// Send a chat message (blocking mode)
var response = await dify.SendChatMsgAsync(new ChatMessageRequest
{
    Query = "Explain the pros and cons of microservices architecture",
    ConversationId = conversationId   // leave empty on the first turn, then pass response.ConversationId to keep context
});

var answer = response.Answer;
```

### 4. Coze Workflows

```csharp
var coze = (CozeAIClient)client;

// Run a workflow (isAsync defaults to true, i.e. asynchronous execution)
var result = coze.RunWorkflow("your-workflow-id", "Analyze data");

// View execution history
var history = coze.GetWorkflowHistory("your-workflow-id", result.execute_id);
```

## Tips

- Use `AIClientBuilder` to construct clients; switching AI platforms only requires changing `AIOptions.AIType`
- Platforms differ in capability: `IAIClient` unifies configuration only, send/receive methods come from the concrete client, so callers cast to the platform client
- RagFlow client has the richest features, covering the full knowledge base lifecycle (datasets → files → chunks → sessions → chat)
- All platform clients implement the `IAIClient` interface, so they can be registered and managed uniformly via DI using `Options`
- RagFlow uses Bearer Token authentication; ensure the API Key is correctly set in configuration
- Combine with LuBan.AIAgent to build complete AI Agent applications

## License

Copyright (c) yswenli.

Licensed under the [Apache License 2.0](../LICENSE).
