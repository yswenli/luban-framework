/****************************************************************************
  _           ____                ______                     __          __        _
 | |         |  _ \              |  ____|                    \ \        / /       | |
 | |    _   _| |_) | __ _ _ __   | |__ _ __ __ _ _ __ ___   __\ \  /\  / /__  _ __| | __
 | |   | | | |  _ < / _` | '_ \  |  __| '__/ _` | '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ /
 | |___| |_| | |_) | (_| | | | |_| |  | | | (_| | | | | | |  __/\  /\  / (_) | |  |   <
 |______\__,_|____/ \__,_|_| |_(_)_|  |_|  \__,_|_| |_| |_|\___| \/  \/ \___/|_|  |_|\_\
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Attachments.Ingestion
*文件名： DefaultAttachmentSourceResolver
*版本号： V1.0.0.0
*唯一标识：e6c0d9a4-7b32-4f18-9d6a-2c8e5b3f10a7
*当前的用户域：WALLE
*创建人：yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/29
*描述：默认附件来源解析：URL 下载落盘、本地文件就地读取、相对路径先工作区后内容根。
*
*=================================================
*修改标记
*修改时间：2026/10/29
*修改人： yswenli
*版本号： V1.0.0.0
*描述：默认附件来源解析：URL 下载落盘、本地文件就地读取、相对路径先工作区后内容根。
*
*****************************************************************************/

using LuBan.Common.IO;

using Microsoft.Extensions.Options;

namespace LuBan.AIAgent.Attachments.Ingestion;

/// <summary>
/// 默认附件来源解析：URL 下载落盘、本地文件就地读取、相对路径先工作区后内容根。
/// </summary>
/// <remarks>
/// 不设主机白名单、不阻断私网地址（设计 D18）：调用方必须是可信前端。
/// 体积与超时作为唯二兜底。
/// </remarks>
public sealed class DefaultAttachmentSourceResolver(
    IOptions<LuBanAgentOptions> options,
    HttpClient? httpClient = null) : IAgentAttachmentSourceResolver
{
    private static readonly HttpClient SharedHttpClient = new() { Timeout = Timeout.InfiniteTimeSpan };
    private readonly HttpClient _http = httpClient ?? SharedHttpClient;

    /// <inheritdoc />
    public async Task<AttachmentSourceResult> ResolveAsync(
        IReadOnlyList<string> sources,
        AttachmentSourceContext context,
        CancellationToken ct = default)
    {
        var resolved = new List<ResolvedAttachment>(sources.Count);
        var failed = new List<AttachmentSourceFailure>();

        foreach (var source in sources)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var item = await ResolveOneAsync(source, context, ct).ConfigureAwait(false);
                resolved.Add(item);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (AttachmentSourceException ex)
            {
                failed.Add(new AttachmentSourceFailure(source, ex.Message));
            }
            catch (Exception ex)
            {
                failed.Add(new AttachmentSourceFailure(source, $"处理失败: {ex.Message}"));
            }
        }

        return new AttachmentSourceResult(resolved, failed);
    }

    private async Task<ResolvedAttachment> ResolveOneAsync(
        string source, AttachmentSourceContext context, CancellationToken ct)
    {
        var opts = options.Value.Attachments;
        var kind = AttachmentPathClassifier.Classify(source);

        return kind switch
        {
            AttachmentSourceKind.Http => await DownloadAsync(source, context, opts, ct).ConfigureAwait(false),
            AttachmentSourceKind.FileUri => InspectLocal(source, new Uri(source.Trim()).LocalPath, opts),
            AttachmentSourceKind.LocalAbsolute => InspectLocal(source, source.Trim(), opts),
            _ => InspectExisting(ResolveRelative(source, context), source, opts)
        };
    }

    private static string? ResolveRelative(string source, AttachmentSourceContext context)
    {
        var relative = AttachmentPathClassifier.ToRelative(source);
        if (relative.Length == 0) return null;

        var inWorkspace = Path.Combine(context.WorkspaceRoot, relative);
        if (File.Exists(inWorkspace)) return inWorkspace;

        if (!string.IsNullOrWhiteSpace(context.ContentRoot))
        {
            var inContentRoot = Path.Combine(context.ContentRoot, relative);
            if (File.Exists(inContentRoot)) return inContentRoot;
        }

        return null;
    }

    private static ResolvedAttachment InspectExisting(string? localPath, string source, AttachmentsOptions opts)
    {
        if (localPath is null || !File.Exists(localPath))
            throw new AttachmentSourceException("路径不存在或不可读");

        return InspectLocal(source, localPath, opts);
    }

    private static ResolvedAttachment InspectLocal(string source, string localPath, AttachmentsOptions opts)
    {
        var full = Path.GetFullPath(localPath);
        if (!File.Exists(full))
            throw new AttachmentSourceException("路径不存在或不可读");

        var file = new FileInfo(full);
        if (file.Length > opts.MaxFileBytes)
            throw new AttachmentSourceException("文件过大");

        var ext = Path.GetExtension(full).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !MediaTypeUtil.ExtensionToMediaType.ContainsKey(ext))
            throw new AttachmentSourceException("不支持的附件类型");

        return new ResolvedAttachment(
            source, full, file.Name, MediaTypeUtil.ResolveMediaType(full), file.Length);
    }

    private async Task<ResolvedAttachment> DownloadAsync(
        string source, AttachmentSourceContext context, AttachmentsOptions opts, CancellationToken ct)
    {
        var uri = new Uri(source.Trim());
        var ext = Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !MediaTypeUtil.ExtensionToMediaType.ContainsKey(ext))
            throw new AttachmentSourceException("不支持的附件类型");

        var dir = Path.Combine(
            context.WorkspaceRoot,
            opts.RootDirectory.Replace('/', Path.DirectorySeparatorChar),
            SafeSegment(context.SessionId),
            DateTimeOffset.Now.ToString("yyyyMMdd"));
        Directory.CreateDirectory(dir);

        var target = Path.Combine(dir, $"{Guid.NewGuid():N}{ext}");
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(opts.TimeoutSeconds > 0 ? opts.TimeoutSeconds : 30));

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await _http.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new AttachmentSourceException($"下载失败: HTTP {(int)response.StatusCode}");

            var declared = response.Content.Headers.ContentLength;
            if (declared is > 0 && declared > opts.MaxFileBytes)
                throw new AttachmentSourceException("文件过大");

            await using (var input = await response.Content.ReadAsStreamAsync(timeout.Token).ConfigureAwait(false))
            await using (var output = File.Create(target))
            {
                var buffer = new byte[81920];
                long total = 0;
                int read;
                while ((read = await input.ReadAsync(buffer, timeout.Token).ConfigureAwait(false)) > 0)
                {
                    total += read;
                    if (total > opts.MaxFileBytes)
                        throw new AttachmentSourceException("文件过大");
                    await output.WriteAsync(buffer.AsMemory(0, read), timeout.Token).ConfigureAwait(false);
                }
            }

            var length = new FileInfo(target).Length;
            // 磁盘名是 guid；响应/事件里回原始 URL 文件名（设计 C19）
            return new ResolvedAttachment(
                source, target, Path.GetFileName(uri.AbsolutePath),
                MediaTypeUtil.ResolveMediaType(target), length);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            TryDelete(target);
            throw;
        }
        catch (OperationCanceledException)
        {
            TryDelete(target);
            throw new AttachmentSourceException("下载失败: 超时");
        }
        catch (AttachmentSourceException)
        {
            TryDelete(target);
            throw;
        }
        catch (Exception ex)
        {
            TryDelete(target);
            throw new AttachmentSourceException($"下载失败: {ex.Message}");
        }
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { /* 清理失败不影响主流程 */ }
    }

    private static string SafeSegment(string value)
    {
        var chars = value.Select(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.' ? c : '_').ToArray();
        return new string(chars);
    }

    private sealed class AttachmentSourceException(string reason) : Exception(reason);
}