/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.AIAgent.MCP
*文件名： IMcpConfigValueResolver
*版本号： V1.0.0.0
*唯一标识：新建
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/19
*描述：MCP 配置值解析（凭据引用注入）—— 厂商无关的通用能力
*
*****************************************************************************/
namespace LuBan.AIAgent.MCP;

/// <summary>
/// MCP 配置值解析器：把配置里的<b>引用</b>（如 <c>$credential:connector:my:token</c>）解析为实际值。
/// </summary>
/// <remarks>
/// <para>
/// 目的：让外部 MCP（HTTP/stdio）可以「带凭据地连接」而<b>不必把令牌明文写进</b>
/// <c>.luban-agent/mcps/*.json</c>——配置文件只保存引用，真值由 <c>ICredentialStore</c> 提供。
/// </para>
/// <para>
/// 解析发生在客户端 <c>ConnectAsync</c>（异步），而非配置加载期，故不与同步构建路径冲突。
/// </para>
/// </remarks>
public interface IMcpConfigValueResolver
{
    /// <summary>
    /// 解析模板字符串。
    /// </summary>
    /// <param name="template">配置中的原始值；非引用时原样返回。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>解析后的值；引用无法解析（凭据缺失）时返回 <c>null</c>。</returns>
    Task<string?> ResolveAsync(string? template, CancellationToken cancellationToken = default);
}

/// <summary>
/// 空解析器：仅原样透传（不解析任何引用）。
/// </summary>
/// <remarks>
/// 供未接入凭据存储的场景（单测、最小宿主）作为缺省实现，保持既有构造签名可用。
/// </remarks>
public sealed class NullMcpConfigValueResolver : IMcpConfigValueResolver
{
    /// <summary>单例。</summary>
    public static NullMcpConfigValueResolver Instance { get; } = new();

    /// <inheritdoc />
    public Task<string?> ResolveAsync(string? template, CancellationToken cancellationToken = default)
        => Task.FromResult(template);
}