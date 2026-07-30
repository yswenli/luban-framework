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
    public IReadOnlyList<AIFunction> GetTools(IServiceProvider sp)
    {
        var session = sp.GetRequiredService<PlaywrightSession>();
        var toolGroup = new BrowserToolGroup(session);
        var tools = new List<AIFunction>();

        foreach (var method in typeof(BrowserToolGroup).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
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
    public bool IsEnabled(LuBanAgentOptions options) => options.Tools.Browser.Enabled;
}

/// <summary>
/// 浏览器工具分组
/// </summary>
public class BrowserToolGroup
{
    private readonly PlaywrightSession _session;

    /// <summary>
    /// 创建 BrowserToolGroup 实例
    /// </summary>
    /// <param name="session">Playwright 会话</param>
    public BrowserToolGroup(PlaywrightSession session)
    {
        _session = session;
    }

    /// <summary>
    /// 导航到指定 URL
    /// </summary>
    /// <param name="url">目标 URL</param>
    /// <returns>导航结果</returns>
    [Description("导航到指定 URL")]
    public async Task<string> NavigateAsync(string url)
    {
        if (!IsValidHttpUrl(url))
            return $"无效的 URL: {url}。仅支持 http:// 和 https:// 协议。";

        try
        {
            var page = await _session.GetPageAsync();
            await page.GotoAsync(url, new Microsoft.Playwright.PageGotoOptions
            {
                Timeout = 30000,
                WaitUntil = Microsoft.Playwright.WaitUntilState.NetworkIdle
            });
            return $"已成功导航到 {url}";
        }
        catch (Microsoft.Playwright.PlaywrightException ex)
        {
            Logger.Error("浏览器导航 Playwright 异常", ex, url);
            return $"导航失败: {ex.Message}\n\n请安装 Playwright 浏览器:\n  npx playwright@1.61.0 install chromium";
        }
        catch (Exception ex)
        {
            Logger.Error("浏览器导航异常", ex, url);
            return $"导航失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 点击页面元素
    /// </summary>
    /// <param name="selector">CSS 选择器</param>
    /// <returns>点击结果</returns>
    [Description("点击页面元素，使用 CSS 选择器定位元素")]
    public async Task<string> ClickAsync(string selector)
    {
        try
        {
            var page = await _session.GetPageAsync();
            await page.ClickAsync(selector, new Microsoft.Playwright.PageClickOptions
            {
                Timeout = 10000
            });
            return $"已成功点击元素: {selector}";
        }
        catch (Microsoft.Playwright.PlaywrightException ex)
        {
            Logger.Error("浏览器点击 Playwright 异常", ex, selector);
            return $"点击失败: {ex.Message}\n元素选择器: {selector}";
        }
        catch (Exception ex)
        {
            Logger.Error("浏览器点击异常", ex, selector);
            return $"点击失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 在输入框中输入文本
    /// </summary>
    /// <param name="selector">CSS 选择器</param>
    /// <param name="text">要输入的文本</param>
    /// <returns>输入结果</returns>
    [Description("在输入框中输入文本，使用 CSS 选择器定位输入框")]
    public async Task<string> TypeTextAsync(string selector, string text)
    {
        try
        {
            var page = await _session.GetPageAsync();
            await page.FillAsync(selector, text, new Microsoft.Playwright.PageFillOptions
            {
                Timeout = 10000
            });
            return $"已在元素 {selector} 中成功输入文本: {text}";
        }
        catch (Microsoft.Playwright.PlaywrightException ex)
        {
            Logger.Error("浏览器输入 Playwright 异常", ex, selector, text);
            return $"输入失败: {ex.Message}\n元素选择器: {selector}";
        }
        catch (Exception ex)
        {
            Logger.Error("浏览器输入异常", ex, selector, text);
            return $"输入失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 截取页面截图
    /// </summary>
    /// <param name="path">截图保存路径（可选）</param>
    /// <returns>截图结果（base64 编码）</returns>
    [Description("截取页面截图，返回 base64 编码的图片数据")]
    public async Task<string> ScreenshotAsync(string? path = null)
    {
        if (!string.IsNullOrEmpty(path) && !LuBan.AIAgent.Infrastructure.PathGuard.IsPathSafe(path))
            return $"截图路径不安全: {path}";

        try
        {
            var page = await _session.GetPageAsync();
            var bytes = await page.ScreenshotAsync(new Microsoft.Playwright.PageScreenshotOptions
            {
                Path = path,
                FullPage = false  // 只截取可视区域，避免图片过大
            });
            
            // 限制 base64 数据大小（最多 100KB）
            var base64 = Convert.ToBase64String(bytes);
            if (base64.Length > 100 * 1024)
            {
                base64 = base64.Substring(0, 100 * 1024) + "\n\n[截图数据已截断]";
            }
            
            return $"截图成功，Base64 数据:\n{base64}";
        }
        catch (Microsoft.Playwright.PlaywrightException ex)
        {
            Logger.Error("浏览器截图 Playwright 异常", ex, path ?? "");
            return $"截图失败: {ex.Message}";
        }
        catch (Exception ex)
        {
            Logger.Error("浏览器截图异常", ex, path ?? "");
            return $"截图失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 获取页面内容
    /// </summary>
    /// <param name="selector">CSS 选择器（可选）</param>
    /// <returns>页面内容</returns>
    [Description("获取页面 HTML 内容或指定元素的文本内容")]
    public async Task<string> GetContentAsync(string? selector = null)
    {
        try
        {
            var page = await _session.GetPageAsync();
            if (string.IsNullOrEmpty(selector))
            {
                var content = await page.ContentAsync();
                
                // 限制内容大小（最多 50KB，约 12K tokens）
                const int maxContentLength = 50 * 1024;
                if (content.Length > maxContentLength)
                {
                    content = content.Substring(0, maxContentLength) + "\n\n[页面内容已截断，请使用 CSS 选择器获取特定元素内容]";
                }
                
                return $"页面内容:\n{content}";
            }
            var text = await page.Locator(selector).TextContentAsync();
            
            // 限制元素内容大小
            const int maxTextLength = 20 * 1024;
            if (text != null && text.Length > maxTextLength)
            {
                text = text.Substring(0, maxTextLength) + "\n\n[元素内容已截断]";
            }
            
            return $"元素 {selector} 的文本内容:\n{text ?? "(空)"}";
        }
        catch (Microsoft.Playwright.PlaywrightException ex)
        {
            Logger.Error("浏览器获取内容 Playwright 异常", ex, selector ?? "");
            return $"获取内容失败: {ex.Message}";
        }
        catch (Exception ex)
        {
            Logger.Error("浏览器获取内容异常", ex, selector ?? "");
            return $"获取内容失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 等待元素出现
    /// </summary>
    /// <param name="selector">CSS 选择器</param>
    /// <param name="timeout">超时时间（毫秒）</param>
    /// <returns>等待结果</returns>
    [Description("等待指定元素出现在页面上")]
    public async Task<string> WaitForSelectorAsync(string selector, int timeout = 10000)
    {
        try
        {
            var page = await _session.GetPageAsync();
            await page.WaitForSelectorAsync(selector, new Microsoft.Playwright.PageWaitForSelectorOptions
            {
                Timeout = timeout
            });
            return $"元素 {selector} 已出现";
        }
        catch (Microsoft.Playwright.PlaywrightException ex)
        {
            Logger.Error("浏览器等待元素 Playwright 异常", ex, selector, timeout);
            return $"等待元素超时: {ex.Message}\n元素选择器: {selector}";
        }
        catch (Exception ex)
        {
            Logger.Error("浏览器等待元素异常", ex, selector, timeout);
            return $"等待元素失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 获取当前页面 URL
    /// </summary>
    /// <returns>当前 URL</returns>
    [Description("获取当前页面的 URL")]
    public async Task<string> GetCurrentUrlAsync()
    {
        try
        {
            var page = await _session.GetPageAsync();
            var url = page.Url;
            return $"当前页面 URL: {url}";
        }
        catch (Exception ex)
        {
            Logger.Error("浏览器获取URL异常", ex);
            return $"获取 URL 失败: {ex.Message}";
        }
    }

    private static bool IsValidHttpUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;
        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }
}
