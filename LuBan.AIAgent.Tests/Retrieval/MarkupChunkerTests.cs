using LuBan.AIAgent.Retrieval.Chunkers;

namespace LuBan.AIAgent.Tests.Retrieval;

[TestClass]
public class MarkupChunkerTests
{
    private static MarkupChunker Html() => new("html", new[] { ".html", ".htm" });

    [TestMethod]
    public void Chunk_Html_StripsScriptAndTags()
    {
        var html = "<html><body><p>Hello &amp; world</p><script>alert('x')</script><p>Second paragraph</p></body></html>";
        var chunks = Html().Chunk("a.html", html);
        Assert.AreEqual(1, chunks.Count);
        StringAssert.Contains(chunks[0].Content, "Hello & world");
        StringAssert.Contains(chunks[0].Content, "Second paragraph");
        Assert.IsFalse(chunks[0].Content.Contains("alert"));
    }

    [TestMethod]
    public void Chunk_Html_MultipleBlocks_Grouped()
    {
        var html = "<div>Block one content here</div><div>Block two content here</div>";
        var chunks = Html().Chunk("a.html", html);
        Assert.AreEqual(1, chunks.Count);
        StringAssert.Contains(chunks[0].Content, "Block one");
        StringAssert.Contains(chunks[0].Content, "Block two");
    }

    [TestMethod]
    public void Chunk_NoText_FallsBackToWindow()
    {
        var chunks = Html().Chunk("a.html", "<div></div>");
        Assert.AreEqual("Window", chunks[0].ChunkType);
    }
}
