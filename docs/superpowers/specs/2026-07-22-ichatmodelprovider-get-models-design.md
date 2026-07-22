# IChatModelProvider 获取模型列表功能设计

## 概述

为 `IChatModelProvider` 接口添加获取可用模型列表的功能。支持动态查询各 provider 的 API，返回模型 ID 及基础元数据。

## 目标

- 调用方可通过统一接口获取各 provider 支持的模型列表
- 支持 OpenAI 兼容 API 的 provider 自动获得实现
- 不支持的 provider 明确抛出异常

## 非目标

- 不在 `IChatClient` 层添加聚合方法
- 不实现模型能力筛选（如按上下文长度过滤）
- 不缓存模型列表

## 设计决策

| 决策点 | 选择 | 理由 |
|--------|------|------|
| 数据来源 | 动态查询 API | 获取最新模型列表 |
| 返回类型 | `IReadOnlyList<ModelInfo>` | 包含 ID 和基础元数据 |
| 接口位置 | 直接加在 `IChatModelProvider` | 简单统一 |
| 不支持场景 | 抛 `NotSupportedException` | 明确表达不支持 |
| 实现策略 | 基类默认实现 + 子类覆盖 | 复用 OpenAI 兼容逻辑 |

## 接口设计

### 新增类型

**`ModelInfo`**（`LuBan.AIAgent/Abstractions/ModelInfo.cs`）

```csharp
public record ModelInfo
{
    public string Id { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? OwnedBy { get; init; }
}
```

### 接口变更

**`IChatModelProvider`** 新增方法：

```csharp
Task<IReadOnlyList<ModelInfo>> GetModelsAsync(CancellationToken cancellationToken = default);
```

## 实现策略

### OpenAI 兼容 provider（9 个）

OpenAI、Kimi、GLM、Qwen、Deepseek、Doubao、HunYuan、ERNIE、AzureOpenAI

基类 `OpenAICompatibleProviderBase` 添加 `virtual GetModelsAsync()`：
- 调用 `{BaseUrl}models`
- 解析 OpenAI 格式响应
- AzureOpenAI 需 override（Azure 格式不同）

### 直接实现接口的 provider（3 个）

| Provider | 策略 |
|----------|------|
| Gemini | 调用 `https://generativelanguage.googleapis.com/v1beta/models?key=...` |
| Claude | 抛 `NotSupportedException`（无公开端点） |
| MiniMax | 抛 `NotSupportedException`（无公开端点） |

### 需 override 基类实现的 provider（1 个）

| Provider | 策略 |
|----------|------|
| AzureOpenAI | override 抛 `NotSupportedException`（Azure 格式不同） |

## 数据流

1. 调用方调用 `provider.GetModelsAsync(ct)`
2. OpenAI 兼容 provider：
   - 构造 URI：`{BaseUrl}models`
   - GET 请求（认证头由 `*RetryHttpMessageHandler` 注入）
   - 解析响应 → `IReadOnlyList<ModelInfo>`
3. 其他 provider 各自实现或抛异常

## 错误处理

| 场景 | 处理 |
|------|------|
| 网络错误/超时 | 由 `*RetryHttpMessageHandler` 重试；重试耗尽后异常冒泡 |
| HTTP 4xx/5xx | 抛 `HttpRequestException` 或 `InvalidOperationException` |
| 响应格式异常 | 抛 `InvalidOperationException("无法解析 {ProviderName} 的模型列表响应")` |
| 不支持的 provider | 抛 `NotSupportedException("{ProviderName} 不支持获取模型列表")` |

## 响应格式

### OpenAI 兼容格式

```json
{
  "object": "list",
  "data": [
    { "id": "gpt-4", "object": "model", "owned_by": "openai" },
    { "id": "gpt-3.5-turbo", "object": "model", "owned_by": "openai" }
  ]
}
```

映射：`id → Id`，`owned_by → OwnedBy`，`Name` 留 null。

### Gemini 格式

```json
{
  "models": [
    { "name": "models/gemini-pro", "supportedGenerationMethods": ["generateContent", "streamGenerateContent"] }
  ]
}
```

映射：`name → Id`（去除 `models/` 前缀），`Name = Id`，`OwnedBy = "google"`。

## 文件变更

### 新增文件

- `LuBan.AIAgent/Abstractions/ModelInfo.cs`
- `LuBan.XTestProject/ChatModelProviderGetModelsTest.cs`

### 修改文件

- `LuBan.AIAgent/Abstractions/IChatModelProvider.cs` — 添加 `GetModelsAsync` 方法
- `LuBan.AIAgent/Core/OpenAICompatibleProviderBase.cs` — 添加 `virtual GetModelsAsync` 实现
- `LuBan.AIAgent/Providers/Gemini/GeminiChatModelProvider.cs` — 实现 `GetModelsAsync`
- `LuBan.AIAgent/Providers/Claude/ClaudeChatModelProvider.cs` — 抛 `NotSupportedException`
- `LuBan.AIAgent/Providers/MiniMax/MiniMaxChatModelProvider.cs` — 抛 `NotSupportedException`
- `LuBan.AIAgent/Providers/AzureOpenAI/AzureOpenAIChatModelProvider.cs` — override 抛 `NotSupportedException`

## 测试

### 测试文件

`LuBan.XTestProject/ChatModelProviderGetModelsTest.cs`

### 测试用例

| 测试方法 | 覆盖场景 |
|----------|----------|
| `TestGLMGetModelsAsync` | OpenAI 兼容 provider 正常返回模型列表 |
| `TestClaudeGetModelsAsync` | 不支持的 provider 抛 `NotSupportedException` |

### 验收标准

- 构建通过，无新增警告
- GLM 测试 Pass：返回列表非空
- Claude 测试 Pass：抛出 `NotSupportedException`

## 约束

- 依赖真实 API 和网络环境
- 部分 provider 可能限制 `/models` 端点访问频率
- 模型列表可能随时间变化，测试断言需灵活
