/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Tests.Orchestration
*文件名： SubAgentRoleTests
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：SubAgentRole 单元测试
*
*****************************************************************************/
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.AIAgent.Tests.Orchestration;

[TestClass]
public class SubAgentRoleTests
{
    [TestMethod]
    public void TestSubAgentRole_ShouldHaveRequiredProperties()
    {
        var role = new SubAgentRole
        {
            Name = "coder",
            SystemPromptTemplate = "You are a coder. Task: {prompt}",
            DefaultToolGroups = new List<string> { "filesystem", "script" }
        };

        Assert.AreEqual("coder", role.Name);
        Assert.AreEqual("You are a coder. Task: {prompt}", role.SystemPromptTemplate);
        Assert.AreEqual(2, role.DefaultToolGroups.Count);
        Assert.IsTrue(role.DefaultToolGroups.Contains("filesystem"));
        Assert.IsTrue(role.DefaultToolGroups.Contains("script"));
    }

    [TestMethod]
    public void TestSubAgentRole_DefaultValues()
    {
        var role = new SubAgentRole();

        Assert.AreEqual("", role.Name);
        Assert.AreEqual("", role.SystemPromptTemplate);
        Assert.IsNotNull(role.DefaultToolGroups);
        Assert.AreEqual(0, role.DefaultToolGroups.Count);
    }
}
