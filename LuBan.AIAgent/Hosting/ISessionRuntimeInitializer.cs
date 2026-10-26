namespace LuBan.AIAgent.Hosting;

/// <summary>
/// 会话 scope 创建后的初始化扩展点（宿主用产品字段补齐上下文）。
/// </summary>
public interface ISessionRuntimeInitializer
{
    /// <summary>在会话 scope 内执行初始化。</summary>
    Task InitializeAsync(SessionRuntimeSeed seed, CancellationToken ct = default);
}

/// <summary>空实现（未注册实现时的回落）。</summary>
public sealed class NoOpSessionRuntimeInitializer : ISessionRuntimeInitializer
{
    /// <inheritdoc />
    public Task InitializeAsync(SessionRuntimeSeed seed, CancellationToken ct = default) => Task.CompletedTask;
}