/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.Tests.Orchestration
*文件名： SubAgentRoleRegistryTests
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：SubAgentRoleRegistry 单元测试
*
*****************************************************************************/
using LuBan.AIAgent.Orchestration;
using LuBan.AIAgent.Orchestration.Models;

namespace LuBan.AIAgent.Tests.Orchestration;

[TestClass]
public class SubAgentRoleRegistryTests
{
    [TestMethod]
    public void TestGetRole_WithValidName_ShouldReturnRole()
    {
        var registry = new SubAgentRoleRegistry();
        var role = registry.GetRole("analyst");

        Assert.IsNotNull(role);
        Assert.AreEqual("analyst", role.Name);
        Assert.IsTrue(role.DefaultToolGroups.Contains("filesystem"));
    }

    [TestMethod]
    public void TestGetRole_WithInvalidName_ShouldReturnNull()
    {
        var registry = new SubAgentRoleRegistry();
        var role = registry.GetRole("nonexistent");

        Assert.IsNull(role);
    }

    [TestMethod]
    public void TestGetAllRoles_ShouldReturnFourBuiltInRoles()
    {
        var registry = new SubAgentRoleRegistry();
        var roles = registry.GetAllRoles();

        Assert.AreEqual(4, roles.Count);
        Assert.IsTrue(roles.Any(r => r.Name == "analyst"));
        Assert.IsTrue(roles.Any(r => r.Name == "researcher"));
        Assert.IsTrue(roles.Any(r => r.Name == "coder"));
        Assert.IsTrue(roles.Any(r => r.Name == "writer"));
    }

    [TestMethod]
    public void TestRegister_CustomRole_ShouldBeRetrievable()
    {
        var registry = new SubAgentRoleRegistry();
        var customRole = new SubAgentRole
        {
            Name = "tester",
            SystemPromptTemplate = "You are a tester. Task: {prompt}",
            DefaultToolGroups = new List<string> { "script" }
        };

        registry.Register(customRole);
        var retrieved = registry.GetRole("tester");

        Assert.IsNotNull(retrieved);
        Assert.AreEqual("tester", retrieved.Name);
        Assert.AreEqual(5, registry.GetAllRoles().Count);
    }
}
