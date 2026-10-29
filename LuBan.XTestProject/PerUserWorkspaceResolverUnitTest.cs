/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.XTestProject
*文件名： PerUserWorkspaceResolverUnitTest
*版本号： V1.0.0.0
*唯一标识：b8d5a3f7-2c96-4e01-8b74-5a1f9c3e0d62
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：按用户隔离工作区解析单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：按用户隔离工作区解析单元测试
*
*****************************************************************************/

using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.XTestProject;

[TestClass]
public class PerUserWorkspaceResolverUnitTest
{
    private string _ws = "";

    [TestInitialize]
    public void Setup()
    {
        _ws = Path.Combine(Path.GetTempPath(), "luban-userws-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_ws);
    }

    [TestCleanup]
    public void Cleanup()
    {
        try { Directory.Delete(_ws, recursive: true); } catch { }
    }

    private PerUserWorkspaceResolver Build(string userId)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAgentUserResolver>(new FixedUserResolver(userId));
        var sp = services.BuildServiceProvider();

        return new PerUserWorkspaceResolver(
            Options.Create(new LuBanAgentOptions { WorkspaceRoot = _ws }),
            sp.GetRequiredService<IAgentUserResolver>(),
            sp);
    }

    [TestMethod]
    public async Task DifferentUsers_GetDifferentRoots_AndDirectoriesAreCreated()
    {
        var a = await Build("alice").ResolveAsync("s1");
        var b = await Build("bob").ResolveAsync("s2");

        Assert.AreEqual("user:alice", a.Id);
        Assert.AreEqual(Path.Combine(_ws, "users", "alice"), a.RootPath);
        Assert.AreNotEqual(a.RootPath, b.RootPath);
        Assert.IsTrue(Directory.Exists(a.RootPath));
    }

    [TestMethod]
    public async Task TraversalInUserId_IsNeutralized()
    {
        var ws = await Build(@"..\..\evil").ResolveAsync("s1");

        StringAssert.StartsWith(ws.RootPath, Path.Combine(_ws, "users"));
        Assert.IsFalse(ws.RootPath.Contains(".."));
    }

    private sealed class FixedUserResolver(string userId) : IAgentUserResolver
    {
        public string ResolveUserId(IServiceProvider services) => userId;
    }
}