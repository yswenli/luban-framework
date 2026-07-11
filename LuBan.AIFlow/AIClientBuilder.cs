/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.AI
*文件名： AIClientBuilder
*版本号： V1.0.0.0
*唯一标识：39bb7a24-ef42-491b-ab11-8db83aeb0fa1
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/6/10 11:12:42
*描述：AIClientBuilder 用于配置并创建AIClient实例的构建器。
*
*=================================================
*修改标记
*修改时间：2025/6/10 11:12:42
*修改人： yswenli
*版本号： V1.0.0.0
*描述：AIClientBuilder 用于配置并创建AIClient实例的构建器。
*
*****************************************************************************/
namespace LuBan.AIFlow;

/// <summary>
/// AIClientBuilder 用于配置并创建AIClient实例的构建器。
/// </summary>
/// <remarks>
/// 使用此类可配置 <see cref="AIOptions"/>，并创建完全初始化的 <see cref="IAIClient"/>。
/// 该构建器模式允许在实例化前以流式方式配置客户端。
/// </remarks>
public class AIClientBuilder
{
    /// <summary>
    /// AI 客户端的配置选项。
    /// </summary>
    private readonly AIOptions _options;

    /// <summary>
    /// 初始化 <see cref="AIClientBuilder"/> 类的新实例。
    /// </summary>
    /// <param name="options">AI 客户端的配置选项。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="options"/> 为 null 时抛出。</exception>
    public AIClientBuilder(AIOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// 配置 AI 客户端选项。
    /// </summary>
    /// <param name="configure">用于配置 <see cref="AIOptions"/> 的委托。</param>
    /// <returns>返回当前 <see cref="AIClientBuilder"/> 实例以支持链式调用。</returns>
    public AIClientBuilder Configure(Action<AIOptions> configure)
    {
        configure?.Invoke(_options);
        return this;
    }

    /// <summary>
    /// 初始化 <see cref="AIClientBuilder"/> 类的新实例。
    /// </summary>    
    public AIClientBuilder() : this(ConfigUtil.Read<AIOptions>() ?? throw new Exception("找不到ai配置"))
    {
    }

    /// <summary>
    /// 构建并返回一个新的 AI 客户端实例。
    /// </summary>
    /// <returns>已配置的 AI 客户端实例。</returns>
    /// <exception cref="ArgumentException">当 <see cref="AIOptions.AIType"/> 不是有效的 AI 类型时抛出。</exception>
    public IAIClient Build()
    {
        return _options.AIType switch
        {
            EnumAIType.RagFlow => new RagFlowAIClient(_options),
            EnumAIType.Dify => new DifyAIClient(_options),
            EnumAIType.Coze => new CozeAIClient(_options),
            _ => throw new ArgumentException($"不支持的 AI 类型: {_options.AIType}")
        };
    }
}
