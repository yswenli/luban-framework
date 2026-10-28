/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Web.Core.AIAgent
*文件名： AIAgentExtension
*版本号： V1.0.0.0
*唯一标识：LuBan Agent 注册扩展
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/28
*描述：按解析结果注册 LuBan Agent 服务，未启用或初始化失败时只降级不抛
*
*****************************************************************************/

using LuBan.AIAgent;

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
    /// <returns></returns>
    public static IServiceCollection AddAIAgent(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            var agentResolution = LuBanAgentConfigResolver.Resolve(configuration);
            if (agentResolution.Enabled && agentResolution.Configuration is { } agentConfig)
            {
                ConsoleUtil.WriteLineWithCount("正在初始化 AIAgent", color: ConsoleColor.Green);
                services.AddLuBanAgentDefaults(agentConfig);
                services.AddLuBanAgentHost();
                services.AddLuBanAgentOpenAI(agentConfig);

                if (!ReferenceEquals(agentConfig, configuration))
                {
                    // D-D：让运行期直接注入 IConfiguration 的 Agent 服务看到同一份值
                    services.Replace(ServiceDescriptor.Singleton<IConfiguration>(agentConfig));
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
