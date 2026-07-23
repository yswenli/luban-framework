using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Providers.Kimi.DependencyInjection;

namespace LuBan.XTestProject;

/// <summary>
/// Kimi 模型提供者测试
/// </summary>
[TestClass]
public class KimiChatModelProviderTest
{
    /// <summary>
    /// 测试 Kimi 同步聊天
    /// </summary>
    [TestMethod]
    public async Task TestKimiCompleteAsync()
    {
        IChatModelProvider chatModelProvider = ServiceProviderUtil.GetRequiredServiceForOnce<IChatModelProvider>((services) =>
        {
            services.AddKimiProvider(options =>
            {
                options.ApiKey = "LuBan";
                options.BaseUrl = "https://api.moonshot.cn/v1";
                options.RequestTimeout = TimeSpan.FromMinutes(2); // 增加超时时间到2分钟
            });
        });

        var models = await chatModelProvider.GetModelsAsync();

        // 构造测试请求
        var request = new ChatRequest
        {
            ModelId = "kimi-k2.5",
            Messages = [
                ChatMessage.System("你是 Kimi，由 Moonshot AI 提供的人工智能助手，你更擅长中文和英文的对话。你会为用户提供安全，有帮助，准确的回答。"),
                ChatMessage.User("请写一个 C# 方法来计算斐波那契数列的第 n 项")
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
        Console.WriteLine("Kimi 响应:");
        Console.WriteLine(response.Message.TextContent);
    }

    /// <summary>
    /// 测试 Kimi 流式聊天
    /// </summary>
    [TestMethod]
    public async Task TestKimiStreamAsync()
    {
        IChatModelProvider chatModelProvider = ServiceProviderUtil.GetRequiredServiceForOnce<IChatModelProvider>((services) =>
        {
            services.AddKimiProvider(options =>
            {
                options.ApiKey = "LuBanFramework";
                options.BaseUrl = "https://api.moonshot.cn/v1/";
                options.RequestTimeout = TimeSpan.FromMinutes(2); // 增加超时时间到2分钟
            });
        });

        // 构造测试请求
        var request = new ChatRequest
        {
            ModelId = "kimi-k2.5",
            Messages = [
                ChatMessage.System("你是 Kimi，由 Moonshot AI 提供的人工智能助手，你更擅长中文和英文的对话。你会为用户提供安全，有帮助，准确的回答。"),
                ChatMessage.User("请简单解释什么是斐波那契数列")
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