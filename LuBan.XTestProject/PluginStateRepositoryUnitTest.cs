/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： PluginStateRepositoryUnitTest
*描述：验证插件状态仓储的 JSON 持久化往返、覆盖保存、删除与损坏文件容错
*
*****************************************************************************/
using LuBan.AIAgent.Plugins;
using LuBan.AIAgent.Plugins.Lifecycle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

/// <summary>
/// <see cref="PluginStateRepository"/> 测试（框架层 JSON 实现，宿主可在 M2 换 SQLite 版）。
/// </summary>
[TestClass]
public class PluginStateRepositoryUnitTest
{
    /// <summary>
    /// 每个测试用独立的临时状态文件，避免相互串扰与污染真实配置。
    /// </summary>
    private static string NewStateFile() => Path.Combine(
        Path.GetTempPath(),
        "luban-plugin-state-" + Guid.NewGuid().ToString("N") + ".json");

    [TestMethod]
    public async Task SaveThenGet_RoundTripsAllFields()
    {
        var file = NewStateFile();
        try
        {
            var repository = new PluginStateRepository(file);
            var record = new PluginStateRecord
            {
                Id = "com.tjc.newproject",
                Version = "1.0.0",
                State = PluginState.Enabled,
                LastError = null,
                RequiresRestart = false
            };

            await repository.SaveAsync(record);

            var loaded = await repository.GetAsync("com.tjc.newproject");

            Assert.IsNotNull(loaded);
            Assert.AreEqual("com.tjc.newproject", loaded!.Id);
            Assert.AreEqual("1.0.0", loaded.Version);
            Assert.AreEqual(PluginState.Enabled, loaded.State);
            Assert.IsFalse(loaded.RequiresRestart);
        }
        finally
        {
            TryDelete(file);
        }
    }

    [TestMethod]
    public async Task RequiresRestart_IsPersisted()
    {
        var file = NewStateFile();
        try
        {
            await new PluginStateRepository(file).SaveAsync(new PluginStateRecord
            {
                Id = "com.tjc.newproject",
                State = PluginState.Disabled,
                RequiresRestart = true,
                LastError = "程序集卸载验证未通过"
            });

            // 新实例重读：验证「降级 + 提示重启」状态能跨进程边界留存（PRD §3.12 第 7 步可观测性）
            var loaded = await new PluginStateRepository(file).GetAsync("com.tjc.newproject");

            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded!.RequiresRestart);
            Assert.AreEqual("程序集卸载验证未通过", loaded.LastError);
        }
        finally
        {
            TryDelete(file);
        }
    }

    [TestMethod]
    public async Task SaveTwice_OverwritesSameRecord()
    {
        var file = NewStateFile();
        try
        {
            var repository = new PluginStateRepository(file);

            await repository.SaveAsync(new PluginStateRecord { Id = "p1", Version = "1.0.0", State = PluginState.Enabled });
            await repository.SaveAsync(new PluginStateRecord { Id = "p1", Version = "1.0.1", State = PluginState.Disabled });

            var all = await repository.GetAllAsync();

            Assert.AreEqual(1, all.Count);
            Assert.AreEqual("1.0.1", all[0].Version);
            Assert.AreEqual(PluginState.Disabled, all[0].State);
        }
        finally
        {
            TryDelete(file);
        }
    }

    [TestMethod]
    public async Task GetAll_ReturnsEveryRecord()
    {
        var file = NewStateFile();
        try
        {
            var repository = new PluginStateRepository(file);
            await repository.SaveAsync(new PluginStateRecord { Id = "b" });
            await repository.SaveAsync(new PluginStateRecord { Id = "a" });

            var all = await repository.GetAllAsync();

            Assert.AreEqual(2, all.Count);
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, all.Select(r => r.Id).ToArray());
        }
        finally
        {
            TryDelete(file);
        }
    }

    [TestMethod]
    public async Task Delete_RemovesRecord()
    {
        var file = NewStateFile();
        try
        {
            var repository = new PluginStateRepository(file);
            await repository.SaveAsync(new PluginStateRecord { Id = "p1" });
            await repository.SaveAsync(new PluginStateRecord { Id = "p2" });

            await repository.DeleteAsync("p1");

            Assert.IsNull(await repository.GetAsync("p1"));
            Assert.IsNotNull(await repository.GetAsync("p2"));
        }
        finally
        {
            TryDelete(file);
        }
    }

    [TestMethod]
    public async Task Get_UnknownId_ReturnsNull()
    {
        var file = NewStateFile();
        try
        {
            Assert.IsNull(await new PluginStateRepository(file).GetAsync("never-installed"));
        }
        finally
        {
            TryDelete(file);
        }
    }

    [TestMethod]
    public async Task MissingFile_YieldsEmptySet()
    {
        var file = NewStateFile();

        var repository = new PluginStateRepository(file);

        Assert.AreEqual(0, (await repository.GetAllAsync()).Count);
    }

    [TestMethod]
    public async Task CorruptedFile_IsToleratedAsEmpty()
    {
        var file = NewStateFile();
        try
        {
            await File.WriteAllTextAsync(file, "{ this is not json ");

            var repository = new PluginStateRepository(file);

            // 损坏的状态文件不能让宿主启动失败：按空集处理，随后覆盖保存即可自愈
            Assert.AreEqual(0, (await repository.GetAllAsync()).Count);

            await repository.SaveAsync(new PluginStateRecord { Id = "p1", State = PluginState.Enabled });
            Assert.AreEqual(PluginState.Enabled, (await repository.GetAsync("p1"))!.State);
        }
        finally
        {
            TryDelete(file);
        }
    }

    [TestMethod]
    public async Task Save_WithoutId_Throws()
    {
        var file = NewStateFile();
        try
        {
            try
            {
                await new PluginStateRepository(file).SaveAsync(new PluginStateRecord { Id = "  " });
                Assert.Fail("缺少 id 应抛出 ArgumentException");
            }
            catch (ArgumentException)
            {
                // 预期
            }
        }
        finally
        {
            TryDelete(file);
        }
    }

    [TestMethod]
    public void DefaultFilePath_IsProvided()
    {
        var repository = new PluginStateRepository();

        Assert.IsFalse(string.IsNullOrWhiteSpace(repository.FilePath));
    }

    private static void TryDelete(string file)
    {
        try
        {
            if (File.Exists(file))
                File.Delete(file);
        }
        catch
        {
            // 忽略
        }
    }
}