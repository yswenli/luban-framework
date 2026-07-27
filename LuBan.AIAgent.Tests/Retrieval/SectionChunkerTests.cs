using LuBan.AIAgent.Retrieval.Chunkers;

namespace LuBan.AIAgent.Tests.Retrieval;

[TestClass]
public class SectionChunkerTests
{
    [TestMethod]
    public void Chunk_Ini_SplitsBySection()
    {
        var pad = new string('x', 200);
        var ini = $"[server]\nhost=localhost\nport=8080\npadding={pad}\n\n[database]\nurl=sqlite\npadding={pad}\n";
        var chunker = new SectionChunker("ini", new[] { ".ini", ".cfg" });
        var chunks = chunker.Chunk("app.ini", ini);
        Assert.AreEqual(2, chunks.Count);
        Assert.AreEqual("Section", chunks[0].ChunkType);
        Assert.AreEqual("server", chunks[0].SymbolName);
        Assert.AreEqual("database", chunks[1].SymbolName);
    }

    [TestMethod]
    public void Chunk_NoSection_FallsBackToWindow()
    {
        var chunker = new SectionChunker("ini", new[] { ".ini" });
        var chunks = chunker.Chunk("a.ini", "key=value\n");
        Assert.AreEqual("Window", chunks[0].ChunkType);
    }
}
