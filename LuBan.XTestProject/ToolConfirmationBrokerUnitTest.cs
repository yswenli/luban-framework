/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.XTestProject
*文件名： ToolConfirmationBrokerUnitTest
*版本号： V1.0.0.0
*唯一标识：a3e9b1d5-2f64-4c08-9b7a-5d0e6f1c3a82
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：工具确认登记簿单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：工具确认登记簿单元测试
*
*****************************************************************************/

using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Hosting;

using Microsoft.Extensions.Options;

namespace LuBan.XTestProject;

[TestClass]
public class ToolConfirmationBrokerUnitTest
{
    private static DefaultToolConfirmationBroker Build(int timeoutSeconds = 60, TimeSpan? overrideTimeout = null)
        => new(Options.Create(new LuBanAgentOptions
        {
            Confirmation = new ToolConfirmationOptions { TimeoutSeconds = timeoutSeconds }
        }), overrideTimeout);

    [TestMethod]
    public async Task Resolve_ReturnsUserChoice()
    {
        var sut = Build();
        var pending = sut.Register("sid", "DeleteFileAsync", new Dictionary<string, object?>());

        Assert.IsTrue(sut.TryResolve("sid", pending.ConfirmationId, ConfirmResult.Allow));
        Assert.AreEqual(ConfirmResult.Allow, await sut.WaitAsync(pending, CancellationToken.None));
    }

    [TestMethod]
    public void Resolve_WrongSession_IsRejected()
    {
        var sut = Build();
        var pending = sut.Register("sid", "DeleteFileAsync", new Dictionary<string, object?>());

        Assert.IsFalse(sut.TryResolve("other", pending.ConfirmationId, ConfirmResult.Allow));
        Assert.IsTrue(sut.TryResolve("sid", pending.ConfirmationId, ConfirmResult.Deny));
    }

    [TestMethod]
    public async Task Resolve_Twice_SecondReturnsFalse()
    {
        var sut = Build();
        var pending = sut.Register("sid", "DeleteFileAsync", new Dictionary<string, object?>());

        Assert.IsTrue(sut.TryResolve("sid", pending.ConfirmationId, ConfirmResult.Allow));
        Assert.IsFalse(sut.TryResolve("sid", pending.ConfirmationId, ConfirmResult.Deny));
        Assert.AreEqual(ConfirmResult.Allow, await sut.WaitAsync(pending, CancellationToken.None));
    }

    [TestMethod]
    public async Task Timeout_ReturnsDeny_AndEntryIsGone()
    {
        var sut = Build(overrideTimeout: TimeSpan.FromMilliseconds(80));
        var pending = sut.Register("sid", "DeleteFileAsync", new Dictionary<string, object?>());

        Assert.AreEqual(ConfirmResult.Deny, await sut.WaitAsync(pending, CancellationToken.None));
        Assert.IsFalse(sut.TryResolve("sid", pending.ConfirmationId, ConfirmResult.Allow));
    }

    [TestMethod]
    public async Task Cancellation_ReturnsDeny()
    {
        var sut = Build();
        var pending = sut.Register("sid", "DeleteFileAsync", new Dictionary<string, object?>());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.AreEqual(ConfirmResult.Deny, await sut.WaitAsync(pending, cts.Token));
    }

    [TestMethod]
    public void TimeoutSeconds_ComesFromOptions()
    {
        Assert.AreEqual(42, Build(42).TimeoutSeconds);
    }
}