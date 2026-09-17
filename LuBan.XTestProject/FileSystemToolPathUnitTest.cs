/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： FileSystemToolPathUnitTest
*唯一标识：文件类工具目录路径检测单测
*创建时间：2026/9/17
*描述：验证文件类工具在收到「目录」而非文件时返回可操作提示，避免「未找到文件」误导模型
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Infrastructure;
using LuBan.AIAgent.MCP.BuiltIn;
using LuBan.AIAgent.Tools.FileSystem;

using Microsoft.Extensions.Options;

namespace LuBan.XTestProject;

[TestClass]
public class FileSystemToolPathUnitTest
{
    private static string _root = null!;
    private static FileSystemToolGroup _group = null!;

    [ClassInitialize]
    public static void Setup(TestContext context)
    {
        _root = Path.Combine(Path.GetTempPath(), "luban-fs-path-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "AGENTS.md"), "# AGENTS.md");

        var options = Options.Create(new LuBanAgentOptions
        {
            Tools = new ToolGroupOptions
            {
                FileSystem = new FileSystemToolOptions
                {
                    Enabled = true,
                    AllowedRoots = new List<string> { _root }
                }
            }
        });

        var confirmationContext = new ToolConfirmationContext
        {
            Callback = (_, _) => Task.FromResult(true),
            WorkspacePathChecker = _ => true
        };

        _group = new FileSystemToolGroup(
            new PathGuard(options),
            new ToolConfirmationService(confirmationContext, options));
    }

    [ClassCleanup]
    public static void Cleanup()
    {
        try { Directory.Delete(_root, true); } catch { }
    }

    [TestMethod]
    public async Task ReadFileAsync_DirectoryPath_ReturnsActionableMessage()
    {
        var result = await _group.ReadFileAsync(_root);

        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.Message);
        StringAssert.Contains(result.Message, "路径是目录");
        StringAssert.Contains(result.Message, "ListDirectory");
        Assert.IsFalse(result.Message.Contains("未找到文件"), "不应再报「未找到文件」误导模型");
    }

    [TestMethod]
    public async Task ReadFileAsync_RealFile_StillWorks()
    {
        var result = await _group.ReadFileAsync(Path.Combine(_root, "AGENTS.md"));

        Assert.IsTrue(result.IsSuccess);
        StringAssert.Contains(result.Data, "AGENTS.md");
    }

    [TestMethod]
    public async Task WriteFileAsync_DirectoryPath_ReturnsActionableMessage()
    {
        var result = await _group.WriteFileAsync(_root, "x");

        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.Message);
        StringAssert.Contains(result.Message, "路径是目录");
    }

    [TestMethod]
    public async Task DeleteFileAsync_DirectoryPath_ReturnsActionableMessage()
    {
        var result = await _group.DeleteFileAsync(_root);

        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.Message);
        StringAssert.Contains(result.Message, "路径是目录");
        StringAssert.Contains(result.Message, "DeleteDirectory");
    }

    [TestMethod]
    public async Task CopyFileAsync_DirectorySource_ReturnsActionableMessage()
    {
        var result = await _group.CopyFileAsync(_root, Path.Combine(_root, "copy.md"));

        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.Message);
        StringAssert.Contains(result.Message, "路径是目录");
    }

    [TestMethod]
    public async Task MoveFileAsync_DirectorySource_ReturnsActionableMessage()
    {
        var result = await _group.MoveFileAsync(_root, Path.Combine(_root, "moved.md"));

        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.Message);
        StringAssert.Contains(result.Message, "路径是目录");
    }

    [TestMethod]
    public async Task McpReadFile_DirectoryPath_ReturnsActionableMessage()
    {
        var client = new FileSystemMCPClient();

        var result = await client.CallToolAsync("read_file", new Dictionary<string, object?> { ["path"] = _root });

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        StringAssert.Contains(result.Error, "路径是目录");
    }

    [TestMethod]
    public async Task McpWriteFile_DirectoryPath_ReturnsActionableMessage()
    {
        var client = new FileSystemMCPClient();

        var result = await client.CallToolAsync("write_file", new Dictionary<string, object?>
        {
            ["path"] = _root,
            ["content"] = "x"
        });

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        StringAssert.Contains(result.Error, "路径是目录");
    }
}