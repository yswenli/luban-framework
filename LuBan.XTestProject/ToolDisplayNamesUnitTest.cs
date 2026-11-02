/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.XTestProject
*文件名： ToolDisplayNamesUnitTest
*版本号： V1.0.0.0
*唯一标识：266288ec-afa6-421f-938b-c53128c26af5
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26 15:23:48
*描述：工具显示名与参数摘要单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/26 15:23:48
*修改人： yswenli
*版本号： V1.0.0.0
*描述：工具显示名与参数摘要单元测试
*
*****************************************************************************/

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