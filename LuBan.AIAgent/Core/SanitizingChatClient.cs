namespace LuBan.AIAgent.Core;

public class SanitizingChatClient : IChatClient
{
    private readonly IChatClient _inner;
    private readonly int _maxConsecutiveToolOnly;

    public SanitizingChatClient(IChatClient inner, int maxConsecutiveToolOnly = 8)
    {
        _inner = inner;
        _maxConsecutiveToolOnly = maxConsecutiveToolOnly;
    }

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sanitized = SanitizeMessages(messages);
            InjectLoopGuardIfNeeded(sanitized);
            return await _inner.GetResponseAsync(sanitized, options, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.Error("SanitizingChatClient.GetResponseAsync 失败", ex, options?.ModelId ?? "unknown");
            throw;
        }
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return StreamCoreAsync(messages, options, cancellationToken);
    }

    private async IAsyncEnumerable<ChatResponseUpdate> StreamCoreAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        List<ChatMessage> sanitized;
        try
        {
            sanitized = SanitizeMessages(messages);
            InjectLoopGuardIfNeeded(sanitized);
        }
        catch (Exception ex)
        {
            Logger.Error("SanitizingChatClient.StreamCoreAsync 消息清洗失败", ex, options?.ModelId ?? "unknown");
            throw;
        }

        await foreach (var update in _inner.GetStreamingResponseAsync(sanitized, options, cancellationToken))
        {
            yield return update;
        }
    }

    public object? GetService(Type serviceType, object? key = null)
        => _inner.GetService(serviceType, key);

    public void Dispose()
    {
        _inner.Dispose();
        GC.SuppressFinalize(this);
    }

    private static List<ChatMessage> SanitizeMessages(IEnumerable<ChatMessage> messages)
    {
        var result = new List<ChatMessage>();

        foreach (var msg in messages)
        {
            if (msg.Contents == null || msg.Contents.Count == 0)
                continue;

            var hasToolCalls = msg.Contents.OfType<FunctionCallContent>().Any();
            var hasToolResults = msg.Contents.OfType<FunctionResultContent>().Any();

            if (msg.Role == ChatRole.Assistant && hasToolCalls)
            {
                var textParts = msg.Contents
                    .OfType<TextContent>()
                    .Where(t => !string.IsNullOrEmpty(t.Text))
                    .ToList();

                var toolCalls = msg.Contents.OfType<FunctionCallContent>().ToList();

                var newContents = new List<AIContent>();
                newContents.AddRange(textParts.Cast<AIContent>());
                newContents.AddRange(toolCalls.Cast<AIContent>());

                result.Add(new ChatMessage(msg.Role, newContents)
                {
                    AdditionalProperties = msg.AdditionalProperties,
                    MessageId = msg.MessageId
                });
            }
            else if (msg.Role == ChatRole.Tool && hasToolResults)
            {
                var newContents = new List<AIContent>();
                foreach (var r in msg.Contents.OfType<FunctionResultContent>())
                {
                    if (IsEmptyResult(r))
                    {
                        newContents.Add(new FunctionResultContent(r.CallId, "(工具返回空结果)"));
                    }
                    else
                    {
                        newContents.Add(r);
                    }
                }

                result.Add(new ChatMessage(msg.Role, newContents)
                {
                    AdditionalProperties = msg.AdditionalProperties,
                    MessageId = msg.MessageId
                });
            }
            else
            {
                var validParts = msg.Contents
                    .Where(c => !IsEmptyContent(c))
                    .ToList();

                if (validParts.Count == 0)
                    continue;

                if (msg.Role == ChatRole.Assistant)
                {
                    var hasText = validParts.OfType<TextContent>().Any();
                    if (!hasText)
                        continue;
                }

                result.Add(new ChatMessage(msg.Role, validParts)
                {
                    AdditionalProperties = msg.AdditionalProperties,
                    MessageId = msg.MessageId
                });
            }
        }

        return result;
    }

    private static bool IsEmptyContent(AIContent content)
    {
        if (content is TextContent text)
            return string.IsNullOrWhiteSpace(text.Text);
        return false;
    }

    private static bool IsEmptyResult(FunctionResultContent result)
    {
        if (result.Result == null) return true;
        if (result.Result is string s) return string.IsNullOrWhiteSpace(s);
        return false;
    }

    private void InjectLoopGuardIfNeeded(List<ChatMessage> messages)
    {
        int consecutiveToolOnly = CountConsecutiveToolOnly(messages);

        if (consecutiveToolOnly >= _maxConsecutiveToolOnly)
        {
            messages.Add(new ChatMessage(ChatRole.System,
                "你已经连续多次调用了工具但没有给出最终回答。" +
                "请立即停止调用工具，根据已获取的结果直接给出最终回答。" +
                "如果某些工具调用失败，请忽略失败的部分，用已有信息回答。"));
        }
    }

    private static int CountConsecutiveToolOnly(List<ChatMessage> messages)
    {
        int count = 0;
        bool foundAssistant = false;

        for (int i = messages.Count - 1; i >= 0; i--)
        {
            var msg = messages[i];

            if (msg.Role == ChatRole.Tool)
                continue;

            if (msg.Role == ChatRole.Assistant)
            {
                bool hasToolCalls = msg.Contents?.OfType<FunctionCallContent>().Any() == true;
                bool hasText = msg.Contents?.OfType<TextContent>()
                    .Any(t => !string.IsNullOrWhiteSpace(t.Text)) == true;

                if (hasToolCalls && !hasText)
                {
                    count++;
                    foundAssistant = true;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        return foundAssistant ? count : 0;
    }
}
