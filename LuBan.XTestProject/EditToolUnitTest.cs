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
using LuBan.AIAgent.Tools.FileSystem;
using Microsoft.Extensions.Options;

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
}