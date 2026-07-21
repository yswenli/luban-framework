using LuBan.Orm;
using LuBan.Orm.Entities;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace LuBan.XTestProject
{
    [TestClass]
    public class BaseRepositoryUnitTest
    {
        [TestInitialize]
        public void Init()
        {
            LuBanOrm.Init();
        }

        [TestMethod]
        public void TestBasicRepositoryOperations()
        {
            var repo = new BaseRepository<DbUser>();
            var list = repo.GetList();
            Assert.IsNotNull(list);
        }

        [TestMethod]
        public void TestGetProviderReturnsNewInstanceEachCall()
        {
            var provider1 = LuBanOrm.GetProvider<DbUser>(LuBanOrmConst.MainConfigId);
            var provider2 = LuBanOrm.GetProvider<DbUser>(LuBanOrmConst.MainConfigId);
            Assert.AreNotSame(provider1, provider2, "GetProvider should return a new instance each call");
        }

        [TestMethod]
        public void TestConcurrentRepositoryCreation()
        {
            var exceptions = new ConcurrentBag<Exception>();
            var repositories = new ConcurrentBag<BaseRepository<DbUser>>();
            var tasks = new List<Task>();

            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var repo = new BaseRepository<DbUser>();
                        repositories.Add(repo);
                        var list = repo.GetList();
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            if (exceptions.Count > 0)
            {
                Assert.Fail($"Concurrent access failed with {exceptions.Count} exceptions. First: {exceptions.First().Message}");
            }
        }

        [TestMethod]
        public void TestConcurrentQueryOperations()
        {
            var exceptions = new ConcurrentBag<Exception>();
            var tasks = new List<Task>();

            for (int i = 0; i < 50; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var repo = new BaseRepository<DbUser>();
                        var list = repo.GetList();
                        var count = list.Count;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            if (exceptions.Count > 0)
            {
                Assert.Fail($"Concurrent query failed with {exceptions.Count} exceptions");
            }
        }

        [TestMethod]
        public void TestConcurrentInsertUpdateDelete()
        {
            var exceptions = new ConcurrentBag<Exception>();
            var insertedIds = new ConcurrentBag<long>();
            var tasks = new List<Task>();

            for (int i = 0; i < 20; i++)
            {
                int index = i;
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var repo = new BaseRepository<DbUser>();
                        var user = new DbUser
                        {
                            Account = $"concurrent_test_{index}_{Guid.NewGuid():N}",
                            Password = "test123",
                            RealName = $"Test User {index}",
                            OrgId = 1,
                            PosId = 1
                        };

                        var inserted = repo.InsertReturnEntity(user);
                        insertedIds.Add(inserted.Id);

                        inserted.Remark = $"Updated at {DateTime.Now}";
                        repo.Update(inserted);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            if (exceptions.Count > 0)
            {
                Assert.Fail($"Concurrent insert/update failed: {exceptions.First().Message}\n{exceptions.First().StackTrace}");
            }
        }

        [TestMethod]
        public void TestMultipleRepositoriesShareSameSqlSugarScope()
        {
            var repo1 = new BaseRepository<DbUser>();
            var repo2 = new BaseRepository<DbUser>();

            Assert.AreNotSame(repo1, repo2);
            Assert.AreSame(repo1.Context, repo2.Context);
        }

        [TestMethod]
        public void TestRepositoryContextIsSetCorrectly()
        {
            var repo = new BaseRepository<DbUser>();
            Assert.IsNotNull(repo.Context);
            Assert.IsNotNull(repo.AsSugarClient());
        }

        [TestMethod]
        public void TestSqlSugarScopeSingletonBehavior()
        {
            var scope1 = LuBanOrm.SqlSugarScope;
            var scope2 = LuBanOrm.SqlSugarScope;

            Assert.AreSame(scope1, scope2, "SqlSugarScope should be singleton");
        }

        [TestMethod]
        public void TestHighConcurrencyScenario()
        {
            var exceptions = new ConcurrentBag<Exception>();
            var successCount = 0;
            var lockObj = new object();
            var tasks = new List<Task>();

            var sw = Stopwatch.StartNew();

            for (int i = 0; i < 200; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var repo = new BaseRepository<DbUser>();
                        repo.Any(q => q.IsDelete == false);
                        lock (lockObj)
                        {
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            sw.Stop();

            if (exceptions.Count > 0)
            {
                var errorMessages = exceptions.Select(e => e.Message).Distinct().Take(5);
                Assert.Fail($"High concurrency test failed: {exceptions.Count} errors in {sw.ElapsedMilliseconds}ms. Sample errors: {string.Join("; ", errorMessages)}");
            }
        }

        [TestMethod]
        public void TestParallelRepositoryWithDifferentTenants()
        {
            var exceptions = new ConcurrentBag<Exception>();
            var tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var repo = new BaseRepository<DbUser>(LuBanOrmConst.DefaultTenantId);
                        var list = repo.GetList();
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            if (exceptions.Count > 0)
            {
                Assert.Fail($"Multi-tenant parallel test failed: {exceptions.First().Message}");
            }
        }

        [TestMethod]
        public void TestDisposePattern()
        {
            for (int i = 0; i < 10; i++)
            {
                var repo = new BaseRepository<DbUser>();
                var list = repo.GetList();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [TestMethod]
        public void TestTransactionIsolation()
        {
            var exceptions = new ConcurrentBag<Exception>();
            var tasks = new List<Task>();

            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var repo = new BaseRepository<DbUser>();
                        using (var tran = repo.CreateTran())
                        {
                            var list = repo.GetList();
                            tran.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            if (exceptions.Count > 0)
            {
                Assert.Fail($"Transaction isolation test failed: {exceptions.First().Message}");
            }
        }
    }
}
