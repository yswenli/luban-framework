namespace LuBan.AIAgent.Hosting;

/// <summary>解析当前会话归属用户。</summary>
public interface IAgentUserResolver
{
    /// <summary>返回用户标识（不得返回 null 或空串）。</summary>
    string ResolveUserId(IServiceProvider services);
}