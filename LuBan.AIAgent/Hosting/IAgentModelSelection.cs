namespace LuBan.AIAgent.Hosting;

/// <summary>当前选定的模型（宿主读取；产品可用自己的配置实现覆盖）。</summary>
public interface IAgentModelSelection
{
    /// <summary>Provider 标识（如 <c>openai</c>）。</summary>
    string? ProviderId { get; }

    /// <summary>模型标识（如 <c>gpt-4.1</c>）。</summary>
    string? ModelId { get; }

    /// <summary>限定名 <c>provider:model</c>；未选择时为空字符串。</summary>
    string QualifiedModelName { get; }
}