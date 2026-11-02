/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.XTestProject
*文件名： AgentHostRegistrationUnitTest
*版本号： V1.0.0.0
*唯一标识：7af80bfe-efb7-4b64-ba48-0d163fadd80d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26 17:35:32
*描述：Agent 宿主 DI 注册契约单元测试（作用域生命周期与默认接缝）
*
*=================================================
*修改标记
*修改时间：2026/10/26 17:35:32
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Agent 宿主 DI 注册契约单元测试（作用域生命周期与默认接缝）
*
*****************************************************************************/

using LuBan.AIAgent;
using LuBan.AIAgent.Attachments.Ingestion;
using LuBan.AIAgent.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LuBan.XTestProject;

[TestClass]
public class AgentHostRegistrationUnitTest
{
    private static ServiceProvider Build()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["LuBanAgent:WorkspaceRoot"] = @"C:\ws" })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging(b => b.AddDebug());
        services.AddSingleton<IChatClient>(new StubChatClient());
        services.AsDiContainer().AddLuBanAgentDefaults(config);
        services.AsDiContainer().AddLuBanAgentHost();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }

    private sealed class StubChatClient : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "stub")));

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages, ChatOptions? options = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield return new ChatResponseUpdate(ChatRole.Assistant, "stub");
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }

    [TestMethod]
    public void Container_Validates()
    {
        using var sp = Build();
        Assert.IsNotNull(sp);
    }

    [TestMethod]
    public async Task IAgentHost_ResolvesPerScope()
    {
        await using var sp = Build();
        using var scope1 = sp.CreateScope();
        using var scope2 = sp.CreateScope();

        var a = scope1.ServiceProvider.GetRequiredService<IAgentHost>();
        var b = scope2.ServiceProvider.GetRequiredService<IAgentHost>();

        Assert.AreNotSame(a, b);
    }

    [TestMethod]
    public async Task SessionRuntimeManager_IsSingleton()
    {
        await using var sp = Build();
        using var s1 = sp.CreateScope();
        using var s2 = sp.CreateScope();

        Assert.AreSame(
            s1.ServiceProvider.GetRequiredService<SessionRuntimeManager>(),
            s2.ServiceProvider.GetRequiredService<SessionRuntimeManager>());
    }

    [TestMethod]
    public async Task DefaultSeams_AreRegistered()
    {
        await using var sp = Build();
        using var scope = sp.CreateScope();

        Assert.IsNotNull(scope.ServiceProvider.GetRequiredService<IAgentWorkspaceResolver>());
        Assert.IsNotNull(scope.ServiceProvider.GetRequiredService<IAgentProfileResolver>());
        Assert.IsNotNull(scope.ServiceProvider.GetRequiredService<IAgentUserResolver>());
        Assert.IsNotNull(scope.ServiceProvider.GetRequiredService<IModelCapabilityGate>());
        Assert.IsNotNull(scope.ServiceProvider.GetRequiredService<IAgentAttachmentOrchestrator>());
    }

    [TestMethod]
    public async Task AttachmentSourceAndConfirmationSeams_AreRegistered()
    {
        await using var sp = Build();
        using var scope = sp.CreateScope();

        Assert.IsInstanceOfType(
            scope.ServiceProvider.GetRequiredService<IAgentAttachmentSourceResolver>(),
            typeof(DefaultAttachmentSourceResolver));
        // broker 必须跨 scope 是同一实例：SSE 请求与确认请求属于不同的 HTTP scope
        Assert.AreSame(
            sp.GetRequiredService<IToolConfirmationBroker>(),
            scope.ServiceProvider.GetRequiredService<IToolConfirmationBroker>());
    }

    [TestMethod]
    public async Task WorkspaceResolver_StaysSingle_WhenIsolationDisabled()
    {
        await using var sp = Build();
        using var scope = sp.CreateScope();

        Assert.IsInstanceOfType(
            scope.ServiceProvider.GetRequiredService<IAgentWorkspaceResolver>(),
            typeof(SingleWorkspaceResolver));
    }
}