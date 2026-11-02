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
*文件名： SubAgentRoleRegistryUnitTest
*版本号： V1.0.0.0
*唯一标识：4c2d3c30-e401-4c98-bece-f0ee611d26c8
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/10 11:13:13
*描述：子 Agent 内置角色注册表单元测试
*
*=================================================
*修改标记
*修改时间：2026/10/10 11:13:13
*修改人： yswenli
*版本号： V1.0.0.0
*描述：子 Agent 内置角色注册表单元测试
*
*****************************************************************************/

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
