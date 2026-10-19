/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.Connectors
*文件名： ConnectorBase
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/19
*描述：连接器抽象基类 —— 幂等初始化、释放与实例名的通用骨架（厂商无关）
*
*****************************************************************************/
namespace LuBan.AIAgent.Connectors;

/// <summary>
/// 连接器抽象基类：提供「实例名 → 幂等初始化 → 释放」的通用骨架。
/// </summary>
/// <remarks>
/// <para>
/// 面向厂商连接器实现者：派生类只需给出 <see cref="Key"/>（提供方键）与
/// <see cref="McpExposures"/>（MCP 暴露面），按需覆写 <see cref="OnInitializeAsync"/> /
/// <see cref="OnDisposeAsync"/> 承载资源准备与清理。
/// </para>
/// <para>
/// 初始化语义（与宿主约定一致）：<b>每个实例只成功初始化一次</b>；失败<b>不</b>置位，
/// 下次调用可重试（宿主在启用注册与按 MCP 名派生时共用同一门禁）。
/// </para>
/// <para>
/// 厂商知识<b>不</b>属于基类：默认实现不做任何厂商假设，凭据一律由派生类经
/// <c>ICredentialStore</c> 取用。
/// </para>
/// </remarks>
public abstract class ConnectorBase : IConnector
{
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;
    private bool _disposed;

    /// <inheritdoc />
    public abstract string Key { get; }

    /// <inheritdoc />
    /// <remarks>实例名由连接器工厂按配置注入。</remarks>
    public string Name { get; set; } = "";

    /// <inheritdoc />
    public virtual string DisplayName => string.IsNullOrWhiteSpace(Name) ? Key : Name;

    /// <inheritdoc />
    public abstract IReadOnlyList<ConnectorMcpExposure> McpExposures { get; }

    /// <summary>
    /// 是否已释放。
    /// </summary>
    protected bool IsDisposed => _disposed;

    /// <inheritdoc />
    public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized) return;

        await _initLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_initialized) return;

            ThrowIfDisposed();
            await OnInitializeAsync(cancellationToken).ConfigureAwait(false);

            // 仅在成功后置位：失败保持可重试
            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// 子类初始化钩子（如回填持久化参数）。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <remarks>抛出异常即视为初始化失败，本实例下次仍可重试。</remarks>
    protected virtual Task OnInitializeAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await OnDisposeAsync().ConfigureAwait(false);
        _initLock.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 子类释放钩子（释放派生类持有的资源）。
    /// </summary>
    protected virtual ValueTask OnDisposeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// 已释放时抛出 <see cref="ObjectDisposedException"/>。
    /// </summary>
    /// <exception cref="ObjectDisposedException">本实例已释放。</exception>
    protected void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(GetType().Name);
    }
}