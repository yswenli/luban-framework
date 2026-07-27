using LuBan.AIAgent.Retrieval.Chunkers;

namespace LuBan.AIAgent.Tests.Retrieval;

[TestClass]
public class BracePairingChunkerTests
{
    private static BracePairingChunker CSharp()
        => new("csharp", new[] { ".cs" }, @"\b(class|struct|interface|enum|record|namespace)\b");

    private const string SampleCs = """
        using System;

        namespace Demo
        {
            public class UserService
            {
                public bool AuthenticateUser(string name, string password)
                {
                    // 验证用户身份
                    return name == "admin" && password == "123456";
                }

                public string GetDisplayName(string name)
                {
                    return $"Hello {name}";
                }
            }
        }
        """;

    [TestMethod]
    public void Chunk_CSharpFile_FindsNamespaceRegion()
    {
        var chunks = CSharp().Chunk("UserService.cs", SampleCs);
        Assert.AreEqual(1, chunks.Count);
        Assert.AreEqual("Namespace", chunks[0].ChunkType);
        Assert.AreEqual("Demo", chunks[0].SymbolName);
        StringAssert.Contains(chunks[0].Content, "AuthenticateUser");
        Assert.AreEqual(3, chunks[0].StartLine);
    }

    [TestMethod]
    public void Chunk_KeywordInComment_Ignored()
    {
        var code = "// class Fake { }\nclass Real\n{\n    void M() { }\n}\n";
        var chunks = CSharp().Chunk("a.cs", code);
        Assert.AreEqual(1, chunks.Count);
        Assert.AreEqual("Real", chunks[0].SymbolName);
        Assert.AreEqual(2, chunks[0].StartLine);
    }

    [TestMethod]
    public void Chunk_BraceInString_NotCounted()
    {
        var pad = new string('a', 200);
        var code = $"class A\n{{\n    string s = \"}}}}}}\";\n    int x = 1;\n    string p = \"{pad}\";\n}}\nclass B\n{{\n    int y = 2;\n    string q = \"{pad}\";\n}}\n";
        var chunks = CSharp().Chunk("a.cs", code);
        Assert.AreEqual(2, chunks.Count);
        Assert.AreEqual("A", chunks[0].SymbolName);
        Assert.AreEqual("B", chunks[1].SymbolName);
    }

    [TestMethod]
    public void Chunk_GoFile_FuncAndType()
    {
        var go = new BracePairingChunker("go", new[] { ".go" }, @"\b(func|type)\b");
        var code = "package main\n\ntype Server struct {\n    Port int\n}\n\nfunc main() {\n    println(\"hi\")\n}\n";
        var chunks = go.Chunk("main.go", code);
        Assert.AreEqual(2, chunks.Count);
        Assert.AreEqual("Type", chunks[0].ChunkType);
        Assert.AreEqual("Function", chunks[1].ChunkType);
    }

    [TestMethod]
    public void Chunk_NoKeyword_FallsBackToWindow()
    {
        var chunks = CSharp().Chunk("a.cs", "x = 1\ny = 2\n");
        Assert.AreEqual(1, chunks.Count);
        Assert.AreEqual("Window", chunks[0].ChunkType);
    }
}
