using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Infrastructure;
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
            var result = await toolGroup.SearchFilesAsync(testDir, "*.cs");

            Assert.IsTrue(result.Contains("a.cs"));
            Assert.IsTrue(result.Contains("b.cs"));
            Assert.IsFalse(result.Contains("c.txt"));
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
            var result = await toolGroup.SearchFilesAsync(testDir, "**/*.cs");

            Assert.IsTrue(result.Contains("a.cs"));
            Assert.IsTrue(result.Contains("b.cs"));
            Assert.IsTrue(result.Contains("c.cs"));
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
            var result = await toolGroup.SearchFilesAsync(testDir, "*");

            Assert.IsTrue(result.Contains("a.cs"));
            Assert.IsFalse(result.Contains("b.dll"));
            Assert.IsFalse(result.Contains("c.png"));
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
            var result = await toolGroup.SearchFilesAsync(testDir, "*.cs", maxResults: 3);

            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
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
        var result = await toolGroup.SearchFilesAsync(@"C:\NotAllowed", "*.cs");

        Assert.IsTrue(result.Contains("错误"));
        Assert.IsTrue(result.Contains("不在允许访问的范围内"));
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
            var result = await toolGroup.GrepAsync(testDir, "class\\s+\\w+");

            Assert.IsTrue(result.Contains("FooBar"));
            Assert.IsTrue(result.Contains("Baz"));
            Assert.IsFalse(result.Contains("no match"));
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
            var result = await toolGroup.GrepAsync(testDir, "TODO", filePattern: "*.cs");

            Assert.IsTrue(result.Contains("a.cs"));
            Assert.IsFalse(result.Contains("b.txt"));
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
            var result = await toolGroup.GrepAsync(testDir, "match", maxResults: 5);

            Assert.IsTrue(result.Contains("已截断"));
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
        var result = await toolGroup.GrepAsync(@"C:\NotAllowed", "pattern");

        Assert.IsTrue(result.Contains("错误"));
        Assert.IsTrue(result.Contains("不在允许访问的范围内"));
    }
}
