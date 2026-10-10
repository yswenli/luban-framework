/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.Log
*文件名： LogToolPlugin
*版本号： V1.0.0.0
*唯一标识：8c2f5d41-6b93-4e7a-9f10-3a8d2e6c4b57
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/10
*描述：大日志分析工具插件（非 opt-in，工具组名 "log"）
*
*****************************************************************************/
using LuBan.AIAgent.Abstractions;
using LuBan.AIAgent.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Tools.Log;

/// <summary>
/// 大日志分析工具插件（非 opt-in，工具组名 "log"）。
/// <para>
/// 本插件注入专用 <see cref="IOptions{LogToolOptions}"/> 配置，忽略 <c>toolsOptions</c> 运行期覆盖参数：
/// 该参数为空时回落到宿主配置、非空时按框架「整组替换」语义取默认值，两种都不是「用注入的 log 配置」，
/// 会造成配置遮蔽，故此处一律以注入配置为准。
/// </para>
/// </summary>
public class LogToolPlugin : ILuBanToolPlugin
{
    private readonly LogToolOptions _options;

    /// <summary>构造。</summary>
    /// <param name="options">大日志分析专用配置。</param>
    public LogToolPlugin(IOptions<LogToolOptions> options)
    {
        _options = options?.Value ?? new LogToolOptions();
        _options.ApplyHardCaps();
    }

    /// <inheritdoc />
    public string GroupName => "log";

    /// <inheritdoc />
    public string? Description => "大日志文件分析：聚合概览、正则检索、按行切片（恒定内存）";

    /// <inheritdoc />
    public bool IsOptIn => false;

    /// <inheritdoc />
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp, ToolGroupOptions? toolsOptions = null)
    {
        if (!_options.Enabled) return Array.Empty<AIFunction>();

        var aggregator = sp.GetService<ILogAggregator>();
        if (aggregator == null) return Array.Empty<AIFunction>();

        var confirmation = sp.GetRequiredService<IToolConfirmationService>();
        var group = new LogToolGroup(aggregator, _options, confirmation);

        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(group, nameof(LogToolGroup.LogStatsAsync), name: nameof(LogToolGroup.LogStatsAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(LogToolGroup.LogGrepAsync), name: nameof(LogToolGroup.LogGrepAsync)),
            AIFunctionFactoryHelper.Create(group, nameof(LogToolGroup.LogSliceAsync), name: nameof(LogToolGroup.LogSliceAsync))
        };
    }

    /// <inheritdoc />
    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.Log.Enabled;
}
