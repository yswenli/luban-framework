/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： EditToolUnitTest
*创建时间：2026/10/9
*描述：验证 EditFileAsync / MultiEditFileAsync 精确替换语义
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Tools;
using LuBan.AIAgent.Tools.FileSystem;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LuBan.XTestProject;

[TestClass]
public class EditToolUnitTest
{
    private static string _root = null!;
    private static FileSystemToolGroup _group = null!;

    [ClassInitialize]
    public static void Setup(TestContext context)
    {
        _root = Path.Combine(Path.GetTempPath(), "luban-edit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);

        var options = Options.Create(new LuBanAgentOptions
        {
            Tools = new ToolGroupOptions
            {
                FileSystem = new FileSystemToolOptions { Enabled = true, AllowedRoots = new List<string> { _root } }
            }
        });
        var ctx = new ToolConfirmationContext
        {
            Callback = (_, _) => Task.FromResult(ConfirmResult.Allow),
            WorkspacePathChecker = _ => true
        };
        _group = new FileSystemToolGroup(new ToolConfirmationService(ctx, options));
    }

    [ClassCleanup]
    public static void Cleanup()
    {
        try { if (Directory.Exists(_root)) Directory.Delete(_root, true); } catch { }
    }

    private string Write(string name, string content)
    {
        var p = Path.Combine(_root, name);
        File.WriteAllText(p, content);
        return p;
    }

    [TestMethod]
    public async Task EditFileAsync_SingleMatch_ReplacesAndWrites()
    {
        var p = Write("a.txt", "hello world");

        var result = await _group.EditFileAsync(p, "world", "luban");

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("hello luban", File.ReadAllText(p));
    }

    [TestMethod]
    public async Task EditFileAsync_NoMatch_ReturnsFail()
    {
        var p = Write("b.txt", "abc");
        var result = await _group.EditFileAsync(p, "zzz", "y");
        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Message!, "未找到匹配");
        Assert.AreEqual("abc", File.ReadAllText(p));
    }

    [TestMethod]
    public async Task EditFileAsync_MultipleMatches_WithoutReplaceAll_ReturnsFail()
    {
        var p = Write("c.txt", "x x x");
        var result = await _group.EditFileAsync(p, "x", "y");
        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Message!, "replace_all");
        Assert.AreEqual("x x x", File.ReadAllText(p));
    }

    [TestMethod]
    public async Task EditFileAsync_MultipleMatches_WithReplaceAll_ReplacesAll()
    {
        var p = Write("d.txt", "x x x");
        var result = await _group.EditFileAsync(p, "x", "y", replace_all: true);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("y y y", File.ReadAllText(p));
    }

    [TestMethod]
    public async Task EditFileAsync_EmptyOldString_ReturnsFail()
    {
        var p = Write("e.txt", "abc");
        var result = await _group.EditFileAsync(p, "", "y");
        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Message!, "old_string 不能为空");
    }

    [TestMethod]
    public async Task MultiEditFileAsync_AllValid_AppliesAtomically()
    {
        var p = Write("m1.txt", "aaa bbb");
        var edits = new List<EditOperation>
        {
            new() { OldString = "aaa", NewString = "111" },
            new() { OldString = "bbb", NewString = "222" }
        };

        var result = await _group.MultiEditFileAsync(p, edits);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("111 222", File.ReadAllText(p));
    }

    [TestMethod]
    public async Task MultiEditFileAsync_AnyInvalid_WritesNothing()
    {
        var p = Write("m2.txt", "aaa bbb");
        var edits = new List<EditOperation>
        {
            new() { OldString = "aaa", NewString = "111" },
            new() { OldString = "zzz", NewString = "999" }
        };

        var result = await _group.MultiEditFileAsync(p, edits);

        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Message!, "第 2 条");
        Assert.AreEqual("aaa bbb", File.ReadAllText(p));
    }

    [TestMethod]
    public async Task EditFileAsync_Success_ReturnsContext()
    {
        var p = Write("ctx.txt", "line1\nline2\nTARGET\nline4\nline5");

        var result = await _group.EditFileAsync(p, "TARGET", "REPLACED");

        Assert.IsTrue(result.IsSuccess);
        StringAssert.Contains(result.Data!, "> 3 | REPLACED");
        StringAssert.Contains(result.Data!, "line2");
        StringAssert.Contains(result.Data!, "line4");
    }

    [TestMethod]
    public async Task EditFileAsync_OldEqualsNew_ReturnsFail()
    {
        var p = Write("same.txt", "abc");
        var result = await _group.EditFileAsync(p, "abc", "abc");
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("abc", File.ReadAllText(p));
    }

    [TestMethod]
    public async Task EditFileAsync_PreservesUtf8Bom()
    {
        var p = Path.Combine(_root, "bom.txt");
        await File.WriteAllBytesAsync(p, new byte[] { 0xEF, 0xBB, 0xBF }
            .Concat(System.Text.Encoding.UTF8.GetBytes("hello world")).ToArray());

        var result = await _group.EditFileAsync(p, "world", "luban");

        Assert.IsTrue(result.IsSuccess);
        var bytes = await File.ReadAllBytesAsync(p);
        CollectionAssert.AreEqual(new byte[] { 0xEF, 0xBB, 0xBF }, bytes[..3]);
        Assert.AreEqual("hello luban", File.ReadAllText(p));
    }

    /// <summary>
    /// 回归：MultiEdit 的对象数组参数必须能经真实 AIFunctionFactory 绑定路径（含生产环境
    /// BuildTools 施加的 RequiredArgumentGuardAIFunction 包装）完成 JsonElement 数组 →
    /// IReadOnlyList&lt;EditOperation&gt; 的转换并落盘。
    /// </summary>
    [TestMethod]
    public async Task MultiEditFileAsync_ViaAIFunctionFactory_BindsObjectArray()
    {
        var p = Write("bind.txt", "aaa bbb ccc");
        var tool = BuildProductionMultiEditFunction();

        // 模拟模型经 JSON 传入的对象数组（与运行时一致，为 JsonElement 数组而非 CLR 对象）。
        var edits = JsonDocument.Parse(
            """[{"oldString":"aaa","newString":"111"},{"oldString":"ccc","newString":"333"}]""").RootElement.Clone();

        var result = await tool.InvokeAsync(new AIFunctionArguments(new Dictionary<string, object?>
        {
            ["path"] = p,
            ["edits"] = edits
        }), CancellationToken.None);

        var json = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(result));
        Assert.IsTrue(json.TryGetProperty("isSuccess", out var s) && s.ValueKind == JsonValueKind.True,
            json.ToString());
        Assert.AreEqual("111 bbb 333", File.ReadAllText(p));
    }

    /// <summary>
    /// 复刻生产路径：FileSystemToolPlugin.GetTools（内部 AIFunctionFactoryHelper → AIFunctionFactory）
    /// 产出 MultiEdit 工具，再套 LuBanAgentFactory.BuildTools 施加的 RequiredArgumentGuardAIFunction。
    /// </summary>
    private static AIFunction BuildProductionMultiEditFunction()
    {
        var options = Options.Create(new LuBanAgentOptions
        {
            Tools = new ToolGroupOptions
            {
                FileSystem = new FileSystemToolOptions { Enabled = true, AllowedRoots = new List<string> { _root } }
            }
        });
        var ctx = new ToolConfirmationContext
        {
            Callback = (_, _) => Task.FromResult(ConfirmResult.Allow),
            WorkspacePathChecker = _ => true
        };
        var provider = new StubServiceProvider(new Dictionary<Type, object>
        {
            [typeof(IToolConfirmationService)] = new ToolConfirmationService(ctx, options),
            [typeof(IOptions<LuBanAgentOptions>)] = options
        });

        var plugin = new FileSystemToolPlugin(options);
        // 生产工具名由 AIFunctionFactory 生成：剥离方法名的 "Async" 后缀（MultiEditFileAsync → MultiEditFile）。
        var inner = plugin.GetTools(provider).Single(t => t.Name == "MultiEditFile");
        return new RequiredArgumentGuardAIFunction(inner);
    }

    private sealed class StubServiceProvider(Dictionary<Type, object> map) : IServiceProvider
    {
        public object? GetService(Type serviceType)
            => map.TryGetValue(serviceType, out var value) ? value : null;
    }
}
