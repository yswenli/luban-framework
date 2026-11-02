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
*文件名： DefaultSeamsUnitTest
*版本号： V1.0.0.0
*唯一标识：19e9d75b-6c10-4baf-acdc-17151da8bb5d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26 14:51:27
*描述：Agent 默认接缝实现单元测试（工作区 / 画像 / 用户 / 模型选择）
*
*=================================================
*修改标记
*修改时间：2026/10/26 14:51:27
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Agent 默认接缝实现单元测试（工作区 / 画像 / 用户 / 模型选择）
*
*****************************************************************************/

using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.XTestProject;

[TestClass]
public class DefaultSeamsUnitTest
{
    private static IOptions<LuBanAgentOptions> Options(string? root = null, string? defaultModel = null)
        => Microsoft.Extensions.Options.Options.Create(new LuBanAgentOptions
        {
            WorkspaceRoot = root,
            DefaultModel = defaultModel
        });

    [TestMethod]
    public async Task SingleWorkspaceResolver_ReturnsConfiguredRoot()
    {
        var resolver = new SingleWorkspaceResolver(Options(@"C:\work\demo"));

        var ws = await resolver.ResolveAsync("s1");

        Assert.AreEqual(@"C:\work\demo", ws.RootPath);
        Assert.AreEqual("default", ws.Id);
        Assert.AreEqual("Default", ws.Name);
        Assert.AreEqual("normal", ws.Type);
    }

    [TestMethod]
    public void DefaultAgentProfileResolver_AlwaysReturnsDefaultProfile()
    {
        var resolver = new DefaultAgentProfileResolver(new DefaultAgentProfile());

        var profile = resolver.Resolve(new WorkspaceDescriptor("w", "n", @"C:\w", "normal"));

        Assert.IsInstanceOfType(profile, typeof(DefaultAgentProfile));
    }

    [TestMethod]
    public void DefaultAgentUserResolver_FallsBackToLocal()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var resolver = new DefaultAgentUserResolver();

        Assert.AreEqual("local", resolver.ResolveUserId(provider.AsDiContainer()));
    }

    [TestMethod]
    public void DefaultAgentModelSelection_EmptyWhenNotConfigured()
    {
        var selection = new DefaultAgentModelSelection(Options());

        Assert.AreEqual(string.Empty, selection.QualifiedModelName);
        Assert.IsNull(selection.ProviderId);
        Assert.IsNull(selection.ModelId);
    }

    [TestMethod]
    public void DefaultAgentModelSelection_ParsesConfiguredDefault()
    {
        var selection = new DefaultAgentModelSelection(Options(defaultModel: "openai:gpt-4.1"));

        Assert.AreEqual("openai", selection.ProviderId);
        Assert.AreEqual("gpt-4.1", selection.ModelId);
        Assert.AreEqual("openai:gpt-4.1", selection.QualifiedModelName);
    }
}