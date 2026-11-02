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
*文件名： LocalCacheUtilUnitTest
*版本号： V1.0.0.0
*唯一标识：96bb67f5-2e60-45c2-b027-35738bb1a76e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/30 14:10:47
*描述：本地缓存读取过期项不阻塞的回归单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/30 14:10:47
*修改人： yswenli
*版本号： V1.0.0.0
*描述：本地缓存读取过期项不阻塞的回归单元测试
*
*****************************************************************************/

namespace LuBan.XTestProject;

/// <summary>
/// 本地缓存工具类单元测试
/// </summary>
/// <remarks>
/// 这两条用例针对「读取过期项」的回归：过期清理必须在调用方持有的缓存锁内完成，
/// 不能再调用会重复获取同一把锁的公开方法，否则读取线程会永久等待。
/// </remarks>
[TestClass]
public class LocalCacheUtilUnitTest
{
    static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 读取已过期项应快速返回默认值，不得卡在缓存锁上
    /// </summary>
    [TestMethod]
    public void Get_ExpiredItem_ReturnsDefaultWithoutBlocking()
    {
        var key = NewKey(nameof(Get_ExpiredItem_ReturnsDefaultWithoutBlocking));

        try
        {
            LocalCacheUtil.Set(key, "expired-value", TimeSpan.FromSeconds(-1));

            var task = Task.Run(() => LocalCacheUtil.Get<string>(key));

            Assert.IsTrue(task.Wait(WaitTimeout), "读取过期缓存项超过 5 秒未返回，疑似锁内重入死锁");

            Assert.IsNull(task.Result);
        }
        finally
        {
            LocalCacheUtil.Remove(key);
        }
    }

    /// <summary>
    /// 过期项应触发工厂重新取值，且不阻塞
    /// </summary>
    [TestMethod]
    public void GetOrSet_ExpiredItem_InvokesFactoryWithoutBlocking()
    {
        var key = NewKey(nameof(GetOrSet_ExpiredItem_InvokesFactoryWithoutBlocking));
        var invoked = 0;

        try
        {
            LocalCacheUtil.Set(key, "expired-value", TimeSpan.FromSeconds(-1));

            var task = Task.Run(() => LocalCacheUtil.GetOrSet<string>(key, k =>
            {
                Interlocked.Increment(ref invoked);

                return "fresh-value";
            }, TimeSpan.FromMinutes(1)));

            Assert.IsTrue(task.Wait(WaitTimeout), "过期缓存项取值超过 5 秒未返回，疑似锁内重入死锁");

            Assert.AreEqual("fresh-value", task.Result);
            Assert.AreEqual(1, Volatile.Read(ref invoked));
        }
        finally
        {
            LocalCacheUtil.Remove(key);
        }
    }

    /// <summary>
    /// 生成互不干扰的缓存键（同一进程内静态字典共享）
    /// </summary>
    static string NewKey(string suffix) => $"LocalCacheUtilUnitTest.{suffix}.{Guid.NewGuid():N}";
}