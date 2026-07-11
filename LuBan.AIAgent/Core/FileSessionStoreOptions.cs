using System.IO;

namespace LuBan.AIAgent.Core;

/// <summary>
/// 文件会话存储选项
/// </summary>
public class FileSessionStoreOptions
{
    /// <summary>
    /// 会话存储的根目录，默认为应用程序基础目录下的"agileai-sessions"文件夹
    /// </summary>
    public string RootDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "agileai-sessions");
}
