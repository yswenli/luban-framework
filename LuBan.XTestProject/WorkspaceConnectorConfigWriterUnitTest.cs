/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： WorkspaceConnectorConfigWriterUnitTest
*创建时间：2026/10/20
*描述：默认连接器配置写入器单测 —— 工作区 <name>.json 的落盘/读取一致性、
*      文件名防穿越、明文凭据门禁、覆盖语义与删除。
*
*****************************************************************************/
using LuBan.AIAgent.Connectors;

namespace LuBan.XTestProject;

[TestClass]
public class WorkspaceConnectorConfigWriterUnitTest
{
    private static string NewWorkspace()
    {
        var workspace = Path.Combine(Path.GetTempPath(), "luban-connector-writer-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspace);
        return workspace;
    }

    private static void Cleanup(string workspace)
    {
        if (Directory.Exists(workspace)) Directory.Delete(workspace, recursive: true);
    }

    [TestMethod]
    public async Task WriteAsync_RoundTripsDeclaration()
    {
        var workspace = NewWorkspace();
        try
        {
            var writer = new WorkspaceConnectorConfigWriter();
            var entry = new ConnectorWorkspaceEntry
            {
                Name = "generated",
                Provider = "demo",
                Settings = new Dictionary<string, string?> { ["endpoint"] = "https://example.test" },
                Enabled = true
            };

            var result = await writer.WriteAsync(entry, overwrite: true, workspace);

            Assert.IsTrue(result.IsSuccess, result.Message);

            var file = Path.Combine(workspace, ".luban-agent", "connectors", "generated.json");
            Assert.AreEqual(file, result.Data);
            Assert.IsTrue(File.Exists(file));

            var json = await File.ReadAllTextAsync(file);
            StringAssert.Contains(json, "\"provider\": \"demo\"");
            StringAssert.Contains(json, "https://example.test");
            StringAssert.Contains(json, "\"enabled\": true");
        }
        finally
        {
            Cleanup(workspace);
        }
    }

    [TestMethod]
    public async Task WriteAsync_RejectsPathTraversalName()
    {
        var workspace = NewWorkspace();
        try
        {
            var writer = new WorkspaceConnectorConfigWriter();
            var result = await writer.WriteAsync(
                new ConnectorWorkspaceEntry { Name = "../evil", Provider = "demo" },
                overwrite: true,
                workspace);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsFalse(File.Exists(Path.Combine(workspace, "evil.json")));
        }
        finally
        {
            Cleanup(workspace);
        }
    }

    [TestMethod]
    public async Task WriteAsync_RejectsPlaintextSecret()
    {
        var workspace = NewWorkspace();
        try
        {
            var writer = new WorkspaceConnectorConfigWriter();
            var result = await writer.WriteAsync(
                new ConnectorWorkspaceEntry
                {
                    Name = "generated",
                    Provider = "demo",
                    Settings = new Dictionary<string, string?> { ["apiKey"] = "sk-literal" }
                },
                overwrite: true,
                workspace);

            Assert.IsFalse(result.IsSuccess);
            StringAssert.Contains(result.Message!, "凭据引用");
        }
        finally
        {
            Cleanup(workspace);
        }
    }

    [TestMethod]
    public async Task WriteAsync_RespectsOverwriteFlag()
    {
        var workspace = NewWorkspace();
        try
        {
            var writer = new WorkspaceConnectorConfigWriter();
            var entry = new ConnectorWorkspaceEntry { Name = "generated", Provider = "demo", Enabled = true };

            Assert.IsTrue((await writer.WriteAsync(entry, overwrite: true, workspace)).IsSuccess);

            var blocked = await writer.WriteAsync(entry, overwrite: false, workspace);
            Assert.IsFalse(blocked.IsSuccess);
            StringAssert.Contains(blocked.Message!, "overwrite");
        }
        finally
        {
            Cleanup(workspace);
        }
    }

    [TestMethod]
    public async Task DeleteAsync_RemovesFile()
    {
        var workspace = NewWorkspace();
        try
        {
            var writer = new WorkspaceConnectorConfigWriter();
            await writer.WriteAsync(new ConnectorWorkspaceEntry { Name = "generated", Provider = "demo" }, true, workspace);

            var result = await writer.DeleteAsync("generated", workspace);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.IsFalse(File.Exists(Path.Combine(workspace, ".luban-agent", "connectors", "generated.json")));

            var missing = await writer.DeleteAsync("generated", workspace);
            Assert.IsFalse(missing.IsSuccess);
        }
        finally
        {
            Cleanup(workspace);
        }
    }
}