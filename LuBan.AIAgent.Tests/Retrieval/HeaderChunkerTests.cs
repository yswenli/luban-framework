using LuBan.AIAgent.Retrieval.Chunkers;

namespace LuBan.AIAgent.Tests.Retrieval;

[TestClass]
public class HeaderChunkerTests
{
    private static HeaderChunker Markdown()
        => new("markdown", new[] { ".md", ".markdown" }, @"^(#{1,6})\s+(.*?)\s*#*\s*$", 1, 2);

    [TestMethod]
    public void Chunk_Markdown_SplitsByHeadings()
    {
        var pad = new string('x', 200);
        var md = $"# 介绍\n{pad}\n## 安装\n{pad}\n## 使用\n{pad}\n";
        var chunks = Markdown().Chunk("README.md", md);
        Assert.AreEqual(3, chunks.Count);
        Assert.AreEqual("Section", chunks[0].ChunkType);
        Assert.AreEqual("介绍", chunks[0].SymbolName);
        Assert.AreEqual("安装", chunks[1].SymbolName);
        Assert.AreEqual("使用", chunks[2].SymbolName);
    }

    [TestMethod]
    public void Chunk_PreambleBeforeFirstHeading_IncludedAsWindow()
    {
        var pad = new string('p', 200);
        var md = $"{pad}\n# 标题\n{pad}\n";
        var chunks = Markdown().Chunk("a.md", md);
        Assert.AreEqual(2, chunks.Count);
        Assert.AreEqual("Window", chunks[0].ChunkType);
        Assert.AreEqual("Section", chunks[1].ChunkType);
    }

    [TestMethod]
    public void Chunk_NoHeading_FallsBackToWindow()
    {
        var chunks = Markdown().Chunk("a.md", "没有标题的内容。\n");
        Assert.AreEqual("Window", chunks[0].ChunkType);
    }
}
