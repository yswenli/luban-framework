/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AIAgent
*文件名： LuBanAgentHostExtensions
*版本号： V1.0.0.0
*唯一标识：48d62835-58e8-4fd9-9e43-591bee987786
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/26
*描述：Agent 宿主注册扩展：AddLuBanAgentHost / AddLuBanAgentOpenAI
*
*****************************************************************************/

using LuBan.AIAgent.Attachments.Ingestion;

using Microsoft.Extensions.Options;

namespace LuBan.AIAgent;

/// <summary>Agent 宿主注册扩展。</summary>
public static class LuBanAgentHostExtensions
{
    /// <summary>
    /// 注册 Agent 宿主（<see cref="IAgentHost"/>）及其默认接缝实现。
    /// 需先调用 <c>AddLuBanAgent</c> 或 <c>AddLuBanAgentDefaults</c>。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <returns>服务集合。</returns>
    /// <remarks>
    /// <para>
    /// 默认注册全部以 <c>TryAdd</c> 实现，宿主可在调用本方法前注册自定义实现以覆盖。
    /// </para>
    /// <list type="bullet">
    /// <item><description>SessionRuntimeManager（Singleton）：按会话维护 scope 与运行循环。</description></item>
    /// <item><description>IAgentHost（Scoped）：每会话作用域内创建独立宿主实例。</description></item>
    /// <item><description>IAgentProfile（Scoped）：默认通用助手 Profile。</description></item>
    /// <item><description>IAgentWorkspaceResolver（Scoped）：单工作区解析（读 LuBanAgent:WorkspaceRoot）。</description></item>
    /// <item><description>IAgentProfileResolver（Scoped）：恒返回注册的 Profile。</description></item>
    /// <item><description>IAgentUserResolver（Scoped）：读 IUserContext.UserId，回退 "local"。</description></item>
    /// <item><description>IAgentModelSelection（Scoped）：读 LuBanAgent:DefaultModel。</description></item>
    /// <item><description>IModelCapabilityGate（Scoped）：默认放行所有附件。</description></item>
    /// <item><description>IAgentAttachmentOrchestrator（Scoped）：默认逐文件处理。</description></item>
    /// <item><description>IAgentAttachmentSourceResolver（Scoped）：默认解析 URL / 本地路径 / 相对路径。</description></item>
    /// <item><description>IToolConfirmationBroker（Singleton）：进程内工具确认登记簿。</description></item>
    /// <item><description>ISessionRuntimeInitializer（Scoped）：空实现，宿主可替换为产品初始化。</description></item>
    /// <item><description>ICommandCatalog（Singleton）：斜杠命令清单。</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddLuBanAgentHost(this IServiceCollection services)
    {
        services.TryAddSingleton<SessionRuntimeManager>();

        services.TryAddScoped<IAgentHost, AgentHost>();
        services.TryAddScoped<IAgentProfile, DefaultAgentProfile>();
        services.TryAddScoped<IAgentWorkspaceResolver>(sp =>
        {
            // 注册期读不到 IsolateWorkspaceByUser（配置单例稍后才被替换），故「用时求值」；
            // 未开启隔离时保持单工作区语义（CLI / Codex 行为不变）。
            var options = sp.GetRequiredService<IOptions<LuBanAgentOptions>>();
            if (!options.Value.IsolateWorkspaceByUser)
                return new SingleWorkspaceResolver(options);

            return new PerUserWorkspaceResolver(
                options,
                sp.GetRequiredService<IAgentUserResolver>(),
                sp);
        });
        services.TryAddScoped<IAgentProfileResolver>(sp =>
            new DefaultAgentProfileResolver(sp.GetRequiredService<IAgentProfile>()));
        services.TryAddScoped<IAgentUserResolver, DefaultAgentUserResolver>();
        services.TryAddScoped<IAgentModelSelection, DefaultAgentModelSelection>();
        services.TryAddScoped<IModelCapabilityGate, AllowAllModelCapabilityGate>();
        services.TryAddScoped<IAgentAttachmentOrchestrator, DefaultAgentAttachmentOrchestrator>();
        services.TryAddSingleton<IAgentAttachmentSourceResolver>(sp =>
            new DefaultAttachmentSourceResolver(sp.GetRequiredService<IOptions<LuBanAgentOptions>>()));
        // broker 必须 Singleton：SSE 请求与确认请求是两个不同的 HTTP scope
        services.TryAddSingleton<IToolConfirmationBroker>(sp =>
            new DefaultToolConfirmationBroker(sp.GetRequiredService<IOptions<LuBanAgentOptions>>()));
        services.TryAddScoped<ISessionRuntimeInitializer, NoOpSessionRuntimeInitializer>();
        services.TryAddSingleton<ICommandCatalog, CommandCatalog>();

        return services;
    }
}