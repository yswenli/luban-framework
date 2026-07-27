using LuBan.AIAgent.Retrieval.Chunkers;

namespace LuBan.AIAgent.Tests.Retrieval;

[TestClass]
public class KeywordEndChunkerTests
{
    private static KeywordEndChunker Ruby()
        => new("ruby", new[] { ".rb" },
            @"^\s*(def|class|module|if|unless|case|begin|while|until|for)\b|\bdo\b",
            @"^\s*end\b");

    [TestMethod]
    public void Chunk_Ruby_DefWithNestedIf()
    {
        var pad = new string('x', 200);
        var code = $"def auth(user)\n  if user.admin?\n    \"{pad}\"\n  end\nend\n\ndef list\n  \"{pad}\"\nend\n";
        var chunks = Ruby().Chunk("a.rb", code);
        Assert.AreEqual(2, chunks.Count);
        Assert.AreEqual("Function", chunks[0].ChunkType);
        Assert.AreEqual("auth", chunks[0].SymbolName);
        Assert.AreEqual(1, chunks[0].StartLine);
        Assert.AreEqual(5, chunks[0].EndLine);
        Assert.AreEqual("list", chunks[1].SymbolName);
    }

    [TestMethod]
    public void Chunk_NoMatch_FallsBackToWindow()
    {
        var chunks = Ruby().Chunk("a.rb", "puts 1\n");
        Assert.AreEqual("Window", chunks[0].ChunkType);
    }
}
