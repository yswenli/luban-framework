/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent.MCP
文件名： MCPToolPlugin
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/29
*描述：将已连接 MCP 客户端的工具并入 Agent 工具注册表
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace LuBan.AIAgent.MCP;

/// <summary>
/// 将已连接 MCP 客户端的工具并入 Agent 工具注册表（GroupName="mcp"）
/// </summary>
public class MCPToolPlugin : ILuBanToolPlugin
{
    /// <inheritdoc />
    public string GroupName => "mcp";

    /// <inheritdoc />
    public string? Description => "MCP 外部服务器工具";

    /// <inheritdoc />
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp)
    {
        var registry = sp.GetService<MCPRegistry>();
        if (registry == null) return Array.Empty<AIFunction>();

        var tools = new List<AIFunction>();
        foreach (var client in registry.GetAll().Where(c => c.IsConnected))
        {
            IEnumerable<MCPTool> mcpTools;
            try
            {
                mcpTools = client.ListToolsAsync().GetAwaiter().GetResult();
            }
            catch
            {
                continue;
            }

            foreach (var tool in mcpTools)
            {
                tools.Add(new MCPToolAIFunction(client, tool));
            }
        }
        return tools;
    }

    /// <inheritdoc />
    public bool IsEnabled(LuBanAgentOptions options) => true;

    /// <summary>
    /// MCP 工具包装为 AIFunction，名称加 mcp_{client}_{tool} 前缀避免冲突
    /// </summary>
    private sealed class MCPToolAIFunction : AIFunction
    {
        private readonly IMCPClient _client;
        private readonly MCPTool _tool;
        private readonly string _name;

        public MCPToolAIFunction(IMCPClient client, MCPTool tool)
        {
            _client = client;
            _tool = tool;
            _name = Sanitize($"mcp_{client.Name}_{tool.Name}");
        }

        public override string Name => _name;

        public override string Description => _tool.Description;

        protected override async ValueTask<object?> InvokeCoreAsync(
            AIFunctionArguments arguments, CancellationToken cancellationToken)
        {
            var result = await _client.CallToolAsync(
                _tool.Name,
                new Dictionary<string, object?>(arguments),
                cancellationToken);
            return result.Success ? result.Content : $"MCP 工具调用失败: {result.Error}";
        }

        private static string Sanitize(string name)
            => new(name.Select(c => char.IsLetterOrDigit(c) || c is '_' or '-' ? c : '_').ToArray());
    }
}
