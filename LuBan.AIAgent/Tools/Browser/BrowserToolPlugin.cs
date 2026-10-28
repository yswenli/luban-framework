/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Tools.Browser
*文件名： BrowserToolPlugin
*版本号： V1.0.0.0
*唯一标识：1fec446d-305d-4f13-b58d-12791eebc65f
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/7/31
*描述：浏览器工具插件
*
*=================================================
*修改标记
*修改时间：2026/7/31
*修改人： yswenli
*版本号： V1.0.0.0
*描述：浏览器工具插件
*
*****************************************************************************/

namespace LuBan.AIAgent.Tools.Browser;

/// <summary>
/// 浏览器工具插件
/// </summary>
public class BrowserToolPlugin : ILuBanToolPlugin
{
    private readonly BrowserToolOptions _options;

    /// <summary>
    /// 创建 BrowserToolPlugin 实例
    /// </summary>
    /// <param name="options">配置选项</param>
    public BrowserToolPlugin(IOptions<LuBanAgentOptions> options)
    {
        _options = options.Value.Tools.Browser;
    }

    /// <summary>
    /// 工具分组名称
    /// </summary>
    public string GroupName => "browser";

    /// <summary>
    /// 工具分组描述
    /// </summary>
    public string? Description => "浏览器自动化工具，支持导航、点击、输入、截图等操作";

    /// <summary>
    /// 获取工具函数列表
    /// </summary>
    /// <param name="sp">服务提供者</param>
    /// <returns>工具函数列表</returns>
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp, ToolGroupOptions? toolsOptions = null)
    {
        var session = sp.GetRequiredService<PlaywrightSession>();
        session.SetOptions(toolsOptions?.Browser ?? _options);
        var confirmationService = sp.GetRequiredService<IToolConfirmationService>();
        var toolGroup = new BrowserToolGroup(session, confirmationService);
        return new List<AIFunction>
        {
            AIFunctionFactoryHelper.Create(toolGroup, nameof(BrowserToolGroup.NavigateAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(BrowserToolGroup.ClickAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(BrowserToolGroup.TypeTextAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(BrowserToolGroup.ScreenshotAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(BrowserToolGroup.GetContentAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(BrowserToolGroup.WaitForSelectorAsync)),
            AIFunctionFactoryHelper.Create(toolGroup, nameof(BrowserToolGroup.GetCurrentUrlAsync))
        };
    }

    /// <summary>
    /// 判断插件是否启用
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <returns>是否启用</returns>
    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.Browser.Enabled;
}

/// <summary>
/// 浏览器工具分组
/// </summary>
public class BrowserToolGroup
{
    private readonly PlaywrightSession _session;
    private readonly IToolConfirmationService _confirmationService;

    /// <summary>
    /// 创建 BrowserToolGroup 实例
    /// </summary>
    /// <param name="session">Playwright 会话</param>
    /// <param name="confirmationService">工具确认服务</param>
    public BrowserToolGroup(PlaywrightSession session, IToolConfirmationService confirmationService)
    {
        _session = session;
        _confirmationService = confirmationService;
    }

    /// <summary>
    /// 导航到指定 URL
    /// </summary>
    /// <param name="url">目标 URL</param>
    /// <param name="cancellationToken">取消令牌（取消时关闭当前页面以打断在途导航）。</param>
    /// <returns>导航结果</returns>
    [Description("导航到指定 URL")]
    public async Task<ToolResult<string>> NavigateAsync(string url, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var outcome = await _confirmationService.EvaluateAsync(nameof(NavigateAsync), null,
            new Dictionary<string, object?> { ["url"] = url });
        if (outcome == EnumConfirmationOutcome.Planned)
            return ToolResult.Plan<string>();
        if (outcome != EnumConfirmationOutcome.Allowed)
            return ToolResult.Denied<string>();
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsValidHttpUrl(url))
            return ToolResult.Fail<string>($"无效的 URL: {url}。仅支持 http:// 和 https:// 协议。");

        // 第一次尝试：NetworkIdle（等待网络完全空闲），失败时不记录错误日志（降级重试会决定是否记录）
        var (ok, msg) = await TryNavigateAsync(url, 30000, Microsoft.Playwright.WaitUntilState.NetworkIdle, cancellationToken, logOnError: false);
        if (ok) return ToolResult.Ok<string>(msg!);

        // NetworkIdle 失败时降级为 DOMContentLoaded（仅等待 DOM 加载完成）
        Logger.Warn($"浏览器导航 NetworkIdle 失败，降级为 DOMContentLoaded 重试: {url}");
        (ok, msg) = await TryNavigateAsync(url, 15000, Microsoft.Playwright.WaitUntilState.DOMContentLoaded, cancellationToken);
        if (ok) return ToolResult.Ok<string>(msg!);

        return ToolResult.Fail<string>(msg!);
    }

    /// <summary>
    /// 尝试导航到指定 URL，返回是否成功及结果消息。
    /// </summary>
    /// <param name="url">目标 URL</param>
    /// <param name="timeoutMs">超时时间（毫秒）</param>
    /// <param name="waitUntil">等待策略</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <param name="logOnError">是否记录错误日志（降级重试场景下由调用方统一记录）</param>
    /// <returns>(是否成功, 结果消息)</returns>
    private async Task<(bool ok, string? msg)> TryNavigateAsync(
        string url, int timeoutMs, Microsoft.Playwright.WaitUntilState waitUntil,
        CancellationToken cancellationToken, bool logOnError = true)
    {
        try
        {
            var page = await _session.GetPageAsync(cancellationToken);
            await RunCancelableAsync(() => page.GotoAsync(url, new Microsoft.Playwright.PageGotoOptions
            {
                Timeout = timeoutMs,
                WaitUntil = waitUntil
            }), cancellationToken);
            return (true, $"已成功导航到 {url}（等待策略: {waitUntil}）");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Microsoft.Playwright.PlaywrightException ex)
        {
            if (logOnError) Logger.Error("浏览器导航 Playwright 异常", ex, url);
            return (false, BuildNavigateErrorMessage(ex, url));
        }
        catch (Exception ex)
        {
            if (logOnError) Logger.Error("浏览器导航异常", ex, url);
            return (false, $"导航失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据 Playwright 异常类型构建可操作的错误消息。
    /// </summary>
    /// <param name="ex">Playwright 异常</param>
    /// <param name="url">目标 URL</param>
    /// <returns>错误消息</returns>
    private static string BuildNavigateErrorMessage(Microsoft.Playwright.PlaywrightException ex, string url)
    {
        var msg = ex.Message;

        // 浏览器未安装
        if (msg.Contains("Executable doesn't exist") || msg.Contains("playwright install"))
        {
            return $"导航失败: 浏览器未安装。\n\n请运行以下命令安装:\n  {PlaywrightDriverResolver.GetInstallBrowsersHint()}";
        }

        // 网络超时 / 连接失败
        if (msg.Contains("ERR_CONNECTION_TIMED_OUT") ||
            msg.Contains("ERR_TIMED_OUT") ||
            msg.Contains("ERR_NAME_NOT_RESOLVED") ||
            msg.Contains("ERR_CONNECTION_REFUSED") ||
            msg.Contains("ERR_CONNECTION_RESET") ||
            msg.Contains("ERR_INTERNET_DISCONNECTED"))
        {
            return $"导航失败: 无法连接到 {url}\n" +
                   $"错误: {msg.Split('\n')[0]}\n\n" +
                   $"可能原因: 网络不通、目标站点不可达、DNS 解析失败或被防火墙拦截。\n" +
                   $"建议: 请尝试更换 URL、使用搜索引擎获取信息，或直接告知用户该站点无法访问。";
        }

        // 导航超时（页面加载太慢）
        if (msg.Contains("Timeout") || msg.Contains("timeout"))
        {
            return $"导航失败: 页面加载超时（{url}）\n" +
                   $"建议: 该站点响应过慢或可能有反爬虫保护。可尝试使用 GetContentAsync 获取已加载的部分内容，或更换目标 URL。";
        }

        // 其他错误
        return $"导航失败: {msg}";
    }

    /// <summary>
    /// 点击页面元素
    /// </summary>
    /// <param name="selector">CSS 选择器</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>点击结果</returns>
    [Description("点击页面元素，使用 CSS 选择器定位元素")]
    public async Task<ToolResult<string>> ClickAsync(string selector, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var outcome = await _confirmationService.EvaluateAsync(nameof(ClickAsync), null,
            new Dictionary<string, object?> { ["selector"] = selector });
        if (outcome == EnumConfirmationOutcome.Planned)
            return ToolResult.Plan<string>();
        if (outcome != EnumConfirmationOutcome.Allowed)
            return ToolResult.Denied<string>();
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var page = await _session.GetPageAsync(cancellationToken);
            await RunCancelableAsync(() => page.ClickAsync(selector, new Microsoft.Playwright.PageClickOptions
            {
                Timeout = 10000
            }), cancellationToken);
            return ToolResult.Ok<string>($"已成功点击元素: {selector}");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Microsoft.Playwright.PlaywrightException ex)
        {
            Logger.Error("浏览器点击 Playwright 异常", ex, selector);
            return ToolResult.Fail<string>($"点击失败: {ex.Message}\n元素选择器: {selector}");
        }
        catch (Exception ex)
        {
            Logger.Error("浏览器点击异常", ex, selector);
            return ToolResult.Fail<string>($"点击失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 在输入框中输入文本
    /// </summary>
    /// <param name="selector">CSS 选择器</param>
    /// <param name="text">要输入的文本</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>输入结果</returns>
    [Description("在输入框中输入文本，使用 CSS 选择器定位输入框")]
    public async Task<ToolResult<string>> TypeTextAsync(string selector, string text, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var outcome = await _confirmationService.EvaluateAsync(nameof(TypeTextAsync), null,
            new Dictionary<string, object?> { ["selector"] = selector, ["text"] = text });
        if (outcome == EnumConfirmationOutcome.Planned)
            return ToolResult.Plan<string>();
        if (outcome != EnumConfirmationOutcome.Allowed)
            return ToolResult.Denied<string>();
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var page = await _session.GetPageAsync(cancellationToken);
            await RunCancelableAsync(() => page.FillAsync(selector, text, new Microsoft.Playwright.PageFillOptions
            {
                Timeout = 10000
            }), cancellationToken);
            return ToolResult.Ok<string>($"已在元素 {selector} 中成功输入文本: {text}");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Microsoft.Playwright.PlaywrightException ex)
        {
            Logger.Error("浏览器输入 Playwright 异常", ex, selector, text);
            return ToolResult.Fail<string>($"输入失败: {ex.Message}\n元素选择器: {selector}");
        }
        catch (Exception ex)
        {
            Logger.Error("浏览器输入异常", ex, selector, text);
            return ToolResult.Fail<string>($"输入失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 截取页面截图
    /// </summary>
    /// <param name="path">截图保存路径（可选）</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>截图结果（base64 编码）</returns>
    [Description("截取页面截图，返回 base64 编码的图片数据")]
    public async Task<ToolResult<string>> ScreenshotAsync(string? path = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!string.IsNullOrEmpty(path) && !LuBan.AIAgent.Infrastructure.PathGuard.IsPathSafe(path))
            return ToolResult.Fail<string>($"截图路径不安全: {path}");

        try
        {
            var page = await _session.GetPageAsync(cancellationToken);
            var bytes = await RunCancelableAsync(() => page.ScreenshotAsync(new Microsoft.Playwright.PageScreenshotOptions
            {
                Path = path,
                FullPage = false  // 只截取可视区域，避免图片过大
            }), cancellationToken);
            
            // 限制 base64 数据大小（最多 100KB）
            var base64 = Convert.ToBase64String(bytes);
            if (base64.Length > 100 * 1024)
            {
                base64 = base64.Substring(0, 100 * 1024) + "\n\n[截图数据已截断]";
            }
            
            return ToolResult.Ok<string>($"截图成功，Base64 数据:\n{base64}");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Microsoft.Playwright.PlaywrightException ex)
        {
            Logger.Error("浏览器截图 Playwright 异常", ex, path ?? "");
            return ToolResult.Fail<string>($"截图失败: {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("浏览器截图异常", ex, path ?? "");
            return ToolResult.Fail<string>($"截图失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取页面内容
    /// </summary>
    /// <param name="selector">CSS 选择器（可选）</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>页面内容</returns>
    [Description("获取页面 HTML 内容或指定元素的文本内容")]
    public async Task<ToolResult<string>> GetContentAsync(string? selector = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var page = await _session.GetPageAsync(cancellationToken);
            if (string.IsNullOrEmpty(selector))
            {
                var content = await RunCancelableAsync(() => page.ContentAsync(), cancellationToken);
                
                // 限制内容大小（最多 50KB，约 12K tokens）
                const int maxContentLength = 50 * 1024;
                if (content.Length > maxContentLength)
                {
                    content = content.Substring(0, maxContentLength) + "\n\n[页面内容已截断，请使用 CSS 选择器获取特定元素内容]";
                }
                
                return ToolResult.Ok<string>($"页面内容:\n{content}");
            }
            var text = await RunCancelableAsync(() => page.Locator(selector).TextContentAsync(), cancellationToken);
            
            // 限制元素内容大小
            const int maxTextLength = 20 * 1024;
            if (text != null && text.Length > maxTextLength)
            {
                text = text.Substring(0, maxTextLength) + "\n\n[元素内容已截断]";
            }
            
            return ToolResult.Ok<string>($"元素 {selector} 的文本内容:\n{text ?? "(空)"}");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Microsoft.Playwright.PlaywrightException ex)
        {
            Logger.Error("浏览器获取内容 Playwright 异常", ex, selector ?? "");
            return ToolResult.Fail<string>($"获取内容失败: {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error("浏览器获取内容异常", ex, selector ?? "");
            return ToolResult.Fail<string>($"获取内容失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 等待元素出现
    /// </summary>
    /// <param name="selector">CSS 选择器</param>
    /// <param name="timeout">超时时间（毫秒）</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>等待结果</returns>
    [Description("等待指定元素出现在页面上")]
    public async Task<ToolResult<string>> WaitForSelectorAsync(string selector, int timeout = 10000, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var page = await _session.GetPageAsync(cancellationToken);
            await RunCancelableAsync(() => page.WaitForSelectorAsync(selector, new Microsoft.Playwright.PageWaitForSelectorOptions
            {
                Timeout = timeout
            }), cancellationToken);
            return ToolResult.Ok<string>($"元素 {selector} 已出现");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Microsoft.Playwright.PlaywrightException ex)
        {
            Logger.Error("浏览器等待元素 Playwright 异常", ex, selector, timeout);
            return ToolResult.Fail<string>($"等待元素超时: {ex.Message}\n元素选择器: {selector}");
        }
        catch (Exception ex)
        {
            Logger.Error("浏览器等待元素异常", ex, selector, timeout);
            return ToolResult.Fail<string>($"等待元素失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取当前页面 URL
    /// </summary>
    /// <returns>当前 URL</returns>
    [Description("获取当前页面的 URL")]
    public async Task<ToolResult<string>> GetCurrentUrlAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var page = await _session.GetPageAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var url = page.Url;
            return ToolResult.Ok<string>($"当前页面 URL: {url}");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error("浏览器获取URL异常", ex);
            return ToolResult.Fail<string>($"获取 URL 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 执行在途可能与取消令牌无关的 Playwright 调用：取消时关闭当前页面以打断调用，
    /// 并把由此产生的异常统一转成 <see cref="OperationCanceledException"/> 交回宿主。
    /// </summary>
    /// <typeparam name="T">返回值类型。</typeparam>
    /// <param name="action">Playwright 调用。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    private async Task<T> RunCancelableAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!cancellationToken.CanBeCanceled)
            return await action();

        using var registration = cancellationToken.Register(
            static state => _ = AbortSafelyAsync((PlaywrightSession)state!), _session);
        try
        {
            var result = await action();
            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }
        catch (Exception ex) when (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException("浏览器操作已取消", ex, cancellationToken);
        }
    }

    /// <summary>
    /// 无返回值的 <see cref="RunCancelableAsync{T}"/> 重载。
    /// </summary>
    private Task RunCancelableAsync(Func<Task> action, CancellationToken cancellationToken)
        => RunCancelableAsync(async () =>
        {
            await action();
            return true;
        }, cancellationToken);

    /// <summary>
    /// 取消回调中关闭页面；任何异常都不能逃逸到未观察的任务里。
    /// </summary>
    private static async Task AbortSafelyAsync(PlaywrightSession session)
    {
        try
        {
            await session.AbortPageAsync();
        }
        catch
        {
            // 会话已释放等情况直接忽略：取消路径不应再抛异常
        }
    }

    private static bool IsValidHttpUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;
        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }
}
