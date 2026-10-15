/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Connectors.AliyunDevOps
*文件名： CodeupClient
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/10/15
*描述：云效代码管理客户端
*
*****************************************************************************/

using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LuBan.AIAgent.Connectors.AliyunDevOps;

/// <summary>
/// 云效代码管理客户端 - 提供仓库管理、Git 操作等能力
/// </summary>
public class CodeupClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly YunxiaoApiOptions _options;
    private readonly IYunxiaoCredentialProvider _credentialProvider;
    private readonly ProcessRunner? _processRunner;
    private bool _disposed;

    // 忽略 null 字段，避免把未设置的可见性/模板等参数以 null 发给云效
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// 创建 CodeupClient 实例
    /// </summary>
    /// <param name="options">API 配置</param>
    /// <param name="credentialProvider">凭据提供者</param>
    /// <param name="processRunner">进程执行器（用于 Git 操作，可选）</param>
    public CodeupClient(
        YunxiaoApiOptions options,
        IYunxiaoCredentialProvider credentialProvider,
        ProcessRunner? processRunner = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
        _processRunner = processRunner;

        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds)
        };
    }

    #region 仓库管理 API

    /// <summary>
    /// 搜索代码仓库
    /// </summary>
    /// <param name="search">搜索关键字</param>
    /// <param name="page">页码（从 1 开始）</param>
    /// <param name="perPage">每页条数</param>
    /// <param name="orderBy">排序字段（可选）</param>
    /// <param name="sort">排序方向：asc / desc（可选）</param>
    /// <param name="archived">是否包含已归档仓库（可选）</param>
    /// <param name="organizationId">组织 ID（可选，优先于配置）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>仓库列表</returns>
    public async Task<YunxiaoPagedResult<YunxiaoRepository>?> SearchRepositoriesAsync(
        string? search = null,
        int page = 1,
        int perPage = 20,
        string? orderBy = null,
        string? sort = null,
        bool? archived = null,
        string? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        var orgId = organizationId ?? _options.OrganizationId;
        if (string.IsNullOrWhiteSpace(orgId))
            throw new InvalidOperationException("未配置组织 ID");

        var path = $"/oapi/v1/codeup/organizations/{orgId}/repositories";
        var queryParams = new List<string>
        {
            $"page={page}",
            $"perPage={perPage}"
        };
        
        if (!string.IsNullOrWhiteSpace(search))
            queryParams.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(orderBy))
            queryParams.Add($"orderBy={Uri.EscapeDataString(orderBy!)}");
        if (!string.IsNullOrWhiteSpace(sort))
            queryParams.Add($"sort={Uri.EscapeDataString(sort!)}");
        if (archived.HasValue)
            queryParams.Add($"archived={(archived.Value ? "true" : "false")}");

        var result = await SendRequestAsync<YunxiaoPagedResult<YunxiaoRepository>>(
            HttpMethod.Get, 
            $"{path}?{string.Join("&", queryParams)}",
            cancellationToken: cancellationToken);

        return result;
    }

    /// <summary>
    /// 获取仓库详情
    /// </summary>
    /// <param name="repoId">仓库 ID</param>
    /// <param name="organizationId">组织 ID（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>仓库详情</returns>
    public async Task<YunxiaoRepository?> GetRepositoryAsync(
        string repoId,
        string? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(repoId))
            throw new ArgumentException("仓库 ID 不能为空", nameof(repoId));

        var orgId = organizationId ?? _options.OrganizationId;
        if (string.IsNullOrWhiteSpace(orgId))
            throw new InvalidOperationException("未配置组织 ID");

        var path = $"/oapi/v1/codeup/organizations/{orgId}/repositories/{repoId}";
        
        var result = await SendRequestAsync<YunxiaoRepository>(
            HttpMethod.Get,
            path,
            cancellationToken: cancellationToken);

        return result;
    }

    /// <summary>
    /// 创建仓库
    /// </summary>
    /// <param name="request">创建请求</param>
    /// <param name="organizationId">组织 ID（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>创建的仓库</returns>
    public async Task<YunxiaoRepository?> CreateRepositoryAsync(
        YunxiaoCreateRepositoryRequest request,
        string? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var orgId = organizationId ?? _options.OrganizationId;
        if (string.IsNullOrWhiteSpace(orgId))
            throw new InvalidOperationException("未配置组织 ID");

        var path = $"/oapi/v1/codeup/organizations/{orgId}/repositories";
        
        var result = await SendRequestAsync<YunxiaoRepository>(
            HttpMethod.Post,
            path,
            request,
            cancellationToken);

        return result;
    }

    /// <summary>
    /// 更新仓库
    /// </summary>
    /// <param name="repoId">仓库 ID</param>
    /// <param name="request">更新请求</param>
    /// <param name="organizationId">组织 ID（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>更新后的仓库</returns>
    public async Task<YunxiaoRepository?> UpdateRepositoryAsync(
        string repoId,
        YunxiaoUpdateRepositoryRequest request,
        string? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(repoId))
            throw new ArgumentException("仓库 ID 不能为空", nameof(repoId));

        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var orgId = organizationId ?? _options.OrganizationId;
        if (string.IsNullOrWhiteSpace(orgId))
            throw new InvalidOperationException("未配置组织 ID");

        var path = $"/oapi/v1/codeup/organizations/{orgId}/repositories/{repoId}";
        
        var result = await SendRequestAsync<YunxiaoRepository>(
            HttpMethod.Put,
            path,
            request,
            cancellationToken);

        return result;
    }

    #endregion

    #region Git 操作

    /// <summary>
    /// 克隆仓库到本地（使用 GitToolPlugin 能力）
    /// </summary>
    /// <param name="repositoryUrl">仓库 URL</param>
    /// <param name="targetDirectory">目标目录</param>
    /// <param name="branch">要克隆的分支或标签（对应 git clone -b，可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>克隆结果</returns>
    public async Task<bool> CloneToLocalAsync(
        string repositoryUrl,
        string targetDirectory,
        string? branch = null,
        CancellationToken cancellationToken = default)
    {
        if (_processRunner == null)
            throw new InvalidOperationException("未配置 ProcessRunner，无法执行 Git 操作");

        var pat = await _credentialProvider.GetPatAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(pat))
            throw new InvalidOperationException("未配置 PAT，无法克隆仓库");

        // 构建带鉴权的 URL
        using var credential = new PatCredential(pat);
        var authenticatedUrl = credential.GetAuthenticatedGitUrl(repositoryUrl);

        // 组装 git clone 参数：按 pin 版本克隆时必须带 -b（PRD §6 #17）
        var cloneArgs = new List<string> { "clone" };
        if (!string.IsNullOrWhiteSpace(branch))
        {
            cloneArgs.Add("-b");
            cloneArgs.Add(branch!);
        }
        cloneArgs.Add(authenticatedUrl);
        cloneArgs.Add(targetDirectory);

        var result = await _processRunner.RunAsync(
            "git",
            string.Empty,
            workingDir: null,
            stdin: null,
            timeoutMs: _options.TimeoutSeconds * 1000,
            argumentList: cloneArgs,
            outputEncoding: Encoding.UTF8);

        return result.ExitCode == 0;
    }

    /// <summary>
    /// 推送到远程仓库
    /// </summary>
    /// <param name="repositoryUrl">仓库 URL</param>
    /// <param name="workingDirectory">仓库工作目录</param>
    /// <param name="branch">分支名</param>
    /// <param name="force">是否强制推送（--force）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>推送结果</returns>
    public async Task<bool> PushAsync(
        string repositoryUrl,
        string workingDirectory,
        string? branch = null,
        bool force = false,
        CancellationToken cancellationToken = default)
    {
        if (_processRunner == null)
            throw new InvalidOperationException("未配置 ProcessRunner，无法执行 Git 操作");

        var pat = await _credentialProvider.GetPatAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(pat))
            throw new InvalidOperationException("未配置 PAT，无法推送仓库");

        // 构建带鉴权的 URL
        using var credential = new PatCredential(pat);
        var authenticatedUrl = credential.GetAuthenticatedGitUrl(repositoryUrl);

        // 更新远程 URL
        var args = new List<string> { "remote", "set-url", "origin", authenticatedUrl };
        await _processRunner.RunAsync(
            "git",
            string.Empty,
            workingDirectory,
            stdin: null,
            timeoutMs: _options.TimeoutSeconds * 1000,
            argumentList: args,
            outputEncoding: Encoding.UTF8);

        // 执行 git push
        var pushArgs = new List<string> { "push", "origin" };
        if (!string.IsNullOrWhiteSpace(branch))
            pushArgs.Add(branch!);
        if (force)
            pushArgs.Add("--force");

        var result = await _processRunner.RunAsync(
            "git",
            string.Empty,
            workingDirectory,
            stdin: null,
            timeoutMs: _options.TimeoutSeconds * 1000,
            argumentList: pushArgs,
            outputEncoding: Encoding.UTF8);

        return result.ExitCode == 0;
    }

    #endregion

    #region HTTP 请求

    private async Task<T?> SendRequestAsync<T>(
        HttpMethod method,
        string path,
        object? body = null,
        CancellationToken cancellationToken = default)
    {
        var pat = await _credentialProvider.GetPatAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(pat))
            throw new InvalidOperationException("未配置 PAT");

        var url = $"https://{_options.Endpoint}{path}";
        var retryCount = 0;

        while (retryCount <= _options.RetryCount)
        {
            try
            {
                using var request = new HttpRequestMessage(method, url);
                
                // 添加鉴权头
                request.Headers.Add("x-yunxiao-token", pat);
                
                // 添加请求体
                if (body != null)
                {
                    var json = JsonSerializer.Serialize(body, JsonOpts);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                if (_options.EnableVerboseLog)
                    Logger.Debug($"云效 API 请求: {method} {path}");

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = YunxiaoErrorMapper.MapHttpStatus((int)response.StatusCode, responseBody);
                    
                    // 检查是否需要重试
                    if (YunxiaoErrorMapper.ShouldRetry((int)response.StatusCode) && retryCount < _options.RetryCount)
                    {
                        var delay = YunxiaoErrorMapper.GetRetryDelay((int)response.StatusCode, retryCount);
                        Logger.Warn($"云效 API 请求失败（{response.StatusCode}），{delay}ms 后重试（第 {retryCount + 1} 次）");
                        await Task.Delay(delay, cancellationToken);
                        retryCount++;
                        continue;
                    }

                    throw new YunxiaoApiException((int)response.StatusCode, errorMsg);
                }

                // 解析响应
                var apiResponse = JsonSerializer.Deserialize<YunxiaoApiResponse<T>>(responseBody, JsonOpts);

                if (apiResponse == null)
                    throw new YunxiaoApiException(0, "响应解析失败");

                if (apiResponse.ErrorCode != 0)
                {
                    var errorMsg = YunxiaoErrorMapper.MapBusinessError(apiResponse.ErrorCode, apiResponse.ErrorMessage);
                    throw new YunxiaoApiException(apiResponse.ErrorCode, errorMsg);
                }

                return apiResponse.Result;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (YunxiaoApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (retryCount < _options.RetryCount)
                {
                    var delay = YunxiaoErrorMapper.GetRetryDelay(0, retryCount);
                    Logger.Warn($"云效 API 请求异常，{delay}ms 后重试（第 {retryCount + 1} 次）", ex);
                    await Task.Delay(delay, cancellationToken);
                    retryCount++;
                    continue;
                }

                throw new YunxiaoApiException(0, $"云效 API 请求失败: {ex.Message}", ex);
            }
        }

        return default;
    }

    #endregion

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _httpClient.Dispose();
    }
}

/// <summary>
/// 云效 API 异常
/// </summary>
public class YunxiaoApiException : Exception
{
    /// <summary>
    /// 错误码
    /// </summary>
    public int ErrorCode { get; }

    /// <summary>
    /// 创建异常
    /// </summary>
    public YunxiaoApiException(int errorCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}