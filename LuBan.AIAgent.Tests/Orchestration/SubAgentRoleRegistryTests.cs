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

    private static string CreateTempWorkspace()
    {
        var dir = Path.Combine(Path.GetTempPath(), "luban-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    [TestMethod]
    public void TestLoadFromWorkspace_加载自定义角色()
    {
        var workspace = CreateTempWorkspace();
        try
        {
            var rolesDir = Path.Combine(workspace, ".luban-agent", "roles");
            Directory.CreateDirectory(rolesDir);
            File.WriteAllText(Path.Combine(rolesDir, "security-expert.json"), """
            {
              "name": "security-expert",
              "systemPromptTemplate": "You are a security expert. Task: {prompt}",
              "defaultToolGroups": ["filesystem", "script"]
            }
            """);

            var registry = new SubAgentRoleRegistry();
            var loaded = registry.LoadFromWorkspace(workspace);

            Assert.AreEqual(1, loaded);
            var role = registry.GetRole("security-expert");
            Assert.IsNotNull(role);
            Assert.AreEqual(2, role!.DefaultToolGroups.Count);
            Assert.AreEqual(5, registry.GetAllRoles().Count);
        }
        finally { Directory.Delete(workspace, true); }
    }

    [TestMethod]
    public void TestLoadFromWorkspace_自定义角色覆盖内置角色()
    {
        var workspace = CreateTempWorkspace();
        try
        {
            var rolesDir = Path.Combine(workspace, ".luban-agent", "roles");
            Directory.CreateDirectory(rolesDir);
            File.WriteAllText(Path.Combine(rolesDir, "coder.json"), """
            {
              "name": "coder",
              "systemPromptTemplate": "Custom coder. Task: {prompt}",
              "defaultToolGroups": ["filesystem"]
            }
            """);

            var registry = new SubAgentRoleRegistry();
            registry.LoadFromWorkspace(workspace);

            var role = registry.GetRole("coder");
            Assert.IsNotNull(role);
            Assert.IsTrue(role!.SystemPromptTemplate.StartsWith("Custom coder"));
            Assert.AreEqual(4, registry.GetAllRoles().Count);
        }
        finally { Directory.Delete(workspace, true); }
    }

    [TestMethod]
    public void TestLoadFromWorkspace_目录不存在返回0()
    {
        var registry = new SubAgentRoleRegistry();
        Assert.AreEqual(0, registry.LoadFromWorkspace(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))));
    }

    [TestMethod]
    public void TestLoadFromWorkspace_无效文件被容忍()
    {
        var workspace = CreateTempWorkspace();
        try
        {
            var rolesDir = Path.Combine(workspace, ".luban-agent", "roles");
            Directory.CreateDirectory(rolesDir);
            File.WriteAllText(Path.Combine(rolesDir, "bad.json"), "not json");
            File.WriteAllText(Path.Combine(rolesDir, "noname.json"), """{ "systemPromptTemplate": "x {prompt}" }""");

            var registry = new SubAgentRoleRegistry();
            Assert.AreEqual(0, registry.LoadFromWorkspace(workspace));
        }
        finally { Directory.Delete(workspace, true); }
    }
}
