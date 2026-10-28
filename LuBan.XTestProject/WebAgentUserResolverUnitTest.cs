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

        var userId = resolver.ResolveUserId(provider);

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

        var userId = resolver.ResolveUserId(provider);

        Assert.AreEqual("local", userId);
    }
}