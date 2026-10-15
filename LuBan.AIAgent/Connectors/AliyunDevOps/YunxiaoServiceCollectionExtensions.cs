/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Connectors.AliyunDevOps
*文件名： YunxiaoServiceCollectionExtensions
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：云效连接器与进程内 MCP 客户端的 DI 注册扩展（PRD §3.10 / §4.11）
*
*****************************************************************************/
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LuBan.AIAgent.Connectors.AliyunDevOps;

/// <summary>
/// 云效连接器与进程内 MCP 客户端的 DI 注册扩展。
/// </summary>
/// <remarks>
/// <para>
/// 注册项（均为单例，构造链不含 Scoped 服务，可在 <c>ValidateScopes=true</c> 下通过构建校验）：
/// <see cref="YunxiaoApiOptions"/>、<see cref="IYunxiaoCredentialProvider"/>（宿主可先注册覆盖）、
/// <see cref="YunxiaoConnector"/>、<see cref="CodeupClient"/>、<see cref="FlowClient"/>、
/// <see cref="Mcp.YunxiaoMCPClient"/>。
/// </para>
/// <para>
/// 注意：<b>不</b>把 <c>YunxiaoMCPClient</c> 注册为 <c>IMCPClient</c>。按 PRD §4.11，MCP 生命周期
/// 跟随插件——插件 Enabled 时由宿主把它注册进 <c>MCPRegistry</c>，Disabled/Uninstalled 时反注册。
/// </para>
/// </remarks>
public static class YunxiaoServiceCollectionExtensions
{
    /// <summary>
    /// 云效配置节名。
    /// </summary>
    public const string ConfigurationSectionName = "LuBanAgent:Connectors:Yunxiao";

    /// <summary>
    /// 注册云效连接器与进程内 MCP 客户端。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="configuration">配置。</param>
    /// <returns>服务集合（链式）。</returns>
    public static IServiceCollection AddYunxiaoConnector(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(ConfigurationSectionName);

        services.TryAddSingleton(_ =>
        {
            var options = new YunxiaoApiOptions();
            section.Bind(options);
            return options;
        });

        // 默认凭据提供者读配置；宿主（如 LubanAgentCodex）可在调用 AddLuBanAgent 后注册自己的实现覆盖。
        // 注意：必须用工厂显式传入 configuration，不能按实现类型注册——否则容器需要解析 IConfiguration，
        // 而宿主（含 AddLuBanAgentDefaults 的最小宿主与 AgentHostBuilder 的 ValidateOnBuild）未必把
        // IConfiguration 本身注册为服务，会直接导致 "Unable to resolve service for type IConfiguration"。
        services.TryAddSingleton<IYunxiaoCredentialProvider>(
            _ => new ConfigurationYunxiaoCredentialProvider(configuration));

        services.TryAddSingleton<YunxiaoConnector>();
        services.TryAddSingleton<CodeupClient>();
        services.TryAddSingleton<FlowClient>();
        services.TryAddSingleton<Mcp.YunxiaoMCPClient>();

        return services;
    }
}

/// <summary>
/// 基于配置的云效凭据提供者（默认实现，便于本地联调）。
/// </summary>
/// <remarks>
/// 读取 <c>LuBanAgent:Connectors:Yunxiao:AccessToken</c>；未配置时返回 <c>null</c>，连接器会给出明确错误。
/// <b>不缓存、不日志回显</b>明文 PAT（PRD §4.9.3）。
/// </remarks>
internal sealed class ConfigurationYunxiaoCredentialProvider : IYunxiaoCredentialProvider
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// 创建配置凭据提供者。
    /// </summary>
    /// <param name="configuration">配置。</param>
    public ConfigurationYunxiaoCredentialProvider(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc />
    public Task<string?> GetPatAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_configuration[$"{YunxiaoServiceCollectionExtensions.ConfigurationSectionName}:AccessToken"]);
}