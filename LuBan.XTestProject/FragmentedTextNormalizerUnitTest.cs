/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： FragmentedTextNormalizerUnitTest
*描述：验证流式正文碎片空行的收拢规则
*
*****************************************************************************/
using LuBan.AIAgent.Sessions;

namespace LuBan.XTestProject;

[TestClass]
public class FragmentedTextNormalizerUnitTest
{
    /// <summary>真实脏样本：句子被按帧切开，空行落在词与词之间。</summary>
    private const string FragmentedProse =
        "第二\n\n个文件已读\n\n（\n\n替代说明见上\n\n）。**第三个要读\n\n的是 `LubanAgent\n\nCore/LubanAgent\n\nCore.csproj\n\n`**，它是全仓\n\n所有下游\n\n工程的共同\n\n引用对象，\n\n需要确认它\n\n对框架是\n\n包引用还是\n\n项目引用\n\n、以及\n\n带入了哪些\n\n第三方包。";

    [TestMethod]
    public void Normalize_Collapses_Fragmented_Prose()
    {
        const string expected =
            "第二个文件已读（替代说明见上）。**第三个要读的是 `LubanAgentCore/LubanAgentCore.csproj`**，它是全仓所有下游工程的共同引用对象，需要确认它对框架是包引用还是项目引用、以及带入了哪些第三方包。";

        Assert.AreEqual(expected, FragmentedTextNormalizer.Normalize(FragmentedProse));
    }

    [TestMethod]
    public void LooksFragmented_Recognizes_Split_Sentence()
    {
        Assert.IsTrue(FragmentedTextNormalizer.LooksFragmented(FragmentedProse));
    }

    [TestMethod]
    public void Normalize_Keeps_Heading_List_And_Table_Boundaries()
    {
        const string input =
            "## 结论\n\n第一\n\n段被切碎\n\n了，\n\n还有\n\n更多\n\n碎片\n\n内容。\n\n- 列表项一\n- 列表项二\n\n| a | b |\n|---|---|\n| 1 | 2 |";
        const string expected =
            "## 结论\n\n第一段被切碎了，还有更多碎片内容。\n\n- 列表项一\n- 列表项二\n\n| a | b |\n|---|---|\n| 1 | 2 |";

        Assert.AreEqual(expected, FragmentedTextNormalizer.Normalize(input));
    }

    [TestMethod]
    public void Normalize_Keeps_Fenced_Code_Intact()
    {
        const string input =
            "说明\n\n文字\n\n被切碎\n\n了，\n\n继续\n\n说明。\n\n```csharp\nvar a = 1;\n\nvar b = 2;\n```\n\n收尾\n\n文字。";
        const string expected =
            "说明文字被切碎了，继续说明。\n\n```csharp\nvar a = 1;\n\nvar b = 2;\n```\n\n收尾文字。";

        Assert.AreEqual(expected, FragmentedTextNormalizer.Normalize(input));
    }

    [TestMethod]
    public void Normalize_Joins_English_Fragments_With_Single_Space()
    {
        const string input = "Part one\n\ntwo three\n\nfour\n\nfive\n\nsix\n\nseven";
        const string expected = "Part one two three four five six seven";

        Assert.AreEqual(expected, FragmentedTextNormalizer.Normalize(input));
    }

    [TestMethod]
    public void Normalize_Single_Line_Returns_Same_Instance()
    {
        const string input = "第二\n个文件已读"; // 单换行不在处理范围

        Assert.AreSame(input, FragmentedTextNormalizer.Normalize(input));
    }

    [TestMethod]
    public void Normalize_Null_Returns_Empty()
    {
        Assert.AreEqual(string.Empty, FragmentedTextNormalizer.Normalize(null));
    }

    [TestMethod]
    public void Normalize_Short_Chat_Is_Untouched()
    {
        const string input = "你好 👋\n\n我在。有什么想让我做的，直接说就行。";

        Assert.AreSame(input, FragmentedTextNormalizer.Normalize(input));
    }

    [TestMethod]
    public void Normalize_Clean_Multi_Paragraph_Document_Is_Untouched()
    {
        const string input =
            "段落一：这是一段足够长的正文内容，长度明显超过阈值。\n\n段落二：这同样是一段足够长的正文内容。\n\n段落三：第三段也是完整句子，不应该被合并。\n\n段落四：第四段同样保持独立。\n\n段落五：第五段继续独立成段。\n\n段落六：第六段结束。";

        Assert.AreSame(input, FragmentedTextNormalizer.Normalize(input));
    }

    [TestMethod]
    public void Normalize_Is_Idempotent()
    {
        var once = FragmentedTextNormalizer.Normalize(FragmentedProse);

        Assert.AreSame(once, FragmentedTextNormalizer.Normalize(once));
    }

    [TestMethod]
    public void Normalize_Force_Skips_Fragmentation_Gate()
    {
        const string input = "第一\n\n段"; // 段数不足，默认判定不会出手

        Assert.AreSame(input, FragmentedTextNormalizer.Normalize(input));
        Assert.AreEqual("第一段", FragmentedTextNormalizer.Normalize(input, force: true));
    }

    [TestMethod]
    public void Normalize_Handles_CrLf()
    {
        const string input =
            "第二\r\n\r\n个文件已读\r\n\r\n（\r\n\r\n替代说明见上\r\n\r\n）。**第三个要读\r\n\r\n的是 `LubanAgent\r\n\r\nCore/LubanAgent\r\n\r\nCore.csproj`**。";
        const string expected =
            "第二个文件已读（替代说明见上）。**第三个要读的是 `LubanAgentCore/LubanAgentCore.csproj`**。";

        Assert.AreEqual(expected, FragmentedTextNormalizer.Normalize(input));
    }

    /// <summary>
    /// 一段正文首尾的空行必须保留：落库时正文由多段 <c>TextContent</c> 拼接，
    /// 跨段拼接处若被吞掉换行，段落之间会被连成一句。
    /// </summary>
    [TestMethod]
    public void Normalize_Preserves_Leading_And_Trailing_Blank_Lines()
    {
        Assert.AreEqual("\n\n下一段。", FragmentedTextNormalizer.Normalize("\n\n下一段。", force: true));
        Assert.AreEqual("上一段。\n\n", FragmentedTextNormalizer.Normalize("上一段。\n\n", force: true));
    }
}