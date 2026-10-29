/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： ManageToolConfirmationUnitTest
*唯一标识：通用 Skill/MCP 管理工具确认链路单测
*创建时间：2026/10/29
*描述：验证 Skill/MCP 写删接入 IToolConfirmationService（Default 弹确认、拒绝不落盘、
*      Plan 记计划）、workspaceRoot 可选兜底、工具描述与必填参数集合。
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.MCP;
using LuBan.AIAgent.Plugins.Security;
using LuBan.AIAgent.Skills;
using LuBan.AIAgent.Tools.McpManage;
using LuBan.AIAgent.Tools.SkillManage;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class ManageToolConfirmationUnitTest
{
    private sealed class StubCredentialStore : ICredentialStore
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

        public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
            => Task.FromResult(_values.TryGetValue(key, out var value) ? value : null);

        public Task SetAsync(string key, string? value, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(value)) _values.Remove(key);
            else _values[key] = value;
            return Task.CompletedTask;
        }

        public bool Contains(string key) => _values.ContainsKey(key);

        public IReadOnlyCollection<string> Keys => _values.Keys.ToList();
    }

    /// <summary>记录确认回调收到的工具名与参数，并返回预设结论。</summary>
    private sealed class RecordingConfirmation
    {
        public List<(string ToolName, IReadOnlyDictionary<string, object?> Arguments)> Calls { get; } = new();

        public ToolPermissionMode Mode { get; init; } = ToolPermissionMode.Default;

        public ConfirmResult Result { get; init; } = ConfirmResult.Allow;

        public IToolConfirmationService Build()
        {
            var context = new ToolConfirmationContext
            {
                Mode = Mode,
                Callback = (toolName, arguments) =>
                {
                    Calls.Add((toolName, arguments));
                    return Task.FromResult(Result);
                }
            };

            return new ToolConfirmationService(context, Options.Create(new LuBanAgentOptions()));
        }
    }

    private static string CreateWorkspace(string prefix)
    {
        var workspace = Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspace);
        return workspace;
    }

    private static SkillManageToolGroup CreateSkillGroup(
        IToolConfirmationService? confirmation, Func<string?>? defaultWorkspaceRoot = null)
        => new(
            new SkillRegistry(Array.Empty<ISkill>()),
            new SkillManageToolOptions(),
            null,
            confirmation,
            defaultWorkspaceRoot);

    private static McpManageToolGroup CreateMcpGroup(
        IToolConfirmationService? confirmation, Func<string?>? defaultWorkspaceRoot = null)
    {
        var registry = new MCPRegistry(
            Array.Empty<IMCPClient>(),
            null,
            new CredentialValueResolver(new StubCredentialStore()));

        return new McpManageToolGroup(
            registry,
            new McpManageToolOptions(),
            new McpOptions(),
            null,
            new CredentialValueResolver(new StubCredentialStore()),
            confirmation,
            defaultWorkspaceRoot);
    }

    // ────────────────────────── Skill ──────────────────────────

    [TestMethod]
    public async Task WriteSkillAsync_Default模式应确认且允许后落盘()
    {
        var confirmation = new RecordingConfirmation();
        var workspace = CreateWorkspace("luban-skillmanage-");
        try
        {
            var group = CreateSkillGroup(confirmation.Build());

            var result = await group.WriteSkillAsync("demo", "Demo", "示例", "正文", workspaceRoot: workspace);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.AreEqual(1, confirmation.Calls.Count, "Default 模式下必须请求用户确认");
            Assert.AreEqual("WriteSkillAsync", confirmation.Calls[0].ToolName, "确认名须与 AlwaysConfirmTools 约定（C# 方法名）一致");

            var file = Path.Combine(workspace, ".luban-agent", "skills", "demo", "SKILL.md");
            Assert.IsTrue(File.Exists(file), "允许后应落盘");
        }
        finally
        {
            TryDelete(workspace);
        }
    }

    [TestMethod]
    public async Task WriteSkillAsync_默认根缺失时用宿主工作区且不要求传workspaceRoot()
    {
        var confirmation = new RecordingConfirmation();
        var workspace = CreateWorkspace("luban-skillmanage-");
        try
        {
            // 不传 workspaceRoot：由宿主提供者兜底
            var group = CreateSkillGroup(confirmation.Build(), () => workspace);

            var result = await group.WriteSkillAsync("demo", "Demo", "示例", "正文");

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.IsTrue(File.Exists(Path.Combine(workspace, ".luban-agent", "skills", "demo", "SKILL.md")));
        }
        finally
        {
            TryDelete(workspace);
        }
    }

    [TestMethod]
    public async Task WriteSkillAsync_用户拒绝时不落盘()
    {
        var confirmation = new RecordingConfirmation { Result = ConfirmResult.Deny };
        var workspace = CreateWorkspace("luban-skillmanage-");
        try
        {
            var group = CreateSkillGroup(confirmation.Build());

            var result = await group.WriteSkillAsync("demo", "Demo", "示例", "正文", workspaceRoot: workspace);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.UserCancelled, "拒绝应标记为用户取消");
            Assert.IsFalse(File.Exists(Path.Combine(workspace, ".luban-agent", "skills", "demo", "SKILL.md")));
        }
        finally
        {
            TryDelete(workspace);
        }
    }

    [TestMethod]
    public async Task WriteSkillAsync_Plan模式记计划不落盘()
    {
        var confirmation = new RecordingConfirmation { Mode = ToolPermissionMode.Plan };
        var workspace = CreateWorkspace("luban-skillmanage-");
        try
        {
            var group = CreateSkillGroup(confirmation.Build());

            var result = await group.WriteSkillAsync("demo", "Demo", "示例", "正文", workspaceRoot: workspace);

            Assert.IsTrue(result.Planned, "Plan 模式应记为计划项");
            Assert.IsFalse(File.Exists(Path.Combine(workspace, ".luban-agent", "skills", "demo", "SKILL.md")));
            Assert.AreEqual(0, confirmation.Calls.Count, "Plan 模式不弹确认回调");
        }
        finally
        {
            TryDelete(workspace);
        }
    }

    [TestMethod]
    public async Task DeleteSkillAsync_Default模式应确认且允许后删除()
    {
        var confirmation = new RecordingConfirmation();
        var workspace = CreateWorkspace("luban-skillmanage-");
        try
        {
            var skillDir = Path.Combine(workspace, ".luban-agent", "skills", "demo");
            Directory.CreateDirectory(skillDir);
            await File.WriteAllTextAsync(Path.Combine(skillDir, "SKILL.md"), "---\nname: demo\ndescription: d\n---\n\nbody");

            var group = CreateSkillGroup(confirmation.Build());
            var result = await group.DeleteSkillAsync("demo", workspace);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.AreEqual("DeleteSkillAsync", confirmation.Calls.Single().ToolName);
            Assert.IsFalse(Directory.Exists(skillDir));
        }
        finally
        {
            TryDelete(workspace);
        }
    }

    [TestMethod]
    public async Task DeleteSkillAsync_用户拒绝时保留目录()
    {
        var confirmation = new RecordingConfirmation { Result = ConfirmResult.Deny };
        var workspace = CreateWorkspace("luban-skillmanage-");
        try
        {
            var skillDir = Path.Combine(workspace, ".luban-agent", "skills", "demo");
            Directory.CreateDirectory(skillDir);
            await File.WriteAllTextAsync(Path.Combine(skillDir, "SKILL.md"), "---\nname: demo\ndescription: d\n---\n\nbody");

            var group = CreateSkillGroup(confirmation.Build());
            var result = await group.DeleteSkillAsync("demo", workspace);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(Directory.Exists(skillDir), "拒绝后不得删除");
        }
        finally
        {
            TryDelete(workspace);
        }
    }

    // ────────────────────────── MCP ──────────────────────────

    [TestMethod]
    public async Task WriteMcpServerAsync_Default模式应确认且允许后落盘()
    {
        var confirmation = new RecordingConfirmation();
        var workspace = CreateWorkspace("luban-mcpmanage-");
        try
        {
            var group = CreateMcpGroup(confirmation.Build());

            var result = await group.WriteMcpServerAsync(
                name: "demo", command: "npx", transport: "stdio", workspaceRoot: workspace);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.AreEqual("WriteMcpServerAsync", confirmation.Calls.Single().ToolName);
            Assert.IsTrue(File.Exists(Path.Combine(workspace, ".luban-agent", "mcps", "demo.json")));
        }
        finally
        {
            TryDelete(workspace);
        }
    }

    [TestMethod]
    public async Task WriteMcpServerAsync_省略workspaceRoot时用宿主工作区()
    {
        var confirmation = new RecordingConfirmation();
        var workspace = CreateWorkspace("luban-mcpmanage-");
        try
        {
            var group = CreateMcpGroup(confirmation.Build(), () => workspace);

            var result = await group.WriteMcpServerAsync(name: "demo", command: "npx");

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.IsTrue(File.Exists(Path.Combine(workspace, ".luban-agent", "mcps", "demo.json")));
        }
        finally
        {
            TryDelete(workspace);
        }
    }

    [TestMethod]
    public async Task DeleteMcpServerAsync_用户拒绝时保留配置()
    {
        var confirmation = new RecordingConfirmation { Result = ConfirmResult.Deny };
        var workspace = CreateWorkspace("luban-mcpmanage-");
        try
        {
            var writer = CreateMcpGroup(null);
            var written = await writer.WriteMcpServerAsync(
                name: "demo", command: "npx", transport: "stdio", workspaceRoot: workspace);
            Assert.IsTrue(written.IsSuccess, written.Message);

            var deleter = CreateMcpGroup(confirmation.Build());
            var result = await deleter.DeleteMcpServerAsync("demo", workspace);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("DeleteMcpServerAsync", confirmation.Calls.Single().ToolName);
            Assert.IsTrue(File.Exists(Path.Combine(workspace, ".luban-agent", "mcps", "demo.json")), "拒绝后不得删除");
        }
        finally
        {
            TryDelete(workspace);
        }
    }

    // ─────────────────── 可发现性 / 参数契约 ───────────────────

    [TestMethod]
    public void Skill与Mcp写删工具应有描述且必需参数不含宿主注入项()
    {
        AssertRequiredParameters(typeof(SkillManageToolGroup), "WriteSkillAsync",
            new[] { "id", "name", "description", "content" },
            CreateSkillGroup(null));
        AssertRequiredParameters(typeof(SkillManageToolGroup), "DeleteSkillAsync",
            new[] { "id" },
            CreateSkillGroup(null));
        AssertRequiredParameters(typeof(McpManageToolGroup), "WriteMcpServerAsync",
            new[] { "name" },
            CreateMcpGroup(null));
        AssertRequiredParameters(typeof(McpManageToolGroup), "DeleteMcpServerAsync",
            new[] { "name" },
            CreateMcpGroup(null));
        AssertRequiredParameters(typeof(McpManageToolGroup), "TestMcpServerAsync",
            Array.Empty<string>(),
            CreateMcpGroup(null));
    }

    private static void AssertRequiredParameters(Type type, string methodName, string[] expectedRequired, object instance)
    {
        var method = type.GetMethod(methodName)
            ?? throw new AssertFailedException($"{type.Name}.{methodName} 不存在");

        var function = AIFunctionFactory.Create(method, instance);

        Assert.IsFalse(string.IsNullOrWhiteSpace(function.Description),
            $"{methodName} 缺少 [Description]，模型无法从工具描述理解用途");

        var required = function.JsonSchema.TryGetProperty("required", out var requiredNode)
            ? requiredNode.EnumerateArray().Select(static e => e.GetString()).Where(static s => s is not null).ToList()
            : new List<string?>();

        CollectionAssert.AreEquivalent(
            expectedRequired.ToList(),
            required!,
            $"{methodName} 的必需参数集合不符（workspaceRoot/overwrite 等宿主可兜底项不应必填）");
    }

    private static void TryDelete(string directory)
    {
        try { Directory.Delete(directory, recursive: true); } catch { }
    }
}