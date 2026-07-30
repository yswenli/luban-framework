namespace LuBan.AIAgent.Infrastructure;

/// <summary>
/// Playwright 浏览器会话管理类，负责初始化浏览器并管理页面生命周期。
/// </summary>
public sealed class PlaywrightSession : IAsyncDisposable, IDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;
    private readonly BrowserToolOptions _options;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _disposed;
    private bool _initialized;

    /// <summary>
    /// 创建 PlaywrightSession 实例。
    /// </summary>
    /// <param name="options">LuBan Agent 配置选项。</param>
    public PlaywrightSession(IOptions<LuBanAgentOptions> options)
    {
        _options = options.Value.Tools.Browser;
    }

    /// <summary>
    /// 获取当前 Playwright 页面实例，若未初始化则自动创建浏览器和页面。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>Playwright 页面实例。</returns>
    /// <exception cref="InvalidOperationException">Playwright 初始化失败或浏览器启动失败时抛出。</exception>
    public async Task<IPage> GetPageAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_page != null && _initialized)
            {
                try
                {
                    _ = _page.Url;
                    return _page;
                }
                catch
                {
                    await ResetAsync();
                }
            }

            try
            {
                _playwright = await Playwright.CreateAsync();
            }
            catch (Exception ex)
            {
                Logger.Error("Playwright 初始化失败", ex);
                throw new InvalidOperationException(
                    $"Playwright 初始化失败: {ex.Message}\n\n请安装 Playwright 浏览器:\n  npx playwright@1.61.0 install chromium", ex);
            }

            var launchOptions = new BrowserTypeLaunchOptions
            {
                Headless = _options.Headless,
                Timeout = _options.Timeout
            };

            try
            {
                _browser = _options.Engine.ToLowerInvariant() switch
                {
                    "firefox" => await _playwright.Firefox.LaunchAsync(launchOptions),
                    "webkit" => await _playwright.Webkit.LaunchAsync(launchOptions),
                    _ => await _playwright.Chromium.LaunchAsync(launchOptions)
                };
            }
            catch (PlaywrightException ex)
            {
                Logger.Error("浏览器启动失败", ex, _options.Engine);
                throw new InvalidOperationException(
                    $"浏览器启动失败: {ex.Message}\n\n请安装 Playwright 浏览器:\n  npx playwright@1.61.0 install chromium", ex);
            }

            _page = await _browser.NewPageAsync();
            _page.SetDefaultTimeout(_options.Timeout);
            _initialized = true;

            return _page;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task ResetAsync()
    {
        if (_page != null)
        {
            try { await _page.CloseAsync(); } catch { }
            _page = null;
        }
        if (_browser != null)
        {
            try { await _browser.CloseAsync(); } catch { }
            _browser = null;
        }
        try { _playwright?.Dispose(); } catch { }
        _playwright = null;
        _initialized = false;
    }

    /// <summary>
    /// 异步释放 Playwright 会话占用的资源。
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await _lock.WaitAsync();
        try
        {
            await ResetAsync();
        }
        finally
        {
            _lock.Release();
        }

        _lock.Dispose();
    }

    /// <summary>
    /// 同步释放 Playwright 会话占用的资源。
    /// </summary>
    public void Dispose()
    {
        DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
