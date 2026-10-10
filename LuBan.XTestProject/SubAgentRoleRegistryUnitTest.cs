using LuBan.AIAgent.Orchestration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class SubAgentRoleRegistryUnitTest
{
    [TestMethod]
    public void BuiltIn_Roles_Include_Log_Group()
    {
        var registry = new SubAgentRoleRegistry();

        foreach (var name in new[] { "analyst", "researcher", "coder", "writer" })
        {
            var role = registry.GetRole(name);
            Assert.IsNotNull(role, name);
            CollectionAssert.Contains(role.DefaultToolGroups, "log", $"{name} 应包含 log 工具组");
        }
    }
}
