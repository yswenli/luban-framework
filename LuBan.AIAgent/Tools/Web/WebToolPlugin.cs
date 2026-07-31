/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
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
        var tools = new List<AIFunction>();

        foreach (var method in typeof(WebToolGroup).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            var func = AIFunctionFactory.Create(method, toolGroup);
            tools.Add(func);
        }

        return tools;
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
public class WebToolGroup : IDisposable
{
    private readonly WebToolOptions _options;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// 创建 WebToolGroup 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    public WebToolGroup(WebToolOptions options)
    {
        _options = options;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// 获取 URL 内容
    /// </summary>
    /// <param name="url">目标 URL</param>
    /// <returns>页面内容</returns>
    [Description("获取 URL 内容")]
    public async Task<string> FetchUrlAsync(string url)
    {
        if (!IsValidHttpUrl(url))
            return JsonSerializer.Serialize(new { statusCode = 0, content = $"无效的 URL: {url}。仅支持 http:// 和 https:// 协议，且不允许访问内网地址。" });

        using var cts = new CancellationTokenSource(30000);
        var response = await _httpClient.GetAsync(url, cts.Token);
        var content = await response.Content.ReadAsStringAsync(cts.Token);

        if (content.Length > _options.MaxCharacters)
        {
            content = content.Substring(0, _options.MaxCharacters) + "\n\n[内容已截断]";
        }

        return JsonSerializer.Serialize(new
        {
            statusCode = (int)response.StatusCode,
            content = content
        });
    }

    /// <summary>
    /// 释放 WebToolGroup 占用的 HttpClient 资源。
    /// </summary>
    public void Dispose()
    {
        _httpClient.Dispose();
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
