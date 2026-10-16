/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： PluginManagerLifecycleUnitTest
*描述：验证 PluginManager 的扫描/启用/禁用/安装/卸载与「插件自行注册的贡献也必须被注销」的记账逻辑
*
*****************************************************************************/
using LuBan.AIAgent.Plugins;
using LuBan.AIAgent.Plugins.Base;
using LuBan.AIAgent.Plugins.Lifecycle;
using LuBan.AIAgent.Plugins.Loading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

/// <summary>
/// <see cref="PluginManager"/> 生命周期测试（内置插件路径 + 目录插件登记/卸载路径）。
/// </summary>
/// <remarks>
/// 说明：真实 ALC 热卸载（collectible + GC 弱引用断言）需要独立编译的插件程序集，
/// 不在单元测试内构造；本节覆盖 manager 的编排、记账与状态持久化。
/// </remarks>
[TestClass]
public class PluginManagerLifecycleUnitTest
{
    private const string BuiltinId = "com.tjc.builtin-demo";

    private static string NewStateFile() => Path.Combine(
        Path.GetTempPath(),
        "luban-plugin-state-" + Guid.NewGuid().ToString("N") + ".json");

    /// <summary>
    /// 组装一个管理器和记账宿主（<paramref name="withHost"/> 为 false 时不注册 IPluginHost）。
    /// </summary>
    private static (PluginManager Manager, RecordingPluginHost Host, string StateFile, ServiceProvider Provider)
        CreateManager(string? stateFile = null, bool withHost = true, IEnumerable<IPlugin>? builtins = null)
    {
        var host = new RecordingPluginHost();
        var services = new ServiceCollection();
        if (withHost)
            services.AddSingleton<IPluginHost>(host);
        var provider = services.BuildServiceProvider();

        host.Services = provider;

        var file = stateFile ?? NewStateFile();
        var manager = new PluginManager(
            new PluginLoader(new ManifestValidator(), new PluginFactory()),
            new PluginDependencyResolver(),
            new BuiltinPluginCatalog(builtins ?? Array.Empty<IPlugin>()),
            new PluginStateRepository(file),
            provider);

        return (manager, host, file, provider);
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

    [TestMethod]
    public async Task ScanAsync_RegistersBuiltinPluginAsInstalled()
    {
        var context = CreateManager(builtins: new[] { new RecordingPlugin(BuiltinId) });
        try
        {
            var count = await context.Manager.ScanAsync(Array.Empty<string>());

            Assert.AreEqual(1, count);
            var descriptor = context.Manager.GetPlugin(BuiltinId);
            Assert.IsNotNull(descriptor);
            Assert.AreEqual(PluginState.Installed, descriptor!.State);
            Assert.IsTrue(descriptor.IsBuiltin);
        }
        finally
        {
            context.Provider.Dispose();
            TryDelete(context.StateFile);
        }
    }

    [TestMethod]
    public async Task EnableBuiltinAsync_RegistersRuntimeAndContributedSkills()
    {
        var plugin = new RecordingPlugin(BuiltinId);
        var context = CreateManager(builtins: new[] { plugin });
        try
        {
            await context.Manager.ScanAsync(Array.Empty<string>());

            Assert.IsTrue(await context.Manager.EnableAsync(BuiltinId));

            Assert.AreEqual(PluginState.Enabled, context.Manager.GetPlugin(BuiltinId)!.State);
            Assert.IsTrue(plugin.Initialized);
            Assert.AreEqual(1, context.Host.SidebarEntries.Count);

            // 三条通道都应落地：InitializeAsync 内自行注册 + ContributeSkills 声明
            CollectionAssert.AreEquivalent(
                new[] { RecordingPlugin.RuntimeSkillId, RecordingPlugin.ContributedSkillId },
                context.Host.Skills.ToArray());
            CollectionAssert.AreEquivalent(
                new[] { RecordingPlugin.RuntimeMcpName },
                context.Host.McpServers.ToArray());
        }
        finally
        {
            context.Provider.Dispose();
            TryDelete(context.StateFile);
        }
    }

    [TestMethod]
    public async Task DisableBuiltinAsync_UnregistersRuntimeRegisteredContributions()
    {
        var plugin = new RecordingPlugin(BuiltinId);
        var context = CreateManager(builtins: new[] { plugin });
        try
        {
            await context.Manager.ScanAsync(Array.Empty<string>());
            await context.Manager.EnableAsync(BuiltinId);

            Assert.IsTrue(await context.Manager.DisableAsync(BuiltinId));

            // 回归守卫（PRD §3.12 第 2/3 步）：插件在 InitializeAsync 中「自行注册」的条目
            // 同样必须被精确注销，否则会成为宿主持有的残留引用，钉死 ALC。
            Assert.AreEqual(PluginState.Disabled, context.Manager.GetPlugin(BuiltinId)!.State);
            Assert.IsTrue(plugin.Shutdown);
            Assert.AreEqual(0, context.Host.SidebarEntries.Count);
            Assert.AreEqual(0, context.Host.Skills.Count);
            Assert.AreEqual(0, context.Host.McpServers.Count);
        }
        finally
        {
            context.Provider.Dispose();
            TryDelete(context.StateFile);
        }
    }

    [TestMethod]
    public async Task EnableBuiltinAsync_WithoutPluginHost_MarksError()
    {
        var context = CreateManager(withHost: false, builtins: new[] { new RecordingPlugin(BuiltinId) });
        try
        {
            await context.Manager.ScanAsync(Array.Empty<string>());

            // M1 行为：宿主机还未提供 IPluginHost 时不得静默成功
            Assert.IsFalse(await context.Manager.EnableAsync(BuiltinId));

            var descriptor = context.Manager.GetPlugin(BuiltinId)!;
            Assert.AreEqual(PluginState.Error, descriptor.State);
            Assert.IsTrue(descriptor.LastError!.Contains("IPluginHost"));
        }
        finally
        {
            context.Provider.Dispose();
            TryDelete(context.StateFile);
        }
    }

    [TestMethod]
    public async Task EnableAsync_UnknownPlugin_ReturnsFalse()
    {
        var context = CreateManager();
        try
        {
            Assert.IsFalse(await context.Manager.EnableAsync("com.tjc.not-installed"));
            Assert.IsFalse(await context.Manager.DisableAsync("com.tjc.not-installed"));
        }
        finally
        {
            context.Provider.Dispose();
            TryDelete(context.StateFile);
        }
    }

    [TestMethod]
    public async Task DisableThenScan_RestoresPersistedEnabledState()
    {
        var stateFile = NewStateFile();
        try
        {
            // 第一轮：启用后停用（ShouldEnableOnStartup 归位 false）
            var first = CreateManager(stateFile, builtins: new[] { new RecordingPlugin(BuiltinId) });
            using (first.Provider)
            {
                await first.Manager.ScanAsync(Array.Empty<string>());
                await first.Manager.EnableAsync(BuiltinId);
                await first.Manager.DisableAsync(BuiltinId);
                Assert.AreEqual(0, await first.Manager.RestoreEnabledAsync());
            }

            // 第二轮：状态为 Disabled，不应被自动启用（内置插件同样持久化，重启仍禁用）
            var second = CreateManager(stateFile, builtins: new[] { new RecordingPlugin(BuiltinId) });
            using (second.Provider)
            {
                await second.Manager.ScanAsync(Array.Empty<string>());
                Assert.AreEqual(PluginState.Disabled, second.Manager.GetPlugin(BuiltinId)!.State);
                Assert.AreEqual(0, await second.Manager.RestoreEnabledAsync());
                Assert.AreEqual(PluginState.Disabled, second.Manager.GetPlugin(BuiltinId)!.State);
            }
        }
        finally
        {
            TryDelete(stateFile);
        }
    }

    [TestMethod]
    public async Task RestoreEnabledAsync_ReEnablesPluginsLeftEnabled()
    {
        var stateFile = NewStateFile();
        try
        {
            // 第一轮：只启用不停用（模拟上次退出时插件仍处于 Enabled）
            var first = CreateManager(stateFile, builtins: new[] { new RecordingPlugin(BuiltinId) });
            using (first.Provider)
            {
                await first.Manager.ScanAsync(Array.Empty<string>());
                await first.Manager.EnableAsync(BuiltinId);
            }

            // 第二轮：扫描应恢复 Enabled 记忆，RestoreEnabledAsync 实际执行启用
            var second = CreateManager(stateFile, builtins: new[] { new RecordingPlugin(BuiltinId) });
            using (second.Provider)
            {
                await second.Manager.ScanAsync(Array.Empty<string>());
                Assert.AreEqual(PluginState.Installed, second.Manager.GetPlugin(BuiltinId)!.State);

                Assert.AreEqual(1, await second.Manager.RestoreEnabledAsync());
                Assert.AreEqual(PluginState.Enabled, second.Manager.GetPlugin(BuiltinId)!.State);
            }
        }
        finally
        {
            TryDelete(stateFile);
        }
    }

    [TestMethod]
    public async Task GetPlugins_EnabledFirstThenByName()
    {
        var context = CreateManager(builtins: new[]
        {
            new RecordingPlugin("com.tjc.b", "乙插件"),
            new RecordingPlugin("com.tjc.a", "甲插件")
        });
        try
        {
            await context.Manager.ScanAsync(Array.Empty<string>());
            await context.Manager.EnableAsync("com.tjc.b");

            var all = context.Manager.GetPlugins();

            Assert.AreEqual(2, all.Count);
            Assert.AreEqual("com.tjc.b", all[0].Id);
            Assert.AreEqual(PluginState.Enabled, all[0].State);
        }
        finally
        {
            context.Provider.Dispose();
            TryDelete(context.StateFile);
        }
    }

    [TestMethod]
    public async Task ChangedEvent_FiresOnEnableAndDisable()
    {
        var context = CreateManager(builtins: new[] { new RecordingPlugin(BuiltinId) });
        try
        {
            var kinds = new List<PluginChangedKind>();
            context.Manager.Changed += (_, args) => kinds.Add(args.Kind);

            await context.Manager.ScanAsync(Array.Empty<string>());
            await context.Manager.EnableAsync(BuiltinId);
            await context.Manager.DisableAsync(BuiltinId);

            CollectionAssert.AreEqual(
                new[] { PluginChangedKind.Enabled, PluginChangedKind.Disabled },
                kinds.ToArray());
        }
        finally
        {
            context.Provider.Dispose();
            TryDelete(context.StateFile);
        }
    }

    [TestMethod]
    public async Task ScanAsync_DiscoversDirectoryPluginByManifest()
    {
        using var root = PluginTestDirectory.Create();
        root.WriteFile(Path.Combine("com.tjc.dir", "plugin.json"), """
        { "id": "com.tjc.dir", "name": "目录插件", "version": "1.2.3", "entryAssembly": "Ghost.dll" }
        """);

        var context = CreateManager();
        try
        {
            var count = await context.Manager.ScanAsync(new[] { root.Path });

            Assert.AreEqual(1, count);
            var descriptor = context.Manager.GetPlugin("com.tjc.dir")!;
            Assert.IsFalse(descriptor.IsBuiltin);
            Assert.AreEqual("目录插件", descriptor.Name);
            Assert.AreEqual("1.2.3", descriptor.Version);
            Assert.AreEqual(PluginState.Installed, descriptor.State);
        }
        finally
        {
            context.Provider.Dispose();
            TryDelete(context.StateFile);
        }
    }

    [TestMethod]
    public async Task ScanAsync_IsolatesUnreadableManifests_AndEnablesValidationLate()
    {
        using var root = PluginTestDirectory.Create();
        root.WriteFile(Path.Combine("bad-no-id", "plugin.json"), """{ "name": "缺 id" }""");
        root.WriteFile(Path.Combine("bad-id-format", "plugin.json"),
            """{ "id": "9bad", "name": "id 非法", "version": "1.0.0", "entryAssembly": "X.dll" }""");
        root.WriteFile(Path.Combine("not-json", "plugin.json"), "{ broken");

        var context = CreateManager();
        try
        {
            // ScanAsync 只负责登记可读的清单：读不出来的（非 JSON）与没有 id 的被隔离跳过，
            // 不会让整个扫描失败。
            Assert.AreEqual(1, await context.Manager.ScanAsync(new[] { root.Path }));

            Assert.IsNull(context.Manager.GetPlugin("bad-no-id"));
            Assert.IsNull(context.Manager.GetPlugin("not-json"));

            // 清单格式违规（id 正则、入口程序集缺失）在启用时才校验并落 Error，
            // 好处是宿主管理器界面仍能列出这个插件并给出可诊断的错误。
            Assert.IsNotNull(context.Manager.GetPlugin("9bad"));

            Assert.IsFalse(await context.Manager.EnableAsync("9bad"));
            Assert.AreEqual(PluginState.Error, context.Manager.GetPlugin("9bad")!.State);
        }
        finally
        {
            context.Provider.Dispose();
            TryDelete(context.StateFile);
        }
    }

    [TestMethod]
    public async Task EnableAsync_PermissionOutOfWhitelist_MarksDisabled()
    {
        using var root = PluginTestDirectory.Create();
        root.WriteFile(Path.Combine("com.tjc.evil", "plugin.json"), """
        {
          "id": "com.tjc.evil",
          "name": "越界插件",
          "version": "1.0.0",
          "entryAssembly": "Evil.dll",
          "permissions": { "network": ["evil.example.com"] }
        }
        """);

        var context = CreateManager();
        try
        {
            await context.Manager.ScanAsync(new[] { root.Path });
            Assert.IsNotNull(context.Manager.GetPlugin("com.tjc.evil"));

            Assert.IsFalse(await context.Manager.EnableAsync("com.tjc.evil"));

            // PRD §3.7：权限声明越界属「宿主策略不允许启用」→ Disabled（而非 Error）
            var descriptor = context.Manager.GetPlugin("com.tjc.evil")!;
            Assert.AreEqual(PluginState.Disabled, descriptor.State);
            Assert.IsTrue(descriptor.LastError!.Contains("越界"), descriptor.LastError);
            CollectionAssert.Contains(descriptor.Permissions.Network.ToArray(), "evil.example.com");
        }
        finally
        {
            context.Provider.Dispose();
            TryDelete(context.StateFile);
        }
    }

    [TestMethod]
    public async Task EnableAsync_PermissionInsideWhitelist_ReachesActivation()
    {
        using var root = PluginTestDirectory.Create();
        // 权限声明合规，但入口程序集缺失 → 应走到激活阶段判 Error
        // （证明权限门禁只拦权限，不会把格式问题误判为 Disabled）
        root.WriteFile(Path.Combine("com.tjc.ok", "plugin.json"), """
        {
          "id": "com.tjc.ok",
          "name": "合规插件",
          "version": "1.0.0",
          "entryAssembly": "Ok.dll",
          "permissions": { "network": ["codeup.aliyun.com"], "credential": ["codeup:pat"] }
        }
        """);

        var context = CreateManager();
        try
        {
            await context.Manager.ScanAsync(new[] { root.Path });

            Assert.IsFalse(await context.Manager.EnableAsync("com.tjc.ok"));
            Assert.AreEqual(PluginState.Error, context.Manager.GetPlugin("com.tjc.ok")!.State);
        }
        finally
        {
            context.Provider.Dispose();
            TryDelete(context.StateFile);
        }
    }

    [TestMethod]
    public async Task EnableAsync_DirectoryPluginWithBadAssembly_MarksError()
    {
        using var root = PluginTestDirectory.Create();
        root.WriteFile(Path.Combine("com.tjc.broken", "plugin.json"), """
        { "id": "com.tjc.broken", "name": "坏程序集", "version": "1.0.0", "entryAssembly": "Broken.dll" }
        """);
        root.WriteFile(Path.Combine("com.tjc.broken", "lib", "Broken.dll"), "这不是一个程序集");

        var context = CreateManager();
        try
        {
            await context.Manager.ScanAsync(new[] { root.Path });

            // 加载失败必须被隔离为 Error，不能把异常抛给调用方
            Assert.IsFalse(await context.Manager.EnableAsync("com.tjc.broken"));

            var descriptor = context.Manager.GetPlugin("com.tjc.broken")!;
            Assert.AreEqual(PluginState.Error, descriptor.State);
            Assert.IsFalse(string.IsNullOrWhiteSpace(descriptor.LastError));

            // Error 态插件仍可被停用（走完卸载流程且无副作用）
            Assert.IsTrue(await context.Manager.DisableAsync("com.tjc.broken"));
        }
        finally
        {
            context.Provider.Dispose();
            TryDelete(context.StateFile);
        }
    }

    [TestMethod]
    public async Task UninstallAsync_DirectoryPlugin_RemovesEntryStateAndDirectory()
    {
        var stateFile = NewStateFile();
        using var root = PluginTestDirectory.Create();
        var pluginDirectory = root.CreateDirectory("com.tjc.uninst");
        root.WriteFile(Path.Combine("com.tjc.uninst", "plugin.json"), """
        { "id": "com.tjc.uninst", "name": "待卸载", "version": "1.0.0", "entryAssembly": "Gone.dll" }
        """);

        var context = CreateManager(stateFile);
        try
        {
            await context.Manager.ScanAsync(new[] { root.Path });
            Assert.IsNotNull(context.Manager.GetPlugin("com.tjc.uninst"));

            Assert.IsTrue(await context.Manager.UninstallAsync("com.tjc.uninst"));

            Assert.IsNull(context.Manager.GetPlugin("com.tjc.uninst"));
            Assert.IsFalse(Directory.Exists(pluginDirectory), "插件目录应被删除");
            Assert.IsNull(await new PluginStateRepository(stateFile).GetAsync("com.tjc.uninst"));
        }
        finally
        {
            context.Provider.Dispose();
            TryDelete(stateFile);
        }
    }

    [TestMethod]
    public async Task UninstallAsync_BuiltinPlugin_ReturnsFalse()
    {
        var context = CreateManager(builtins: new[] { new RecordingPlugin(BuiltinId) });
        try
        {
            await context.Manager.ScanAsync(Array.Empty<string>());

            // 内置插件随宿主发布，不允许卸载
            Assert.IsFalse(await context.Manager.UninstallAsync(BuiltinId));
            Assert.IsNotNull(context.Manager.GetPlugin(BuiltinId));
        }
        finally
        {
            context.Provider.Dispose();
            TryDelete(context.StateFile);
        }
    }

    [TestMethod]
    public async Task UpgradeAsync_ReplacesPackageForNotEnabledPlugin()
    {
        var stateFile = NewStateFile();
        using var installed = PluginTestDirectory.Create();
        using var source = PluginTestDirectory.Create();

        installed.WriteFile(Path.Combine("com.tjc.up", "plugin.json"), """
        { "id": "com.tjc.up", "name": "升级样例", "version": "1.0.0", "entryAssembly": "Ghost.dll" }
        """);
        installed.WriteFile(Path.Combine("com.tjc.up", "lib", "Ghost.dll"), "v1");

        source.WriteFile(Path.Combine("com.tjc.up", "plugin.json"), """
        { "id": "com.tjc.up", "name": "升级样例", "version": "2.0.0", "entryAssembly": "Ghost.dll" }
        """);
        source.WriteFile(Path.Combine("com.tjc.up", "lib", "Ghost.dll"), "v2");
        source.WriteFile(Path.Combine("com.tjc.up", "assets", "icon.png"), "icon");

        var context = CreateManager(stateFile);
        try
        {
            await context.Manager.ScanAsync(new[] { installed.Path });
            Assert.AreEqual("1.0.0", context.Manager.GetPlugin("com.tjc.up")!.Version);

            Assert.IsTrue(await context.Manager.UpgradeAsync(
                "com.tjc.up",
                Path.Combine(source.Path, "com.tjc.up")));

            var descriptor = context.Manager.GetPlugin("com.tjc.up")!;
            Assert.AreEqual("2.0.0", descriptor.Version);
            Assert.AreEqual(PluginState.Installed, descriptor.State);
            Assert.IsFalse(descriptor.RequiresRestart);

            // 旧包内容必须被完全替换（先清空再复制），不能残留 v1 的文件
            var target = Path.Combine(installed.Path, "com.tjc.up");
            Assert.AreEqual("v2", File.ReadAllText(Path.Combine(target, "lib", "Ghost.dll")));
            Assert.IsTrue(File.Exists(Path.Combine(target, "assets", "icon.png")));
        }
        finally
        {
            context.Provider.Dispose();
            TryDelete(stateFile);
        }
    }

    [TestMethod]
    public async Task UpgradeAsync_RejectsManifestIdMismatch()
    {
        var stateFile = NewStateFile();
        using var installed = PluginTestDirectory.Create();
        using var source = PluginTestDirectory.Create();

        installed.WriteFile(Path.Combine("com.tjc.up", "plugin.json"), """
        { "id": "com.tjc.up", "name": "升级样例", "version": "1.0.0", "entryAssembly": "Ghost.dll" }
        """);
        installed.WriteFile(Path.Combine("com.tjc.up", "lib", "Ghost.dll"), "v1");

        source.WriteFile(Path.Combine("other", "plugin.json"), """
        { "id": "com.tjc.other", "name": "另一个插件", "version": "2.0.0", "entryAssembly": "Ghost.dll" }
        """);
        source.WriteFile(Path.Combine("other", "lib", "Ghost.dll"), "v2");

        var context = CreateManager(stateFile);
        try
        {
            await context.Manager.ScanAsync(new[] { installed.Path });

            // 源目录清单 id 与目标插件不一致 → 拒绝（否则等于把 B 插件塞进 A 的目录）
            Assert.IsFalse(await context.Manager.UpgradeAsync(
                "com.tjc.up",
                Path.Combine(source.Path, "other")));

            Assert.AreEqual("1.0.0", context.Manager.GetPlugin("com.tjc.up")!.Version);
            Assert.AreEqual("v1", File.ReadAllText(
                Path.Combine(installed.Path, "com.tjc.up", "lib", "Ghost.dll")));
        }
        finally
        {
            context.Provider.Dispose();
            TryDelete(stateFile);
        }
    }

    [TestMethod]
    public async Task UpgradeAsync_BuiltinPlugin_ReturnsFalse()
    {
        var context = CreateManager(builtins: new[] { new RecordingPlugin(BuiltinId) });
        try
        {
            await context.Manager.ScanAsync(Array.Empty<string>());

            // 内置插件随宿主发布，热替换无意义
            Assert.IsFalse(await context.Manager.UpgradeAsync(BuiltinId, Path.GetTempPath()));
        }
        finally
        {
            context.Provider.Dispose();
            TryDelete(context.StateFile);
        }
    }

    /// <summary>
    /// 记账用宿主：只记录注册/注销请求，验证 manager 的编排顺序与清理完整性。
    /// </summary>
    private sealed class RecordingPluginHost : IPluginHost
    {
        public IServiceProvider Services { get; set; } = null!;

        public string PluginId => BuiltinId;

        public PluginPermissions Permissions => new();

        public List<string> Logs { get; } = new();

        public Dictionary<string, PluginSidebarEntry> SidebarEntries { get; } = new(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> Skills { get; } = new(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> McpServers { get; } = new(StringComparer.OrdinalIgnoreCase);

        public void Log(string message) => Logs.Add(message);

        public void LogError(string message, Exception? exception = null) => Logs.Add("[ERROR] " + message);

        public string? GetConfig(string key) => null;

        public void RegisterPanel(string entryId, Func<IPluginPanel> factory)
        {
        }

        public void UnregisterPanel(string entryId)
        {
        }

        public void RegisterSidebarEntry(PluginSidebarEntry entry) => SidebarEntries[entry.Id] = entry;

        public void UnregisterSidebarEntry(string entryId) => SidebarEntries.Remove(entryId);

        public void RegisterCommand(PluginCommandDescriptor command)
        {
        }

        public void RegisterSkill(SkillDefinition definition) => Skills.Add(definition.Id);

        public void UnregisterSkill(string skillId) => Skills.Remove(skillId);

        public void RegisterMcpServer(string name) => McpServers.Add(name);

        public void UnregisterMcpServer(string name) => McpServers.Remove(name);

        public void PostToUi(Action action)
        {
        }

        public IReadOnlyList<PluginSidebarEntry> GetSidebarEntries() => SidebarEntries.Values.ToArray();

        public IPluginPanel? GetPanel(string entryId) => null;
    }

    /// <summary>
    /// 记账用插件：模拟「清单声明之外的自行注册」（PRD §3.4 第 5 步）。
    /// </summary>
    private sealed class RecordingPlugin : IPlugin
    {
        public const string RuntimeSkillId = "runtime-skill";
        public const string ContributedSkillId = "contributed-skill";
        public const string RuntimeMcpName = "runtime-mcp";
        public const string SidebarEntryId = "runtime-entry";

        public RecordingPlugin(string id, string? name = null)
        {
            Id = id;
            Name = name ?? id;
        }

        public string Id { get; }

        public string Name { get; }

        public string Version => "1.0.0";

        public string Description => "记账用内置插件";

        public bool Initialized { get; private set; }

        public bool Shutdown { get; private set; }

        public Task InitializeAsync(IPluginHost host, CancellationToken cancellationToken = default)
        {
            Initialized = true;

            host.RegisterSidebarEntry(new PluginSidebarEntry { Id = SidebarEntryId, Label = "运行时入口" });
            host.RegisterSkill(new SkillDefinition { Id = RuntimeSkillId, Name = "运行时技能" });
            host.RegisterMcpServer(RuntimeMcpName);

            return Task.CompletedTask;
        }

        public Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            Shutdown = true;
            return Task.CompletedTask;
        }

        public IEnumerable<SkillDefinition> ContributeSkills()
        {
            yield return new SkillDefinition { Id = ContributedSkillId, Name = "声明技能" };
        }
    }
}