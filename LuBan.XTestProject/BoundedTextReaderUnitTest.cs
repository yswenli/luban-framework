using System.Text;
using LuBan.AIAgent.Wiki.Extractors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class BoundedTextReaderUnitTest
{
    private static async Task<string> WriteTempAsync(string content)
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, content);
        return path;
    }

    [TestMethod]
    public async Task Reads_Small_File_Without_Truncation()
    {
        var path = await WriteTempAsync("hello world");
        try
        {
            var (text, truncated) = await BoundedTextReader.ReadAsync(path, 1000);
            Assert.AreEqual("hello world", text);
            Assert.IsFalse(truncated);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task Truncates_At_MaxChars_And_Flags()
    {
        var path = await WriteTempAsync(new string('a', 5000));
        try
        {
            var (text, truncated) = await BoundedTextReader.ReadAsync(path, 100);
            Assert.AreEqual(100, text.Length);
            Assert.IsTrue(truncated);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task Exact_Length_Is_Not_Truncated()
    {
        var path = await WriteTempAsync(new string('b', 100));
        try
        {
            var (text, truncated) = await BoundedTextReader.ReadAsync(path, 100);
            Assert.AreEqual(100, text.Length);
            Assert.IsFalse(truncated);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task Does_Not_Split_Multibyte_Chars()
    {
        var path = await WriteTempAsync(string.Concat(Enumerable.Repeat("中", 1000)));
        try
        {
            var (text, truncated) = await BoundedTextReader.ReadAsync(path, 50);
            Assert.AreEqual(50, text.Length);
            Assert.IsTrue(text.All(c => c == '中'));
            Assert.IsTrue(truncated);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public async Task TextExtractor_Large_File_Is_Truncated_Not_Oom()
    {
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, new string('x', TextExtractor.MaxChars + 50_000));
        try
        {
            var source = await new TextExtractor().ExtractAsync(path);
            Assert.AreEqual(TextExtractor.MaxChars, source.Text.Length);
            Assert.IsTrue(source.Truncated);
        }
        finally { File.Delete(path); }
    }
}