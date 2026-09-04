/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Logging
*文件名： LuBanLogger.cs
*版本号： V1.0.0.0
*唯一标识：52b63b88-af1c-4566-adac-597ac635a33d
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31 14:36:22
*描述：LuBanLogger 类
*
*=================================================
*修改标记
*修改时间：2026/7/31 14:36:22
*修改人： yswenli
*版本号： V1.0.0.0
*描述：LuBanLogger 类
*
*****************************************************************************/

namespace LuBan.Logging;

/// <summary>
/// LuBan 日志服务，通过 DI 单例方式提供 ILogger 创建能力。
/// </summary>
public interface ILuBanLogger
{
    /// <summary>
    /// 按类别名称创建日志记录器。
    /// </summary>
    ILogger CreateLogger(string categoryName);

    /// <summary>
    /// 按类型创建日志记录器。
    /// </summary>
    ILogger<T> CreateLogger<T>();
}

/// <summary>
/// LuBan 日志服务实现，封装 ILoggerFactory。
/// </summary>
public sealed class LuBanLogger : ILuBanLogger, ISingleton
{
    private readonly ILoggerFactory _factory;

    /// <summary>
    /// 初始化 LuBan 日志服务。
    /// </summary>
    /// <param name="factory">日志工厂。</param>
    public LuBanLogger(ILoggerFactory factory)
    {
        _factory = factory;
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return _factory.CreateLogger(categoryName);
    }

    /// <inheritdoc/>
    public ILogger<T> CreateLogger<T>()
    {
        return _factory.CreateLogger<T>();
    }
}
