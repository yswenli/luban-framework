/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*命名空间：LuBan.XTestProject
*文件名： PluginTestDirectory
*描述：插件相关测试的临时目录夹具（自建自清，避免污染真实插件目录）
*
*****************************************************************************/
namespace LuBan.XTestProject;

/// <summary>
/// 临时目录夹具：为插件相关测试提供隔离的目录，<see cref="Dispose"/> 时递归删除。
/// </summary>
internal sealed class PluginTestDirectory : IDisposable
{
    private bool _disposed;

    private PluginTestDirectory(string path)
    {
        Path = path;
    }

    /// <summary>
    /// 临时目录绝对路径。
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// 创建一个唯一的临时目录。
    /// </summary>
    public static PluginTestDirectory Create()
    {
        var path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "luban-plugin-test-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(path);
        return new PluginTestDirectory(path);
    }

    /// <summary>
    /// 在夹具目录下写文件（自动创建父目录）。
    /// </summary>
    /// <param name="relativePath">相对路径（用 <c>Path.Combine</c> 拼接）。</param>
    /// <param name="content">文件内容。</param>
    /// <returns>写入文件的绝对路径。</returns>
    public string WriteFile(string relativePath, string content)
    {
        var fullPath = System.IO.Path.Combine(Path, relativePath);

        var parent = System.IO.Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(parent))
            Directory.CreateDirectory(parent);

        File.WriteAllText(fullPath, content);
        return fullPath;
    }

    /// <summary>
    /// 在夹具目录下创建子目录。
    /// </summary>
    /// <param name="relativePath">相对路径。</param>
    /// <returns>子目录绝对路径。</returns>
    public string CreateDirectory(string relativePath)
    {
        var fullPath = System.IO.Path.Combine(Path, relativePath);
        Directory.CreateDirectory(fullPath);
        return fullPath;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
        catch
        {
            // 清理失败不影响测试结论（临时目录由系统回收）
        }
    }
}