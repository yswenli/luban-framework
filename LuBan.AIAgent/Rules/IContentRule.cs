namespace LuBan.AIAgent.Rules;

/// <summary>
/// 提供内容文本的规则（如 base-behavior 引导文本）
/// </summary>
public interface IContentRule
{
    /// <summary>
    /// 规则内容文本
    /// </summary>
    string Content { get; }
}
