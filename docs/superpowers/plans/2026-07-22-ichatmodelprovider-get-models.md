# IChatModelProvider GetModelsAsync Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 为 `IChatModelProvider` 接口添加 `GetModelsAsync` 方法，支持动态查询各 provider 的可用模型列表。

**Architecture:** 在 `IChatModelProvider` 接口新增 `GetModelsAsync` 方法，返回 `IReadOnlyList<ModelInfo>`。`OpenAICompatibleProviderBase` 提供 virtual 默认实现（调用 `/models` 端点），9 个 OpenAI 兼容 provider 自动获得能力。Gemini 独立实现，Claude/MiniMax/MiMo/AzureOpenAI 抛 `NotSupportedException`。

**Tech Stack:** C# .NET 8、`System.Text.Json`、MSTest（集成测试）

**Spec:** `docs/superpowers/specs/2026-07-22-ichatmodelprovider-get-models-design.md`

---

## 文件结构

| 操作 | 路径 | 责任 |
| --- | --- | --- |
| 新增 | `LuBan.AIAgent/Abstractions/ModelInfo.cs` | 模型信息 record |
| 修改 | `LuBan.AIAgent/Abstractions/IChatModelProvider.cs` | 添加 `GetModelsAsync` 方法签名 |
| 修改 | `LuBan.AIAgent/Core/OpenAICompatibleProviderBase.cs` | 添加 virtual `GetModelsAsync` 实现 |
| 修改 | `LuBan.AIAgent/Providers/Gemini/GeminiChatModelProvider.cs` | 实现 `GetModelsAsync` |
| 修改 | `LuBan.AIAgent/Providers/Claude/ClaudeChatModelProvider.cs` | 抛 `NotSupportedException` |
| 修改 | `LuBan.AIAgent/Providers/MiniMax/MiniMaxChatModelProvider.cs` | 抛 `NotSupportedException` |
| 修改 | `LuBan.AIAgent/Providers/MiMo/MiMoChatModelProvider.cs` | 抛 `NotSupportedException` |
| 修改 | `LuBan.AIAgent/Providers/AzureOpenAI/AzureOpenAIChatModelProvider.cs` | override 抛 `NotSupportedException` |
| 新增 | `LuBan.XTestProject/ChatModelProviderGetModelsTest.cs` | 集成测试 |

---

## Task 1: 新增 `ModelInfo` record 并修改 `IChatModelProvider` 接口

**Files:**
- Create: `LuBan.AIAgent/Abstractions/ModelInfo.cs`
- Modify: `LuBan.AIAgent/Abstractions/IChatModelProvider.cs`

- [ ] **Step 1: 创建 `ModelInfo.cs`**

```csharp
namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 模型信息，包含模型的基本元数据
/// </summary>
public record ModelInfo
{
    /// <summary>
    /// 模型 ID
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// 模型名称
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// 模型所有者
    /// </summary>
    public string? OwnedBy { get; init; }
}
```

- [ ] **Step 2: 修改 `IChatModelProvider.cs`，添加 `GetModelsAsync` 方法**

在 `IChatModelProvider` 接口的 `StreamAsync` 方法之后添加：

```csharp
    /// <summary>
    /// 异步获取该提供者支持的模型列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>模型信息列表</returns>
    /// <exception cref="NotSupportedException">当提供者不支持获取模型列表时抛出</exception>
    Task<IReadOnlyList<ModelInfo>> GetModelsAsync(CancellationToken cancellationToken = default);
```

- [ ] **Step 3: 构建验证（预期失败）**

Run: `dotnet build LuBan.AIAgent/LuBan.AIAgent.csproj`
Expected: 编译失败，所有 `IChatModelProvider` 实现类报 CS0535 错误（未实现接口成员 `GetModelsAsync`）

- [ ] **Step 4: 提交接口变更**

```bash
git add LuBan.AIAgent/Abstractions/ModelInfo.cs LuBan.AIAgent/Abstractions/IChatModelProvider.cs
git commit -m "feat: 添加 ModelInfo 类型和 IChatModelProvider.GetModelsAsync 接口方法"
```

---

## Task 2: 在 `OpenAICompatibleProviderBase` 实现 `GetModelsAsync`

**Files:**
- Modify: `LuBan.AIAgent/Core/OpenAICompatibleProviderBase.cs`

- [ ] **Step 1: 在 `OpenAICompatibleProviderBase` 类中添加内部 DTO 和 `GetModelsAsync` 方法**

在类的末尾（`GetInvalidResponseMessage` 等 abstract 方法之后）添加：

```csharp
    /// <summary>
    /// 异步获取该提供者支持的模型列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>模型信息列表</returns>
    public virtual async Task<IReadOnlyList<ModelInfo>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var baseUrl = GetBaseUrl();
        var absoluteUri = new Uri(new Uri(baseUrl), "models");

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, absoluteUri);
        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"获取模型列表失败: {response.StatusCode}: {errorContent}");
        }

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var modelsResponse = JsonSerializer.Deserialize<OpenAICompatibleModelsResponse>(responseJson, _responseJsonOptions);

        if (modelsResponse?.Data == null)
        {
            throw new InvalidOperationException($"无法解析 {ProviderName} 的模型列表响应");
        }

        return modelsResponse.Data.Select(m => new ModelInfo
        {
            Id = m.Id,
            OwnedBy = m.OwnedBy
        }).ToList();
    }

    private class OpenAICompatibleModelsResponse
    {
        public List<OpenAICompatibleModelItem>? Data { get; set; }
    }

    private class OpenAICompatibleModelItem
    {
        public string Id { get; set; } = string.Empty;
        public string? OwnedBy { get; set; }
    }
```

- [ ] **Step 2: 构建验证**

Run: `dotnet build LuBan.AIAgent/LuBan.AIAgent.csproj`
Expected: 编译失败，仍有 4 个直接实现 `IChatModelProvider` 的类未实现 `GetModelsAsync`（Gemini、Claude、MiniMax、MiMo）

- [ ] **Step 3: 提交基类实现**

```bash
git add LuBan.AIAgent/Core/OpenAICompatibleProviderBase.cs
git commit -m "feat: OpenAICompatibleProviderBase 添加 virtual GetModelsAsync 实现"
```

---

## Task 3: 实现 `GeminiChatModelProvider.GetModelsAsync`

**Files:**
- Modify: `LuBan.AIAgent/Providers/Gemini/GeminiChatModelProvider.cs`

- [ ] **Step 1: 在 `GeminiChatModelProvider` 类中添加 `GetModelsAsync` 方法**

在 `StreamAsync` 方法之后添加：

```csharp
    /// <summary>
    /// 异步获取该提供者支持的模型列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>模型信息列表</returns>
    public async Task<IReadOnlyList<ModelInfo>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var baseUrl = GetBaseUrl();
        var apiKey = _httpClient.DefaultRequestHeaders.GetValues("x-goog-api-key").First();
        var absoluteUri = new Uri($"{baseUrl}models?key={apiKey}");

        var response = await _httpClient.GetAsync(absoluteUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"获取模型列表失败: {response.StatusCode}: {errorContent}");
        }

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var modelsResponse = JsonSerializer.Deserialize<GeminiModelsResponse>(responseJson, _jsonOptions);

        if (modelsResponse?.Models == null)
        {
            throw new InvalidOperationException("无法解析 Gemini 的模型列表响应");
        }

        return modelsResponse.Models.Select(m =>
        {
            var id = m.Name?.Replace("models/", "") ?? string.Empty;
            return new ModelInfo
            {
                Id = id,
                Name = id,
                OwnedBy = "google"
            };
        }).ToList();
    }

    private class GeminiModelsResponse
    {
        public List<GeminiModelItem>? Models { get; set; }
    }

    private class GeminiModelItem
    {
        public string? Name { get; set; }
    }
```

- [ ] **Step 2: 构建验证**

Run: `dotnet build LuBan.AIAgent/LuBan.AIAgent.csproj`
Expected: 编译失败，仍有 3 个类未实现 `GetModelsAsync`（Claude、MiniMax、MiMo）

- [ ] **Step 3: 提交 Gemini 实现**

```bash
git add LuBan.AIAgent/Providers/Gemini/GeminiChatModelProvider.cs
git commit -m "feat: GeminiChatModelProvider 实现 GetModelsAsync"
```

---

## Task 4: 实现 Claude/MiniMax/MiMo 的 `NotSupportedException`

**Files:**
- Modify: `LuBan.AIAgent/Providers/Claude/ClaudeChatModelProvider.cs`
- Modify: `LuBan.AIAgent/Providers/MiniMax/MiniMaxChatModelProvider.cs`
- Modify: `LuBan.AIAgent/Providers/MiMo/MiMoChatModelProvider.cs`

- [ ] **Step 1: 在 `ClaudeChatModelProvider` 中添加 `GetModelsAsync`**

在类的末尾添加：

```csharp
    /// <summary>
    /// 异步获取该提供者支持的模型列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>模型信息列表</returns>
    /// <exception cref="NotSupportedException">Claude 不支持获取模型列表</exception>
    public Task<IReadOnlyList<ModelInfo>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Claude 不支持获取模型列表");
    }
```

- [ ] **Step 2: 在 `MiniMaxChatModelProvider` 中添加 `GetModelsAsync`**

在类的末尾添加：

```csharp
    /// <summary>
    /// 异步获取该提供者支持的模型列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>模型信息列表</returns>
    /// <exception cref="NotSupportedException">MiniMax 不支持获取模型列表</exception>
    public Task<IReadOnlyList<ModelInfo>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("MiniMax 不支持获取模型列表");
    }
```

- [ ] **Step 3: 在 `MiMoChatModelProvider` 中添加 `GetModelsAsync`**

在类的末尾添加：

```csharp
    /// <summary>
    /// 异步获取该提供者支持的模型列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>模型信息列表</returns>
    /// <exception cref="NotSupportedException">MiMo 不支持获取模型列表</exception>
    public Task<IReadOnlyList<ModelInfo>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("MiMo 不支持获取模型列表");
    }
```

- [ ] **Step 4: 构建验证**

Run: `dotnet build LuBan.AIAgent/LuBan.AIAgent.csproj`
Expected: 编译成功，0 错误

- [ ] **Step 5: 提交**

```bash
git add LuBan.AIAgent/Providers/Claude/ClaudeChatModelProvider.cs LuBan.AIAgent/Providers/MiniMax/MiniMaxChatModelProvider.cs LuBan.AIAgent/Providers/MiMo/MiMoChatModelProvider.cs
git commit -m "feat: Claude/MiniMax/MiMo 实现 GetModelsAsync 抛出 NotSupportedException"
```

---

## Task 5: AzureOpenAI override 抛 `NotSupportedException`

**Files:**
- Modify: `LuBan.AIAgent/Providers/AzureOpenAI/AzureOpenAIChatModelProvider.cs`

- [ ] **Step 1: 在 `AzureOpenAIChatModelProvider` 中添加 `GetModelsAsync` override**

在 `GetBaseUrl` 方法之后添加：

```csharp
    /// <summary>
    /// 异步获取该提供者支持的模型列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>模型信息列表</returns>
    /// <exception cref="NotSupportedException">Azure OpenAI 不支持获取模型列表</exception>
    public override Task<IReadOnlyList<ModelInfo>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Azure OpenAI 不支持获取模型列表");
    }
```

- [ ] **Step 2: 构建验证**

Run: `dotnet build LuBan.AIAgent/LuBan.AIAgent.csproj`
Expected: 编译成功，0 错误

- [ ] **Step 3: 提交**

```bash
git add LuBan.AIAgent/Providers/AzureOpenAI/AzureOpenAIChatModelProvider.cs
git commit -m "feat: AzureOpenAIChatModelProvider override GetModelsAsync 抛出 NotSupportedException"
```

---

## Task 6: 编写集成测试

**Files:**
- Create: `LuBan.XTestProject/ChatModelProviderGetModelsTest.cs`

- [ ] **Step 1: 创建测试文件**

```csharp
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Providers.GLM.DependencyInjection;

namespace LuBan.XTestProject;

/// <summary>
/// 模型提供者获取模型列表测试
/// </summary>
[TestClass]
public class ChatModelProviderGetModelsTest
{
    /// <summary>
    /// 测试 GLM 获取模型列表（OpenAI 兼容 provider 正常返回）
    /// </summary>
    [TestMethod]
    public async Task TestGLMGetModelsAsync()
    {
        IChatModelProvider chatModelProvider = ServiceProviderUtil.GetRequiredServiceForOnce<IChatModelProvider>((services) =>
        {
            services.AddGLMProvider(options =>
            {
                options.ApiKey = "YOUR_API_KEY";
                options.BaseUrl = "https://opencode.ai/zen/go/v1/";
                options.RequestTimeout = TimeSpan.FromMinutes(2);
            });
        });

        var models = await chatModelProvider.GetModelsAsync();

        Assert.IsNotNull(models);
        Assert.IsTrue(models.Count > 0, "模型列表不应为空");

        Console.WriteLine($"GLM 返回 {models.Count} 个模型:");
        foreach (var model in models)
        {
            Console.WriteLine($"  - {model.Id} (owned_by: {model.OwnedBy ?? "unknown"})");
        }
    }

    /// <summary>
    /// 测试 Claude 获取模型列表（不支持的 provider 抛异常）
    /// </summary>
    [TestMethod]
    public void TestClaudeGetModelsAsync()
    {
        var provider = new LuBan.AIAgent.Providers.Claude.ClaudeChatModelProvider(
            new HttpClient(),
            Microsoft.Extensions.Options.Options.Create(new LuBan.AIAgent.Providers.Claude.ClaudeOptions
            {
                ApiKey = "test-key"
            }));

        var exception = Assert.ThrowsException<NotSupportedException>(() =>
        {
            provider.GetModelsAsync().GetAwaiter().GetResult();
        });

        Assert.IsTrue(exception.Message.Contains("Claude 不支持获取模型列表"));
    }
}
```

- [ ] **Step 2: 构建验证**

Run: `dotnet build LuBan.XTestProject/LuBan.XTestProject.csproj`
Expected: 编译成功，0 错误

- [ ] **Step 3: 运行 GLM 测试**

Run: `dotnet test LuBan.XTestProject/LuBan.XTestProject.csproj --filter "FullyQualifiedName~ChatModelProviderGetModelsTest.TestGLMGetModelsAsync" --logger "console;verbosity=normal"`
Expected: Pass（返回列表非空，控制台打印模型 ID）

- [ ] **Step 4: 运行 Claude 测试**

Run: `dotnet test LuBan.XTestProject/LuBan.XTestProject.csproj --filter "FullyQualifiedName~ChatModelProviderGetModelsTest.TestClaudeGetModelsAsync" --logger "console;verbosity=normal"`
Expected: Pass（抛出 `NotSupportedException`）

- [ ] **Step 5: 提交测试**

```bash
git add LuBan.XTestProject/ChatModelProviderGetModelsTest.cs
git commit -m "test: 添加 GetModelsAsync 集成测试"
```

---

## Task 7: 全量验证

- [ ] **Step 1: 全量构建**

Run: `dotnet build LuBan.XTestProject/LuBan.XTestProject.csproj`
Expected: 编译成功，无新增警告

- [ ] **Step 2: 运行所有 GetModels 测试**

Run: `dotnet test LuBan.XTestProject/LuBan.XTestProject.csproj --filter "FullyQualifiedName~ChatModelProviderGetModelsTest" --logger "console;verbosity=normal"`
Expected: `Passed: 2, Failed: 0`

- [ ] **Step 3: 终态检查**

Run: `git log --oneline -8`
Expected: 顶部可见 7 个本次提交（接口 + 基类 + Gemini + Claude/MiniMax/MiMo + AzureOpenAI + 测试）

Run: `git diff --stat HEAD~7..HEAD`
Expected: 改动仅涉及 spec 中列出的文件

---

## Self-Review

**1. Spec coverage:**
- §接口设计 → Task 1（ModelInfo + IChatModelProvider）
- §OpenAI 兼容 provider → Task 2（基类实现）
- §Gemini → Task 3
- §Claude/MiniMax/MiMo → Task 4
- §AzureOpenAI → Task 5
- §测试 → Task 6
- §验收标准 → Task 7

**2. Placeholder scan:** 无 TBD/TODO/占位符。所有步骤包含完整代码。

**3. Type consistency:**
- `ModelInfo` record 在 Task 1 定义，后续所有 task 使用一致
- `GetModelsAsync` 签名在所有 task 一致：`Task<IReadOnlyList<ModelInfo>> GetModelsAsync(CancellationToken cancellationToken = default)`
- `NotSupportedException` message 格式一致：`"{ProviderName} 不支持获取模型列表"`

无待修复项。Plan 可执行。
