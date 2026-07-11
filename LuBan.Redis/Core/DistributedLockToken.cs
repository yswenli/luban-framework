/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Redis.Core
*文件名： DistributedLockToken
*版本号： V2.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/6/2
*描述：分布式锁令牌，用于 using 模式自动释放锁
*
*=================================================
*修改标记
*修改时间：2026/6/2
*修改人： yswenli
*版本号： V2.0.0.0
*描述：重构为支持多种锁实现
*
*****************************************************************************/
namespace LuBan.Redis.Core;

/// <summary>
/// 分布式锁令牌，用于 using 模式自动释放锁
/// 实现 IDisposable 和 IAsyncDisposable 接口，支持同步和异步释放
/// 使用委托抽象释放逻辑，支持不同的锁实现
/// </summary>
public sealed class DistributedLockToken : IAsyncDisposable, IDisposable
{
    private readonly Func<string, Task<bool>> _releaseFunc;
    private bool _released;

    /// <summary>
    /// 构造函数（内部使用）
    /// </summary>
    /// <param name="key">锁键名</param>
    /// <param name="value">持有者标识（token）</param>
    /// <param name="releaseFunc">释放锁的委托函数</param>
    internal DistributedLockToken(string key, string value, Func<string, Task<bool>> releaseFunc)
    {
        Key = key;
        Value = value;
        _releaseFunc = releaseFunc;
    }

    /// <summary>
    /// 锁键名
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// 持有者标识（token）
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 释放锁（仅当持有者匹配时）
    /// </summary>
    /// <returns>是否释放成功</returns>
    public async Task<bool> ReleaseAsync()
    {
        if (_released) return true;
        _released = true;
        return await _releaseFunc(Value);
    }

    /// <summary>
    /// 同步释放锁（通过 IAsyncDisposable 的同步包装）
    /// </summary>
    public void Dispose()
    {
        ReleaseAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// 异步释放锁
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await ReleaseAsync();
    }
}
