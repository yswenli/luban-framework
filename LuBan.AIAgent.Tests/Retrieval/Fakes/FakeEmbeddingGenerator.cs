using Microsoft.Extensions.AI;

namespace LuBan.AIAgent.Tests.Retrieval.Fakes;

public class FakeEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    public int CallCount { get; private set; }
    public int EmbeddedCount { get; private set; }

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        CallCount++;
        var list = values.ToList();
        EmbeddedCount += list.Count;
        var result = new GeneratedEmbeddings<Embedding<float>>();
        foreach (var v in list) result.Add(new Embedding<float>(VectorFor(v)));
        return Task.FromResult(result);
    }

    public static float[] VectorFor(string s)
    {
        var v = new float[4];
        if (s.Contains("auth", StringComparison.OrdinalIgnoreCase)) v[0] = 1;
        if (s.Contains("payment", StringComparison.OrdinalIgnoreCase)) v[1] = 1;
        if (s.Contains("user", StringComparison.OrdinalIgnoreCase)) v[2] = 1;
        if (v.All(x => x == 0)) v[3] = 1;
        return v;
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;
    public void Dispose() { }
}
