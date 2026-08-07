/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Tests.Orchestration
*文件名： HeuristicFilterTests
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：HeuristicFilterOptions 单元测试
*
*****************************************************************************/
using LuBan.AIAgent.Configuration;

namespace LuBan.AIAgent.Tests.Orchestration;

[TestClass]
public class HeuristicFilterTests
{
    [TestMethod]
    public void TestShouldSkipPlanning_短输入无关键词_跳过()
    {
        var filter = new HeuristicFilterOptions();
        Assert.IsTrue(filter.ShouldSkipPlanning("你好"));
    }

    [TestMethod]
    public void TestShouldSkipPlanning_短输入含关键词_不跳过()
    {
        var filter = new HeuristicFilterOptions();
        Assert.IsFalse(filter.ShouldSkipPlanning("搜索并总结"));
    }

    [TestMethod]
    public void TestShouldSkipPlanning_长输入_不跳过()
    {
        var filter = new HeuristicFilterOptions { MaxLength = 20 };
        Assert.IsFalse(filter.ShouldSkipPlanning("这是一个长度明显超过二十个字符的用户输入内容"));
    }

    [TestMethod]
    public void TestShouldSkipPlanning_禁用时不跳过()
    {
        var filter = new HeuristicFilterOptions { Enabled = false };
        Assert.IsFalse(filter.ShouldSkipPlanning("你好"));
    }

    [TestMethod]
    public void TestShouldSkipPlanning_空输入不跳过()
    {
        var filter = new HeuristicFilterOptions();
        Assert.IsFalse(filter.ShouldSkipPlanning(""));
    }

    [TestMethod]
    public void TestOrchestrationOptions_包含HeuristicFilter默认值()
    {
        var opts = new OrchestrationOptions();
        Assert.IsNotNull(opts.HeuristicFilter);
        Assert.IsTrue(opts.HeuristicFilter.Enabled);
        Assert.AreEqual(20, opts.HeuristicFilter.MaxLength);
        Assert.IsTrue(opts.HeuristicFilter.Keywords.Count > 0);
    }
}
