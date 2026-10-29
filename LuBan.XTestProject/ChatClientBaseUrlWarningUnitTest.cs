/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： ChatClientBaseUrlWarningUnitTest
*唯一标识：BaseUrl 缺省告警单测
*创建时间：2026/10/29
*描述：验证省略 BaseUrl 时：内置 Provider 走内置地址不告警；
*      非内置 Provider 会落到 OpenAI 默认端点并写入 ChatClientFactoryWarning。
*
*****************************************************************************/
using LuBan.AIAgent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class ChatClientBaseUrlWarningUnitTest
{
    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] settings)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(settings.Select(s => new KeyValuePair<string, string?>(s.Key, s.Value)))
            .Build();

    private static ChatClientFactoryWarning? BuildWarning(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddLuBanAgentOpenAI(configuration);
        using var provider = services.BuildServiceProvider();
        return provider.GetService<ChatClientFactoryWarning>();
    }

    [TestMethod]
    public void UnknownProvider_WithoutBaseUrl_WarnsAboutOpenAiDefaultEndpoint()
    {
        var configuration = BuildConfiguration(
            ("LuBanAgent:Models:0:Provider", "my-inner-gateway"),
            ("LuBanAgent:Models:0:Model", "deepseek-v4.1-flash"),
            ("LuBanAgent:Models:0:ApiKey", "sk-test"));

        var warning = BuildWarning(configuration);

        Assert.IsNotNull(warning, "非内置 Provider 且未配置 BaseUrl 时必须给出启动期提示。");
        StringAssert.Contains(warning.Message, "my-inner-gateway");
        StringAssert.Contains(warning.Message, "https://api.openai.com/v1/");
        StringAssert.Contains(warning.Message, "BaseUrl");
    }

    [TestMethod]
    public void BuiltInProvider_WithoutBaseUrl_DoesNotWarn()
    {
        var configuration = BuildConfiguration(
            ("LuBanAgent:Models:0:Provider", "deepseek"),
            ("LuBanAgent:Models:0:Model", "deepseek-v4-pro"),
            ("LuBanAgent:Models:0:ApiKey", "sk-test"));

        var warning = BuildWarning(configuration);

        Assert.IsNull(warning, "deepseek 有内置地址，省略 BaseUrl 不应告警。");
    }

    [TestMethod]
    public void UnknownProvider_WithExplicitBaseUrl_DoesNotWarn()
    {
        var configuration = BuildConfiguration(
            ("LuBanAgent:Models:0:Provider", "my-inner-gateway"),
            ("LuBanAgent:Models:0:Model", "deepseek-v4.1-flash"),
            ("LuBanAgent:Models:0:ApiKey", "sk-test"),
            ("LuBanAgent:Models:0:BaseUrl", "https://gateway.internal/v1/"));

        var warning = BuildWarning(configuration);

        Assert.IsNull(warning, "显式配置 BaseUrl 后不应再提示默认端点。");
    }
}