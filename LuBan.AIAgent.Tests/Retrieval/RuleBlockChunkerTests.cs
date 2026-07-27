using LuBan.AIAgent.Retrieval.Chunkers;

namespace LuBan.AIAgent.Tests.Retrieval;

[TestClass]
public class RuleBlockChunkerTests
{
    private static RuleBlockChunker Css() => new("css", new[] { ".css", ".scss", ".less" });

    [TestMethod]
    public void Chunk_Css_SplitsByRules()
    {
        var pad = new string('x', 200);
        var css = $".header {{ display: flex; padding: 16px; margin: 0 auto; background: #fff; border-bottom: 1px solid #eee; color: #333; font-size: 14px; line-height: 1.5; padding-bottom: 8px; padding-top: 8px; padding-left: {pad}; }}\n.main {{ display: block; max-width: 1200px; margin: 0 auto; padding: 24px; background: #fafafa; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); color: #333; font-size: 16px; line-height: 1.6; padding-bottom: {pad}; }}\n";
        var chunks = Css().Chunk("a.css", css);
        Assert.AreEqual(2, chunks.Count);
        Assert.AreEqual("Rule", chunks[0].ChunkType);
        StringAssert.Contains(chunks[0].Content, ".header");
        StringAssert.Contains(chunks[1].Content, ".main");
    }

    [TestMethod]
    public void Chunk_NoBraces_FallsBackToWindow()
    {
        var chunks = Css().Chunk("a.css", "/* comment */\n");
        Assert.AreEqual("Window", chunks[0].ChunkType);
    }
}
