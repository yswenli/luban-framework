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
*文件名： AgentHostResetUnitTest
*版本号： V1.0.0.0
*唯一标识：ffc04de6-3e8b-43b3-88d7-8688d91894ba
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/27 16:00:46
*描述：IAgentHost.Reset 运行态语义单元测试（回合运行中延迟卸载）
*
*=================================================
*修改标记
*修改时间：2026/10/27 16:00:46
*修改人： yswenli
*版本号： V1.0.0.0
*描述：IAgentHost.Reset 运行态语义单元测试（回合运行中延迟卸载）
*
*****************************************************************************/

using LuBan.AIAgent;
using LuBan.AIAgent.Hosting;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuBan.XTestProject;

/// <summary>
/// <see cref="IAgentHost.Reset"/> 运行态语义：回合运行中只登记延迟重载，
/// 待回合收尾时释放 Agent，下一次对话按最新配置重建。
/// </summary>
[TestClass]
public class AgentHostResetUnitTest
{
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);

    [TestMethod]
    public async Task Reset_WhileTurnRunning_DefersDetachUntilTurnEnds()
    {
        var root = CreateWorkspaceRoot();
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var sp = Build(root, gate);

        using var scope = sp.CreateScope();
        var host = scope.ServiceProvider.GetRequiredService<IAgentHost>();
        var runtimes = scope.ServiceProvider.GetRequiredService<SessionRuntimeManager>();

        var consumer = ConsumeAsync(host, "s1");

        await WaitUntilAsync(
            () => runtimes.Get("s1") is { IsRunning: true, Agent: not null },
            "回合进入运行状态且 Agent 已构建");

        var runtime = runtimes.Get("s1")!;
        Assert.IsNotNull(runtime.Agent, "回合运行中不应释放 Agent");
        var agentDuringTurn = runtime.Agent;

        host.Reset("s1");

        // 关键断言：运行中 Reset 不释放、只打延迟重载标记
        Assert.AreSame(agentDuringTurn, runtime.Agent, "运行中 Reset 不得替换/释放 Agent");
        Assert.IsTrue(runtime.PendingReload, "运行中 Reset 必须登记延迟重载");

        gate.SetResult();

        var events = await consumer.WaitAsync(WaitTimeout);
        Assert.IsTrue(events.Exists(e => e is TextDeltaEvent), "回合应正常产出正文事件");

        await WaitUntilAsync(
            () => runtime.Agent is null && !runtime.PendingReload,
            "回合收尾后释放 Agent");

        Assert.IsNull(runtime.Agent, "回合结束后应释放 Agent");
        Assert.IsNull(runtime.Workspace, "回合结束后应解绑工作区，下次对话重建");
        Assert.IsFalse(runtime.IsRunning);
    }

    [TestMethod]
    public async Task Reset_WhenIdle_DetachesImmediately()
    {
        var root = CreateWorkspaceRoot();
        await using var sp = Build(root, gate: null);

        using var scope = sp.CreateScope();
        var host = scope.ServiceProvider.GetRequiredService<IAgentHost>();
        var runtimes = scope.ServiceProvider.GetRequiredService<SessionRuntimeManager>();

        var runtime = runtimes.GetOrCreate("s1", "local", "default", root);
        runtime.Agent = NewAgent();
        runtime.Workspace = new WorkspaceDescriptor("default", "Default", root, "normal");

        host.Reset("s1");

        Assert.IsNull(runtime.Agent, "空闲 Reset 应立即释放 Agent");
        Assert.IsNull(runtime.Workspace);
        Assert.IsFalse(runtime.PendingReload, "空闲 Reset 不需要延迟重载");
    }

    [TestMethod]
    public async Task Reset_UnknownSession_IsNoOp()
    {
        var root = CreateWorkspaceRoot();
        await using var sp = Build(root, gate: null);

        using var scope = sp.CreateScope();
        var host = scope.ServiceProvider.GetRequiredService<IAgentHost>();
        var runtimes = scope.ServiceProvider.GetRequiredService<SessionRuntimeManager>();

        host.Reset("not-exists");
        host.Reset(null);

        Assert.IsNull(runtimes.Get("not-exists"), "未知会话不应被创建");
    }

    [TestMethod]
    public async Task PendingReload_IsConsumedExactlyOnce()
    {
        var root = CreateWorkspaceRoot();
        await using var sp = Build(root, gate: null);

        using var scope = sp.CreateScope();
        var runtimes = scope.ServiceProvider.GetRequiredService<SessionRuntimeManager>();
        var runtime = runtimes.GetOrCreate("s1", "local", "default", root);

        Assert.IsFalse(runtime.ConsumePendingReload(), "未请求时不应消费到标记");

        runtime.RequestReload();
        Assert.IsTrue(runtime.PendingReload);

        Assert.IsTrue(runtime.ConsumePendingReload(), "首次消费应命中");
        Assert.IsFalse(runtime.PendingReload, "消费后标记应复位");
        Assert.IsFalse(runtime.ConsumePendingReload(), "同一标记只能被消费一次");

        // 多次请求合并为一次重载（幂等）
        runtime.RequestReload();
        runtime.RequestReload();
        Assert.IsTrue(runtime.ConsumePendingReload());
        Assert.IsFalse(runtime.ConsumePendingReload());
    }

    private static async Task<List<AgentStreamEvent>> ConsumeAsync(IAgentHost host, string sessionId)
    {
        var list = new List<AgentStreamEvent>();
        using var cts = new CancellationTokenSource(WaitTimeout);
        await foreach (var evt in host.RunStreamingAsync(
            new AgentRunRequest { SessionId = sessionId, Input = "hi" }, cts.Token))
        {
            list.Add(evt);
        }
        return list;
    }

    private static async Task WaitUntilAsync(Func<bool> condition, string what)
    {
        var deadline = DateTime.UtcNow + WaitTimeout;
        while (DateTime.UtcNow < deadline)
        {
            if (condition()) return;
            await Task.Delay(20);
        }
        Assert.Fail($"等待超时：{what}");
    }

    private static string CreateWorkspaceRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "luban-agenthost-reset-test", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static LuBanAgent NewAgent() =>
        new(new ChatClientAgent(new StubChatClient(gate: null), new ChatClientAgentOptions
        {
            Name = "TestAgent",
            ChatOptions = new ChatOptions { Instructions = "test" }
        }));

    private static ServiceProvider Build(string workspaceRoot, TaskCompletionSource? gate)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LuBanAgent:WorkspaceRoot"] = workspaceRoot,
                ["LuBanAgent:DefaultModel"] = "test:stub"
            })
            .Build();

        var chatClient = new StubChatClient(gate);

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddSingleton<IChatClient>(chatClient);
        services.AsDiContainer().AddLuBanAgentDefaults(config);
        // 先注册假 Profile（TryAdd 语义下优先于 DefaultAgentProfile）：直接返回基于桩客户端的 Agent，
        // 避免测试依赖工作区组件加载与 Provider 路由。
        services.AddScoped<IAgentProfile>(_ => new FakeProfile(chatClient));
        services.AsDiContainer().AddLuBanAgentHost();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }

    private sealed class FakeProfile(IChatClient chatClient) : IAgentProfile
    {
        public string SystemPrompt => "test";

        public string[]? ToolGroups => null;

        public string? RetrievalMode => null;

        public Task<LuBanAgent> CreateAgentAsync(
            ILuBanAgentFactory factory,
            string modelName,
            WorkspaceDescriptor workspace,
            LuBan.AIAgent.Rules.RuleEngine ruleEngine,
            LuBan.AIAgent.Abstractions.ToolPluginRegistry pluginRegistry,
            LuBan.AIAgent.Skills.SkillRegistry skillRegistry,
            LuBan.AIAgent.MCP.MCPRegistry mcpRegistry,
            CancellationToken ct = default,
            AgentBuildOverrides? overrides = null)
            => Task.FromResult(new LuBanAgent(new ChatClientAgent(chatClient, new ChatClientAgentOptions
            {
                Name = "TestAgent",
                ChatOptions = new ChatOptions { Instructions = SystemPrompt }
            })));
    }

    /// <summary>流式桩客户端：配置 <c>gate</c> 时阻塞到放行，用于模拟「回合进行中」。</summary>
    private sealed class StubChatClient(TaskCompletionSource? gate) : IChatClient
    {
        public async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages, ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (gate is not null)
                await gate.Task.WaitAsync(cancellationToken);
            return new ChatResponse(new ChatMessage(ChatRole.Assistant, "stub"));
        }

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages, ChatOptions? options = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (gate is not null)
                await gate.Task.WaitAsync(cancellationToken);
            yield return new ChatResponseUpdate(ChatRole.Assistant, "stub reply");
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }
}