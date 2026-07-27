using LuBan.AIAgent.Retrieval.Chunkers;

namespace LuBan.AIAgent.Tests.Retrieval;

[TestClass]
public class StatementChunkerTests
{
    private static StatementChunker Sql() => new("sql", new[] { ".sql" });

    [TestMethod]
    public void Chunk_Sql_SplitsStatements()
    {
        var pad = new string('x', 200);
        var sql = $"CREATE TABLE users (id INTEGER PRIMARY KEY, name TEXT, padding TEXT DEFAULT '{pad}');\nCREATE TABLE orders (id INTEGER PRIMARY KEY, user_id INTEGER, padding TEXT DEFAULT '{pad}');\n";
        var chunks = Sql().Chunk("a.sql", sql);
        Assert.AreEqual(2, chunks.Count);
        Assert.AreEqual("Statement", chunks[0].ChunkType);
        StringAssert.Contains(chunks[0].Content, "CREATE TABLE users");
        StringAssert.Contains(chunks[1].Content, "CREATE TABLE orders");
    }

    [TestMethod]
    public void Chunk_Sql_SemicolonInString_NotSplit()
    {
        var sql = "INSERT INTO logs VALUES ('a;b');\nINSERT INTO logs VALUES ('c');\n";
        var chunks = Sql().Chunk("a.sql", sql);
        StringAssert.Contains(chunks[0].Content, "'a;b'");
    }

    [TestMethod]
    public void Chunk_NoSemicolon_FallsBackToWindow()
    {
        var chunks = Sql().Chunk("a.sql", "SELECT 1\n");
        Assert.AreEqual("Window", chunks[0].ChunkType);
    }
}
