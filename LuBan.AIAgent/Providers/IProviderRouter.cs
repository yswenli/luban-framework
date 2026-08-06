using Microsoft.Extensions.AI;

namespace LuBan.AIAgent.Providers;

public interface IProviderRouter
{
    IChatClient CreateChatClient(string? providerModel = null);
    IReadOnlyList<ProviderInfo> GetAvailableProviders();
}

public record ProviderInfo(string Name, string DisplayName, string[] Models);
