/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Hosting
*文件名： ISessionRuntimeInitializer
*版本号： V1.0.0.0
*唯一标识：会话运行时初始化
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：会话 scope 创建后的初始化扩展点与空实现
*
*****************************************************************************/namespace LuBan.AIAgent.Hosting;

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