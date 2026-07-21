using LuBan.Orm;
using LuBan.Orm.Entities;
using System.Collections.Concurrent;
using System.Diagnostics;
using SqlSugar;

namespace LuBan.XTestProject;

public class OrmConcurrentTest
{
    public static void RunAllTests()
    {
        Console.WriteLine("=== LuBan.ORM Concurrent Tests ===\n");

        Console.Write("Initializing ORM... ");
        try
        {
            LuBanOrm.Init();
            Console.WriteLine("OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAILED: {ex.Message}");
            return;
        }

        TestBasicOperations();
        TestConcurrentRepositoryCreation();
        TestConcurrentQueryOperations();
        TestConcurrentInsertUpdate();
        TestMultipleRepositoriesShareSameContext();
        TestContextBehavior();
        TestHighConcurrencyScenario();
        TestTransactionIsolation();
        TestDbTranRollbackSemantics();
        TestSimpleClientContextOverride();

        Console.WriteLine("\n=== All Tests Completed ===");
    }

    static void TestBasicOperations()
    {
        Console.WriteLine("\n[Test 1] Basic Operations");
        try
        {
            var repo = new BaseRepository<DbUser>();
            var list = repo.GetList();
            Console.WriteLine($"  PASS: GetList returned {list.Count} items");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  FAIL: {ex.Message}");
        }
    }

    static void TestConcurrentRepositoryCreation()
    {
        Console.WriteLine("\n[Test 2] Concurrent Repository Creation (100 threads)");
        var exceptions = new ConcurrentBag<Exception>();
        var tasks = new List<Task>();

        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    var repo = new BaseRepository<DbUser>();
                    repo.GetList();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        if (exceptions.Count == 0)
            Console.WriteLine("  PASS: All 100 threads completed successfully");
        else
        {
            Console.WriteLine($"  FAIL: {exceptions.Count} exceptions occurred");
            foreach (var ex in exceptions.Take(3))
                Console.WriteLine($"    - {ex.GetType().Name}: {ex.Message}");
        }
    }

    static void TestConcurrentQueryOperations()
    {
        Console.WriteLine("\n[Test 3] Concurrent Query Operations (50 threads)");
        var exceptions = new ConcurrentBag<Exception>();
        var tasks = new List<Task>();

        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    var repo = new BaseRepository<DbUser>();
                    repo.GetList();
                    repo.Any(q => q.IsDelete == false);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        if (exceptions.Count == 0)
            Console.WriteLine("  PASS: All 50 query operations completed");
        else
        {
            Console.WriteLine($"  FAIL: {exceptions.Count} exceptions");
            foreach (var ex in exceptions.Take(3))
                Console.WriteLine($"    - {ex.GetType().Name}: {ex.Message}");
        }
    }

    static void TestConcurrentInsertUpdate()
    {
        Console.WriteLine("\n[Test 4] Concurrent Insert/Update (20 threads)");
        var exceptions = new ConcurrentBag<Exception>();
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

        if (exceptions.Count == 0)
            Console.WriteLine("  PASS: All insert/update operations completed");
        else
        {
            Console.WriteLine($"  FAIL: {exceptions.Count} exceptions");
            foreach (var ex in exceptions.Take(3))
            {
                Console.WriteLine($"    - {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"      StackTrace: {ex.StackTrace?.Split('\n').FirstOrDefault()}");
            }
        }
    }

    static void TestMultipleRepositoriesShareSameContext()
    {
        Console.WriteLine("\n[Test 5] Multiple Repositories Context Sharing");
        try
        {
            var repo1 = new BaseRepository<DbUser>();
            var repo2 = new BaseRepository<DbUser>();

            bool sameContext = object.ReferenceEquals(repo1.Context, repo2.Context);
            Console.WriteLine($"  repo1.Context == repo2.Context: {sameContext}");

            if (sameContext)
                Console.WriteLine("  PASS: Repositories share the same SqlSugarClient context");
            else
                Console.WriteLine("  INFO: Repositories have different contexts (may indicate issue)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  FAIL: {ex.Message}");
        }
    }

    static void TestContextBehavior()
    {
        Console.WriteLine("\n[Test 5.1] Deep Context Analysis");
        try
        {
            var scope = LuBanOrm.SqlSugarScope;
            Console.WriteLine($"  SqlSugarScope instance: {scope?.GetHashCode()}");

            var provider1 = LuBanOrm.GetProvider<DbUser>(LuBanOrmConst.MainConfigId);
            var provider2 = LuBanOrm.GetProvider<DbUser>(LuBanOrmConst.MainConfigId);

            Console.WriteLine($"  provider1.GetHashCode: {provider1.GetHashCode()}");
            Console.WriteLine($"  provider2.GetHashCode: {provider2.GetHashCode()}");
            Console.WriteLine($"  provider1.Provider.GetHashCode: {provider1.Provider.GetHashCode()}");
            Console.WriteLine($"  provider2.Provider.GetHashCode: {provider2.Provider.GetHashCode()}");
            Console.WriteLine($"  provider1.Provider == provider2.Provider: {object.ReferenceEquals(provider1.Provider, provider2.Provider)}");

            var tenant = scope.AsTenant();
            var connScope1 = tenant.GetConnectionScope(LuBanOrmConst.MainConfigId);
            var connScope2 = tenant.GetConnectionScope(LuBanOrmConst.MainConfigId);
            Console.WriteLine($"  connScope1 == connScope2: {object.ReferenceEquals(connScope1, connScope2)}");

            Console.WriteLine($"  connScope1.Ado.Connection.GetHashCode: {connScope1.Ado.Connection?.GetHashCode()}");
            Console.WriteLine($"  connScope2.Ado.Connection.GetHashCode: {connScope2.Ado.Connection?.GetHashCode()}");
            Console.WriteLine($"  Underlying connection is same: {object.ReferenceEquals(connScope1.Ado.Connection, connScope2.Ado.Connection)}");

            var repo1 = new BaseRepository<DbUser>();
            var repo2 = new BaseRepository<DbUser>();
            Console.WriteLine($"  repo1.Context.GetHashCode: {repo1.Context?.GetHashCode()}");
            Console.WriteLine($"  repo2.Context.GetHashCode: {repo2.Context?.GetHashCode()}");

            Console.WriteLine($"  repo1.Ado.Connection.GetHashCode: {repo1.AsSugarClient().Ado.Connection?.GetHashCode()}");
            Console.WriteLine($"  repo2.Ado.Connection.GetHashCode: {repo2.AsSugarClient().Ado.Connection?.GetHashCode()}");
            Console.WriteLine($"  repo1 and repo2 share connection: {object.ReferenceEquals(repo1.AsSugarClient().Ado.Connection, repo2.AsSugarClient().Ado.Connection)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  FAIL: {ex.Message}");
            Console.WriteLine($"  StackTrace: {ex.StackTrace}");
        }
    }

    static void TestHighConcurrencyScenario()
    {
        Console.WriteLine("\n[Test 6] High Concurrency Scenario (200 threads)");
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

        Console.WriteLine($"  Completed in {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"  Success: {successCount}/200");

        if (exceptions.Count == 0)
            Console.WriteLine("  PASS: All operations completed successfully");
        else
        {
            Console.WriteLine($"  FAIL: {exceptions.Count} exceptions");
            var distinctErrors = exceptions.Select(e => e.Message).Distinct().Take(5);
            foreach (var err in distinctErrors)
                Console.WriteLine($"    - {err}");
        }
    }

    static void TestTransactionIsolation()
    {
        Console.WriteLine("\n[Test 7] Transaction Isolation (5 concurrent transactions)");
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
                        repo.GetList();
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

        if (exceptions.Count == 0)
            Console.WriteLine("  PASS: All transactions completed");
        else
        {
            Console.WriteLine($"  FAIL: {exceptions.Count} exceptions");
            foreach (var ex in exceptions.Take(3))
                Console.WriteLine($"    - {ex.GetType().Name}: {ex.Message}");
        }
    }

    static void TestDbTranRollbackSemantics()
    {
        Console.WriteLine("\n[Test 7.1] DbTran Rollback Semantics");

        // 未Commit时Dispose自动回滚
        try
        {
            var marker = $"rollback_test_{Guid.NewGuid():N}";
            var repo = new BaseRepository<DbUser>();
            var beforeCount = repo.GetList().Count(u => u.Account.StartsWith("rollback_test_"));

            using (var tran = repo.CreateTran())
            {
                repo.Insert(new DbUser
                {
                    Account = marker,
                    Password = "test",
                    RealName = "Rollback Test",
                    OrgId = 1,
                    PosId = 1
                });
                //不调用Commit，Dispose时应自动回滚
            }

            var afterCount = repo.GetList().Count(u => u.Account == marker);
            if (afterCount == beforeCount)
                Console.WriteLine("  PASS: Uncommitted transaction was rolled back on Dispose");
            else
                Console.WriteLine($"  FAIL: Expected rollback but found {afterCount - beforeCount} committed row(s)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  FAIL: {ex.Message}");
        }

        // 重复Commit幂等
        try
        {
            var repo = new BaseRepository<DbUser>();
            using (var tran = repo.CreateTran())
            {
                repo.GetList();
                tran.Commit();
                tran.Commit();
            }
            Console.WriteLine("  PASS: Double Commit is idempotent");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  FAIL: Double Commit threw: {ex.Message}");
        }

        // Commit-after-Dispose应抛ObjectDisposedException
        try
        {
            var repo = new BaseRepository<DbUser>();
            DbTran tran;
            using (tran = repo.CreateTran())
            {
                repo.GetList();
            }
            try
            {
                tran.Commit();
                Console.WriteLine("  FAIL: Commit after Dispose did not throw");
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("  PASS: Commit after Dispose throws ObjectDisposedException");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  FAIL: {ex.Message}");
        }
    }

    static void TestSimpleClientContextOverride()
    {
        Console.WriteLine("\n[Test 8] SimpleClient Context Override Analysis");
        try
        {
            var repo1 = new BaseRepository<DbUser>();
            Console.WriteLine($"  After repo1 creation, Context type: {repo1.Context?.GetType().Name}");

            var repo2 = new BaseRepository<DbUser>();
            Console.WriteLine($"  After repo2 creation:");
            Console.WriteLine($"    repo1.Context.GetHashCode: {repo1.Context?.GetHashCode()}");
            Console.WriteLine($"    repo2.Context.GetHashCode: {repo2.Context?.GetHashCode()}");

            repo1.GetList();
            Console.WriteLine($"  After repo1.GetList(), repo1.Context: {repo1.Context?.GetHashCode()}");

            repo2.GetList();
            Console.WriteLine($"  After repo2.GetList(), repo2.Context: {repo2.Context?.GetHashCode()}");

            repo1.GetList();
            Console.WriteLine($"  After repo1.GetList() again, repo1.Context: {repo1.Context?.GetHashCode()}");

            Console.WriteLine("  PASS: Context behavior analyzed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  FAIL: {ex.Message}");
        }
    }

    public static void RunStressTest()
    {
        Console.WriteLine("\n=== STRESS TEST: High Concurrent Write Operations ===\n");

        var exceptions = new ConcurrentBag<Exception>();
        var successCount = 0;
        var failCount = 0;
        var lockObj = new object();
        var stopwatch = Stopwatch.StartNew();

        var tasks = new List<Task>();

        Console.WriteLine("Running 500 concurrent insert operations (3 inserts each)...");

        for (int i = 0; i < 500; i++)
        {
            int index = i;
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    var repo = new BaseRepository<DbUser>();

                    for (int j = 0; j < 3; j++)
                    {
                        var user = new DbUser
                        {
                            Account = $"stress_{index}_{j}_{Guid.NewGuid():N}",
                            Password = "test",
                            RealName = $"Stress User {index}",
                            OrgId = 1,
                            PosId = 1
                        };

                        repo.Insert(user);
                    }

                    lock (lockObj)
                    {
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    lock (lockObj)
                    {
                        failCount++;
                    }
                    exceptions.Add(ex);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());
        stopwatch.Stop();

        Console.WriteLine($"\nResults:");
        Console.WriteLine($"  Total time: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"  Success: {successCount}/500");
        Console.WriteLine($"  Failed: {failCount}/500");

        if (exceptions.Count > 0)
        {
            Console.WriteLine($"\n  Error types:");
            var groupedErrors = exceptions.GroupBy(e => e.GetType().Name).Select(g => new { Type = g.Key, Count = g.Count() });
            foreach (var error in groupedErrors)
            {
                Console.WriteLine($"    {error.Type}: {error.Count}");
            }

            Console.WriteLine($"\n  Sample errors:");
            foreach (var ex in exceptions.Take(3))
            {
                Console.WriteLine($"    - {ex.Message}");
                Console.WriteLine($"      {ex.StackTrace?.Split('\n').FirstOrDefault()?.Trim()}");
            }
        }

        var repo2 = new BaseRepository<DbUser>();
        var totalUsers = repo2.GetList().Count(u => u.Account.StartsWith("stress_"));
        Console.WriteLine($"\n  Total users in DB with 'stress_' prefix: {totalUsers}");
    }
}