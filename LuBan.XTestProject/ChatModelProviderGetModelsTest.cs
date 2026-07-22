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