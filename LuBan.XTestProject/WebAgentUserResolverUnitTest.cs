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
*文件名： WebAgentUserResolverUnitTest
*版本号： V1.0.0.0
*唯一标识：278e8f8c-3fb2-48ea-8fb2-9a8b370f006e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/28 18:22:45
*描述：Web 宿主用户解析单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/28 18:22:45
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Web 宿主用户解析单元测试
*
*****************************************************************************/

using LuBan.AIAgent.Abstractions;
using LuBan.Web.Core.AIAgent;

using Microsoft.Extensions.DependencyInjection;

namespace LuBan.XTestProject;

/// <summary>
/// Web 宿主用户解析（<see cref="WebAgentUserResolver"/>）单元测试。
/// </summary>
[TestClass]
public class WebAgentUserResolverUnitTest
{
    /// <summary>固定返回指定用户Id的上下文桩。</summary>
    private sealed class StubUserContext(string? userId) : IUserContext
    {
        public string? UserId { get; } = userId;
    }

    /// <summary>造一个只含指定用户上下文的服务容器。</summary>
    private static ServiceProvider BuildServices(string? userId)
    {
        var services = new ServiceCollection();
        services.AddScoped<IUserContext>(_ => new StubUserContext(userId));
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// 会话级 scope 内已回填用户Id时，解析必须直接采用该值（不回落 local）。
    /// </summary>
    [TestMethod]
    public void ResolveUserId_PrefersUserContext()
    {
        using var provider = BuildServices("1300000000101");
        var resolver = new WebAgentUserResolver();

        var userId = resolver.ResolveUserId(provider.AsDiContainer());

        Assert.AreEqual("1300000000101", userId);
    }

    /// <summary>
    /// 拿不到用户上下文、也不在请求上下文里时，回落到 local。
    /// </summary>
    [TestMethod]
    public void ResolveUserId_FallsBackToLocal()
    {
        using var provider = BuildServices(null);
        var resolver = new WebAgentUserResolver();

        var userId = resolver.ResolveUserId(provider.AsDiContainer());

        Assert.AreEqual("local", userId);
    }
}