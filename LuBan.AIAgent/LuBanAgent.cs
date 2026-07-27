using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace LuBan.AIAgent;

public class LuBanAgent
{
    private readonly ChatClientAgent _innerAgent;
    private AgentSession? _session;
    private readonly SemaphoreSlim _sessionLock = new(1, 1);

    public LuBanAgent(ChatClientAgent innerAgent)
    {
        ArgumentNullException.ThrowIfNull(innerAgent);
        _innerAgent = innerAgent;
    }

    public string Id => _innerAgent.Id;
    public string Name => _innerAgent.Name ?? "LuBanAgent";
    public string? Description => _innerAgent.Description;

    private async Task<AgentSession> GetOrCreateSessionAsync(CancellationToken cancellationToken)
    {
        if (_session != null) return _session;
        
        await _sessionLock.WaitAsync(cancellationToken);
        try
        {
            _session ??= await _innerAgent.CreateSessionAsync(cancellationToken);
            return _session;
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    public async Task<AgentResponse> RunAsync(string input, CancellationToken cancellationToken = default)
    {
        var session = await GetOrCreateSessionAsync(cancellationToken);
        return await _innerAgent.RunAsync(input, session, cancellationToken: cancellationToken);
    }

    public async Task<AgentResponse> RunAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        var session = await GetOrCreateSessionAsync(cancellationToken);
        return await _innerAgent.RunAsync(messages, session, cancellationToken: cancellationToken);
    }

    public async IAsyncEnumerable<AgentResponseUpdate> RunStreamingAsync(
        string input,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var session = await GetOrCreateSessionAsync(cancellationToken);
        await foreach (var update in _innerAgent.RunStreamingAsync(input, session, cancellationToken: cancellationToken))
        {
            yield return update;
        }
    }

    public async IAsyncEnumerable<AgentResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var session = await GetOrCreateSessionAsync(cancellationToken);
        await foreach (var update in _innerAgent.RunStreamingAsync(messages, session, cancellationToken: cancellationToken))
        {
            yield return update;
        }
    }

    public async ValueTask<AgentSession> CreateSessionAsync(CancellationToken cancellationToken = default)
        => await _innerAgent.CreateSessionAsync(cancellationToken);
}