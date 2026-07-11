using LuBan.XTestProject.Models;

namespace LuBan.XTestProject
{
    [TestClass]
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

            Assert.IsNotNull(val1);


            var key2 = "test2";
            _cache.Set(key2, new TestInfo() { Name = "yswenli", Description = "yswenli is a good man", StartTime = DateTime.Now });

            var val2 = _cache.Get<TestInfo>(key2);

            Assert.IsNotNull(val2);
        }
    }
}
