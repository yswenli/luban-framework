# GLM 聊天模型提供者单元测试设计

- 日期：2026-07-22
- 主题：为 `LuBan.AIAgent.Providers.GLM` 添加单元测试，参考现有 Kimi 测试方式
- 状态：已与用户 brainstorming 确认，待 writing-plans 据此产出实现计划

## 1. 背景与目标

项目中已存在完整的 GLM 提供者（`LuBan.AIAgent/Providers/GLM/`：`GLMChatModelProvider`、`GLMOptions`、`GLMRetryHttpMessageHandler`、`AddGLMProvider` DI 扩展），以及作为参考的 Kimi 集成测试 `LuBan.XTestProject/KimiChatModelProviderTest.cs`（含同步 `TestKimiCompleteAsync` 与流式 `TestKimiStreamAsync` 两个用例）。

GLM 提供者目前缺少同等级别的测试覆盖。本次任务参考 Kimi 测试的编排方式，新增 `GLMChatModelProviderTest`，覆盖 GLM 同步与流式两条链路，并顺带修复 `AddGLMProvider` 中一处与 Kimi 不一致的 DI 注册，使 `GLMOptions.RequestTimeout` 真正作用于 `HttpClient.Timeout`。

超出范围：
- 不抽取 Kimi/GLM 共享测试基类或重试处理器基类。
- 不改动 `GLMChatModelProvider`、`GLMOptions`、`GLMRetryHttpMessageHandler` 的实现逻辑。
- 不改动 Kimi 或其它任何 provider 的代码。
- 不引入 mock、不读 appsettings.json、不加 CI 条件跳过机制。

## 2. 测试参数

固定写在测试代码中（与 Kimi 现状一致，不外置到配置）：

| 项 | 值 |
| --- | --- |
| `ApiKey` | `YOUR_API_KEY` |
| `BaseUrl` | `https://opencode.ai/zen/go/v1` |
| `ModelId` | `GLM-5.2` |
| `RequestTimeout` | `TimeSpan.FromMinutes(2)` |

系统提示词统一为：`你是 GLM，由智谱 AI 提供的人工智能助手，你更擅长中文和英文的对话。你会为用户提供安全，有帮助，准确的回答。`

## 3. 文件结构与改动清单

### 3.1 新增（1 个文件）

`LuBan.XTestProject/GLMChatModelProviderTest.cs`

- 命名空间 `LuBan.XTestProject`，`[TestClass] public class GLMChatModelProviderTest`。
- `using LuBan.AIAgent.Abstractions;` + `using LuBan.AIAgent.Providers.GLM.DependencyInjection;`。
- 两个 `[TestMethod]`：

**`TestGLMCompleteAsync`（同步）**
- 装配：
  ```csharp
  IChatModelProvider chatModelProvider = ServiceProviderUtil.GetRequiredServiceForOnce<IChatModelProvider>(services =>
  {
      services.AddGLMProvider(options =>
      {
          options.ApiKey = "YOUR_API_KEY";
          options.BaseUrl = "https://opencode.ai/zen/go/v1";
          options.RequestTimeout = TimeSpan.FromMinutes(2);
      });
  });
  ```
- 请求：
  - `ModelId = "GLM-5.2"`
  - `Messages = [ ChatMessage.System("你是 GLM，由智谱 AI 提供..."), ChatMessage.User("请用通俗语言解释 Transformer 中的注意力机制") ]`
  - `Options = new ChatOptions { Temperature = 1.0f, MaxTokens = 1000 }`
- 调用 `await chatModelProvider.CompleteAsync(request)`，断言：
  - `response != null`
  - 失败时 `Console.WriteLine($"错误信息: {response.ErrorMessage}")` 后断言
  - `Assert.IsTrue(response.IsSuccess)`
  - `response.Message != null`、`response.Message.TextContent != null`、`!string.IsNullOrEmpty(response.Message.TextContent)`
  - 成功时打印 `GLM 响应:` + `response.Message.TextContent`

**`TestGLMStreamAsync`（流式）**
- 装配：与同步测试相同的 `AddGLMProvider` 配置。
- 请求：`ModelId = "GLM-5.2"`，`MaxTokens = 500`，User 改为 `请简单解释什么是注意力机制`。
- `await foreach (var update in chatModelProvider.StreamAsync(request))`，对 `TextDeltaUpdate` 用 `StringBuilder` 追加并 `Console.Write(textDelta.Delta)`。
- 断言：`!string.IsNullOrEmpty(fullResponse)`；末尾打印空行 + `完整响应:` + 全文。

### 3.2 修改（1 个文件，1 处）

`LuBan.AIAgent/Providers/GLM/DependencyInjection/ServiceCollectionExtensions.cs`

在 `AddHttpClient<GLMChatModelProvider>()`（当前第 23 行）与 `.AddHttpMessageHandler(...)` 之间插入 `.ConfigureHttpClient(...)`，使其与 Kimi 版（`Providers/Kimi/DependencyInjection/ServiceCollectionExtensions.cs:24-28`）对齐：

```csharp
services.AddHttpClient<GLMChatModelProvider>()
    .ConfigureHttpClient((sp, client) =>
    {
        var options = sp.GetRequiredService<IOptions<GLMOptions>>().Value;
        client.Timeout = options.RequestTimeout;
    })
    .AddHttpMessageHandler(sp =>
    {
        var options = sp.GetRequiredService<IOptions<GLMOptions>>().Value;
        var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<GLMRetryHttpMessageHandler>>();
        return new GLMRetryHttpMessageHandler(options, logger);
    });
```

### 3.3 不改动

`GLMChatModelProvider.cs`、`GLMOptions.cs`、`GLMRetryHttpMessageHandler.cs`、Kimi 相关任何文件、`GlobalUsing.cs`、其它 provider。

## 4. 数据流与预期行为

### 4.1 装配阶段

1. `ServiceProviderUtil.GetRequiredServiceForOnce<IChatModelProvider>` 构建临时 DI 容器。
2. `AddGLMProvider`：
   - `Configure<GLMOptions>` 写入 ApiKey/BaseUrl/RequestTimeout。
   - `AddHttpClient<GLMChatModelProvider>`：
     - `ConfigureHttpClient` 把 `HttpClient.Timeout` 设为 2 分钟（**本次修复点**）。
     - `AddHttpMessageHandler` 挂载 `GLMRetryHttpMessageHandler`（消息处理器链不变）。
   - `AddSingleton<IChatModelProvider>` 转发到 `GLMChatModelProvider`。
3. 容器解析出 `GLMChatModelProvider`，基类以蛇形请求命名 + 驼峰响应命名的 `JsonSerializerOptions` 初始化。

### 4.2 同步测试 `TestGLMCompleteAsync`

4. 构造 `ChatRequest`，`ModelId="GLM-5.2"`，System + User（注意力机制问题），`Options{Temperature=1.0f, MaxTokens=1000}`。
5. `CompleteAsync` → `CreateProviderRequest`（`CreateBaseRequest(includeModel:true)`）→ 蛇形 JSON 序列化 → POST 到 `https://opencode.ai/zen/go/v1/chat/completions`（Base 来自 `GLMChatModelProvider.GetBaseUrl()` 返回 `GLMOptions.BaseUrl`）。
6. `GLMRetryHttpMessageHandler` 注入 `Authorization: Bearer <ApiKey>`；暂时性失败（429 / 5xx）按指数退避最多 3 次重试。
7. 成功响应 → 驼峰反序列化为 `OpenAICompatibleChatCompletionResponse` → `MapFromResponse` → `ChatResponse`。
8. 断言 `IsSuccess` 与 `Message.TextContent` 非空；失败时先打印 `ErrorMessage` 再断言失败；成功打印响应文本。

### 4.3 流式测试 `TestGLMStreamAsync`

9. 同步构造请求，`MaxTokens=500`，User 改为 `请简单解释什么是注意力机制`。
10. `StreamAsync` 发送 `stream=true` 请求 → SSE 增量 → `TextDeltaUpdate` 逐块 `StringBuilder` 追加并 `Console.Write`。
11. 断言拼接结果非空；末尾打印空行 + `完整响应:` + 全文。

### 4.4 测试定位

集成测试，依赖真实 HTTPS 与有效 ApiKey，与 Kimi 测试同等定位；不引入 mock、不读 appsettings.json、不加 CI 条件跳过。

### 4.5 DI 修复带来的行为变化

修复后 `HttpClient.Timeout=2 分钟` 生效；与 `GLMRetryHttpMessageHandler` 内部 CTS（同样 2 分钟）并存时，HttpClient 先超时抛 `TaskCanceledException`，被现有 `OperationCanceledException` 分支捕获并重试 —— 与 Kimi 修复后语义一致，不引入新分支。

## 5. 验证与执行

### 5.1 命令

- 构建：`dotnet build LuBan.XTestProject/LuBan.XTestProject.csproj`
- 只跑 GLM 测试：`dotnet test LuBan.XTestProject/LuBan.XTestProject.csproj --filter "FullyQualifiedName~GLMChatModelProviderTest" --logger "console;verbosity=normal"`

### 5.2 验收标准

- 构建通过，无编译警告引入。
- 两个 GLM 测试全部 Pass（`Passed: 2, Failed: 0`），控制台出现 GLM 返回的中文文本。
- 网络或配额导致失败时，错误信息可读且仅来自 HTTP 层，不来自本次新增/修改的代码路径。
- `git diff` 仅含 1 个新增文件 + 1 个修改文件；Kimi 与其它 provider 零改动。

### 5.3 过门检查

- 此仓库无专用 lint 脚本，以 `dotnet build` 无警告为准；`csproj` 未见 `TreatWarningsAsErrors`，遵循默认。
- 不提交，除非用户后续显式要求。

## 6. 风险与取舍

- **测试可重复性**：依赖真实网络与 API Key 有效性，与 Kimi 测试同档，属既有约定，不作为本次新增风险处理。
- **DI 修复的副作用**：`HttpClient.Timeout` 从默认（100s）变短为 2 分钟；不会缩短既有调用路径，但会让超时配置真正可控。这正是修复意图。
- **并发装配**：GLM 与 Kimi 测试各自通过 `GetRequiredServiceForOnce` 独立容器装配，`IChatModelProvider` 单例互不污染（来自 `GetRequiredServiceForOnce` 现有语义，本次不改）。

## 7. 参考实现锚点

- 上游模板：`LuBan.XTestProject/KimiChatModelProviderTest.cs:1`
- 同步装配参考：`KimiChatModelProviderTest.cs:18-26`
- 流式装配参考：`KimiChatModelProviderTest.cs:66-76`
- Kimi DI 参考（`ConfigureHttpClient`）：`LuBan.AIAgent/Providers/Kimi/DependencyInjection/ServiceCollectionExtensions.cs:24-28`
- 待修改的 GLM DI：`LuBan.AIAgent/Providers/GLM/DependencyInjection/ServiceCollectionExtensions.cs:23`
- 提供者基类：`LuBan.AIAgent/Core/OpenAICompatibleProviderBase.cs:12`
- GLM 提供者：`LuBan.AIAgent/Providers/GLM/GLMChatModelProvider.cs:10`
- 重试处理器：`LuBan.AIAgent/Providers/GLM/GLMRetryHttpMessageHandler.cs:9`