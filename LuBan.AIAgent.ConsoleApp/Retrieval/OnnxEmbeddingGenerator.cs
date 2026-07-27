using Microsoft.Extensions.AI;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;

namespace LuBan.AIAgent.ConsoleApp.Retrieval;

/// <summary>
/// ONNX 嵌入生成器（本地推理）
/// </summary>
public class OnnxEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>, IDisposable
{
    private const int MaxTokens = 512;
    private readonly string _modelDir;
    private readonly EmbeddingModelSpec _spec;
    private readonly object _initLock = new();
    private InferenceSession? _session;
    private Tokenizer? _tokenizer;

    /// <summary>
    /// 创建 ONNX 嵌入生成器
    /// </summary>
    public OnnxEmbeddingGenerator(string modelDir, EmbeddingModelSpec spec)
    {
        _modelDir = modelDir;
        _spec = spec;
    }

    private (InferenceSession session, Tokenizer tokenizer) EnsureLoaded()
    {
        if (_session != null && _tokenizer != null) return (_session, _tokenizer);
        lock (_initLock)
        {
            // TODO: Microsoft.ML.Tokenizers 2.0.0 API 变更，需要确认正确的 tokenizer 加载方式
            // 当前使用反射尝试加载 BertTokenizer
            var tokenizerPath = Path.Combine(_modelDir, "tokenizer.json");
            var bertTokenizerType = typeof(Tokenizer).Assembly.GetType("Microsoft.ML.Tokenizers.BertTokenizer");
            if (bertTokenizerType != null)
            {
                var loadMethod = bertTokenizerType.GetMethod("Load", new[] { typeof(string) });
                if (loadMethod != null)
                {
                    _tokenizer = loadMethod.Invoke(null, new object[] { tokenizerPath }) as Tokenizer;
                }
            }
            if (_tokenizer == null)
                throw new NotSupportedException("无法加载 tokenizer，请检查 Microsoft.ML.Tokenizers 版本和 API");
            _session ??= new InferenceSession(Path.Combine(_modelDir, "model.onnx"));
            return (_session, _tokenizer);
        }
    }

    /// <inheritdoc />
    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        var (session, tokenizer) = EnsureLoaded();
        var texts = values.ToList();
        var result = new GeneratedEmbeddings<Embedding<float>>();

        foreach (var batch in texts.Chunk(32))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var encoded = batch.Select(t =>
            {
                var ids = tokenizer.EncodeToIds(t);
                if (ids.Count > MaxTokens)
                    ids = ids.Take(MaxTokens - 1).Append(ids[^1]).ToList();
                return (IReadOnlyList<int>)ids;
            }).ToList();

            int maxLen = encoded.Max(e => e.Count);
            var inputIds = new long[batch.Length * maxLen];
            var attention = new long[batch.Length * maxLen];
            var tokenTypes = new long[batch.Length * maxLen];
            for (int i = 0; i < batch.Length; i++)
                for (int j = 0; j < encoded[i].Count; j++)
                {
                    inputIds[i * maxLen + j] = encoded[i][j];
                    attention[i * maxLen + j] = 1;
                }
            var dims = new[] { batch.Length, maxLen };
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", new DenseTensor<long>(inputIds, dims)),
                NamedOnnxValue.CreateFromTensor("attention_mask", new DenseTensor<long>(attention, dims)),
            };
            if (session.InputMetadata.ContainsKey("token_type_ids"))
                inputs.Add(NamedOnnxValue.CreateFromTensor("token_type_ids", new DenseTensor<long>(tokenTypes, dims)));

            using var outputs = session.Run(inputs);
            var hidden = outputs.First(o => o.Name == session.OutputNames[0]).AsTensor<float>();
            int hiddenDim = hidden.Dimensions[2];
            for (int i = 0; i < batch.Length; i++)
            {
                int len = encoded[i].Count;
                var vec = new float[hiddenDim];
                for (int j = 0; j < len; j++)
                    for (int d = 0; d < hiddenDim; d++)
                        vec[d] += hidden[i, j, d];
                for (int d = 0; d < hiddenDim; d++) vec[d] /= len;
                Normalize(vec);
                result.Add(new Embedding<float>(vec));
            }
        }
        return Task.FromResult(result);
    }

    private static void Normalize(float[] v)
    {
        double norm = 0;
        foreach (var x in v) norm += x * x;
        norm = Math.Sqrt(norm);
        if (norm > 0) for (int i = 0; i < v.Length; i++) v[i] = (float)(v[i] / norm);
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceType == typeof(EmbeddingGeneratorMetadata))
            return new EmbeddingGeneratorMetadata(_spec.ModelId, null, null, _spec.Dimension);
        return serviceType.IsInstanceOfType(this) ? this : null;
    }

    /// <inheritdoc />
    public void Dispose() => _session?.Dispose();
}
