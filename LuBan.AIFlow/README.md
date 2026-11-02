[English](README.en.md) | 中文

# LuBan.AIFlow

> **作者**: yswenli | **联系邮箱**: yswenli@outlook.com | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> 统一构建入口对接多个 AI 平台，RagFlow / Dify / Coze 按需选择（其余 `EnumAIType` 值暂未在 `Build()` 中实现）。

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.AIAgent](../LuBan.AIAgent/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## 为什么需要它？

- 每接入一个 AI 平台就要写一套完全不同的 SDK 调用逻辑？
- 切换 AI 供应商需要改动大量业务代码？
- RAG 知识库管理、会话对话、Agent 编排分散在不同模块？

LuBan.AIFlow 提供统一构建入口与平台差异化客户端：`IAIClient` 只暴露 `AIOptions` 配置（不含收发方法），RagFlow / Dify / Coze 各自提供符合自身 API 的方法。切换平台只改配置，具体调用按平台客户端进行。

## 快速预览

```csharp
// 构建 AI 客户端（无参构造会自动读取 AIOptions 配置节）
var client = new AIClientBuilder()
    .Configure(options =>
    {
        options.AIType = EnumAIType.RagFlow;
        options.BaseUrl = "https://ragflow.example.com";
        options.ApiKey = "your-api-key";
    })
    .Build();

// IAIClient 仅暴露配置
var aiOptions = client.Options;

// 收发方法在具体平台客户端上：先转型再调用
var ragflow = (RagFlowAIClient)client;
var datasets = await ragflow.ListDatasetsAsync(new ListDatasetsQuery());
var chunks = await ragflow.ListChunksAsync(datasetId, new ListChunksQuery());
```

## 技术栈

| 组件 | 说明 |
|------|------|
| ASP.NET Core | Web 框架基础 |
| 模型层（LuBan.AIFlow.Models） | `EnumAIType` 与各平台请求/响应模型 |
| Builder 模式 | 配置驱动的客户端构建 |
| 多平台 API | RagFlow / Dify / Coze REST API 封装 |

## 安装

```bash
dotnet add package LuBan.AIFlow
```

## 功能总览

### 核心架构

| 组件 | 说明 |
|------|------|
| `IAIClient` | AI 客户端统一入口，仅暴露 `AIOptions` 配置，不含收发方法 |
| `AIClientBuilder` | Builder 模式构建器，按 `EnumAIType` 分发；`Build()` 只支持 RagFlow / Dify / Coze |
| `EnumAIType` | AI 平台枚举，共 8 个值：`OpenAI`/`AzureOpenAI`/`AliyunBaiLian`/`Baidu`/`TengXun`/`RagFlow`/`Dify`/`Coze` |

### 平台支持

| 平台 | 客户端 | 核心能力 |
|------|--------|----------|
| **RagFlow** | `RagFlowAIClient` | 知识库（Datasets）、文件管理、分块（Chunks）、会话（Sessions）、对话（Chat Completion / Session Chat）、Agent、Chat Assistant，Bearer Token 认证 |
| **Dify** | `DifyAIClient` | 发送聊天消息（`SendChatMsgAsync`，blocking 模式） |
| **Coze** | `CozeAIClient` | 工作流执行（Workflow）、执行历史（History） |

## 使用指南

### 1. 配置与构建

```csharp
// 通过 AIClientBuilder 构建客户端：Configure 接收 Action<AIOptions>
var client = new AIClientBuilder()
    .Configure(options =>
    {
        options.AIType = EnumAIType.Dify;          // 决定 Build() 产出哪个客户端
        options.BaseUrl = "https://api.dify.ai";
        options.ApiKey = "your-api-key";
    })
    .Build();

// 也可无参构造：自动读取 AIOptions 配置节（如 appsettings.json），再按需覆盖
var fromConfig = new AIClientBuilder().Build();
```

### 2. RagFlow 知识库与对话

```csharp
var ragflow = (RagFlowAIClient)client;

// 知识库管理
var datasets = await ragflow.ListDatasetsAsync(new ListDatasetsQuery { Page = 1, PageSize = 30 });
var files = await ragflow.ListFilesAsync(datasetId, new ListFilesQuery());

// 文档分块
var chunks = await ragflow.ListChunksAsync(datasetId, new ListChunksQuery());

// 会话与对话（以 chatId 为入口）
var session = await ragflow.CreateSessionAsync(chatId, new CreateSessionRequest { Name = "季度报告分析" });
var chatResponse = await ragflow.SessionChatAsync(chatId, session.Id, new SessionChatRequest { Message = "请分析这份报告" });
var completion = await ragflow.CreateChatCompletionAsync(chatId, new ChatCompletionRequest
{
    Messages = new List<ChatMessage> { new() { Role = "user", Content = "请分析这份报告" } }
});

// Agent 与 Assistant
var agents = await ragflow.ListAgentsAsync(new ListAgentsQuery());
var assistant = await ragflow.CreateChatAssistantAsync(new CreateChatAssistantRequest { Name = "报告助手", DataSetIds = new List<string> { datasetId } });
```

### 3. Dify 对话

```csharp
var dify = (DifyAIClient)client;

// 发送聊天消息（blocking 模式）
var response = await dify.SendChatMsgAsync(new ChatMessageRequest
{
    Query = "解释一下微服务架构的优缺点",
    ConversationId = conversationId   // 首轮留空，后续传 response.ConversationId 延续上下文
});

var answer = response.Answer;
```

### 4. Coze 工作流

```csharp
var coze = (CozeAIClient)client;

// 执行工作流（isAsync 默认 true，表示异步执行）
var result = coze.RunWorkflow("your-workflow-id", "分析数据");

// 查看执行历史
var history = coze.GetWorkflowHistory("your-workflow-id", result.execute_id);
```

## 小贴士

- 使用 `AIClientBuilder` 构建客户端，切换 AI 平台只需修改 `AIOptions.AIType`
- 各平台能力不同：`IAIClient` 只统一配置入口，收发方法由具体客户端提供，调用侧需按平台转型后使用
- RagFlow 客户端功能最丰富，涵盖知识库全生命周期管理（数据集 → 文件 → 分块 → 会话 → 对话）
- 所有平台客户端实现 `IAIClient` 接口，可依据 `Options` 做依赖注入与统一管理
- RagFlow 使用 Bearer Token 认证，确保配置中正确设置 API Key
- 结合 LuBan.AIAgent 可构建完整的 AI Agent 应用

## 许可证

Copyright (c) yswenli.

Licensed under the [Apache License 2.0](../LICENSE).
