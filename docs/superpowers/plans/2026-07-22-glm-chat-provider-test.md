# GLM 聊天模型提供者单元测试 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 参考 Kimi 测试方式，为 `LuBan.AIAgent.Providers.GLM` 新增同步与流式两个集成测试，并修复 `AddGLMProvider` 的 DI 注册使 `RequestTimeout` 真正作用于 `HttpClient.Timeout`。

**Architecture:** 新增一个测试类 `GLMChatModelProviderTest`，1:1 镜像现有 `KimiChatModelProviderTest` 的编排：通过 `ServiceProviderUtil.GetRequiredServiceForOnce<IChatModelProvider>` + `AddGLMProvider` 装配真实 HttpClient 调用 GLM 端点。同时给 `AddGLMProvider` 的 `AddHttpClient<GLMChatModelProvider>()` 链补上 `.ConfigureHttpClient(...)`，与 `AddKimiProvider` 对齐。改动边界严格限定为 1 个新增文件 + 1 个修改文件，不触碰 `GLMChatModelProvider`/`GLMOptions`/`GLMRetryHttpMessageHandler` 的实现。

**Tech Stack:** C# .NET 8、MSTest（`[TestClass]`/`[TestMethod]`/`Assert`，已 global using）、`Microsoft.Extensions.DependencyInjection`、`HttpClient`。测试项目 `LuBan.XTestProject.csproj` 已引用 `LuBan.AIAgent.csproj`（传递引用 `LuBan.DI`，因此 `ServiceProviderUtil` 在 `namespace System;` 下可直接调用）。

参考 spec：`docs/superpowers/specs/2026-07-22-glm-chat-provider-test-design.md`

---

## 文件结构

| 操作 | 路径 | 责任 |
| --- | --- | --- |
| 新增 | `LuBan.XTestProject/GLMChatModelProviderTest.cs` | GLM provider 同步 + 流式集成测试 |
| 修改 | `LuBan.AIAgent/Providers/GLM/DependencyInjection/ServiceCollectionExtensions.cs:23-29` | 在 `AddHttpClient<GLMChatModelProvider>()` 链上补 `.ConfigureHttpClient`，让 `RequestTimeout` 生效 |

不动文件：`GLMChatModelProvider.cs`、`GLMOptions.cs`、`GLMRetryHttpMessageHandler.cs`、`KimiChatModelProviderTest.cs`、`Kimi/*` 全部、`GlobalUsing.cs`。

---

## 约定

- 测试为集成测试，依赖真实 HTTPS 与有效 ApiKey（与 Kimi 测试同档）。
- 参数写死在测试代码中：`ApiKey="YOUR_API_KEY"`、`BaseUrl="https://opencode.ai/zen/go/v1"`、`ModelId="GLM-5.2"`、`RequestTimeout=TimeSpan.FromMinutes(2)`。
- 系统提示词统一：`你是 GLM，由智谱 AI 提供的人工智能助手，你更擅长中文和英文的对话。你会为用户提供安全，有帮助，准确的回答。`
- 提交风格沿用仓库中文 commit（参考 `git log` 示例：`feat:` / `test:` / `fixed`）。
- 不打 git tag、不推远端、不提 PR；除非用户显式要求。

---

## Task 1: 修复 `AddGLMProvider` 的 `ConfigureHttpClient` 缺失

**Files:**
- Modify: `LuBan.AIAgent/Providers/GLM/DependencyInjection/ServiceCollectionExtensions.cs:23-29`
- Test: 通过 Task 2 的测试间接验证（HttpClient.Timeout 行为变化不直接断言，但需要此修复保证 2 分钟超时不被默认 100s 截掉，流式长输出场景下尤其关键）

**为什么先做这一步：** 修复 DI 后 `RequestTimeout` 才会让测试中 `options.RequestTimeout = TimeSpan.FromMinutes(2)` 真正生效；否则 Task 2 的同步测试在长响应下可能先撞默认 100s 超时。

- [ ] **Step 1: 读取当前 `ServiceCollectionExtensions.cs` 确认起点**

Run: 读取 `LuBan.AIAgent/Providers/GLM/DependencyInjection/ServiceCollectionExtensions.cs`
Expected（关键片段，第 23-29 行）:
```csharp
services.AddHttpClient<GLMChatModelProvider>()
    .AddHttpMessageHandler(sp =>
    {
        var options = sp.GetRequiredService<IOptions<GLMOptions>>().Value;
        var logger = sp.GetService<Microsoft.Extensions.Logging.ILogger<GLMRetryHttpMessageHandler>>();
        return new GLMRetryHttpMessageHandler(options, logger);
    });
```
确认中间没有 `.ConfigureHttpClient(...)` 即为修复点。

- [ ] **Step 2: 在 `AddHttpClient<GLMChatModelProvider>()` 与 `.AddHttpMessageHandler(...)` 之间插入 `.ConfigureHttpClient(...)`**

替换 `services.AddHttpClient<GLMChatModelProvider>()\n            .AddHttpMessageHandler(sp =>` 这段为：

```csharp
        services.AddHttpClient<GLMChatModelProvider>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<GLMOptions>>().Value;
                client.Timeout = options.RequestTimeout;
            })
            .AddHttpMessageHandler(sp =>
```

修改后该 lambda 段应与 Kimi 版（`LuBan.AIAgent/Providers/Kimi/DependencyInjection/ServiceCollectionExtensions.cs:24-28`）结构一致。

- [ ] **Step 3: 构建验证**

Run: `dotnet build LuBan.AIAgent/LuBan.AIAgent.csproj`
Expected: 构建成功，无新增警告（现有项目可能存在的 warning 数量保持不变，重点看是否引入 "`IOptions<GLMOptions>` 解析" 相关 CS 警告 —— 不会，因为同文件下方同模式已存在）。

- [ ] **Step 4: 提交 DI 修复**

```bash
git add LuBan.AIAgent/Providers/GLM/DependencyInjection/ServiceCollectionExtensions.cs
git commit -m "fix: AddGLMProvider 补 ConfigureHttpClient 使 RequestTimeout 生效，与 Kimi 对齐"
```

---

## Task 2: 新增 `GLMChatModelProviderTest` 测试类（同步 + 流式）

**Files:**
- Create: `LuBan.XTestProject/GLMChatModelProviderTest.cs`
- Test: 本文件本身就是测试；将在 Step 4/Step 8 跑通过验证

> 说明：本项目的"测试先行"对集成测试而言是把测试代码本身作为待验证产物。Step 1 直接写出完整测试文件，Step 2 构建确认编译，Step 3/4 跑同步测试，Step 5/6/7/8 跑流式测试。每步都是可独立提交的最小进展。

- [ ] **Step 1: 写出完整测试文件 `GLMChatModelProviderTest.cs`**

Create `LuBan.XTestProject/GLMChatModelProviderTest.cs` 完整内容如下（1:1 镜像 `KimiChatModelProviderTest.cs` 结构，仅替换 provider 装配、参数、提示词与示例问题）:

```csharp
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Providers.GLM.DependencyInjection;

namespace LuBan.XTestProject;

/// <summary>
/// GLM 模型提供者测试
/// </summary>
[TestClass]
public class GLMChatModelProviderTest
{
    /// <summary>
    /// 测试 GLM 同步聊天
    /// </summary>
    [TestMethod]
    public async Task TestGLMCompleteAsync()
    {
        IChatModelProvider chatModelProvider = ServiceProviderUtil.GetRequiredServiceForOnce<IChatModelProvider>((services) =>
        {
            services.AddGLMProvider(options =>
            {
                options.ApiKey = "YOUR_API_KEY";
                options.BaseUrl = "https://opencode.ai/zen/go/v1";
                options.RequestTimeout = TimeSpan.FromMinutes(2); // 增加超时时间到2分钟
            });
        });

        // 构造测试请求
        var request = new ChatRequest
        {
            ModelId = "GLM-5.2",
            Messages = [
                ChatMessage.System("你是 GLM，由智谱 AI 提供的人工智能助手，你更擅长中文和英文的对话。你会为用户提供安全，有帮助，准确的回答。"),
                ChatMessage.User("请用通俗语言解释 Transformer 中的注意力机制")
            ],
            Options = new ChatOptions
            {
                Temperature = 1.0f,
                MaxTokens = 1000
            }
        };

        // 执行测试
        var response = await chatModelProvider.CompleteAsync(request);

        // 验证结果
        Assert.IsNotNull(response);
        if (!response.IsSuccess)
        {
            Console.WriteLine($"错误信息: {response.ErrorMessage}");
        }
        Assert.IsTrue(response.IsSuccess);
        Assert.IsNotNull(response.Message);
        Assert.IsNotNull(response.Message.TextContent);
        Assert.IsFalse(string.IsNullOrEmpty(response.Message.TextContent));

        // 输出结果
        Console.WriteLine("GLM 响应:");
        Console.WriteLine(response.Message.TextContent);
    }

    /// <summary>
    /// 测试 GLM 流式聊天
    /// </summary>
    [TestMethod]
    public async Task TestGLMStreamAsync()
    {
        IChatModelProvider chatModelProvider = ServiceProviderUtil.GetRequiredServiceForOnce<IChatModelProvider>((services) =>
        {
            services.AddGLMProvider(options =>
            {
                options.ApiKey = "YOUR_API_KEY";
                options.BaseUrl = "https://opencode.ai/zen/go/v1";
                options.RequestTimeout = TimeSpan.FromMinutes(2); // 增加超时时间到2分钟
            });
        });

        // 构造测试请求
        var request = new ChatRequest
        {
            ModelId = "GLM-5.2",
            Messages = [
                ChatMessage.System("你是 GLM，由智谱 AI 提供的人工智能助手，你更擅长中文和英文的对话。你会为用户提供安全，有帮助，准确的回答。"),
                ChatMessage.User("请简单解释什么是注意力机制")
            ],
            Options = new ChatOptions
            {
                Temperature = 1.0f,
                MaxTokens = 500
            }
        };

        // 执行流式测试
        var responseBuilder = new StringBuilder();

        await foreach (var update in chatModelProvider.StreamAsync(request))
        {
            if (update is TextDeltaUpdate textDelta)
            {
                responseBuilder.Append(textDelta.Delta);
                Console.Write(textDelta.Delta);
            }
        }

        var fullResponse = responseBuilder.ToString();

        // 验证结果
        Assert.IsFalse(string.IsNullOrEmpty(fullResponse));

        // 输出完整结果
        Console.WriteLine();
        Console.WriteLine("\n完整响应:");
        Console.WriteLine(fullResponse);
    }
}
```

要点确认（如不匹配立即停止并核对）:
- `ChatRequest` 是 `record` + `init` 属性，支持 `new ChatRequest { ... }`（`LuBan.AIAgent/Abstractions/ChatRequest.cs:6-22`）。
- `ChatMessage.System(...)` / `ChatMessage.User(...)` 工厂与 Kimi 测试用法一致（`KimiChatModelProviderTest.cs:33-34`）。
- `ChatOptions` 字段 `Temperature`(float) / `MaxTokens`(int) 与 Kimi 用法一致。
- `ServiceProviderUtil` 在 `namespace System;`（`LuBan.DI/ServiceProviderUtil.cs:26`），无需 using。
- `StringBuilder` 由 `GlobalUsing.cs:30` 的 `global using System.Text;` 提供。
- `IChatModelProvider` / `ChatMessage` / `ChatRequest` / `ChatOptions` / `TextDeltaUpdate` / `StreamingChatUpdate` 均在 `LuBan.AIAgent.Abstractions`，已显式 using。
- `AddGLMProvider` 在 `LuBan.AIAgent.Providers.GLM.DependencyInjection`，已显式 using。

- [ ] **Step 2: 构建测试项目，确认编译通过**

Run: `dotnet build LuBan.XTestProject/LuBan.XTestProject.csproj`
Expected: 构建成功，`GLMChatModelProviderTest.cs` 无错误无新增警告。若提示 `ServiceProviderUtil` 找不到，说明 `LuBan.DI` 未被传递引用——此种情况不应发生（`LuBan.AIAgent.csproj` 已 `ProjectReference` 到 `LuBan.DI`），如真发生需停下来核查 csproj 引用链。

- [ ] **Step 3: 只跑同步测试 `TestGLMCompleteAsync`**

Run: `dotnet test LuBan.XTestProject/LuBan.XTestProject.csproj --filter "FullyQualifiedName~GLMChatModelProviderTest.TestGLMCompleteAsync" --logger "console;verbosity=normal"`
Expected: `Passed: 1, Failed: 0`，控制台出现 "GLM 响应:" 与中文解释文本。

**失败排查分支**:
- 若失败信息形如 `401 Unauthorized` / `403 Forbidden`：ApiKey 失效或被限流；停止并请用户确认 ApiKey。
- 若失败信息为 `404`：BaseUrl/路径不匹配；`GLMChatModelProvider.GetBaseUrl()` 已返回 `GLMOptions.BaseUrl`，配合 `BuildRelativeUrl` 返回 `chat/completions`，最终 URL 应为 `https://opencode.ai/zen/go/v1/chat/completions`。如端点不同，停下来与用户确认 BaseUrl，不要在此 task 内修改生产代码。
- 若超时：确认 Task 1 的 `ConfigureHttpClient` 修复已提交生效；可临时把 `RequestTimeout` 提到 5 分钟复测。
- 其它错误：打印完整 `response.ErrorMessage` 再判断。

- [ ] **Step 4: 提交（同步测试通过）**

```bash
git add LuBan.XTestProject/GLMChatModelProviderTest.cs
git commit -m "test: 添加 GLMChatModelProviderTest 同步聊天测试 TestGLMCompleteAsync"
```

- [ ] **Step 5: 只跑流式测试 `TestGLMStreamAsync`**

Run: `dotnet test LuBan.XTestProject/LuBan.XTestProject.csproj --filter "FullyQualifiedName~GLMChatModelProviderTest.TestGLMStreamAsync" --logger "console;verbosity=normal"`
Expected: `Passed: 1, Failed: 0`，控制台逐块打印中文增量文本，末尾出现 "完整响应:" 与完整段落。

**失败排查分支**:
- 若流式无任何文本增量（断言 `fullResponse` 为空）：可能 SSE 字段名与 `OpenAICompatibleProviderBase` 期望不一致；先打印每行 `line` 诊断——但此调试动作超出本 plan 范围，应停下报告。
- 同 Task 2 Step 3 的 401/404/超时分支沿用。

- [ ] **Step 6: 提交（流式测试通过）**

注意：测试文件已在 Step 4 全量提交，此处若无代码改动，则本步跳过；若 Step 5 调试中改动了测试代码，则用以下命令追加补丁提交:

```bash
git add LuBan.XTestProject/GLMChatModelProviderTest.cs
git commit -m "test: 调整 GLMChatModelProviderTest 流式测试 TestGLMStreamAsync"
```

默认情况（无改动）直接进入 Step 7，不要制造空提交。

- [ ] **Step 7: 一次性跑全量 GLM 测试做回归**

Run: `dotnet test LuBan.XTestProject/LuBan.XTestProject.csproj --filter "FullyQualifiedName~GLMChatModelProviderTest" --logger "console;verbosity=normal"`
Expected: `Passed: 2, Failed: 0`。同时跑两次可验证两条测试各自独立装配 `IChatModelProvider`、互不污染（`GetRequiredServiceForOnce` 独立容器）。

- [ ] **Step 8: 终态确认**

Run:
- `dotnet build LuBan.XTestProject/LuBan.XTestProject.csproj` → 无错误无新增警告
- `git status` → working tree clean
- `git log --oneline -3` → 顶部按时间顺序应能看到本次 3 个提交（DI 修复 + 同步测试 + 可选的流式补丁）

符合即视为本 plan 完成。**不要** 修改 `docs/` 内的 spec/plan 文件，除非用户审查中发现错误并显式告知。

---

## Self-Review

**1. Spec coverage:**
- §2 测试参数 → Task 2 Step 1 代码块逐字落实（ApiKey/BaseUrl/ModelId/RequestTimeout）。
- §3.1 新增测试文件 → Task 2 Step 1 完整给出文件内容。
- §3.1 同步测试 `TestGLMCompleteAsync` → Task 2 Step 1/3/4。
- §3.1 流式测试 `TestGLMStreamAsync` → Task 2 Step 1/5/6。
- §3.2 DI 修复（`ConfigureHttpClient`）→ Task 1 全程。
- §3.3 不改动清单 → "文件结构" 表 + Task 1/2 仅触及 2 个文件。
- §4 数据流 → 通过 Task 2 Step 3/5 的真实调用被端到端验证。
- §5.1 命令 → 每个 Step 的 `Run:` 与 §5.1 完全一致。
- §5.2 验收标准 → Step 7（Passed: 2）+ Step 8 终态确认覆盖 `git diff` 范围；构建无新增警告覆盖「构建通过」。
- §5.3 不提交除非显式 → 已写入 Task 2 Step 6 默认不创建空提交、全 plan 不打 tag/不推 PR。

**2. Placeholder scan:** 无 TBD/TODO/「适当的错误处理」/「类似 Task N」/无代码块纯描述步骤。失败排查分支给出具体状态码与处置，非占位符。

**3. Type consistency:**
- `ChatRequest` / `ChatMessage.System` / `ChatMessage.User` / `ChatOptions.Temperature`(float 1.0f) / `ChatOptions.MaxTokens`(int) / `IChatModelProvider.CompleteAsync` / `IChatModelProvider.StreamAsync` / `TextDeltaUpdate` / `StreamingChatUpdate` 名字一致（与 `KimiChatModelProviderTest.cs` 一一对应）。
- `AddGLMProvider` / `GLMOptions.ApiKey` / `BaseUrl` / `RequestTimeout` / `MaxRetryCount`(未用) / `InitialRetryDelay`(未用) 与 spec §2 一致。
- `ConfigureHttpClient((sp, client) => ...)` 签名与 Kimi 版 `ServiceCollectionExtensions.cs:24-28` 一致。

无待修复项。Plan 可执行。