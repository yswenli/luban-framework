namespace LuBan.AIAgent.Abstractions;

/// <summary>
/// 代理运行时接口，负责执行代理请求并返回结果
/// </summary>
public interface IAgentRuntime
{
    /// <summary>
    /// 异步执行代理请求
    /// </summary>
    /// <param name="request">代理请求对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>代理执行结果</returns>
    Task<AgentResult> ExecuteAsync(AgentRequest request, CancellationToken cancellationToken = default);
}
