using LuBan.AIAgent.Retrieval.Chunkers;

namespace LuBan.AIAgent.Tests.Retrieval;

[TestClass]
public class SlidingWindowChunkerTests
{
    private static string MakeText(int lines, int lineLen = 100)
        => string.Join("\n", Enumerable.Range(1, lines).Select(i => $"line{i} " + new string('x', lineLen)));

    [TestMethod]
    public void Chunk_SmallText_SingleChunk()
    {
        var chunker = new SlidingWindowChunker();
        var chunks = chunker.Chunk("a.txt", "hello\nworld");
        Assert.AreEqual(1, chunks.Count);
        Assert.AreEqual(1, chunks[0].StartLine);
        Assert.AreEqual(2, chunks[0].EndLine);
        Assert.AreEqual("Window", chunks[0].ChunkType);
        Assert.AreEqual("text", chunks[0].Language);
        Assert.AreEqual(0, chunks[0].ChunkIndex);
    }

    [TestMethod]
    public void Chunk_LargeText_SplitsWithMaxSize()
    {
        var chunker = new SlidingWindowChunker();
        var chunks = chunker.Chunk("a.txt", MakeText(100));
        Assert.IsTrue(chunks.Count >= 3);
        foreach (var c in chunks)
            Assert.IsTrue(c.Content.Length <= CodeChunkerBase.MaxChars, $"chunk 超限: {c.Content.Length}");
    }

    [TestMethod]
    public void Chunk_LargeText_HasOverlap()
    {
        var chunker = new SlidingWindowChunker();
        var chunks = chunker.Chunk("a.txt", MakeText(100));
        Assert.IsTrue(chunks[1].StartLine < chunks[0].EndLine, "相邻 chunk 应有重叠行");
    }

    [TestMethod]
    public void Chunk_CrlfNormalized()
    {
        var chunker = new SlidingWindowChunker();
        var chunks = chunker.Chunk("a.txt", "a\r\nb\r\nc");
        Assert.AreEqual(1, chunks.Count);
        Assert.IsFalse(chunks[0].Content.Contains('\r'));
    }
}
