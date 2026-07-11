/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： Limiter
*版本号： V1.0.0.0
*唯一标识：5c8c6b34-94fb-4d3b-87c1-3214af43bb68
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/7/8 13:10:34
*描述：限制器
*
*=====================================================================
*修改标记
*修改时间：2022/7/8 13:10:34
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：限制器
*
*****************************************************************************/
namespace LuBan.Common;

/// <summary>
/// 限制器，
/// 一般用于进程级指定速度限制的处理场景
/// </summary>
public class Limiter
{
    int _maxSize = 15;

    int _size = 1;

    float _recoverySeconds = 60.0F;

    DateTime _dateTime;

    /// <summary>
    /// 限制器
    /// </summary>
    /// <param name="maxSize">最大请求量</param>
    /// <param name="recoverySeconds">恢复速度为指定时间（秒）</param>
    public Limiter(int maxSize, float recoverySeconds = 60.0F)
    {
        _maxSize = maxSize;

        _recoverySeconds = recoverySeconds;

        _dateTime = DateTimeUtil.Now;
    }

    /// <summary>
    /// 执行被限制的方法
    /// </summary>
    /// <param name="action"></param>
    public void Execut(Action action)
    {
        using var lockInfo = LockerBuilder.Default.Create("Limiter.GetOrSet");

        if (_size < _maxSize)
        {
            _size++;
        }
        else if (_size == _maxSize)
        {
            _dateTime = DateTimeUtil.Now;
            _size++;
        }
        else
        {
            var timeSpan = DateTimeUtil.Now - _dateTime;

            if (timeSpan.TotalSeconds < _recoverySeconds)
            {
                Thread.Sleep((int)((_recoverySeconds - timeSpan.TotalSeconds) * 1000));
                _dateTime = DateTimeUtil.Now;
            }
            else
            {
                _size = _maxSize - ((int)(timeSpan.TotalSeconds / _recoverySeconds));

                if (_size <= 1)
                {
                    _size = 1;
                }
            }
        }
        action.Invoke();

    }

    /// <summary>
    /// 执行被限制的方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public T Execut<T>(Func<T> func)
    {
        using var lockInfo = LockerBuilder.Default.Create("Limiter.GetOrSet");

        if (_size < _maxSize)
        {
            _size++;
        }
        else if (_size == _maxSize)
        {
            _dateTime = DateTimeUtil.Now;
            _size++;
        }
        else
        {
            var timeSpan = DateTimeUtil.Now - _dateTime;

            if (timeSpan.TotalSeconds < _recoverySeconds)
            {
                Thread.Sleep((int)((_recoverySeconds - (int)timeSpan.TotalSeconds) * 1000));
                _dateTime = DateTimeUtil.Now;
            }
            else
            {
                _size = _maxSize - ((int)(timeSpan.TotalSeconds / _recoverySeconds));

                if (_size <= 1)
                {
                    _size = 1;
                }
            }
        }
        return func.Invoke();

    }

    #region static

    static ConcurrentDictionary<string, Limiter> _limiterCache;

    static Limiter()
    {
        _limiterCache = new ConcurrentDictionary<string, Limiter>();
    }

    /// <summary>
    /// 创建限制器，根据指定的key
    /// </summary>
    /// <param name="key">不同的key的单例</param>
    /// <param name="maxSize">最大次数</param>
    /// <param name="recoverySeconds">恢复时长</param>
    /// <returns></returns>
    public static Limiter Create(string key, int maxSize, float recoverySeconds = 60.0F)
    {
        return _limiterCache.GetOrAdd(key, (k) => new Limiter(maxSize, recoverySeconds));
    }

    #endregion
}
