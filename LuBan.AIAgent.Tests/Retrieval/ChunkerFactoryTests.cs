using LuBan.AIAgent.Retrieval.Chunkers;

namespace LuBan.AIAgent.Tests.Retrieval;

[TestClass]
public class ChunkerFactoryTests
{
    [TestMethod]
    public void GetChunker_CSharp_ReturnsCSharpChunker()
    {
        var factory = new ChunkerFactory();
        var chunker = factory.GetChunker("a.cs");
        Assert.AreEqual("csharp", chunker.Language);
    }

    [TestMethod]
    public void GetChunker_Python_ReturnsPythonChunker()
    {
        var factory = new ChunkerFactory();
        var chunker = factory.GetChunker("main.py");
        Assert.AreEqual("python", chunker.Language);
    }

    [TestMethod]
    public void GetChunker_UnknownExtension_ReturnsFallback()
    {
        var factory = new ChunkerFactory();
        var chunker = factory.GetChunker("unknown.xyz");
        Assert.AreEqual("text", chunker.Language);
    }

    [TestMethod]
    public void ShouldIndex_ExcludesNodeModules()
    {
        var factory = new ChunkerFactory();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var nodeDir = Path.Combine(tempDir, "node_modules", "pkg");
        Directory.CreateDirectory(nodeDir);
        var file = Path.Combine(nodeDir, "index.js");
        File.WriteAllText(file, "module.exports = {}");
        try
        {
            Assert.IsFalse(factory.ShouldIndex(file, tempDir, 5120 * 1024));
        }
        finally { Directory.Delete(tempDir, true); }
    }

    [TestMethod]
    public void ShouldIndex_ExcludesMinJs()
    {
        var factory = new ChunkerFactory();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var file = Path.Combine(tempDir, "app.min.js");
        File.WriteAllText(file, "var a=1;");
        try
        {
            Assert.IsFalse(factory.ShouldIndex(file, tempDir, 5120 * 1024));
        }
        finally { Directory.Delete(tempDir, true); }
    }

    [TestMethod]
    public void ShouldIndex_ExcludesBinaryExtension()
    {
        var factory = new ChunkerFactory();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var file = Path.Combine(tempDir, "app.dll");
        File.WriteAllBytes(file, new byte[] { 1, 2, 3 });
        try
        {
            Assert.IsFalse(factory.ShouldIndex(file, tempDir, 5120 * 1024));
        }
        finally { Directory.Delete(tempDir, true); }
    }

    [TestMethod]
    public void ShouldIndex_ExcludesOversizedFile()
    {
        var factory = new ChunkerFactory();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var file = Path.Combine(tempDir, "big.txt");
        File.WriteAllText(file, new string('x', 1024 * 1024));
        try
        {
            Assert.IsFalse(factory.ShouldIndex(file, tempDir, 512 * 1024));
        }
        finally { Directory.Delete(tempDir, true); }
    }

    [TestMethod]
    public void ShouldIndex_IncludesNormalFile()
    {
        var factory = new ChunkerFactory();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var file = Path.Combine(tempDir, "Program.cs");
        File.WriteAllText(file, "class Program { }");
        try
        {
            Assert.IsTrue(factory.ShouldIndex(file, tempDir, 5120 * 1024));
        }
        finally { Directory.Delete(tempDir, true); }
    }
}
