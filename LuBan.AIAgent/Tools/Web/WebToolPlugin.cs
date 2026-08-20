/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.Web
*文件名： WebToolPlugin
*版本号： V1.0.0.0
*唯一标识：773f80c8-c995-4981-8f4a-c366d294a2aa
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：网页工具插件
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：网页工具插件
*
*****************************************************************************/
namespace LuBan.AIAgent.Tools.Web;

/// <summary>
/// Web 工具插件
/// </summary>
public class WebToolPlugin : ILuBanToolPlugin
{
    private readonly WebToolOptions _options;

    /// <summary>
    /// 创建 WebToolPlugin 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    public WebToolPlugin(IOptions<LuBanAgentOptions> options)
    {
        _options = options.Value.Tools.Web;
    }

    /// <summary>
    /// 工具分组名称
    /// </summary>
    public string GroupName => "web";

    /// <summary>
    /// 工具分组描述
    /// </summary>
    public string? Description => "Web 请求工具，支持 HTTP GET 等操作";

    /// <summary>
    /// 获取工具函数列表
    /// </summary>
    /// <param name="sp">服务提供者</param>
    /// <returns>工具函数列表</returns>
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp)
    {
        var toolGroup = new WebToolGroup(_options);
        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(toolGroup, nameof(WebToolGroup.FetchUrlAsync))
        };
    }

    /// <summary>
    /// 判断插件是否启用
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <returns>是否启用</returns>
    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.Web.Enabled;
}

/// <summary>
/// Web 工具分组
/// </summary>
public class WebToolGroup
{
    private readonly WebToolOptions _options;

    /// <summary>
    /// 创建 WebToolGroup 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    public WebToolGroup(WebToolOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// 获取 URL 内容
    /// </summary>
    /// <param name="url">目标 URL</param>
    /// <returns>页面内容</returns>
    [Description("获取 URL 内容")]
    public async Task<ToolResult<string>> FetchUrlAsync(string url)
    {
        if (!IsValidHttpUrl(url))
            return ToolResult.Fail<string>($"无效的 URL: {url}。仅支持 http:// 和 https:// 协议，且不允许访问内网地址。");

        try
        {
            var uri = new Uri(url);
            // 使用 origin 作为 base，共享同一主机的 HttpClient；保留路径和查询作为 resource
            var baseUri = new Uri(uri.GetLeftPart(UriPartial.Authority) + "/");
            var resource = uri.PathAndQuery.TrimStart('/');

            var proxy = HttpClientProxy.Create(baseUri, timeout: 30, useLog: true);
            var bytes = await proxy.GetBytesAsync(resource, timeout: 30);
            var content = Encoding.UTF8.GetString(bytes);

            if (content.Length > _options.MaxCharacters)
            {
                content = content.Substring(0, _options.MaxCharacters) + "\n\n[内容已截断]";
            }

            return ToolResult.Ok(content, $"statusCode=200; url={url}");
        }
        catch (OperationCanceledException ex)
        {
            Logger.Error("获取 URL 异常：请求超时", ex, url);
            return ToolResult.Fail<string>($"请求超时（30 秒）: {url}\n建议: 该站点响应过慢或不可达，请尝试更换 URL 或使用搜索引擎获取信息。");
        }
        catch (HttpRequestException ex)
        {
            Logger.Error("获取 URL 异常：HTTP 请求失败", ex, url);
            return ToolResult.Fail<string>(BuildHttpRequestErrorMessage(ex, url), ((int?)ex.StatusCode ?? 0).ToString());
        }
        catch (IOException ex)
        {
            Logger.Error("获取 URL 异常：内容读取失败", ex, url);
            return ToolResult.Fail<string>($"读取 URL 内容失败: {url}\n错误: {ex.Message}\n建议: 请尝试更换 URL 或使用搜索引擎获取信息。");
        }
        catch (Exception ex)
        {
            Logger.Error("获取 URL 异常", ex, url);
            return ToolResult.Fail<string>($"获取 URL 失败: {url}\n错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据 HttpRequestException 构建可操作的网络错误消息。
    /// </summary>
    /// <param name="ex">HTTP 请求异常</param>
    /// <param name="url">目标 URL</param>
    /// <returns>错误消息</returns>
    private static string BuildHttpRequestErrorMessage(HttpRequestException ex, string url)
    {
        var msg = ex.Message ?? string.Empty;

        // DNS 解析失败
        if (msg.Contains("No such host is known", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("name or service not known", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("Name resolution", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("resolve", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("DNS", StringComparison.OrdinalIgnoreCase))
        {
            return $"DNS 解析失败: {url}\n错误: {msg}\n建议: 域名无法解析，请确认 URL 是否正确，或尝试使用搜索引擎获取信息。";
        }

        // 连接失败 / 拒绝 / 重置
        if (msg.Contains("connection refused", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("connection reset", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("connection denied", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("unable to connect", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("Connection forcibly closed", StringComparison.OrdinalIgnoreCase))
        {
            return $"连接失败: {url}\n错误: {msg}\n建议: 目标站点不可达或拒绝连接，请尝试更换 URL 或使用搜索引擎获取信息。";
        }

        // SSL/TLS 错误
        if (msg.Contains("SSL", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("TLS", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("certificate", StringComparison.OrdinalIgnoreCase))
        {
            return $"SSL/TLS 错误: {url}\n错误: {msg}\n建议: 证书验证失败或加密协议不兼容，请尝试更换 URL 或使用搜索引擎获取信息。";
        }

        // 其他 HTTP 错误
        return $"HTTP 请求失败: {url}\n错误: {msg}\n建议: 请尝试更换 URL 或使用搜索引擎获取信息。";
    }

    private static bool IsValidHttpUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;
        var host = uri.HostNameType == UriHostNameType.Dns ? uri.Host : uri.IdnHost;
        if (host == "169.254.169.254" || host == "localhost" || host == "127.0.0.1" || host == "::1" || host == "0.0.0.0")
            return false;
        return true;
    }
}
