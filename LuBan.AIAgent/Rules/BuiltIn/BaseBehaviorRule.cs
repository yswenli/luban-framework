using LuBan.AIAgent.Configuration;
using Microsoft.Extensions.Options;

/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Rules.BuiltIn
*文件名： BaseBehaviorRule
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/7
*描述：基础行为规则，提供回复语言与记忆使用时机的引导文本
*
*****************************************************************************/
namespace LuBan.AIAgent.Rules.BuiltIn;

/// <summary>
/// 基础行为规则：提供回复语言与记忆使用时机的静态引导文本，经 IContentRule 进入系统提示词
/// </summary>
public class BaseBehaviorRule : RuleBase, IContentRule
{
    private readonly IOptions<LuBanAgentOptions> _options;

    /// <summary>
    /// 创建基础行为规则
    /// </summary>
    /// <param name="options">LuBan Agent 配置选项</param>
    public BaseBehaviorRule(IOptions<LuBanAgentOptions> options)
    {
        _options = options;
    }

    /// <summary>
    /// 内置默认引导文本
    /// </summary>
    public const string DefaultContent = """
        ## 语言
        - 默认使用中文回复，除非用户明确要求使用其他语言。

        ## 记忆使用
        - 当用户分享偏好、事实或长期目标时，调用 memory_save 将其保存为长期记忆（偏好可保存为 global 类别以便跨工作区使用）。
        - 当问题可能依赖跨会话的上下文时，先调用 memory_search 检索相关记忆。
        - 检索到相关记忆时，优先参考记忆内容再回答。
        """;

    /// <summary>
    /// 规则 ID
    /// </summary>
    public override string Id => "base-behavior";

    /// <summary>
    /// 规则名称
    /// </summary>
    public override string Name => "基础行为规则";

    /// <summary>
    /// 规则描述
    /// </summary>
    public override string Description => "定义回复语言与记忆使用时机";

    /// <summary>
    /// 规则优先级
    /// </summary>
    public override int Priority => 100;

    /// <summary>
    /// 引导内容（appsettings 覆盖 > 内置默认）
    /// </summary>
    public string Content => string.IsNullOrWhiteSpace(_options.Value.BaseBehavior)
        ? DefaultContent
        : _options.Value.BaseBehavior;

    /// <summary>
    /// 纯内容规则，不参与评估
    /// </summary>
    public override bool IsApplicable(RuleContext context) => false;

    /// <inheritdoc />
    public override Task<RuleResult> ExecuteAsync(RuleContext context) => Task.FromResult(RuleResult.AllowResult());
}
