/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.XTestProject
*文件名： MemoryCacheUnitTest.cs
*版本号： V1.0.0.0
*唯一标识：a1f191a0-f989-497a-b34f-823d82b9f09e
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/13 12:05:30
*描述：MemoryCacheUnitTest 类
*
*=================================================
*修改标记
*修改时间：2026/7/13 12:05:30
*修改人： yswenli
*版本号： V1.0.0.0
*描述：MemoryCacheUnitTest 类
*
*****************************************************************************/

using LuBan.XTestProject.Models;

namespace LuBan.XTestProject
{
    [TestClass]
    /// <summary>
/// MemoryCacheUnitTest 单元测试类
/// </summary>
    public class MemoryCacheUnitTest
    {
        MemoryCache _cache;

        [TestInitialize]
        public void Initialize()
        {
            _cache = MemoryCache.Instance;
        }


        [TestMethod]
        public void TestMethod1()
        {
            var key = "test";
            _cache.Set(key, "test value");

            var val = _cache.Get<string>(key);

            Assert.IsNotNull(val);

            var key1 = "test1";
            _cache.Set(key1, 39654);

            var val1 = _cache.Get<int>(key1);

            Assert.AreEqual(39654, val1);


            var key2 = "test2";
            _cache.Set(key2, new TestInfo() { Name = "yswenli", Description = "yswenli is a good man", StartTime = DateTime.Now });

            var val2 = _cache.Get<TestInfo>(key2);

            Assert.IsNotNull(val2);
        }
    }
}
