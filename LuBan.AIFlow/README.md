[English](README.en.md) | 中文

# LuBan.AIFlow

> **作者**: yswenli | **代码仓库**: [https://github.com/yswenli/luban-framework](https://github.com/yswenli/luban-framework)

> 一个接口对接多个 AI 平台，RagFlow / Dify / Coze 即插即用。

---
**Related Projects**: [LuBan.Framework](../README.md) | [LuBan.Common](../LuBan.Common/README.md) | [LuBan.AIAgent](../LuBan.AIAgent/README.md) | [LuBan.Web.Core](../LuBan.Web.Core/README.md)
---

## 为什么需要它？

- 每接入一个 AI 平台就要写一套完全不同的 SDK 调用逻辑？
- 切换 AI 供应商需要改动大量业务代码？
- RAG 知识库管理、会话对话、Agent 编排分散在不同模块？

LuBan.AIFlow 通过统一的 `IAIClient` 接口与 Builder 模式，将 RagFlow、Dify、Coze 三大 AI 平台封装为一致的 API 体验。一套代码，多平台切换，零业务代码改动。

## 快速预览

```csharp
// 构建 AI 客户端
var client = new AIClientBuilder()
    .Configure(config)
    .Build();

// 对话
var response = await client.ChatAsync("帮我分析一下这段代码的性能问题");

// 知识库管理（RagFlow）
var datasets = await client.GetDatasetsAsync();
var chunks = await client.GetChunksAsync(datasetId, fileId);
```

## 技术栈

| 组件 | 说明 |
|------|------|
| ASP.NET Core | Web 框架基础 |
| LuBan.Common | 基础接口与枚举定义（EnumAIType） |
| Builder 模式 | 配置驱动的客户端构建 |
| 多平台 API | RagFlow / Dify / Coze REST API 封装 |

## 安装

```xml
<PackageReference Include="LuBan.AIFlow" Version="*" />
```

## 功能总览

### 核心架构

| 组件 | 说明 |
|------|------|
| `IAIClient` | 统一 AI 客户端接口，暴露 `AIOptions` 配置 |
| `AIClientBuilder` | Builder 模式构建器，读取配置并按 `EnumAIType` 分发 |
| `EnumAIType` | AI 平台枚举（RagFlow / Dify / Coze） |

### 平台支持

| 平台 | 客户端 | 核心能力 |
|------|--------|----------|
| **RagFlow** | `RagFlowAIClient` | 知识库（Datasets）、文件管理、分块（Chunks）、会话（Sessions）、对话（Chat）、Agent、Chat Assistant，Bearer Token 认证 |
| **Dify** | `DifyAIClient` | 对话补全（Chat Completion）、消息管理 |
| **Coze** | `CozeAIClient` | 工作流执行（Workflow）、执行历史（History） |

## 使用指南

### 1. 配置与构建

```csharp
// 通过 AIClientBuilder 构建客户端
var client = new AIClientBuilder()
    .Configure(configuration)  // 传入 IConfiguration
    .Build();                  // 根据配置中的 EnumAIType 自动选择实现
```

### 2. RagFlow 知识库与对话

```csharp
// 知识库管理
var datasets = await client.GetDatasetsAsync();
var files = await client.GetFilesAsync(datasetId);

// 文档分块
var chunks = await client.GetChunksAsync(datasetId, fileId);

// 会话与对话
var session = await client.CreateSessionAsync(sessionName);
var chatResponse = await client.ChatAsync(sessionId, "请分析这份报告");

// Agent 与 Assistant
var agents = await client.GetAgentsAsync();
var assistant = await client.CreateChatAssistantAsync(config);
```

### 3. Dify 对话

```csharp
// 对话补全
var response = await client.ChatCompletionAsync(new ChatRequest
{
    Query = "解释一下微服务架构的优缺点",
    ConversationId = conversationId
});

// 消息管理
var messages = await client.GetMessagesAsync(conversationId);
```

### 4. Coze 工作流

```csharp
// 执行工作流
var result = await client.ExecuteWorkflowAsync(new WorkflowRequest
{
    WorkflowId = "your-workflow-id",
    Parameters = new { input = "分析数据" }
});

// 查看执行历史
var history = await client.GetWorkflowHistoryAsync(workflowId);
```

## 小贴士

- 使用 `AIClientBuilder` 构建客户端，切换 AI 平台只需修改配置，无需改动业务代码
- RagFlow 客户端功能最丰富，涵盖知识库全生命周期管理（数据集 → 文件 → 分块 → 会话 → 对话）
- 所有平台客户端实现 `IAIClient` 接口，可依赖注入灵活替换
- RagFlow 使用 Bearer Token 认证，确保配置中正确设置 API Key
- 结合 LuBan.AIAgent 可构建完整的 AI Agent 应用

## 许可证

MIT
