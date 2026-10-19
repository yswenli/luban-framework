/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： ConnectorBaseUnitTest
*描述：验证连接器抽象基类的幂等初始化 / 失败可重试 / 释放幂等（W2/T2.10）
*
*****************************************************************************/
using LuBan.AIAgent.Connectors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

/// <summary>
/// <see cref="ConnectorBase"/> 模板语义测试。
/// </summary>
/// <remarks>
/// 关键约定：每个实例只成功初始化一次；初始化失败<b>不</b>置位（下次可重试）；释放幂等。
/// </remarks>
[TestClass]
public class ConnectorBaseUnitTest
{
    private sealed class CountingConnector : ConnectorBase
    {
        private readonly Func<int, Task>? _onInitialize;

        public CountingConnector(Func<int, Task>? onInitialize = null) => _onInitialize = onInitialize;

        public override string Key => "demo";

        public override IReadOnlyList<ConnectorMcpExposure> McpExposures => Array.Empty<ConnectorMcpExposure>();

        public int InitializeCount { get; private set; }
        public int DisposeCount { get; private set; }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            InitializeCount++;
            return _onInitialize?.Invoke(InitializeCount) ?? Task.CompletedTask;
        }

        protected override ValueTask OnDisposeAsync()
        {
            DisposeCount++;
            return ValueTask.CompletedTask;
        }
    }

    [TestMethod]
    public async Task InitializeAsync_RunsOnlyOncePerInstance()
    {
        var connector = new CountingConnector();

        await connector.InitializeAsync();
        await connector.InitializeAsync();
        await connector.InitializeAsync();

        Assert.AreEqual(1, connector.InitializeCount);
    }

    [TestMethod]
    public async Task InitializeAsync_ConcurrentCalls_RunOnlyOnce()
    {
        var connector = new CountingConnector(attempt => attempt == 1
            ? Task.Delay(30)
            : Task.CompletedTask);

        await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => connector.InitializeAsync().AsTask()));

        Assert.AreEqual(1, connector.InitializeCount);
    }

    [TestMethod]
    public async Task InitializeAsync_FailureIsNotCached_AndIsRetryable()
    {
        var connector = new CountingConnector(attempt => attempt == 1
            ? Task.FromException(new InvalidOperationException("首次失败"))
            : Task.CompletedTask);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => connector.InitializeAsync().AsTask());

        await connector.InitializeAsync();

        Assert.AreEqual(2, connector.InitializeCount, "失败不得置位，下次必须重试");
    }

    [TestMethod]
    public async Task DisposeAsync_IsIdempotent()
    {
        var connector = new CountingConnector();

        await connector.DisposeAsync();
        await connector.DisposeAsync();

        Assert.AreEqual(1, connector.DisposeCount);
    }

    [TestMethod]
    public async Task InitializeAsync_AfterDispose_Throws()
    {
        var connector = new CountingConnector();
        await connector.DisposeAsync();

        await Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => connector.InitializeAsync().AsTask());
    }

    [TestMethod]
    public void DisplayName_FallsBackToKey_WhenNameMissing()
    {
        var connector = new CountingConnector();

        Assert.AreEqual("demo", connector.DisplayName);

        connector.Name = "myconnector";

        Assert.AreEqual("myconnector", connector.DisplayName);
    }
}