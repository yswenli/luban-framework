using LuBan.AIAgent.Retrieval.Chunkers;

namespace LuBan.AIAgent.Tests.Retrieval;

[TestClass]
public class JsonChunkerTests
{
    private static JsonChunker Json() => new("json", new[] { ".json", ".jsonc", ".ipynb" });

    [TestMethod]
    public void Chunk_Object_GroupsTopLevelKeys()
    {
        var json = "{\"server\":{\"host\":\"localhost\"},\"database\":{\"url\":\"sqlite\"}}";
        var chunks = Json().Chunk("app.json", json);
        Assert.AreEqual(1, chunks.Count);
        StringAssert.Contains(chunks[0].Content, "localhost");
        StringAssert.Contains(chunks[0].Content, "sqlite");
        Assert.AreEqual("server", chunks[0].SymbolName);
    }

    [TestMethod]
    public void Chunk_Array_GroupsItems()
    {
        var json = "[" + string.Join(",", Enumerable.Range(1, 20).Select(i => $"{{\"id\":{i}}}")) + "]";
        var chunks = Json().Chunk("a.json", json);
        Assert.IsTrue(chunks.Count >= 1);
        Assert.AreEqual("Array", chunks[0].ChunkType);
    }

    [TestMethod]
    public void Chunk_InvalidJson_FallsBackToWindow()
    {
        var chunks = Json().Chunk("a.json", "{not json");
        Assert.AreEqual("Window", chunks[0].ChunkType);
    }

    [TestMethod]
    public void Chunk_Ipynb_ChunksPerCell()
    {
        var nb = "{\"cells\":[{\"cell_type\":\"markdown\",\"source\":[\"# 标题\\n\",\"说明文字\"]},{\"cell_type\":\"code\",\"source\":[\"import os\\n\",\"print(os.name)\"]}],\"metadata\":{},\"nbformat\":4}";
        var chunks = Json().Chunk("a.ipynb", nb);
        Assert.AreEqual(2, chunks.Count);
        Assert.AreEqual("Markdown", chunks[0].ChunkType);
        Assert.AreEqual("Code", chunks[1].ChunkType);
        StringAssert.Contains(chunks[1].Content, "import os");
    }
}
