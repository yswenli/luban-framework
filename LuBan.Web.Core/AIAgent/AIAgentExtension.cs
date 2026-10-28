/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core.AIAgent
*文件名： AIAgentExtension
*版本号： V1.0.0.0
*唯一标识：5c543be0-947c-43ed-9967-96af81b683f1
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/28
*描述：按解析结果注册 LuBan Agent 服务，未启用或初始化失败时只降级不抛
*
*****************************************************************************/

using LuBan.AIAgent;
using LuBan.AIAgent.Hosting;

namespace LuBan.Web.Core.AIAgent;

/// <summary>
/// AIAgent 扩展方法
/// </summary>
public static class AIAgentExtension
{
    /// <summary>
    /// 添加 AIAgent 服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="contentRootPath">站点内容根；用于把相对的 <c>WorkspaceRoot</c> 解析成绝对路径，
    /// 为空时回退到程序目录（非 Web 宿主 / 单测）</param>
    /// <returns></returns>
    public static IServiceCollection AddAIAgent(
        this IServiceCollection services,
        IConfiguration configuration,
        string? contentRootPath = null)
    {
        try
        {
            var agentResolution = LuBanAgentConfigResolver.Resolve(configuration);
            if (agentResolution.Enabled && agentResolution.Configuration is { } agentConfig)
            {
                // W1：注册前把相对 WorkspaceRoot 归一化为绝对路径，并补齐缺省的 DefaultModel
                var effectiveConfig = LuBanAgentConfigResolver.CreateEffectiveConfiguration(agentConfig, contentRootPath);

                ConsoleUtil.WriteLineWithCount("正在初始化 AIAgent", color: ConsoleColor.Green);
                services.AddLuBanAgentDefaults(effectiveConfig);
                // 用户隔离：把 JWT 用户身份接入会话运行时（默认实现恒为 local）
                services.TryAddScoped<IAgentUserResolver, WebAgentUserResolver>();
                services.AddLuBanAgentHost();
                services.AddLuBanAgentOpenAI(effectiveConfig);

                if (!ReferenceEquals(effectiveConfig, configuration))
                {
                    // D-D：让运行期直接注入 IConfiguration 的 Agent 服务看到同一份（已归一化的）值
                    services.Replace(ServiceDescriptor.Singleton<IConfiguration>(effectiveConfig));
                }
            }
            else
            {
                ConsoleUtil.WriteLine($"LuBan Agent 未启用：{agentResolution.DisabledReason}", color: ConsoleColor.Yellow);
            }
        }
        catch (Exception ex)
        {
            // Agent 属可选能力：初始化失败只降级为不启用，不得拖垮整个 Web 应用
            ConsoleUtil.WriteLine($"LuBan Agent 初始化失败，已跳过：{ex}", color: ConsoleColor.Yellow);
        }
        return services;
    }
}
