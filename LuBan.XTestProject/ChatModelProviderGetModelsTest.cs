using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Providers.GLM.DependencyInjection;

namespace LuBan.XTestProject;

/// <summary>
/// 模型提供者获取模型列表测试
/// </summary>
[TestClass]
[TestCategory("Integration")]
public class ChatModelProviderGetModelsTest
{
    /// <summary>
    /// 测试 GLM 获取模型列表（OpenAI 兼容 provider）
    /// </summary>
    [TestMethod]
    public async Task TestGLMGetModelsAsync()
    {
        IChatModelProvider chatModelProvider = ServiceProviderUtil.GetRequiredServiceForOnce<IChatModelProvider>((services) =>
        {
            services.AddGLMProvider(options =>
            {
                options.ApiKey = "LuBan";
                options.BaseUrl = "https://opencode.ai/zen/go/v1/";
                options.RequestTimeout = TimeSpan.FromMinutes(2);
            });
        });

        var models = await chatModelProvider.GetModelsAsync();

        Assert.IsNotNull(models);
        Console.WriteLine($"GLM 返回 {models.Count} 个模型:");
        foreach (var model in models)
        {
            Console.WriteLine($"  - {model.Id} | name: {model.Name} | status: {model.Status} | domain: {model.Domain} | context: {model.TokenLimits?.ContextWindow}");
        }
    }

    /// <summary>
    /// 测试 Claude 获取模型列表（返回空列表）
    /// </summary>
    [TestMethod]
    public async Task TestClaudeGetModelsAsync()
    {
        var provider = new LuBan.AIAgent.Providers.Claude.ClaudeChatModelProvider(
            new HttpClient(),
            Microsoft.Extensions.Options.Options.Create(new LuBan.AIAgent.Providers.Claude.ClaudeOptions
            {
                ApiKey = "test-key"
            }));

        var models = await provider.GetModelsAsync();

        Assert.IsNotNull(models);
        Assert.AreEqual(0, models.Count, "Claude 应返回空列表");
    }
}