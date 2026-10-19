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
/// <see cref="YunxiaoConnector"/>、<see cref="CodeupClient"/>、<see cref="FlowClient"/>。
/// </para>
/// <para>
/// 注意：<b>不</b>把 <c>YunxiaoMCPClient</c> 注册进容器。按 PRD §4.11，MCP 生命周期跟随插件/工作区——
/// 由 <c>ConnectorRegistry</c> 从 <see cref="YunxiaoConnector.McpExposures"/> 按需派生并注册进会话的
/// <c>MCPRegistry</c>，停用/卸载时反注册。
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

        // 默认凭据提供者：优先读 ICredentialStore（宿主可覆盖的凭据存储），未命中再回退配置节点。
        // 注意：必须用工厂显式传入 configuration，不能按实现类型注册——否则容器需要解析 IConfiguration，
        // 而宿主（含 AddLuBanAgentDefaults 的最小宿主与 AgentHostBuilder 的 ValidateOnBuild）未必把
        // IConfiguration 本身注册为服务，会直接导致 "Unable to resolve service for type IConfiguration"。
        services.TryAddSingleton<IYunxiaoCredentialProvider>(
            sp => new ConfigurationYunxiaoCredentialProvider(configuration, sp.GetService<ICredentialStore>()));

        services.TryAddSingleton<YunxiaoConnector>(sp => new YunxiaoConnector(
            sp.GetRequiredService<YunxiaoApiOptions>(),
            sp.GetRequiredService<IYunxiaoCredentialProvider>(),
            sp.GetService<ProcessRunner>(),
            sp.GetService<PluginPermissionGuard>() ?? PluginPermissionGuard.Default));

        services.TryAddSingleton(sp => new CodeupClient(
            sp.GetRequiredService<YunxiaoApiOptions>(),
            sp.GetRequiredService<IYunxiaoCredentialProvider>(),
            sp.GetService<ProcessRunner>(),
            sp.GetService<PluginPermissionGuard>() ?? PluginPermissionGuard.Default));

        services.TryAddSingleton(sp => new FlowClient(
            sp.GetRequiredService<YunxiaoApiOptions>(),
            sp.GetRequiredService<IYunxiaoCredentialProvider>(),
            sp.GetService<PluginPermissionGuard>() ?? PluginPermissionGuard.Default));

        return services;
    }
}

/// <summary>
/// 基于配置的云效凭据提供者（默认实现，便于本地联调）。
/// </summary>
/// <remarks>
/// <para>
/// 依次尝试：① <see cref="ICredentialStore"/>（宿主可写凭据存储，PRD §3.7）；
/// ② 配置节点 <c>LuBanAgent:Connectors:Yunxiao:AccessToken</c>。均未命中时返回 <c>null</c>，
/// 连接器会给出明确错误。<b>不缓存、不日志回显</b>明文 PAT（PRD §4.9.3）。
/// </para>
/// </remarks>
internal sealed class ConfigurationYunxiaoCredentialProvider : IYunxiaoCredentialProvider
{
    private readonly IConfiguration _configuration;
    private readonly ICredentialStore? _credentialStore;

    /// <summary>
    /// 创建配置凭据提供者。
    /// </summary>
    /// <param name="configuration">配置。</param>
    /// <param name="credentialStore">凭据存储；为空时仅读配置节点。</param>
    public ConfigurationYunxiaoCredentialProvider(IConfiguration configuration, ICredentialStore? credentialStore = null)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _credentialStore = credentialStore;
    }

    /// <inheritdoc />
    public async Task<string?> GetPatAsync(CancellationToken cancellationToken = default)
    {
        if (_credentialStore is not null)
        {
            var stored = await _credentialStore.GetAsync(CredentialKeys.YunxiaoPat, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(stored))
                return stored;
        }

        return _configuration[$"{YunxiaoServiceCollectionExtensions.ConfigurationSectionName}:AccessToken"];
    }
}