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
                options.BaseUrl = "https://ark.cn-beijing.volces.com/api/v3";//"https://opencode.ai/zen/go/v1/";
                options.RequestTimeout = TimeSpan.FromMinutes(2);
            });
        });

        var models = await chatModelProvider.GetModelsAsync();

        var request = new ChatRequest
        {
            ModelId = "glm-5.2",
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

        var response = await chatModelProvider.CompleteAsync(request);

        Assert.IsNotNull(response);
        if (!response.IsSuccess)
        {
            Console.WriteLine($"错误信息: {response.ErrorMessage}");
        }
        Assert.IsTrue(response.IsSuccess);
        Assert.IsNotNull(response.Message);
        Assert.IsNotNull(response.Message.TextContent);
        Assert.IsFalse(string.IsNullOrEmpty(response.Message.TextContent));

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
                options.BaseUrl = "https://opencode.ai/zen/go/v1/";
                options.RequestTimeout = TimeSpan.FromMinutes(2);
            });
        });

        var request = new ChatRequest
        {
            ModelId = "glm-5.2",
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

        Assert.IsFalse(string.IsNullOrEmpty(fullResponse));

        Console.WriteLine();
        Console.WriteLine("\n完整响应:");
        Console.WriteLine(fullResponse);
    }
}
