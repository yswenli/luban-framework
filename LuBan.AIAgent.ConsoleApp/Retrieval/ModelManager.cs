using System.Net.Http.Headers;

namespace LuBan.AIAgent.ConsoleApp.Retrieval;

/// <summary>
/// 嵌入模型管理器（下载/校验/缓存）
/// </summary>
public class ModelManager
{
    private static readonly Lazy<HttpClient> _http = new(() => new HttpClient
    {
        Timeout = TimeSpan.FromMinutes(30),
        DefaultRequestVersion = new Version(2, 0)
    });
    private readonly EmbeddingModelSpec _spec;

    /// <summary>
    /// 创建模型管理器
    /// </summary>
    public ModelManager(EmbeddingModelSpec spec) => _spec = spec;

    /// <summary>
    /// 模型规格
    /// </summary>
    public EmbeddingModelSpec Spec => _spec;

    /// <summary>
    /// 模型目录
    /// </summary>
    public string ModelDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LuBan", "AIAgent", "models", _spec.ModelId);

    /// <summary>
    /// 检查模型是否就绪
    /// </summary>
    public bool IsModelReady()
    {
        var dir = ModelDirectory;
        return _spec.Files.All(f =>
        {
            var p = Path.Combine(dir, f.LocalName);
            return File.Exists(p) && new FileInfo(p).Length >= f.MinSizeBytes;
        });
    }

    /// <summary>
    /// 确保模型存在（缺失则下载）
    /// </summary>
    public async Task<bool> EnsureModelAsync(Action<string>? reportStatus = null, CancellationToken cancellationToken = default)
    {
        if (IsModelReady()) return true;
        Directory.CreateDirectory(ModelDirectory);
        foreach (var file in _spec.Files)
        {
            var ok = await DownloadWithFallbackAsync(file, reportStatus, cancellationToken);
            if (!ok) return false;
        }
        return IsModelReady();
    }

    private async Task<bool> DownloadWithFallbackAsync(ModelFileSpec file, Action<string>? report, CancellationToken ct)
    {
        foreach (var baseUrl in new[] { _spec.MirrorBase, _spec.RemoteBase })
        {
            try
            {
                await DownloadFileAsync(baseUrl + file.RemotePath, Path.Combine(ModelDirectory, file.LocalName), file.LocalName, report, ct);
                return true;
            }
            catch (Exception ex)
            {
                report?.Invoke($"{file.LocalName} 从 {new Uri(baseUrl).Host} 下载失败：{ex.Message}，尝试备用源…");
            }
        }
        return false;
    }

    private static async Task DownloadFileAsync(string url, string targetPath, string displayName, Action<string>? report, CancellationToken ct)
    {
        var tmpPath = targetPath + ".tmp";
        long existing = File.Exists(tmpPath) ? new FileInfo(tmpPath).Length : 0;
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (existing > 0) request.Headers.Range = new RangeHeaderValue(existing, null);
        using var response = await _http.Value.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (existing > 0 && response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            File.Delete(tmpPath); existing = 0;
        }
        response.EnsureSuccessStatusCode();
        long total = 0;
        var contentRange = response.Content.Headers.ContentRange;
        if (contentRange != null && contentRange.HasLength)
            total = contentRange.Length ?? 0;
        else if (response.Content.Headers.ContentLength.HasValue)
            total = response.Content.Headers.ContentLength.Value + existing;
        await using var input = await response.Content.ReadAsStreamAsync(ct);
        await using var output = new FileStream(tmpPath, existing > 0 ? FileMode.Append : FileMode.Create, FileAccess.Write);
        var buffer = new byte[81920];
        long received = existing;
        int read;
        while ((read = await input.ReadAsync(buffer, ct)) > 0)
        {
            await output.WriteAsync(buffer.AsMemory(0, read), ct);
            received += read;
            if (total > 0) report?.Invoke($"下载 {displayName}：{(received * 100.0 / total):F0}%（{received / 1048576}MB/{total / 1048576}MB）");
            else report?.Invoke($"下载 {displayName}：{received / 1048576}MB");
        }
        File.Move(tmpPath, targetPath, true);
    }
}
