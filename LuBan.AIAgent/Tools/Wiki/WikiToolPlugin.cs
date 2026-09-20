/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.Wiki
*文件名： WikiToolPlugin
*版本号： V1.0.0.0
*唯一标识：4d6b9e28-1f70-4a3c-b5e8-2c9a7f1d6e03
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：wiki 工具插件（opt-in，工具组名 "wiki"）
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using LuBan.AIAgent.Wiki;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tools.Wiki;

/// <summary>wiki 工具插件（opt-in，工具组名 "wiki"）。</summary>
public class WikiToolPlugin : ILuBanToolPlugin
{
    private readonly IOptions<LuBanAgentOptions> _options;

    /// <summary>构造。</summary>
    public WikiToolPlugin(IOptions<LuBanAgentOptions> options) => _options = options;

    /// <inheritdoc />
    public string GroupName => "wiki";

    /// <inheritdoc />
    public string? Description => "LLM Wiki 知识库：读取/写入/删除 wiki 页面并维护索引";

    /// <inheritdoc />
    public bool IsOptIn => true;

    /// <inheritdoc />
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider serviceProvider, ToolGroupOptions? toolsOptions = null)
    {
        var wiki = serviceProvider.GetService<IWikiService>();
        if (wiki == null) return Array.Empty<AIFunction>();

        var options = toolsOptions?.Wiki ?? _options.Value.Tools.Wiki;
        if (!options.Enabled) return Array.Empty<AIFunction>();

        var confirmation = serviceProvider.GetRequiredService<IToolConfirmationService>();
        var group = new WikiToolGroup(wiki, options, confirmation);

        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(group, nameof(WikiToolGroup.ReadIndexAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(WikiToolGroup.ReadPageAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(WikiToolGroup.SavePageAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(WikiToolGroup.DeletePageAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(WikiToolGroup.SearchAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(WikiToolGroup.LintAsync))
        };
    }

    /// <inheritdoc />
    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.Wiki.Enabled;
}