/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： SkillCatalogInjectionUnitTest
*唯一标识：Skill 清单注入 systemPrompt 单测
*创建时间：2026/10/29
*描述：验证 LuBanAgentFactory 在显式传入 systemPrompt 时仍注入 Skill 清单
*      （旧实现仅在 systemPrompt 为空时注入，导致产品侧 Profile 永无 Skill 目录）。
*
*****************************************************************************/
using LuBan.AIAgent;
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Skills;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class SkillCatalogInjectionUnitTest
{
    /// <summary>记录最后一次请求的 ChatOptions，用于断言 Instructions 内容。</summary>
    private sealed class CapturingChatClient : IChatClient
    {
        public ChatOptions? LastOptions { get; private set; }

        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            LastOptions = options;
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok")));
        }

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages, ChatOptions? options = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            LastOptions = options;
            yield return new ChatResponseUpdate(ChatRole.Assistant, "ok");
            await Task.CompletedTask;
        }

        public object? GetService(Type serviceType, object? key = null) => null;

        public void Dispose() { }
    }

    private sealed class CatalogSkill : ISkill
    {
        public string Id => "demo-skill";
        public string Name => "演示";
        public string Description => "用于断言清单注入";
        public string Category => "test";
        public IEnumerable<string> Examples => Array.Empty<string>();
        public IEnumerable<string> TriggerKeywords => Array.Empty<string>();
        public string? PromptTemplate => "template";
        public Task<SkillResult> ExecuteAsync(SkillContext context, string input)
            => Task.FromResult(SkillResult.Ok("ok"));
    }

    private static (LuBanAgentFactory Factory, CapturingChatClient Client) CreateFactory(
        LuBanAgentOptions options, SkillRegistry? skillRegistry)
    {
        var services = new ServiceCollection();
        if (skillRegistry != null)
            services.AddSingleton(skillRegistry);
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var client = new CapturingChatClient();
        var registry = new ToolPluginRegistry(Array.Empty<ILuBanToolPlugin>(), Options.Create(options));
        return (new LuBanAgentFactory(client, registry, Options.Create(options), sp.AsDiContainer()), client);
    }

    [TestMethod]
    public async Task CreateAsync_显式systemPrompt时仍注入Skill清单()
    {
        var options = new LuBanAgentOptions { SystemPrompt = "基线提示词" };
        var (factory, client) = CreateFactory(options, new SkillRegistry(new ISkill[] { new CatalogSkill() }));

        var agent = await factory.CreateAsync(systemPrompt: "自定义提示词");
        await agent.RunAsync("你好");

        var instructions = client.LastOptions?.Instructions;
        Assert.IsNotNull(instructions, "系统提示词应随请求下发");
        StringAssert.Contains(instructions, "自定义提示词");
        StringAssert.Contains(instructions, "demo-skill", "显式 systemPrompt 也必须附带 Skill 清单");
        StringAssert.Contains(instructions, "UseSkill");
    }

    [TestMethod]
    public async Task CreateAsync_CatalogEnabled关闭时不注入清单()
    {
        var options = new LuBanAgentOptions
        {
            SystemPrompt = "基线提示词",
            Tools = new ToolGroupOptions { Skill = new SkillToolOptions { CatalogEnabled = false } }
        };
        var (factory, client) = CreateFactory(options, new SkillRegistry(new ISkill[] { new CatalogSkill() }));

        var agent = await factory.CreateAsync(systemPrompt: "自定义提示词");
        await agent.RunAsync("你好");

        var instructions = client.LastOptions?.Instructions;
        Assert.IsNotNull(instructions);
        Assert.IsFalse(instructions!.Contains("demo-skill"), "关闭 CatalogEnabled 后不应注入清单");
    }

    [TestMethod]
    public async Task CreateAsync_无可用Skill时不注入空清单()
    {
        var options = new LuBanAgentOptions { SystemPrompt = "基线提示词" };
        var (factory, client) = CreateFactory(options, new SkillRegistry(Array.Empty<ISkill>()));

        var agent = await factory.CreateAsync(systemPrompt: "自定义提示词");
        await agent.RunAsync("你好");

        var instructions = client.LastOptions?.Instructions;
        Assert.AreEqual("自定义提示词", instructions?.Trim(), "0 个 Skill 时不应注入只含「当前无可用 Skill」的空清单");
    }

    [TestMethod]
    public async Task CreateAsync_无Skill注册表时不注入清单()
    {
        var options = new LuBanAgentOptions { SystemPrompt = "基线提示词" };
        var (factory, client) = CreateFactory(options, skillRegistry: null);

        var agent = await factory.CreateAsync(systemPrompt: "自定义提示词");
        await agent.RunAsync("你好");

        var instructions = client.LastOptions?.Instructions;
        Assert.AreEqual("自定义提示词", instructions?.Trim(), "未注册 SkillRegistry 时提示词原样透传");
    }
}