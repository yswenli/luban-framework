/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Infrastructure
*文件名： PlaywrightDriverResolver
*版本号： V1.0.0.0
*唯一标识：f2a7c1e4-3b6d-4c2a-9e1f-7a5b8d0c3e21
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/8/17
*描述：Playwright 驱动按需解析与安装，避免随应用分发约 100MB 的 Node 驱动
*
*=================================================
*修改标记
*修改时间：2026/8/17
*修改人： yswenli
*版本号： V1.0.0.0
*描述：Playwright 驱动按需解析与安装，避免随应用分发约 100MB 的 Node 驱动
*
*****************************************************************************/
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace LuBan.AIAgent.Infrastructure;

/// <summary>
/// Playwright 驱动解析器：优先使用应用程序旁的 .playwright 目录；
/// 不存在时从 nuget.org 下载对应版本的 Microsoft.Playwright 包，
/// 将驱动释放到用户目录缓存（~/.luban-agent/playwright-driver/{version}），
/// 并通过 PLAYWRIGHT_DRIVER_SEARCH_PATH 指向该目录。
/// </summary>
public static class PlaywrightDriverResolver
{
    private static readonly SemaphoreSlim _gate = new(1, 1);
    private static string? _resolvedDriverRoot;

    private static readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromMinutes(10)
    };

    static PlaywrightDriverResolver()
    {
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("luban-agent-cli");
    }

    /// <summary>
    /// 当前引用的 Microsoft.Playwright 版本（如 1.62.0）。
    /// </summary>
    public static string PlaywrightVersion =>
        typeof(Playwright).Assembly.GetName().Version?.ToString(3) ?? "1.62.0";

    /// <summary>
    /// 确保 Playwright 驱动可用，返回驱动根目录（其下含 .playwright 子目录）。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>驱动根目录。</returns>
    /// <exception cref="InvalidOperationException">驱动下载或释放失败时抛出。</exception>
    public static async Task<string> EnsureDriverAsync(CancellationToken cancellationToken = default)
    {
        if (_resolvedDriverRoot != null)
        {
            return _resolvedDriverRoot;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_resolvedDriverRoot != null)
            {
                return _resolvedDriverRoot;
            }

            // 1. 应用程序旁已带驱动（开发/自包含场景），无需任何处理
            var baseDir = AppContext.BaseDirectory;
            if (IsValidDriverRoot(baseDir))
            {
                _resolvedDriverRoot = baseDir;
                return _resolvedDriverRoot;
            }

            // 2. 用户目录缓存
            var driverRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".luban-agent", "playwright-driver", PlaywrightVersion);

            if (!IsValidDriverRoot(driverRoot))
            {
                await DownloadDriverAsync(driverRoot, cancellationToken);
            }

            if (!IsValidDriverRoot(driverRoot))
            {
                throw new InvalidOperationException($"Playwright 驱动释放后校验失败: {driverRoot}");
            }

            Environment.SetEnvironmentVariable("PLAYWRIGHT_DRIVER_SEARCH_PATH", driverRoot);
            _resolvedDriverRoot = driverRoot;
            Logger.Info($"Playwright 驱动已就绪: {driverRoot}");
            return driverRoot;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// 获取"安装浏览器"的操作提示（驱动就绪后给出可直接执行的命令）。
    /// </summary>
    public static string GetInstallBrowsersHint()
    {
        if (_resolvedDriverRoot != null)
        {
            var node = GetNodePath(_resolvedDriverRoot);
            var cli = Path.Combine(_resolvedDriverRoot, ".playwright", "package", "cli.js");
            return $"\"{node}\" \"{cli}\" install chromium";
        }
        return $"npx playwright@{PlaywrightVersion} install chromium";
    }

    private static bool IsValidDriverRoot(string root)
    {
        return File.Exists(Path.Combine(root, ".playwright", "package", "cli.js"))
            && File.Exists(GetNodePath(root));
    }

    private static string GetNodePath(string root)
    {
        var (platformId, nodeExe) = GetPlatformNode();
        return Path.Combine(root, ".playwright", "node", platformId, nodeExe);
    }

    private static (string PlatformId, string NodeExe) GetPlatformNode()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return ("win32_x64", "node.exe");
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return (RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "darwin-arm64" : "darwin-x64", "node");
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return (RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "linux-arm64" : "linux-x64", "node");
        }
        throw new InvalidOperationException("当前操作系统不支持 Playwright 驱动自动安装");
    }

    private static async Task DownloadDriverAsync(string driverRoot, CancellationToken cancellationToken)
    {
        // 先释放到临时目录，完成后原子移动，避免中断留下半成品
        var staging = driverRoot.TrimEnd(Path.DirectorySeparatorChar) + ".staging-" + Guid.NewGuid().ToString("N")[..8];
        var nupkgPath = Path.Combine(Path.GetTempPath(), $"microsoft.playwright.{PlaywrightVersion}.{Guid.NewGuid():N}.nupkg");

        try
        {
            var url = $"https://api.nuget.org/v3-flatcontainer/microsoft.playwright/{PlaywrightVersion}/microsoft.playwright.{PlaywrightVersion}.nupkg";
            Logger.Info($"正在下载 Playwright 驱动: {url}");
            var bytes = await _http.GetByteArrayAsync(url, cancellationToken);
            await File.WriteAllBytesAsync(nupkgPath, bytes, cancellationToken);

            var (platformId, nodeExe) = GetPlatformNode();
            var nodePrefix = $".playwright/node/{platformId}/";
            const string packagePrefix = ".playwright/package/";

            using (var zip = ZipFile.OpenRead(nupkgPath))
            {
                foreach (var entry in zip.Entries)
                {
                    var name = entry.FullName.Replace('\\', '/');
                    var isNeeded = name.StartsWith(nodePrefix, StringComparison.OrdinalIgnoreCase)
                        || name.StartsWith(packagePrefix, StringComparison.OrdinalIgnoreCase)
                        || name.Equals(".playwright/node/LICENSE", StringComparison.OrdinalIgnoreCase);
                    if (!isNeeded || string.IsNullOrEmpty(entry.Name))
                    {
                        continue;
                    }

                    var dest = Path.Combine(staging, name.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                    entry.ExtractToFile(dest, overwrite: true);

                    // zip 不保留 unix 可执行位，linux/osx 下补赋权
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && entry.Name == nodeExe)
                    {
                        File.SetUnixFileMode(dest, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute
                            | UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                            | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
                    }
                }
            }

            if (Directory.Exists(driverRoot))
            {
                Directory.Delete(driverRoot, recursive: true);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(driverRoot)!);
            Directory.Move(staging, driverRoot);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            Logger.Error("Playwright 驱动下载失败", ex, driverRoot);
            throw new InvalidOperationException(
                $"Playwright 驱动自动安装失败: {ex.Message}\n\n可手动下载 https://www.nuget.org/packages/Microsoft.Playwright/{PlaywrightVersion} 的 nupkg，" +
                $"将其中的 .playwright 目录释放到: {driverRoot}", ex);
        }
        finally
        {
            try { if (File.Exists(nupkgPath)) File.Delete(nupkgPath); } catch { }
            try { if (Directory.Exists(staging)) Directory.Delete(staging, recursive: true); } catch { }
        }
    }
}
