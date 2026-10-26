using LuBan.AIAgent.Abstractions;

namespace LuBan.XTestProject;

[TestClass]
public class ToolDisplayNamesUnitTest
{
    [TestMethod]
    public void GetAction_KnownTool_ReturnsChineseName()
    {
        Assert.AreEqual("读取文件", ToolDisplayNames.GetAction("ReadFileAsync"));
    }

    [TestMethod]
    public void GetAction_UnknownTool_FallsBackToMethodName()
    {
        Assert.AreEqual("NoSuchTool", ToolDisplayNames.GetAction("NoSuchTool"));
    }

    [TestMethod]
    public void GetAction_Blank_ReturnsDefault()
    {
        Assert.AreEqual("执行工具", ToolDisplayNames.GetAction("  "));
    }

    [TestMethod]
    public void GetIcon_KnownTool_DiffersFromFallback()
    {
        Assert.AreNotEqual(ToolDisplayNames.GetIcon("NoSuchTool"), ToolDisplayNames.GetIcon("Grep"));
    }

    [TestMethod]
    public void GetIcon_UnknownTool_SameAsBlankFallback()
    {
        Assert.AreEqual(ToolDisplayNames.GetIcon("NoSuchTool"), ToolDisplayNames.GetIcon("  "));
    }

    [TestMethod]
    public void Summarize_LongValue_IsTruncated()
    {
        var args = new Dictionary<string, object?> { ["path"] = new string('x', 100) };

        var text = ToolArgsFormatter.Summarize(args);

        Assert.IsTrue(text.StartsWith("path="));
        Assert.IsTrue(text.EndsWith(ToolArgsFormatter.Ellipsis));
        Assert.IsTrue(text.Length <= ToolArgsFormatter.DefaultMaxLength + 8);
    }

    [TestMethod]
    public void Summarize_NullArgs_ReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, ToolArgsFormatter.Summarize(null));
    }
}