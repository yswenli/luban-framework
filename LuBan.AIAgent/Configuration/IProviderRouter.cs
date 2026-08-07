using Microsoft.Extensions.AI;

namespace LuBan.AIAgent.Configuration;

public interface IProviderRouter
{
    IChatClient CreateChatClient(string? providerModel = null);
    IReadOnlyList<ProviderInfo> GetAvailableProviders();
}

public record ProviderInfo(string Name, string DisplayName, string[] Models);
