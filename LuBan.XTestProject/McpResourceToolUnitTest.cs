/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： McpResourceToolUnitTest
*创建时间：2026/10/9
*描述：验证 MCP 资源列表/读取工具
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.MCP;
using LuBan.AIAgent.MCP.BuiltIn;

namespace LuBan.XTestProject;

[TestClass]
public class McpResourceToolUnitTest
{
    [TestMethod]
    public async Task ListResources_ForUnsupportedClient_ReturnsEnumerable()
    {
        // FileSystemMCPClient 若不支持 resources，应返回空集合而非异常
        var client = new FileSystemMCPClient();
        var resources = await client.ListResourcesAsync();
        Assert.IsNotNull(resources);
    }
}
