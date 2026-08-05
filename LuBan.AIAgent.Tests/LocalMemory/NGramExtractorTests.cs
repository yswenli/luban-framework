using LuBan.AIAgent.LocalMemory;

namespace LuBan.AIAgent.Tests.LocalMemory;

[TestClass]
public class NGramExtractorTests
{
    [TestMethod]
    public void Fnv1a32_IsDeterministic()
    {
        Assert.AreEqual(NGramExtractor.Fnv1a32("项目开发"),
                        NGramExtractor.Fnv1a32("项目开发"));
    }

    [TestMethod]
    public void Fnv1a32_DifferentStrings_UsuallyDiffer()
    {
        Assert.AreNotEqual(NGramExtractor.Fnv1a32("项目开发"),
                           NGramExtractor.Fnv1a32("天气很好"));
    }

    [TestMethod]
    public void Normalize_LowercasesAndStripsPunctuation()
    {
        Assert.AreEqual("项目net开发", NGramExtractor.Normalize("项目.NET 开发！"));
    }

    [TestMethod]
    public void Extract_Chinese_ProducesBigramsAndTrigrams()
    {
        var grams = NGramExtractor.Extract("我喜欢编程").ToList();
        Assert.IsTrue(grams.Contains("我喜欢"));
        Assert.IsTrue(grams.Contains("编程"));
    }

    [TestMethod]
    public void Extract_SemanticallyRelated_ShareGrams()
    {
        var a = NGramExtractor.ExtractHashes("我非常喜欢编程").ToHashSet();
        var b = NGramExtractor.ExtractHashes("我喜欢编程").ToHashSet();
        var c = NGramExtractor.ExtractHashes("今天天气很好").ToHashSet();
        Assert.IsTrue(a.Overlaps(b), "相关文本应共享 n-gram");
        Assert.IsFalse(a.Overlaps(c), "无关文本不应共享 n-gram");
    }
}
