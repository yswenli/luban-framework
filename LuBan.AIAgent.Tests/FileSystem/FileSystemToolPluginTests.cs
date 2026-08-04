using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Infrastructure;
using LuBan.AIAgent.Services;
using LuBan.AIAgent.Tools.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tests.FileSystem;

[TestClass]
public class FileSystemToolPluginTests
{
    private static FileSystemToolGroup CreateToolGroup(List<string>? allowedRoots = null)
    {
        var options = new LuBanAgentOptions
        {
            Tools = new ToolGroupOptions
            {
                FileSystem = new FileSystemToolOptions
                {
                    Enabled = true,
                    AllowedRoots = allowedRoots ?? new List<string>()
                }
            }
        };
        var pathGuard = new PathGuard(Options.Create(options));
        ToolConfirmationService.WorkspacePathChecker = path => true;
        return new FileSystemToolGroup(pathGuard);
    }

    [TestMethod]
    public async Task SearchFilesAsync_GlobPattern_MatchesFiles()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"search-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        try
        {
            File.WriteAllText(Path.Combine(testDir, "a.cs"), "");
            File.WriteAllText(Path.Combine(testDir, "b.cs"), "");
            File.WriteAllText(Path.Combine(testDir, "c.txt"), "");

            var toolGroup = CreateToolGroup(new List<string> { testDir });
            ToolResult<string> result = await toolGroup.SearchFilesAsync(testDir, "*.cs");

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data!.Contains("a.cs"));
            Assert.IsTrue(result.Data!.Contains("b.cs"));
            Assert.IsFalse(result.Data!.Contains("c.txt"));
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [TestMethod]
    public async Task SearchFilesAsync_RecursiveGlob_MatchesNestedFiles()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"search-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(Path.Combine(testDir, "sub1", "deep"));
        try
        {
            File.WriteAllText(Path.Combine(testDir, "a.cs"), "");
            File.WriteAllText(Path.Combine(testDir, "sub1", "b.cs"), "");
            File.WriteAllText(Path.Combine(testDir, "sub1", "deep", "c.cs"), "");

            var toolGroup = CreateToolGroup(new List<string> { testDir });
            ToolResult<string> result = await toolGroup.SearchFilesAsync(testDir, "**/*.cs");

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data!.Contains("a.cs"));
            Assert.IsTrue(result.Data!.Contains("b.cs"));
            Assert.IsTrue(result.Data!.Contains("c.cs"));
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [TestMethod]
    public async Task SearchFilesAsync_BinaryFiles_Skipped()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"search-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        try
        {
            File.WriteAllText(Path.Combine(testDir, "a.cs"), "");
            File.WriteAllBytes(Path.Combine(testDir, "b.dll"), new byte[] { 1, 2, 3 });
            File.WriteAllBytes(Path.Combine(testDir, "c.png"), new byte[] { 1, 2, 3 });

            var toolGroup = CreateToolGroup(new List<string> { testDir });
            ToolResult<string> result = await toolGroup.SearchFilesAsync(testDir, "*");

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data!.Contains("a.cs"));
            Assert.IsFalse(result.Data!.Contains("b.dll"));
            Assert.IsFalse(result.Data!.Contains("c.png"));
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [TestMethod]
    public async Task SearchFilesAsync_MaxResults_Truncates()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"search-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        try
        {
            for (int i = 0; i < 10; i++)
                File.WriteAllText(Path.Combine(testDir, $"file{i}.cs"), "");

            var toolGroup = CreateToolGroup(new List<string> { testDir });
            ToolResult<string> result = await toolGroup.SearchFilesAsync(testDir, "*.cs", maxResults: 3);

            Assert.IsTrue(result.IsSuccess);
            var lines = result.Data!.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(lines.Length <= 4);
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [TestMethod]
    public async Task SearchFilesAsync_PathNotAllowed_ReturnsError()
    {
        var toolGroup = CreateToolGroup(new List<string> { @"C:\Allowed" });
        ToolResult<string> result = await toolGroup.SearchFilesAsync(@"C:\NotAllowed", "*.cs");

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("错误"));
        Assert.IsTrue(result.Message!.Contains("不在允许访问的范围内"));
    }

    [TestMethod]
    public async Task GrepAsync_RegexPattern_MatchesContent()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"grep-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        try
        {
            File.WriteAllText(Path.Combine(testDir, "a.cs"), "public class FooBar\npublic class Baz");
            File.WriteAllText(Path.Combine(testDir, "b.cs"), "no match here");

            var toolGroup = CreateToolGroup(new List<string> { testDir });
            ToolResult<string> result = await toolGroup.GrepAsync(testDir, "class\\s+\\w+");

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data!.Contains("FooBar"));
            Assert.IsTrue(result.Data!.Contains("Baz"));
            Assert.IsFalse(result.Data!.Contains("no match"));
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [TestMethod]
    public async Task GrepAsync_FilePattern_FiltersFiles()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"grep-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        try
        {
            File.WriteAllText(Path.Combine(testDir, "a.cs"), "TODO: fix this");
            File.WriteAllText(Path.Combine(testDir, "b.txt"), "TODO: fix this too");

            var toolGroup = CreateToolGroup(new List<string> { testDir });
            ToolResult<string> result = await toolGroup.GrepAsync(testDir, "TODO", filePattern: "*.cs");

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data!.Contains("a.cs"));
            Assert.IsFalse(result.Data!.Contains("b.txt"));
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [TestMethod]
    public async Task GrepAsync_MaxResults_Truncates()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"grep-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        try
        {
            for (int i = 0; i < 10; i++)
                File.WriteAllText(Path.Combine(testDir, $"file{i}.cs"), "match line 1\nmatch line 2");

            var toolGroup = CreateToolGroup(new List<string> { testDir });
            ToolResult<string> result = await toolGroup.GrepAsync(testDir, "match", maxResults: 5);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data!.Contains("已截断"));
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [TestMethod]
    public async Task GrepAsync_PathNotAllowed_ReturnsError()
    {
        var toolGroup = CreateToolGroup(new List<string> { @"C:\Allowed" });
        ToolResult<string> result = await toolGroup.GrepAsync(@"C:\NotAllowed", "pattern");

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("错误"));
        Assert.IsTrue(result.Message!.Contains("不在允许访问的范围内"));
    }

    [TestMethod]
    public async Task CreateDirectoryAsync_NewDirectory_CreatesSuccessfully()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"create-dir-test-{Guid.NewGuid()}");
        try
        {
            var toolGroup = CreateToolGroup(new List<string> { Path.GetTempPath() });
            ToolResult<string> result = await toolGroup.CreateDirectoryAsync(Path.Combine(testDir, "sub", "deep"));

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data!.Contains("已创建目录"));
            Assert.IsTrue(Directory.Exists(Path.Combine(testDir, "sub", "deep")));
        }
        finally
        {
            if (Directory.Exists(testDir))
                Directory.Delete(testDir, true);
        }
    }

    [TestMethod]
    public async Task CreateDirectoryAsync_ExistingDirectory_ReturnsAlreadyExists()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"create-dir-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        try
        {
            var toolGroup = CreateToolGroup(new List<string> { testDir });
            ToolResult<string> result = await toolGroup.CreateDirectoryAsync(testDir);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data!.Contains("目录已存在"));
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [TestMethod]
    public async Task CreateDirectoryAsync_PathNotAllowed_ReturnsError()
    {
        var toolGroup = CreateToolGroup(new List<string> { @"C:\Allowed" });
        ToolResult<string> result = await toolGroup.CreateDirectoryAsync(@"C:\NotAllowed\dir");

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("错误"));
        Assert.IsTrue(result.Message!.Contains("不在允许访问的范围内"));
    }

    [TestMethod]
    public async Task CopyFileAsync_ValidPaths_CopiesSuccessfully()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"copy-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        try
        {
            var source = Path.Combine(testDir, "source.txt");
            var dest = Path.Combine(testDir, "dest.txt");
            File.WriteAllText(source, "test content");

            var toolGroup = CreateToolGroup(new List<string> { testDir });
            ToolResult<string> result = await toolGroup.CopyFileAsync(source, dest);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data!.Contains("已复制文件"));
            Assert.IsTrue(File.Exists(dest));
            Assert.AreEqual("test content", File.ReadAllText(dest));
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [TestMethod]
    public async Task CopyFileAsync_SourceNotExists_ReturnsError()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"copy-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        try
        {
            var source = Path.Combine(testDir, "notexist.txt");
            var dest = Path.Combine(testDir, "dest.txt");

            var toolGroup = CreateToolGroup(new List<string> { testDir });
            ToolResult<string> result = await toolGroup.CopyFileAsync(source, dest);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message!.Contains("错误"));
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [TestMethod]
    public async Task CopyFileAsync_PathNotAllowed_ReturnsError()
    {
        var toolGroup = CreateToolGroup(new List<string> { @"C:\Allowed" });
        ToolResult<string> result = await toolGroup.CopyFileAsync(@"C:\NotAllowed\source.txt", @"C:\Allowed\dest.txt");

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("错误"));
        Assert.IsTrue(result.Message!.Contains("不在允许访问的范围内"));
    }

    [TestMethod]
    public async Task MoveFileAsync_ValidPaths_MovesSuccessfully()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"move-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        try
        {
            var source = Path.Combine(testDir, "source.txt");
            var dest = Path.Combine(testDir, "dest.txt");
            File.WriteAllText(source, "test content");

            var toolGroup = CreateToolGroup(new List<string> { testDir });
            ToolResult<string> result = await toolGroup.MoveFileAsync(source, dest);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data!.Contains("已移动文件"));
            Assert.IsFalse(File.Exists(source));
            Assert.IsTrue(File.Exists(dest));
            Assert.AreEqual("test content", File.ReadAllText(dest));
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [TestMethod]
    public async Task MoveFileAsync_DestExists_ReturnsError()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"move-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        try
        {
            var source = Path.Combine(testDir, "source.txt");
            var dest = Path.Combine(testDir, "dest.txt");
            File.WriteAllText(source, "source content");
            File.WriteAllText(dest, "dest content");

            var toolGroup = CreateToolGroup(new List<string> { testDir });
            ToolResult<string> result = await toolGroup.MoveFileAsync(source, dest);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message!.Contains("错误"));
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [TestMethod]
    public async Task MoveFileAsync_PathNotAllowed_ReturnsError()
    {
        var toolGroup = CreateToolGroup(new List<string> { @"C:\Allowed" });
        ToolResult<string> result = await toolGroup.MoveFileAsync(@"C:\NotAllowed\source.txt", @"C:\Allowed\dest.txt");

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("错误"));
        Assert.IsTrue(result.Message!.Contains("不在允许访问的范围内"));
    }

    [TestMethod]
    public async Task GetFileInfoAsync_File_ReturnsFileInfo()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"info-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        try
        {
            var file = Path.Combine(testDir, "test.cs");
            File.WriteAllText(file, "test content");

            var toolGroup = CreateToolGroup(new List<string> { testDir });
            ToolResult<string> result = await toolGroup.GetFileInfoAsync(file);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data!.Contains("文件:"));
            Assert.IsTrue(result.Data!.Contains("大小:"));
            Assert.IsTrue(result.Data!.Contains(".cs"));
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [TestMethod]
    public async Task GetFileInfoAsync_Directory_ReturnsDirInfo()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"info-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        try
        {
            File.WriteAllText(Path.Combine(testDir, "a.cs"), "");
            File.WriteAllText(Path.Combine(testDir, "b.cs"), "");
            Directory.CreateDirectory(Path.Combine(testDir, "sub"));

            var toolGroup = CreateToolGroup(new List<string> { testDir });
            ToolResult<string> result = await toolGroup.GetFileInfoAsync(testDir);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data!.Contains("目录:"));
            Assert.IsTrue(result.Data!.Contains("文件数:"));
            Assert.IsTrue(result.Data!.Contains("子目录数:"));
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [TestMethod]
    public async Task GetFileInfoAsync_PathNotExists_ReturnsError()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"info-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        try
        {
            var toolGroup = CreateToolGroup(new List<string> { testDir });
            ToolResult<string> result = await toolGroup.GetFileInfoAsync(Path.Combine(testDir, "notexist"));

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Message!.Contains("错误") || result.Message!.Contains("不存在"));
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [TestMethod]
    public async Task GetFileInfoAsync_PathNotAllowed_ReturnsError()
    {
        var toolGroup = CreateToolGroup(new List<string> { @"C:\Allowed" });
        ToolResult<string> result = await toolGroup.GetFileInfoAsync(@"C:\NotAllowed\file.txt");

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("错误"));
        Assert.IsTrue(result.Message!.Contains("不在允许访问的范围内"));
    }

    [TestMethod]
    public void FileSystemToolPlugin_GetTools_Returns11Functions()
    {
        var options = new LuBanAgentOptions
        {
            Tools = new ToolGroupOptions
            {
                FileSystem = new FileSystemToolOptions
                {
                    Enabled = true,
                    AllowedRoots = new List<string>()
                }
            }
        };
        var pathGuard = new PathGuard(Options.Create(options));
        var plugin = new FileSystemToolPlugin(Options.Create(options), pathGuard);
        var sp = new ServiceCollection().BuildServiceProvider();

        var tools = plugin.GetTools(sp);

        Assert.AreEqual(12, tools.Count);
    }
}
