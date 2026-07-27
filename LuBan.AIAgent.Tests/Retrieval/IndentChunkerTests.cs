using LuBan.AIAgent.Retrieval.Chunkers;

namespace LuBan.AIAgent.Tests.Retrieval;

[TestClass]
public class IndentChunkerTests
{
    private static IndentChunker Python()
        => new("python", new[] { ".py" }, @"^\s*(async\s+def|def|class)\s+");

    [TestMethod]
    public void Chunk_Python_DefAndClass()
    {
        var code = "def auth(user, pwd):\n    return user == 'a'\n\nclass UserService:\n    def get(self):\n        return 1\n";
        var chunks = Python().Chunk("a.py", code);
        Assert.AreEqual(2, chunks.Count);
        Assert.AreEqual("Function", chunks[0].ChunkType);
        Assert.AreEqual("auth", chunks[0].SymbolName);
        Assert.AreEqual("Class", chunks[1].ChunkType);
        Assert.AreEqual("UserService", chunks[1].SymbolName);
        StringAssert.Contains(chunks[1].Content, "def get");
    }

    [TestMethod]
    public void Chunk_Yaml_TopLevelKeys()
    {
        var yaml = new IndentChunker("yaml", new[] { ".yaml", ".yml" }, @"^[A-Za-z_][\w.\-]*\s*:");
        var pad = new string('x', 200);
        var code = $"server:\n  host: localhost\n  port: 8080\n  padding: {pad}\ndatabase:\n  url: sqlite\n  padding: {pad}\n";
        var chunks = yaml.Chunk("app.yaml", code);
        Assert.AreEqual(2, chunks.Count);
        Assert.AreEqual("server", chunks[0].SymbolName);
        Assert.AreEqual("database", chunks[1].SymbolName);
    }

    [TestMethod]
    public void Chunk_NoMatch_FallsBackToWindow()
    {
        var chunks = Python().Chunk("a.py", "x = 1\n");
        Assert.AreEqual("Window", chunks[0].ChunkType);
    }
}
